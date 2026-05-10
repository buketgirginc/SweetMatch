using System.Collections;
using System.Collections.Generic;
using SweetMatch.Model;
using SweetMatch.Presentation.Game;
using UnityEngine;

namespace SweetMatch.Presentation.Animation
{
    public class BoardAnimator : MonoBehaviour
    {
        [SerializeField] private GridView gridView;
        //scaffold: animasyon yok, sadece anlık RenderAll.
        // Sonraki commit'lerde DOTween Sequence'ler ile gerçek animasyona dönüşecek.

        public IEnumerator PlayClearAnimation(List<CellModel> cleared)
        {
            gridView.RenderAll();
            yield break;
        }

        public IEnumerator PlayFallAnimation()
        {
            gridView.RenderAll();
            yield break;
        }

        public IEnumerator PlayFillAnimation()
        {
            gridView.RenderAll();
            yield break;
        }
    }
}