using SweetMatch.Data;
using SweetMatch.Model;
using SweetMatch.Systems;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SweetMatch.Presentation.Game
{
    public class CellView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private ItemView itemView;

        public GridPosition Position { get; private set; }
        private InputHandler _inputHandler;

        public void Initialize(GridPosition position, InputHandler inputHandler)
        {
            Position = position;
            _inputHandler = inputHandler;
        }

        public void Bind(CellModel cell, ItemVisualConfigSO visualConfig)
        {
            if (cell == null || cell.IsEmpty)
                itemView.SetVisible(false);
            else
                itemView.Bind(cell.Item, visualConfig);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _inputHandler?.HandleCellClick(Position);
        }
    }
}