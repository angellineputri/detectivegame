using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaseChipUI : MonoBehaviour
{
    public enum ChipIcon { Pin, Tick, Question }

    [Header("References")]
    [SerializeField] Image    iconImage;
    [SerializeField] TMP_Text label;


    [Header("Icon Sprites (optional — colour tint used as fallback)")]
    [SerializeField] Sprite pinSprite;
    [SerializeField] Sprite tickSprite;
    [SerializeField] Sprite questionSprite;

    [Header("Fallback tint colours")]
    [SerializeField] Color pinColor      = new Color(1f,   0.75f, 0.1f);
    [SerializeField] Color tickColor     = new Color(0.2f, 0.85f, 0.3f);
    [SerializeField] Color questionColor = new Color(0.6f, 0.6f,  0.6f);

    [Header("Animation")]
    [Tooltip("Duration of all transitions: cross-fade, scale-pop, fade-out (seconds).")]
    public float transitionDuration = 0.25f;

    CanvasGroup _group;

    void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        if (_group == null)
            _group = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetImmediate(ChipIcon icon, string text)
    {
        StopAllCoroutines();
        ApplyIcon(icon);
        if (label != null) label.text = text;
        _group.alpha         = 1f;
        transform.localScale = Vector3.one;
        gameObject.SetActive(true);
    }

    public void AnimateTransition(ChipIcon newIcon, string newLabel)
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(CrossFade(newIcon, newLabel));
    }

    public void AnimateSlideIn(ChipIcon icon, string text)
    {
        StopAllCoroutines();
        ApplyIcon(icon);
        if (label != null) label.text = text;
        gameObject.SetActive(true);
        StartCoroutine(ScalePop());
    }

    public void AnimateOut(System.Action onDone = null)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut(onDone));
    }

    IEnumerator CrossFade(ChipIcon newIcon, string newLabel)
    {
        float half = transitionDuration * 0.5f;

        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            _group.alpha = 1f - (t / half);
            yield return null;
        }

        ApplyIcon(newIcon);
        if (label != null) label.text = newLabel;

        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            _group.alpha = t / half;
            yield return null;
        }
        _group.alpha = 1f;
    }

    IEnumerator ScalePop()
    {
        _group.alpha         = 0f;
        transform.localScale = new Vector3(0.6f, 0.6f, 1f);

        for (float t = 0f; t < transitionDuration; t += Time.deltaTime)
        {
            float p = t / transitionDuration;
            _group.alpha = p;

            float s = p < 0.7f
                ? Mathf.Lerp(0.6f, 1.1f, p / 0.7f)
                : Mathf.Lerp(1.1f, 1.0f, (p - 0.7f) / 0.3f);
            transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        _group.alpha         = 1f;
        transform.localScale = Vector3.one;
    }

    IEnumerator FadeOut(System.Action onDone)
    {
        for (float t = 0f; t < transitionDuration; t += Time.deltaTime)
        {
            _group.alpha = 1f - (t / transitionDuration);
            yield return null;
        }
        _group.alpha = 0f;
        gameObject.SetActive(false);
        onDone?.Invoke();
    }

    void ApplyIcon(ChipIcon icon)
    {
        if (iconImage == null) return;

        switch (icon)
        {
            case ChipIcon.Pin:
                iconImage.sprite = pinSprite;
                iconImage.color  = pinSprite      != null ? Color.white : pinColor;
                break;
            case ChipIcon.Tick:
                iconImage.sprite = tickSprite;
                iconImage.color  = tickSprite     != null ? Color.white : tickColor;
                break;
            case ChipIcon.Question:
                iconImage.sprite = questionSprite;
                iconImage.color  = questionSprite != null ? Color.white : questionColor;
                break;
        }
    }
}
