using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DraggingActionsTest : MonoBehaviour
{
    public abstract void OnStartDrag();//抽象方法，开始拖拽

    public abstract void OnEndDrag();//抽象方法, 结束拖拽

    public abstract void OnDraggingInUpdate();//抽象方法，正在拖拽
    
    public virtual bool CanDrag//能否拖拽
    {
        get
        { 
            return true;
        }
    }
    protected abstract bool DragSuccessful();//抽象方法，拖拽成功
}
