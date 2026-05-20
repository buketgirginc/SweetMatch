using System.Collections;
using SweetMatch.Data;
using SweetMatch.Events;
using SweetMatch.Model;
using SweetMatch.Systems;
using SweetMatch.Presentation.Animation;
using SweetMatch.Presentation.Effects;
using SweetMatch.Presentation.Game;
using SweetMatch.Presentation.UI;
using UnityEngine;

namespace SweetMatch.Bootstrap
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private LevelConfigSO[] levels;
        [SerializeField] private GridConfigSO gridConfig;

        [Header("Views")]
        [SerializeField] private GridView gridView;
        [SerializeField] private GridFrameView frameView;
        [SerializeField] private MovesView movesView;
        [SerializeField] private LevelLabelView levelLabelView;
        [SerializeField] private GoalPanelView goalPanelView;
        [SerializeField] private EndPanelView endPanelView;
        [SerializeField] private StartPanelView startPanelView;

        [Header("Animation")]
        [SerializeField] private BoardAnimator boardAnimator;
        [SerializeField] private GoalFlyController goalFlyController;

        [Header("Effects")]
        [SerializeField] private SoundController soundController;
        [SerializeField] private VFXController vfxController;

        [Header("Data")]
        [SerializeField] private ItemVisualConfigSO visualConfig;

        private EventBus _eventBus;
        private GridModel _gridModel;
        private GameStateMachine _stateMachine;
        private MovesTracker _movesTracker;
        private GoalSystem _goalSystem;
        private MatchDetector _matchDetector;
        private ClearSystem _clearSystem;
        private FallSystem _fallSystem;
        private FillSystem _fillSystem;
        private PowerUpSpawner _powerUpSpawner;
        private NeighborTrigger _neighborTrigger;
        private BottomTrigger _bottomTrigger;
        private InputHandler _inputHandler;
        private MoveResolver _moveResolver;
        private IItemFactory _itemFactory;
        private BoardBuilder _boardBuilder;

        private LevelConfigSO _activeLevel;

        private IEnumerator Start()
        {
            if (!ValidateReferences()) yield break;

            BuildEventBus();
            SelectActiveLevel();
            BuildModel();
            BuildSystems();
            BuildViews();
            BuildInitialBoard();

            if (!LevelProgress.HasSeenStartScreen)
            {
                yield return startPanelView.ShowAndWaitForClick();
                LevelProgress.MarkStartScreenSeen();
            }

            yield return StartGame();
        }

        private bool ValidateReferences()
        {
            if (levels == null || levels.Length == 0) { Debug.LogError("[Bootstrap] Levels array is empty!"); return false; }
            if (gridConfig == null) { Debug.LogError("[Bootstrap] GridConfig is missing!"); return false; }
            if (gridView == null) { Debug.LogError("[Bootstrap] GridView is missing!"); return false; }
            if (frameView == null) { Debug.LogError("[Bootstrap] GridFrameView is missing!"); return false; }
            if (movesView == null) { Debug.LogError("[Bootstrap] MovesView is missing!"); return false; }
            if (levelLabelView == null) { Debug.LogError("[Bootstrap] LevelLabelView is missing!"); return false; }
            if (goalPanelView == null) { Debug.LogError("[Bootstrap] GoalPanelView is missing!"); return false; }
            if (visualConfig == null) { Debug.LogError("[Bootstrap] ItemVisualConfig is missing!"); return false; }
            if (endPanelView == null) { Debug.LogError("[Bootstrap] EndPanelView is missing!"); return false; }
            if (startPanelView == null) { Debug.LogError("[Bootstrap] StartPanelView is missing!"); return false; }
            if (boardAnimator == null) { Debug.LogError("[Bootstrap] BoardAnimator is missing!"); return false; }
            if (goalFlyController == null) { Debug.LogError("[Bootstrap] GoalFlyController is missing!"); return false; }
            if (soundController == null) { Debug.LogError("[Bootstrap] SoundController is missing!"); return false; }
            if (vfxController == null) { Debug.LogError("[Bootstrap] VFXController is missing!"); return false; }
            return true;
        }

        private void BuildEventBus()
        {
            _eventBus = new EventBus();
        }

        // PlayerPrefs'ten gelen index dış kaynaklı (eski build farklı level sayısı yazmış olabilir) → clamp.
        private void SelectActiveLevel()
        {
            int index = Mathf.Clamp(LevelProgress.CurrentIndex, 0, levels.Length - 1);
            _activeLevel = levels[index];
        }

        private void BuildModel()
        {
            _gridModel = new GridModel(gridConfig.Width, gridConfig.Height);
        }

        private void BuildSystems()
        {
            _itemFactory = new ItemFactory();

            _stateMachine = new GameStateMachine(_eventBus);
            _movesTracker = new MovesTracker(_activeLevel.Moves, _eventBus);
            _goalSystem = new GoalSystem(_activeLevel.Goals, _eventBus);

            _matchDetector = new MatchDetector(_gridModel);
            _clearSystem = new ClearSystem(_gridModel, _eventBus);
            _fallSystem = new FallSystem(_gridModel, _eventBus);
            _fillSystem = new FillSystem(_gridModel, _eventBus, _itemFactory);
            _powerUpSpawner = new PowerUpSpawner(_gridModel, _itemFactory);
            _neighborTrigger = new NeighborTrigger(_gridModel);
            _bottomTrigger = new BottomTrigger(_gridModel);

            // BoardBuilder MatchDetector'a bağımlı (HasAnyMatch),
            // MoveResolver'a inject edilecek (deadlock'ta Shuffle) → MoveResolver'dan ÖNCE yaratılır.
            _boardBuilder = new BoardBuilder(
                _gridModel, _activeLevel, _itemFactory, _matchDetector);

            _inputHandler = new InputHandler(_stateMachine, _eventBus);

            _moveResolver = new MoveResolver(
                _eventBus, _gridModel,
                _matchDetector, _clearSystem,
                _neighborTrigger, _powerUpSpawner,
                _fallSystem, _fillSystem, _bottomTrigger,
                _movesTracker, _stateMachine,
                boardAnimator, goalFlyController,
                _boardBuilder, this);
        }

        private void BuildViews()
        {
            gridView.Build(_gridModel, _inputHandler);
            frameView.Fit(gridConfig.Width, gridConfig.Height, gridView.CellSize, gridView.CellSpacing);
            movesView.Initialize(_eventBus, _activeLevel.Moves);
            levelLabelView.Initialize(LevelProgress.CurrentIndex + 1);
            goalPanelView.Initialize(_eventBus, _activeLevel, visualConfig);
            endPanelView.Initialize(_eventBus, levels.Length);
            boardAnimator.Initialize(_eventBus);
            goalFlyController.Initialize(_eventBus);
            soundController.Initialize(_eventBus);
            vfxController.Initialize(_eventBus);
        }

        // Sadece modeli kurar. View bind'i (RenderAll) cascade'in içinde —
        // tek sorumlu nokta, hem initial hem shuffle aynı yolu kullanır.
        private void BuildInitialBoard()
        {
            _boardBuilder.Build();
        }

        private IEnumerator StartGame()
        {
            yield return boardAnimator.PlayInitialBoardCascade();
            _stateMachine.SetState(GameState.Idle);
        }
    }
}