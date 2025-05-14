using DialogueEditor;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FightAndDialogController : MonoBehaviour
{
    public int currentFight = 0;//用于对战计数
    public int currentDialog = 1;//用于对话计数
    public float duration;
    public ConversationManager conversationManager;
    [Header("广播")]
    public SceneLoadEventSO loadEventSO;
   
    [Header("事件监听")]
    public VoidEventSO afterSceneLoadedStartDialogEvent;
    public VoidEventSO dialogFinishEvent; 

    [Header("场景")]
    public List<GameSceneSO> fight;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    private void OnEnable()
    {
        afterSceneLoadedStartDialogEvent.OnEventRaised += StartDialog;  //场景转换完成后启动对话
        dialogFinishEvent.OnEventRaised += FinishDialog;          //对话完成后开启对战
    } 
    private void OnDisable()
    {
        afterSceneLoadedStartDialogEvent.OnEventRaised -= StartDialog;
        dialogFinishEvent.OnEventRaised -= FinishDialog;
    }
    #region 对话相关
    public void StartDialog()
    {
        StartCoroutine(WaitSecondStartDialog());
    }
    //等待几秒再开始对话，给玩家一个缓冲的时间，以免过于突兀
    IEnumerator WaitSecondStartDialog()
    {
        yield return new WaitForSeconds(duration);
        conversationManager = GameObject.Find("ConversationManager").GetComponent<ConversationManager>();
        conversationManager.gameObject.SetActive(true);
        if (currentDialog > 4) { //此处为demo特定，设置从头开始剧情防止游戏崩坏
            currentDialog = 1;
        }
        string dialogNum = "Dialog Part" + currentDialog.ToString();
        conversationManager.StartConversation(GameObject.Find(dialogNum).GetComponent<NPCConversation>());
    }
    //结束对话，开始战斗
    public void FinishDialog()
    {
        Debug.Log("战斗开始");
        if (currentFight >= fight.Count)//此处为demo特定，设置从第一场正式战斗开始防止游戏崩坏
        {
            currentFight = 1;
            loadEventSO.RaiseLoadRequestEvent(fight[currentFight], true);
        }
        
    }
    #endregion
}
