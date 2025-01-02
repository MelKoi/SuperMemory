using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CardRotation : MonoBehaviour//չʾ�����Ľű����ŵ�������
{
    [Header("�ڼ������������")]
    public Transform cardFront;//���������λ��
    public Transform cardBack;//���Ʊ���λ��
    public Transform targetFacePonint;//Ŀ����λ��
    public Collider col;//��ײ��

    [Header("�ڳ���������")]
    private bool showingBack = false;//չʾ����
    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit[] hits;//�趨һ������
        hits = Physics.RaycastAll(Camera.main.transform.position,
            (-Camera.main.transform.position + targetFacePonint.position).normalized,
            ((-Camera.main.transform.position + targetFacePonint.position).magnitude));//��һ�����������λ�ã��ڶ���������Ŀ��㵽������ĵ�λ��������������Ŀ��㵽������ĳ���
        bool passedThorough = false;

        foreach(RaycastHit h in hits)//�����ж�
        {
            if(h.collider == col)//������ߴ�������ײ��
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
