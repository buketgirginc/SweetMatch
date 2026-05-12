using SweetMatch.Events;
using SweetMatch.Model.Items;
using UnityEngine;

namespace SweetMatch.Presentation.Effects
{
    // Event-driven sound playback. AudioSource.PlayOneShot ile her ses paralel çalabilir
    // (3 sweet patlaması aynı anda 3 pop sesi gibi).
    // SerializeField clip'leri Inspector'dan atanır, kod sadece event handler.
    public class SoundController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        [Header("Match & Pop")]
        [SerializeField] private AudioClip popSweet;
        [SerializeField] private AudioClip popCupcake;
        [SerializeField] private AudioClip popCroissant;

        [Header("Power-ups & Collection")]
        [SerializeField] private AudioClip candybarActivate;
        [SerializeField] private AudioClip goalCollect;

        [Header("Game State")]
        [SerializeField] private AudioClip levelWin;
        [SerializeField] private AudioClip levelLose;

        [Header("UI")]
        [SerializeField] private AudioClip buttonClick;

        private EventBus _eventBus;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<ItemsClearedEvent>(OnItemsCleared);
            _eventBus.Subscribe<GoalCollectedEvent>(OnGoalCollected);
            _eventBus.Subscribe<GameStateChangedEvent>(OnStateChanged);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<ItemsClearedEvent>(OnItemsCleared);
            _eventBus?.Unsubscribe<GoalCollectedEvent>(OnGoalCollected);
            _eventBus?.Unsubscribe<GameStateChangedEvent>(OnStateChanged);
        }

        // Cleared items içinde hangi türler varsa o türün sesi paralel çalınır.
        // Birden fazla aynı türden item olsa bile her tür için tek seferlik ses.
        private void OnItemsCleared(ItemsClearedEvent e)
        {
            bool hasSweet = false;
            bool hasCupcake = false;
            bool hasCroissant = false;
            bool hasCandyBar = false;

            foreach (var item in e.ClearedItems)
            {
                if (item is SweetItem) hasSweet = true;
                else if (item is CupcakeItem) hasCupcake = true;
                else if (item is CroissantItem) hasCroissant = true;
                else if (item is CandyBarItem) hasCandyBar = true;
            }

            if (hasSweet) Play(popSweet);
            if (hasCupcake) Play(popCupcake);
            if (hasCroissant) Play(popCroissant);
            if (hasCandyBar) Play(candybarActivate);
        }

        private void OnGoalCollected(GoalCollectedEvent e)
        {
            Play(goalCollect);
        }

        private void OnStateChanged(GameStateChangedEvent e)
        {
            if (e.Current == GameState.Won) Play(levelWin);
            else if (e.Current == GameState.Lost) Play(levelLose);
        }

        // Public — UI button'lar direkt çağırır (event üzerinden değil, UI sistemi event'siz).
        public void PlayButtonClick()
        {
            Play(buttonClick);
        }

        private void Play(AudioClip clip)
        {
            if (clip == null || audioSource == null) return;
            audioSource.PlayOneShot(clip);
        }
    }
}