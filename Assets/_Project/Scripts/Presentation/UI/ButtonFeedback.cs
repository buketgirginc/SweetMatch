using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SweetMatch.Presentation.UI
{
    // Herhangi bir butona eklenebilen scale-based feedback.
    // Hover (pointer enter) hafif büyür, press küçülür, release normale döner.
    // Unity Button'ın renk geçişinden daha hissedilir feedback.
    public class ButtonFeedback : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        private const float HoverScale = 1.05f;
        private const float PressScale = 0.95f;
        private const float HoverDuration = 0.1f;
        private const float PressDuration = 0.05f;

        private Vector3 _baseScale;
        private bool _pointerInside;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _pointerInside = true;
            ScaleTo(HoverScale, HoverDuration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _pointerInside = false;
            ScaleTo(1f, HoverDuration);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ScaleTo(PressScale, PressDuration);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Release sonrası: pointer hâlâ üstteyse hover ölçeğine, değilse normale.
            ScaleTo(_pointerInside ? HoverScale : 1f, PressDuration);
        }

        private void ScaleTo(float multiplier, float duration)
        {
            transform.DOKill();
            transform.DOScale(_baseScale * multiplier, duration);
        }

        private void OnDisable()
        {
            // Panel kapanırken tween yarıda kalmasın, scale tutarlı dönsün.
            transform.DOKill();
            transform.localScale = _baseScale;
        }
    }
}