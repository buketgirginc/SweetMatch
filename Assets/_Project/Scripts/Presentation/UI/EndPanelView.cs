using System.Collections;
using DG.Tweening;
using SweetMatch.Events;
using SweetMatch.Presentation.Effects;
using SweetMatch.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SweetMatch.Presentation.UI
{
    public class EndPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject backdrop;
        [SerializeField] private GameObject panelContent;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button restartButton;
        [SerializeField] private TMP_Text restartButtonLabel;
        [SerializeField] private Button closeButton;
        [SerializeField] private SoundController soundController;

        private const float EntryDuration = 0.3f;
        private const float ButtonClickDelay = 0.25f;  // ses başlasın diye scene/quit öncesi bekleme

        private EventBus _eventBus;
        private int _totalLevels;
        private bool _won;
        private bool _wonAtLastLevel;

        public void Initialize(EventBus bus, int totalLevels)
        {
            _eventBus = bus;
            _totalLevels = totalLevels;
            Hide();

            _eventBus.Subscribe<GameStateChangedEvent>(OnStateChanged);

            restartButton.onClick.AddListener(OnRestartClicked);
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnStateChanged(GameStateChangedEvent e)
        {
            if (e.Current == GameState.Won)
            {
                _won = true;
                _wonAtLastLevel = LevelProgress.CurrentIndex >= _totalLevels - 1;
                Show("You Won!", _wonAtLastLevel ? "Restart Game" : "Next Level");
            }
            else if (e.Current == GameState.Lost)
            {
                _won = false;
                _wonAtLastLevel = false;
                Show("Game Over", "Restart");
            }
        }

        private void Show(string title, string restartLabel)
        {
            titleText.text = title;
            restartButtonLabel.text = restartLabel;
            backdrop.SetActive(true);
            panelContent.SetActive(true);

            // Scale-in bounce: 0'dan 1'e OutBack ile hafif overshoot (pop hissi).
            panelContent.transform.localScale = Vector3.zero;
            panelContent.transform.DOScale(Vector3.one, EntryDuration).SetEase(Ease.OutBack);
        }

        private void Hide()
        {
            // Aktif tween varsa durdur — restart sonrası scale tutarlı kalır.
            panelContent.transform.DOKill();
            panelContent.transform.localScale = Vector3.one;

            backdrop.SetActive(false);
            panelContent.SetActive(false);
        }

        // Ses başladıktan kısa süre sonra eylemi gerçekleştir.
        private void OnRestartClicked()
        {
            StartCoroutine(RestartRoutine());
        }

        private void OnCloseClicked()
        {
            StartCoroutine(CloseRoutine());
        }

        private IEnumerator RestartRoutine()
        {
            if (soundController != null) soundController.PlayButtonClick();
            yield return new WaitForSeconds(ButtonClickDelay);

            // Progression kararı reload'dan ÖNCE yazılmalı — Bootstrapper yeni sahnede okuyacak.
            if (_wonAtLastLevel)
                LevelProgress.Reset();
            else if (_won)
                LevelProgress.Advance(_totalLevels - 1);
            // Lost: index dokunulmaz, aynı level reload.

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private IEnumerator CloseRoutine()
        {
            if (soundController != null) soundController.PlayButtonClick();
            yield return new WaitForSeconds(ButtonClickDelay);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<GameStateChangedEvent>(OnStateChanged);

            if (restartButton != null) restartButton.onClick.RemoveListener(OnRestartClicked);
            if (closeButton != null) closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }
}