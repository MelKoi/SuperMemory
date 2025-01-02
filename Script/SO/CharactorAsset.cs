using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "My Character", menuName ="New Charactor", order = 1)]

public class CharactorAsset : ScriptableObject
{
    public int MaxHealth;//����

    public int StartDef;//��ֵ

    public int MaxSp;//�����

    public int speed;//�ٶ�

    [TextArea(2, 3)]//��д������2�У����3��
    public string description;//��������

    [TextArea(2, 3)]//��д������2�У����3��
    public string Skill;//��������

    public int HandCardNum;//������

    public string[] Power;//����

    public Sprite HeroImage;//ͼƬ

    public Sprite[] PowerImage;//����ͼƬ
}
