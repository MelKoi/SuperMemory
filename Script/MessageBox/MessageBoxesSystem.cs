using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MessageBoxesSystem : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] private GameObject MessageBoxPrefeb;//提示框预制体
    [SerializeField] private string Message;//显示的文本
    [SerializeField] private BattleManager BattleManager;//战斗管理器
    private GameObject currentTooltip;//实际生成的信息框

    public void OnPointerEnter(PointerEventData eventData)
    {
        string text = Message;

        //实例化提示框
        currentTooltip = Instantiate(MessageBoxPrefeb, transform.root);
        if (gameObject.name.Equals("Power"))
            if (gameObject.transform.parent.name.Equals("Enemy"))
            {
                currentTooltip.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = BattleManager.Enemy.CharacterAsset.Power + "\n" +
                   BattleManager.Enemy.CharacterAsset.PowerCost + "\n" +
                   BattleManager.Enemy.CharacterAsset.PowerDescription;
            }
            else
            {
                currentTooltip.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = BattleManager.Player.CharacterAsset.Power + "\n" +
                                  BattleManager.Player.CharacterAsset.PowerCost + "\n" +
                                  BattleManager.Player.CharacterAsset.PowerDescription;
            }
        if(gameObject.name.Equals("CardPanel(Clone)"))
            currentTooltip.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = gameObject.GetComponent<OneCardManager>().cardAsset.CardName + "\n" +
                  gameObject.GetComponent<OneCardManager>().cardAsset.cost + "\n" +
                  gameObject.GetComponent<OneCardManager>().cardAsset.description;

        //currentTooltip.GetComponent<TMP_Text>().text = text;

        //设置位置
        Vector3 offset = new Vector3(0,-0,0);
        currentTooltip.transform.position = Input.mousePosition + offset;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(currentTooltip);
    }
}
