using UnityEngine;

namespace SweetMatch.Events
{
    // Bir goal sweet'i fly animation'ı tamamlanıp goal panel'ine "oturduğunda" raise edilir.
    // worldPosition: goal ikonun world space pozisyonu — VFXController particle burada oynatır.
    // SoundController bunu dinleyip ding sesi çalar, VFXController parıltı oynatır.
    public class GoalCollectedEvent
    {
        public Vector3 WorldPosition { get; }

        public GoalCollectedEvent(Vector3 worldPosition)
        {
            WorldPosition = worldPosition;
        }
    }
}