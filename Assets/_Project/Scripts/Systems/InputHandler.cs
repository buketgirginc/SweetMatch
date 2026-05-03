using SweetMatch.Events;

using SweetMatch.Model;

namespace SweetMatch.Systems

{

    public class InputHandler

    {

        private readonly GameStateMachine _stateMachine;

        private readonly EventBus _eventBus;

        public InputHandler(GameStateMachine stateMachine, EventBus eventBus)

        {

            _stateMachine = stateMachine;

            _eventBus = eventBus;

        }

        // Hücre tıklamasını CellView çağıracak,

        // Sadece Idle stateinde tıklama eventi fırlatıyoruz.

        public void HandleCellClick(GridPosition pos)

        {

            if (_stateMachine.Current != GameState.Idle) return;

            _eventBus.Raise(new CellClickedEvent(pos));

        }

    }

}