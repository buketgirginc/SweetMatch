using System.Collections;
using System.Collections.Generic;
using SweetMatch.Events;
using SweetMatch.Model;
using SweetMatch.Model.Items;
using SweetMatch.Presentation.Game;
using SweetMatch.Presentation.UI;
using UnityEngine;

namespace SweetMatch.Presentation.Animation
{
    // Goal collection feedback'ini yönetir.
    // Sweet patladığında goal panel'deki ilgili ikona uçan kopyalar oluşturur, bezier yörünge
    // ile animate eder, hedefe ulaştığında görsel sayacı azaltır. Tüm fly'lar bittikten sonra
    // mantıksal olarak tamamlanmış goal'ler için Win event'ini tetikler.
    public class GoalFlyController : MonoBehaviour
    {
        [SerializeField] private GridView gridView;
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private GoalPanelView goalPanelView;

        // Fly yörüngesi parametreleri.
        private const float FlyDuration = 0.5f;
        private const float Stagger = 0.1f;                 // ardışık fly'lar arası gecikme (ritim)
        private const float BezierArcHeightMin = 150f;      // kavis tepe noktası alt sınır (Canvas piksel)
        private const float BezierArcHeightMax = 250f;
        private const float BezierLateralOffset = 80f;      // control point X eksen rastgele sapma (±)
        private const float EndScaleFactor = 0.45f;         // başlangıç scale'inin kaçta kaçına küçülür

        private EventBus _eventBus;

        // Aktif + planlanmış fly'ları signature başına takip eder (race condition önleme).
        // LocalCount görsel sayaç ve fly bittiğinde düşer; bu tablo "havada olan" fly'ları sayar.
        // Kapasite = LocalCount - activeReservations.
        private readonly Dictionary<string, int> _activeFlyReservations = new();

        private int _activeFlies;
        private bool _goalsLogicallyCompleted;

        // CandyBar akışında ItemsClearedEvent fly başlatmamalı (CandyBar her sweet'i kendi pop anında uçurur).
        // MoveResolver Clear çağrısı öncesi true, sonrası false yapar.
        private bool _flySuppressed;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<ItemsClearedEvent>(OnItemsCleared);
            _eventBus.Subscribe<GoalsLogicallyCompletedEvent>(OnGoalsLogicallyCompleted);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<ItemsClearedEvent>(OnItemsCleared);
            _eventBus?.Unsubscribe<GoalsLogicallyCompletedEvent>(OnGoalsLogicallyCompleted);
        }

        // Match akışında çağrılır: stagger ile fly'ları başlatır.
        // CandyBar akışında suppressed olur — fly'lar PlayCandyBarActivation içinde dalga dalga başlatılır.
        private void OnItemsCleared(ItemsClearedEvent e)
        {
            if (_flySuppressed) return;

            int flyIndex = 0;

            foreach (var item in e.ClearedItems)
            {
                if (!TryReserveFlySlot(item, out var sig, out var goalView))
                    continue;

                var startCellView = gridView.GetCellView(item.Position);

                _activeFlies++;
                StartCoroutine(FlyToGoal(sig, goalView, startCellView, flyIndex * Stagger));
                flyIndex++;
            }
        }

        // CandyBar koreografisi için: tek bir sweet için kapasite kontrolü + fly başlat (stagger yok).
        // BoardAnimator.PlayCandyBarActivation içindeki DelayedCall'lardan çağrılır.
        public void TryStartFlyForSweet(CellModel cell, GridItem item)
        {
            if (!TryReserveFlySlot(item, out var sig, out var goalView))
                return;

            var startCellView = gridView.GetCellView(cell.Position);

            _activeFlies++;
            StartCoroutine(FlyToGoal(sig, goalView, startCellView, 0f));
        }

        // MoveResolver çağırır: CandyBar akışında ItemsClearedEvent'in fly başlatmasını engeller.
        public void SetFlySuppressed(bool suppressed)
        {
            _flySuppressed = suppressed;
        }

        // Item fly için uygun mu kontrol eder + global reservation tablosuna kaydeder.
        // Reservation fly bitince azalır → aynı goal'e ardışık aktive fly'lar doğru sayılır.
        private bool TryReserveFlySlot(GridItem item, out string signature, out GoalItemView goalView)
        {
            signature = null;
            goalView = null;

            if (!(item is SweetItem)) return false;

            string sig = item.GetGoalSignature();
            if (sig == null) return false;

            var view = goalPanelView.GetGoalView(sig);
            if (view == null) return false;

            int activeCount = _activeFlyReservations.TryGetValue(sig, out var v) ? v : 0;
            int remainingCapacity = view.LocalCount - activeCount;
            if (remainingCapacity <= 0) return false;

            _activeFlyReservations[sig] = activeCount + 1;
            signature = sig;
            goalView = view;
            return true;
        }

        private void OnGoalsLogicallyCompleted(GoalsLogicallyCompletedEvent e)
        {
            _goalsLogicallyCompleted = true;
            CheckWinCondition();
        }

        // Tüm fly'lar bittikten + goal mantıksal olarak tamamlandıktan sonra Win event'ini tetikler.
        private void CheckWinCondition()
        {
            if (_goalsLogicallyCompleted && _activeFlies == 0)
            {
                _goalsLogicallyCompleted = false;
                _eventBus.Raise(new AllGoalsCompletedEvent());
            }
        }

        // Tek bir sweet kopyasının goal'e bezier yörüngeyle uçuşu.
        // ItemView klonlanır, Canvas-local koordinatlarda quadratic bezier ile animate edilir.
        private IEnumerator FlyToGoal(
            string signature,
            GoalItemView goalView,
            CellView sourceCellView,
            float startDelay)
        {
            if (startDelay > 0f)
                yield return new WaitForSeconds(startDelay);

            // Klonu hazırla — mevcut ItemView'dan kopyala, Canvas'a parent'la (her şeyin önünde).
            var clone = Instantiate(sourceCellView.ItemView.gameObject, canvasRect);
            var cloneRect = (RectTransform)clone.transform;
            cloneRect.SetAsLastSibling();

            // Canvas-local pozisyonlar: world'den çevir, koordinat sistemleri tutarlı.
            Vector2 startPos = canvasRect.InverseTransformPoint(sourceCellView.transform.position);
            Vector2 endPos = canvasRect.InverseTransformPoint(goalView.IconImage.transform.position);

            cloneRect.anchoredPosition = startPos;

            // Klonlanan ItemView görünür olmalı (orijinal pop yüzünden invisible olabilir).
            var clonedItemView = clone.GetComponent<ItemView>();
            clonedItemView.SetVisible(true);

            Vector3 startScale = cloneRect.localScale;
            Vector3 endScale = startScale * EndScaleFactor;

            // Bezier control point: tepe noktası yukarı, X ekseni rastgele sapma.
            // Her fly farklı yörünge → doğal hissi, robotik dizilim yok.
            float arcHeight = Random.Range(BezierArcHeightMin, BezierArcHeightMax);
            float lateralOffset = Random.Range(-BezierLateralOffset, BezierLateralOffset);
            Vector2 midPoint = (startPos + endPos) * 0.5f;
            Vector2 controlPoint = midPoint + new Vector2(lateralOffset, arcHeight);

            float elapsed = 0f;
            while (elapsed < FlyDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / FlyDuration);
                float tSmooth = t * t * (3f - 2f * t);  // smoothstep ease-in-out

                // Quadratic bezier: B(t) = (1-t)² P0 + 2(1-t)t P1 + t² P2
                float u = 1f - tSmooth;
                Vector2 pos = (u * u * startPos) + (2f * u * tSmooth * controlPoint) + (tSmooth * tSmooth * endPos);

                cloneRect.anchoredPosition = pos;
                cloneRect.localScale = Vector3.Lerp(startScale, endScale, tSmooth);

                yield return null;
            }

            Destroy(clone);
            goalPanelView.DecrementGoal(signature);

            // Goal collect sesi için event.
            _eventBus.Raise(new GoalCollectedEvent());

            // Reservation'ı düş — bu fly artık aktif değil, sonraki sayımlar bunu hesaba katmaz.
            if (_activeFlyReservations.TryGetValue(signature, out var count))
                _activeFlyReservations[signature] = Mathf.Max(0, count - 1);

            _activeFlies--;
            CheckWinCondition();
        }
    }
}