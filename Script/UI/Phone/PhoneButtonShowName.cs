using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PhoneButtonShowName : MonoBehaviour
{
    public Text ButtonName;
    // Start is called before the first frame update
    public GameObject GetOverUI(GameObject canvas)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        GraphicRaycaster gr = canvas.GetComponent<GraphicRaycaster>();
        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(pointerEventData, results);
        if (results.Count != 0)
        {
            return results[0].gameObject;
        }
        return null;
    }
    private void Update()
    {
        if(GetOverUI(transform.parent.gameObject) != null)
        switch (GetOverUI(transform.parent.gameObject).name)
        {
            case "Load":
                ButtonName.text = "�ֻ����ݶ�ȡ����Demo�������鿴����   ����";
                break;
            case "Music":
                ButtonName.text = "�������֣�����������������������   ���ֲ�����";
                break;
            case "Community":
                ButtonName.text = "������������Ծ�ڽ�������ǰ��   ����";
                break;
            case "Shop":
                ButtonName.text = "�������ӣ����������磬������������   ��������";
                break;
            case "Challenge":
                ButtonName.text = "÷¶�����飺����õ�������˵����    ��ս";
                break;
            case "News":
                ButtonName.text = "�������ţ������ǳ���ÿ���ȵ�Χ��   ����";
                break;
            case "Taxi":
                ButtonName.text = "÷¶����������Ϊ��Խ�߽�ľ�����  ����ͨ������ȥ��Ժ";
                break;
            default:
                ButtonName.text = "�������������������ڷ��óԵĲ����ˡ���";
                break;
        }
    }
}
