using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeCard : MonoBehaviour//�����ڴ����϶���������Ʒ,������Ҫ�϶�����Ʒ��
{
    [Header("����������")]
    public bool usePointerDisplacement = true;//����ʹ��������϶�����ľ���

    private bool dragging = false;//�����϶�״̬

    private Vector3 pointDisplacement = Vector3.zero;//������λ��

    private float zDisplacement;//���Ϳ���֮��ľ���

    private DraggingActionsTest da;//������קʱ״̬���жϣ��������������󷽷�
    void Awake()
    {
        da = GetComponent<DraggingActionsTest>();
    }
    void Update()
    {
        if (dragging)
        {
            Vector3 mousePos = MouseInWorldCroods();//��ȡ�����������
            da.OnDraggingInUpdate();
            transform.position = new Vector3(mousePos.x - pointDisplacement.x,mousePos.y - pointDisplacement.y, transform.position.z);//����ҷ��Ʒ���ڵ�λ�õ�����������λ�ü�ȥ���λ�ã�z�᲻��
        }
            
    }
    private void OnMouseDown()
    {
        if(da.CanDrag)
        {
            dragging = true;//�����϶�״̬
            da.OnStartDrag();//����da�еĿ�ʼ��ק��ť
            zDisplacement = -Camera.main.transform.position.z + transform.position.z;//����z�����λ��
            if (usePointerDisplacement)//��Ҫ���λ��
                pointDisplacement = -transform.position + MouseInWorldCroods();//���㿨�ƺ��������λ��
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
    private Vector3 MouseInWorldCroods()//����������������
    {
        var screenMousePos = Input.mousePosition;//��ȡ������Ļ����
        screenMousePos.z = zDisplacement;//�޸�z��
        return Camera.main.ScreenToWorldPoint(screenMousePos);//ת��
    }

}
