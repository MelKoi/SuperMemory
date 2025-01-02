using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DraggingActionTestReturn : DraggingActionsTest
{
    private Vector3 savePos;
    // Start is called before the first frame update
    public override void OnStartDrag()//��д��������ʼʱ���浱ǰ����
    {
        savePos = transform.position;
    }
    public override void OnEndDrag()//��д����������ʱ�ص���ʼ����
    {
        if(transform.position.y >= 0)
        {
            Destroy(this.gameObject);//����϶�����Ӧ��Χ�����ٿ��ƣ�ִ�п���Ч����ȱ�ӿڣ�
        }
        else
        {
            transform.DOMove(savePos,1f);//ʹ���˲�������ݣ�λ�ûع�
        }
            
    }
    public override void OnDraggingInUpdate()//��
    {}
    protected override bool DragSuccessful()//�ɹ���ק
    {
        return true;
    }
}
