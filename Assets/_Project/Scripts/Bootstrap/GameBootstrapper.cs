using SweetMatch.Data;
using SweetMatch.Events;
using SweetMatch.Model;
using SweetMatch.Systems;
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

        private void Start()
        {
            if (!ValidateReferences()) return;

            BuildEventBus();
            BuildModel();
            BuildSystems();
            SubscribeDebugLogs();
            BuildViews();
            BuildInitialBoard();
            StartGame();

            LogGridSnapshot("Initial board");
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
                _movesTracker, _stateMachine);
        }

        private void BuildViews()
        {
            gridView.Build(_gridModel, _inputHandler);
            frameView.Fit(gridConfig.Width, gridConfig.Height, gridView.CellSize, gridView.CellSpacing);
            movesView.Initialize(_eventBus, levelConfig.Moves);
            goalPanelView.Initialize(_eventBus, levelConfig, visualConfig);
            endPanelView.Initialize(_eventBus);

            _eventBus.Subscribe<ItemsClearedEvent>(_ => gridView.RenderAll());
            _eventBus.Subscribe<ItemsFellEvent>(_ => gridView.RenderAll());
            _eventBus.Subscribe<ItemsSpawnedEvent>(_ => gridView.RenderAll());
        }

        // Faz 4 boyunca akışı console'da izlemek için event'lere debug abonelikleri
        private void SubscribeDebugLogs()
        {
            _eventBus.Subscribe<CellClickedEvent>(e =>
                Debug.Log($"[Click] {e.Position}"));

            _eventBus.Subscribe<ItemsClearedEvent>(e =>
                Debug.Log($"[Clear] {e.ClearedItems.Count} items destroyed"));

            _eventBus.Subscribe<ItemsFellEvent>(e =>
                Debug.Log($"[Fall] {e.Moves.Count} items fell"));

            _eventBus.Subscribe<ItemsSpawnedEvent>(e =>
                Debug.Log($"[Spawn] {e.Spawns.Count} new items"));

            _eventBus.Subscribe<MovesChangedEvent>(e =>
                Debug.Log($"[Moves] Remaining: {e.Remaining}"));

            _eventBus.Subscribe<GoalProgressEvent>(e =>
                Debug.Log($"[Goal] {e.Signature} → {e.Remaining} left"));

            _eventBus.Subscribe<AllGoalsCompletedEvent>(_ =>
                Debug.Log("[Goal] ALL COMPLETE!"));

            _eventBus.Subscribe<NoMovesLeftEvent>(_ =>
                Debug.Log("[Moves] OUT OF MOVES"));

            _eventBus.Subscribe<GameStateChangedEvent>(e =>
                Debug.Log($"[State] {e.Previous} → {e.Current}"));
        }

        private void BuildInitialBoard()
        {
            var builder = new InitialBoardBuilder(_gridModel, levelConfig, _itemFactory);
            builder.Build();
            gridView.RenderAll();
        }

        private void StartGame()
        {
            _stateMachine.SetState(GameState.Idle);
        }

        // === Manuel test için public API ===
        // Faz 5'te CellView InputHandler'ı çağırınca bu method gereksiz olacak

        // Inspector veya başka script'ten tıklama simüle et
        public void SimulateClick(int x, int y)
        {
            _inputHandler.HandleCellClick(new GridPosition(x, y));
        }

        // Grid'in mevcut durumunu console'a yazdır
        public void LogGridSnapshot(string label)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== {label} ===");

            for (int y = _gridModel.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < _gridModel.Width; x++)
                {
                    var cell = _gridModel.GetCell(x, y);
                    sb.Append(cell.IsEmpty ? "[ . ]" : $"[{ItemSymbol(cell.Item)}]");
                }
                sb.AppendLine();
            }

            Debug.Log(sb.ToString());
        }

        // Item'ı 3 karakterlik kısa sembolle göster (grid log için)
        private string ItemSymbol(Model.Items.GridItem item)
        {
            return item switch
            {
                Model.Items.SweetItem s => $" {s.SweetType.ToString().Substring(0, 1)} ",
                Model.Items.CandyBarItem cb => cb.Direction == Model.Items.CandyBarDirection.Horizontal ? "═══" : " ║ ",
                Model.Items.CupcakeItem => "CUP",
                Model.Items.CroissantItem => "CRO",
                _ => " ? "
            };
        }
    }
}