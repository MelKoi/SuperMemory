using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DraggingActionTestReturn : DraggingActionsTest
{
    private Vector3 savePos;
    // Start is called before the first frame update
    public override void OnStartDrag()//重写方法，开始时保存当前坐标
    {
        savePos = transform.position;
    }
    public override void OnEndDrag()//重写方法，放手时回到起始坐标
    {
        if(transform.position.y >= 0)
        {
            Destroy(this.gameObject);//如果拖动了相应范围就销毁卡牌，执行卡牌效果（缺接口）
        }
        else
        {
            transform.DOMove(savePos,1f);//使用了插件的内容，位置回归
        }
            
    }
    public override void OnDraggingInUpdate()//？
    {}
    protected override bool DragSuccessful()//成功拖拽
    {
        return true;
    }
}
