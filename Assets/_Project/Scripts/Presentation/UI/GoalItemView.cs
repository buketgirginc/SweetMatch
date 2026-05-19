using SweetMatch.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SweetMatch.Presentation.UI
{
    public class GoalItemView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text remainingText;

        public Image IconImage => iconImage;

        // Goal ikonunun world-space genişliği (lossyScale dahil).
        // Fly animation kopyasının bitiş boyutunu buna eşitlemek için kullanılır.
        public float IconWorldWidth =>
            iconImage.rectTransform.rect.width * iconImage.rectTransform.lossyScale.x;
        private int _localCount;
        public int LocalCount => _localCount;

        public void Setup(GoalEntry goal, ItemVisualConfigSO visualConfig)
        {
            string visualKey = SignatureToVisualKey(goal.Signature);
            var visual = visualConfig.Get(visualKey);

            if (visual != null)
            {
                iconImage.sprite = visual.sprite;
                iconImage.preserveAspect = true;
            }

            _localCount = goal.Count;
            UpdateRemaining(_localCount);
        }

        public void Decrement()
        {
            _localCount = Mathf.Max(0, _localCount - 1);
            UpdateRemaining(_localCount);
        }

        public void UpdateRemaining(int remaining)
        {
            remainingText.text = remaining.ToString();
        }

        // Goal signature ("sweet:bonbon") → visual key ("sweet_bonbon")
        private string SignatureToVisualKey(string signature)
        {
            return signature.Replace(":", "_");
        }
    }
}