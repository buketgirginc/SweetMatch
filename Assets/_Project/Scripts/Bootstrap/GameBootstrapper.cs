using System.Collections;
using SweetMatch.Data;
using SweetMatch.Events;
using SweetMatch.Model;
using SweetMatch.Systems;
using SweetMatch.Presentation.Animation;
using SweetMatch.Presentation.Game;
using SweetMatch.Presentation.UI;
using UnityEngine;

namespace SweetMatch.Bootstrap
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private LevelConfigSO levelConfig;
        [SerializeField] private GridConfigSO gridConfig;

        [Header("Views")]
        [SerializeField] private GridView gridView;
        [SerializeField] private GridFrameView frameView;
        [SerializeField] private MovesView movesView;
        [SerializeField] private GoalPanelView goalPanelView;
        [SerializeField] private EndPanelView endPanelView;

        [Header("Animation")]
        [SerializeField] private BoardAnimator boardAnimator;
        [SerializeField] private GoalFlyController goalFlyController;

        [Header("Data")]
        [SerializeField] private ItemVisualConfigSO visualConfig;

        // Sistemler — sahne içinde tek instance
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

        private IEnumerator Start()
        {
            if (!ValidateReferences()) yield break;

            BuildEventBus();
            BuildModel();
            BuildSystems();
            BuildViews();
            BuildInitialBoard();
            yield return StartGame();
        }

        // Inspector referanslarının dolu olduğunu kontrol et
        private bool ValidateReferences()
        {
            if (levelConfig == null)
            {
                Debug.LogError("[Bootstrap] LevelConfig is missing!");
                return false;
            }
            if (gridConfig == null)
            {
                Debug.LogError("[Bootstrap] GridConfig is missing!");
                return false;
            }
            if (gridView == null)
            {
                Debug.LogError("[Bootstrap] GridView is missing!");
                return false;
            }
            if (frameView == null)
            {
                Debug.LogError("[Bootstrap] GridFrameView is missing!");
                return false;
            }
            if (movesView == null)
            {
                Debug.LogError("[Bootstrap] MovesView is missing!");
                return false;
            }
            if (goalPanelView == null)
            {
                Debug.LogError("[Bootstrap] GoalPanelView is missing!");
                return false;
            }
            if (visualConfig == null)
            {
                Debug.LogError("[Bootstrap] ItemVisualConfig is missing!");
                return false;
            }
            if (endPanelView == null)
            {
                Debug.LogError("[Bootstrap] EndPanelView is missing!");
                return false;
            }
            if (boardAnimator == null)
            {
                Debug.LogError("[Bootstrap] BoardAnimator is missing!");
                return false;
            }

            if (goalFlyController == null)
            {
                Debug.LogError("[Bootstrap] GoalFlyController is missing!");
                return false;
            }
            return true;
        }

        private void BuildEventBus()
        {
            _eventBus = new EventBus();
        }

        private void BuildModel()
        {
            _gridModel = new GridModel(gridConfig.Width, gridConfig.Height);
        }

        private void BuildSystems()
        {
            _itemFactory = new ItemFactory();

            _stateMachine = new GameStateMachine(_eventBus);
            _movesTracker = new MovesTracker(levelConfig.Moves, _eventBus);
            _goalSystem = new GoalSystem(levelConfig.Goals, _eventBus);

            _matchDetector = new MatchDetector(_gridModel);
            _clearSystem = new ClearSystem(_gridModel, _eventBus);
            _fallSystem = new FallSystem(_gridModel, _eventBus);
            _fillSystem = new FillSystem(_gridModel, _eventBus, _itemFactory);
            _powerUpSpawner = new PowerUpSpawner(_gridModel, _itemFactory);
            _neighborTrigger = new NeighborTrigger(_gridModel);
            _bottomTrigger = new BottomTrigger(_gridModel);

            _inputHandler = new InputHandler(_stateMachine, _eventBus);

            _moveResolver = new MoveResolver(
                _eventBus, _gridModel,
                _matchDetector, _clearSystem,
                _neighborTrigger, _powerUpSpawner,
                _fallSystem, _fillSystem, _bottomTrigger,
                _movesTracker, _stateMachine,
                boardAnimator, goalFlyController, this);
        }

        private void BuildViews()
        {
            gridView.Build(_gridModel, _inputHandler);
            frameView.Fit(gridConfig.Width, gridConfig.Height, gridView.CellSize, gridView.CellSpacing);
            movesView.Initialize(_eventBus, levelConfig.Moves);
            goalPanelView.Initialize(_eventBus, levelConfig, visualConfig);
            endPanelView.Initialize(_eventBus);
            boardAnimator.Initialize(_eventBus);
            goalFlyController.Initialize(_eventBus);
        }

        private void BuildInitialBoard()
        {
            var builder = new InitialBoardBuilder(_gridModel, levelConfig, _itemFactory);
            builder.Build();
            gridView.RenderAll();
        }

        // Initial cascade animasyonu oyun başlamadan oynar; state Loading'de kalır
        // (InputHandler tıklamayı yutar), animasyon bitince Idle'a geçer.
        private IEnumerator StartGame()
        {
            yield return boardAnimator.PlayInitialBoardCascade();
            _stateMachine.SetState(GameState.Idle);
        }
    }
}