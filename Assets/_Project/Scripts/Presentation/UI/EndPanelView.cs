using SweetMatch.Events;
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
        [SerializeField] private Button closeButton;

        private EventBus _eventBus;

        public void Initialize(EventBus bus)
        {
            _eventBus = bus;
            Hide();

            _eventBus.Subscribe<GameStateChangedEvent>(OnStateChanged);

            restartButton.onClick.AddListener(OnRestartClicked);
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnStateChanged(GameStateChangedEvent e)
        {
            if (e.Current == GameState.Won)
                Show("You Won!");
            else if (e.Current == GameState.Lost)
                Show("Game Over");
        }

        private void Show(string title)
        {
            titleText.text = title;
            backdrop.SetActive(true);
            panelContent.SetActive(true);
        }

        private void Hide()
        {
            backdrop.SetActive(false);
            panelContent.SetActive(false);
        }

        private void OnRestartClicked()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnCloseClicked()
        {
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