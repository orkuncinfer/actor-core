using AudioSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class FancyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Define the scale values and animation duration
    [SerializeField] private float scaleDownValue = 0.8f;
    [SerializeField] private float animationDuration = 0.2f;
    
    [SerializeField] private Ease easeType = Ease.OutBounce;

    private Vector3 originalScale;
    private Sequence currentSequence;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Kill any ongoing animation
        if (currentSequence != null && currentSequence.IsPlaying())
        {
            currentSequence.Kill();
        }

        // Create a new sequence for the scale down animation
        currentSequence = DOTween.Sequence();
        currentSequence.Append(transform.DOScale(scaleDownValue, animationDuration).SetEase(Ease.OutBounce));
        currentSequence.Play();
        SoundManager.Instance.CreateSoundBuilder().Play(SoundManager.Instance.Container.ButtonDown);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Kill any ongoing animation
        if (currentSequence != null && currentSequence.IsPlaying())
        {
            currentSequence.Kill();
        }

        // Create a new sequence for the scale up animation
        currentSequence = DOTween.Sequence();
        currentSequence.Append(transform.DOScale(originalScale, animationDuration).SetEase(Ease.OutBounce));
        currentSequence.Play();
        SoundManager.Instance.CreateSoundBuilder().Play(SoundManager.Instance.Container.ButtonUp);
    }
}