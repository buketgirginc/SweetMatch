using TMPro;
using UnityEngine;

namespace SweetMatch.Presentation.UI
{
    public class LevelLabelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelText;

        public void Initialize(int levelNumber)
        {
            levelText.text = $"Level {levelNumber}";
        }
    }
}