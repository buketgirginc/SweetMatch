using System.Collections.Generic;

namespace SweetMatch.Events
{
    public class ItemsSpawnedEvent
    {
        public IReadOnlyList<SpawnInfo> Spawns { get; }

        public ItemsSpawnedEvent(IReadOnlyList<SpawnInfo> spawns)
        {
            Spawns = spawns;
        }
    }
}