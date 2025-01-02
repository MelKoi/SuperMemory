using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SameDistanceChildren : MonoBehaviour
{
    public Transform[] Children;
    void Awake()
    {
        Vector3 firstElementPos = Children[0].transform.position;
        Vector3 lastElementPos = Children[Children.Length - 1].transform.position;//��ȡ���Ƶ�һ��λ�ú����һ��λ��

        //��������λ������֮���ƽ��ֵ
        float XDist = (lastElementPos.x - firstElementPos.x) / (float)(Children.Length - 1) + 2;
        float YDist = (lastElementPos.y - firstElementPos.y - 1) / (float)(Children.Length - 1);
        float ZDist = (lastElementPos.z - firstElementPos.z + 2) / (float)(Children.Length - 1);

        Vector3 Dist = new Vector3(XDist, YDist, ZDist);

        //����������λ��ƽ���ֲ�
        for(int i = 1; i < Children.Length; i++)
        {
            Children[i].transform.position = Children[i - 1].transform.position + Dist;
        }
    }
}
