using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeCard : MonoBehaviour//可用于处理拖动场景内物品,放在需要拖动的物品上
{
    [Header("代码内设置")]
    public bool usePointerDisplacement = true;//允许使用鼠标与拖动物体的距离

    private bool dragging = false;//处于拖动状态

    private Vector3 pointDisplacement = Vector3.zero;//鼠标相对位置

    private float zDisplacement;//鼠标和卡牌之间的距离

    private DraggingActionsTest da;//关于拖拽时状态的判断，引入了其他抽象方法
    void Awake()
    {
        da = GetComponent<DraggingActionsTest>();
    }
    void Update()
    {
        if (dragging)
        {
            Vector3 mousePos = MouseInWorldCroods();//获取鼠标世界坐标
            da.OnDraggingInUpdate();
            transform.position = new Vector3(mousePos.x - pointDisplacement.x,mousePos.y - pointDisplacement.y, transform.position.z);//需拖曳物品现在的位置等于鼠标的世界位置减去相对位置，z轴不变
        }
            
    }
    private void OnMouseDown()
    {
        if(da.CanDrag)
        {
            dragging = true;//处于拖动状态
            da.OnStartDrag();//引用da中的开始拖拽按钮
            zDisplacement = -Camera.main.transform.position.z + transform.position.z;//计算z轴相对位置
            if (usePointerDisplacement)//需要相对位置
                pointDisplacement = -transform.position + MouseInWorldCroods();//计算卡牌和鼠标的相对位置
            else
                pointDisplacement = Vector3.zero;
        }
    }
    private void OnMouseUp()
    {
        if (dragging)
        {
            dragging = false;
            da.OnEndDrag();
        }
            
    }
    private Vector3 MouseInWorldCroods()//计算鼠标的世界坐标
    {
        var screenMousePos = Input.mousePosition;//获取鼠标的屏幕坐标
        screenMousePos.z = zDisplacement;//修改z轴
        return Camera.main.ScreenToWorldPoint(screenMousePos);//转换
    }

}
