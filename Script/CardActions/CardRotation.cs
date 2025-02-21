using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CardRotation : MonoBehaviour//展示卡背的脚本，放到卡牌上
{
    [Header("在监视面板中设置")]
    public Transform cardFront;//卡牌正面的位置
    public Transform cardBack;//卡牌背面位置
    public Transform targetFacePonint;//目标面位置
    public Collider col;//碰撞箱

    [Header("在程序中设置")]
    private bool showingBack = false;//展示背面
    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit[] hits;//设定一个涉嫌
        hits = Physics.RaycastAll(Camera.main.transform.position,
            (-Camera.main.transform.position + targetFacePonint.position).normalized,
            ((-Camera.main.transform.position + targetFacePonint.position).magnitude));//第一个参数是相机位置，第二个参数是目标点到摄像机的单位向量，第三格是目标点到摄像机的长度
        bool passedThorough = false;

        foreach(RaycastHit h in hits)//射线判断
        {
            if(h.collider == col)//如果射线穿过了碰撞箱
                passedThorough = true;
        }
        
        if(passedThorough != showingBack)
        {
            showingBack = passedThorough;
            if(showingBack)
            {
                cardFront.gameObject.SetActive(false);
                cardBack.gameObject.SetActive(true);
            }
            else
            {
                cardFront.gameObject.SetActive(true);
                cardBack.gameObject.SetActive(false);  
            }
        }
    }
}
