using SweetMatch.Data;
using SweetMatch.Model.Items;
using UnityEngine;
using UnityEngine.UI;

namespace SweetMatch.Presentation.Game
{
    public class ItemView : MonoBehaviour
    {
        [SerializeField] private Image image;

        public void Bind(GridItem item, ItemVisualConfigSO visualConfig)
        {
            if (item == null)
            {
                SetVisible(false);
                return;
            }

            var visual = visualConfig.Get(item.GetVisualKey());
            if (visual == null)
            {
                SetVisible(false);
                return;
            }

            image.sprite = visual.sprite;
            transform.localScale = Vector3.one * visual.scale;
            transform.localEulerAngles = new Vector3(0, 0, visual.rotation);
            transform.localPosition = Vector3.zero;
            SetVisible(true);
        }

        public void SetVisible(bool visible)
        {
            if (image != null) image.enabled = visible;
        }
    }
}