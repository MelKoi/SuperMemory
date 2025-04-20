using DG.Tweening;
using DialogueEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoneController : MonoBehaviour
{
    public GameObject lockFade;
    public ConversationManager conversationManager;
    [HideInInspector]public Slider lockSlider;

    [Header("基本信息")]
    public bool isHide;
    public float returnSpeed = 0.5f;
    public int currentDialog = 1;
    public int currentFight = 0;
    private bool isUnlocking;
    private RectTransform rectTransform;

    [Header("事件监听")]
    public VoidEventSO dialogFinishEvent;

    [Header("广播")]
    public SceneLoadEventSO loadEventSO;

    [Header("场景")]
    public List<GameSceneSO> fight;

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
    private void OnEnable()
    {
        dialogFinishEvent.OnEventRiased += FinishDialog;
    }
    private void OnDisable()
    {
        dialogFinishEvent.OnEventRiased -= FinishDialog;
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

    #region 对话相关
    public void StartDialog()
    {
        StartCoroutine(WaitPhoneHide());
    }
    IEnumerator WaitPhoneHide()
    {
        yield return StartCoroutine(HidePhoneAndLock());
        while (isUnlocking)
        {
            yield return null;
        }
        string dialogNum = "Dialog Part" + currentDialog.ToString();
        conversationManager.StartConversation(GameObject.Find(dialogNum).GetComponent<NPCConversation>());
        currentDialog++;
    }
    public void FinishDialog()
    {
        Debug.Log("战斗开始");
        loadEventSO.RaiseLoadRequestEvent(fight[currentFight],true);
        currentFight++;
    }
    #endregion
}
