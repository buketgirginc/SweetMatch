using UnityEngine;

namespace SweetMatch.Data
{
    [CreateAssetMenu(fileName = "GridConfig", menuName = "SweetMatch/Grid Config")]
    public class GridConfigSO : ScriptableObject
    {
        public int Width = 8;
        public int Height = 9;
    }
}