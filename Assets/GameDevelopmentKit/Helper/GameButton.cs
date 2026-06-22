using System;
using System.Collections;
using System.Collections.Generic;
using GameDevelopmentKit.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] public ButtonPreset buttonPreset;
    [SerializeField] private Vector3 originalScale = Vector3.one;

    private void OnEnable()
    {
        transform.localScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimDown());
        AudioManager.Instance.PlaySound(SoundKey.ButtonClick);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimUp());
    }

    private IEnumerator AnimDown()
    {
        float timeElapsed = 0f;
        Vector3 targetScale = originalScale * buttonPreset.scaleDown;
        while (timeElapsed < buttonPreset.duration)
        {
            float t = buttonPreset.curve.Evaluate(timeElapsed / buttonPreset.duration);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
    }

    private IEnumerator AnimUp()
    {
        float timeElapsed = 0f;
        Vector3 targetScale = originalScale * buttonPreset.scaleDown;
        while (timeElapsed < buttonPreset.duration)
        {
            float t = buttonPreset.curve.Evaluate(timeElapsed / buttonPreset.duration);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }
}