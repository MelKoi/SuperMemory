using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DraggingActionsTest : MonoBehaviour
{
    public abstract void OnStartDrag();//���󷽷�����ʼ��ק

    public abstract void OnEndDrag();//���󷽷�, ������ק

    public abstract void OnDraggingInUpdate();//���󷽷���������ק
    
    public virtual bool CanDrag//�ܷ���ק
    {
        get
        { 
            return true;
        }
    }
    protected abstract bool DragSuccessful();//���󷽷�����ק�ɹ�
}
