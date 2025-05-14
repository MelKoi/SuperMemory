using DG.Tweening;
using DialogueEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoneController : MonoBehaviour
{
    public GameObject lockFade;
    [HideInInspector]public Slider lockSlider;

    [Header("基本信息")]
    public bool isHide;
    public float returnSpeed = 0.5f;
    private bool isUnlocking;        //判断是否正在锁屏
    private RectTransform rectTransform;
    public GameObject TextPanel;//文件显示面板

    [Header("广播")]
    public SceneLoadEventSO loadEventSO;
    public VoidEventSO showBagPanelEvent;

    [Header("场景")]
    public GameSceneSO theater;

    private void Awake()
    {
        lockFade.SetActive(true);
        lockSlider = lockFade.GetComponentInChildren<Slider>();
        rectTransform = GetComponent<RectTransform>();
        lockSlider.onValueChanged.AddListener(OnSliderValueChanged); //监听手机解锁滑动条
        lockSlider.value = 0;
        isUnlocking = false;
        isHide = true;
    }
    private void Update()
    {
        if (lockFade)
        {
            // 如果鼠标松开且Slider的值未达到1，则缓慢回归到0
            if (Input.GetMouseButtonUp(0) && lockSlider.value < 1.0f && !isUnlocking)
            {
                StartCoroutine(ReturnSliderToZero());
            }
        }
    }

    #region 解锁功能
    //监听手机解锁
    void OnSliderValueChanged(float value)
    {
        if (value >= 1.0f && !isUnlocking)
        {
            // 达到1时开始解锁
            StartCoroutine(UnlockPhone());
        }
    }
    //解锁手机停顿
    IEnumerator UnlockPhone()
    {
        isUnlocking = true;
        yield return new WaitForSeconds(0.5f); // 停顿0.5秒
        lockSlider.value = 0; //解锁后重置Slider
        isUnlocking = false;
        lockFade.SetActive(false);
    }
    //滑块回归0
    IEnumerator ReturnSliderToZero()
    {
        while (lockSlider.value > 0)
        {
            lockSlider.value = Mathf.MoveTowards(lockSlider.value, 0, returnSpeed * Time.deltaTime);
            yield return null;
        }
    }
    #endregion

    #region 手机面版显示
    public void CallOutPhone()
    {
        if (isHide)
        {
            rectTransform.DOAnchorPosX(-564, 1);
            isHide = false;
        }
        else
        {
            StartCoroutine(HidePhoneAndLock());
        }
    }
    IEnumerator HidePhoneAndLock()
    {
        isHide = true;
        yield return rectTransform.DOAnchorPosX(-1426, 1).WaitForCompletion(); //等待面板移动到屏幕以外
        isUnlocking = false;
        lockFade.SetActive(true);
    }

    #endregion

    #region 前往剧场
    public void MoveToTheater() {
        StartCoroutine(HidePhoneAndMoveToTheater());
    }
    IEnumerator HidePhoneAndMoveToTheater()
    {
        yield return StartCoroutine(HidePhoneAndLock());
        if (isHide)
        {
            yield return null;
        }
        loadEventSO.RaiseLoadRequestEvent(theater,true);
    }
    #endregion

    #region 背包面板
    public void ShowBagPanel()
    {
        StartCoroutine(WaitPhoneHideShowPanel());
    }
    IEnumerator WaitPhoneHideShowPanel()
    {
        TextPanel.SetActive(false);
        yield return StartCoroutine(HidePhoneAndLock());
        while (isUnlocking)
        {
            yield return null;
        }
        showBagPanelEvent.RaiseEvent();
    }
    #endregion
}
