using System.Collections;
using DG.Tweening;
using SweetMatch.Presentation.Effects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SweetMatch.Presentation.UI
{
    public class StartPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject backdrop;
        [SerializeField] private GameObject panelContent;
        [SerializeField] private Button startButton;
        [SerializeField] private SoundController soundController;

        private const float EntryDuration = 0.3f;
        private const float ExitDuration = 0.2f;
        private const float ButtonClickDelay = 0.25f;

        private bool _clicked;

        private void Awake()
        {
            // Sahne yüklenir yüklenmez gizli — Bootstrapper Show çağırana kadar görünmemeli.
            backdrop.SetActive(false);
            panelContent.SetActive(false);

            startButton.onClick.AddListener(OnStartClicked);
        }

        public IEnumerator ShowAndWaitForClick()
        {
            _clicked = false;
            Show();
            yield return new WaitWhile(() => !_clicked);
            yield return HideRoutine();
        }

        private void Show()
        {
            backdrop.SetActive(true);
            panelContent.SetActive(true);

            panelContent.transform.localScale = Vector3.zero;
            panelContent.transform.DOScale(Vector3.one, EntryDuration).SetEase(Ease.OutBack);
        }

        private void OnStartClicked()
        {
            if (_clicked) return;  // çift tık koruması — coroutine çıkmadan ikinci tıklama
            _clicked = true;
            if (soundController != null) soundController.PlayButtonClick();
        }

        private IEnumerator HideRoutine()
        {
            // Ses başlasın diye delay (EndPanel'le aynı timing kuralı)
            // ve aynı anda scale-out — paralel.
            panelContent.transform.DOScale(Vector3.zero, ExitDuration).SetEase(Ease.InBack);
            yield return new WaitForSeconds(Mathf.Max(ButtonClickDelay, ExitDuration));

            panelContent.SetActive(false);
            backdrop.SetActive(false);
        }

        private void OnDestroy()
        {
            if (startButton != null) startButton.onClick.RemoveListener(OnStartClicked);
        }
    }
}