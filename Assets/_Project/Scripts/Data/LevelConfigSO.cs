using System.Collections.Generic;
using UnityEngine;

namespace SweetMatch.Data
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "SweetMatch/Level Config")]
    public class LevelConfigSO : ScriptableObject
    {
        public int Moves = 30;
        public List<GoalEntry> Goals = new();

        public bool UseInitialLayout = false;
        public List<InitialCell> InitialCells = new();
    }
}