using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PhoneButtonShowName : MonoBehaviour
{
    public Text ButtonName;
    public GameObject textPanel;
    [Header("广播")]
    public VoidEventSO bagPanelOpen;

    
    private bool textPanelFlag = true;
    private void Awake()
    {
        textPanel.SetActive(true);
    }
    private void OnEnable()
    {
        bagPanelOpen.OnEventRaised += OnBagPannalOpen;
    }
    private void OnDisable()
    {
        bagPanelOpen.OnEventRaised -= OnBagPannalOpen;
    }
    public void OnBagPannalOpen()
    {
        textPanelFlag = !textPanelFlag;
        textPanel.SetActive(textPanelFlag);
    }
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
                ButtonName.text = "手机数据读取：此Demo中用来查看背包   背包";
                break;
            case "Music":
                ButtonName.text = "塞壬音乐：给你带来海妖般的乐曲体验   音乐播放器";
                break;
            case "Community":
                ButtonName.text = "敲敲社区：活跃在交流的最前线   社区";
                break;
            case "Shop":
                ButtonName.text = "崩崩铺子：崩坏的世界，让网购来拯救   网购中心";
                break;
            case "Challenge":
                ButtonName.text = "梅露辛听书：用最好的语气诉说故事    挑战";
                break;
            case "News":
                ButtonName.text = "新闻六号：让我们朝着每日热点围攻   新闻";
                break;
            case "Taxi":
                ButtonName.text = "梅露辛搭车：让你成为穿越边界的巨龙！  可以通过此项去剧院";
                break;
            default:
                ButtonName.text = "徐恋璃：啊，我妈又在发好吃的馋我了……";
                break;
        }
    }
}
