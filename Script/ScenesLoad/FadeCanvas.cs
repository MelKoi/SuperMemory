using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

public class FadeCanvas : MonoBehaviour
{
    [Header("事件监听")]
    public FadeEventSO fadeEvent;

    public Canvas overlayCanvas;
    public Image fadeImage;

    private void Awake()
    {
        overlayCanvas.sortingOrder = 32767; // 最大可能值
        overlayCanvas.overrideSorting = true;
        fadeImage.raycastTarget = false; // 关键设置
    }
    private void OnEnable()
    {
        fadeEvent.OnEventRaised += OnFadeEvent;
    }
    private void OnDisable()
    {
        fadeEvent.OnEventRaised -= OnFadeEvent;
    }
    private void OnFadeEvent(Color target, float duration, bool fadeIn)
    {
        fadeImage.DOBlendableColor(target, duration);
    }
}
