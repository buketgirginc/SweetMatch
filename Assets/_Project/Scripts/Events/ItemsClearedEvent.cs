using System.Collections.Generic;
using SweetMatch.Model.Items;

namespace SweetMatch.Events
{
    public class ItemsClearedEvent
    {
        public IReadOnlyList<GridItem> ClearedItems { get; }

        public ItemsClearedEvent(IReadOnlyList<GridItem> clearedItems)
        {
            ClearedItems = clearedItems;
        }
    }
}