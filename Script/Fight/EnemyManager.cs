using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
    [Header("������������")]
    public GameObject enemyHand;//����������
    public WeaponAsset[] ChooseWeapon;//��ǰԤѡ��3������
    public List<GameObject> ChooseCards;//���ִ����Ҫ����ʱѡ��Ŀ���

    [Header("���˿��ƻ�������")]
    public CharactorAsset charactor;//���˵Ľ�ɫ
    public WeaponAsset Weapon1;//��һ������
    public WeaponAsset Weapon2;//�ڶ�������
    public List<CardAsset> shuffledCards;//�����Ŀ���
    

    [Header("������ϸ����")]
    public string power;//���˵Ľ�ɫ����
    public int speed;//���˽�ɫ���ٶ�
    public int HandCardNum;//��������
    public int Hp;
    public int Def;
    public int Sp;
    public int Mp;
    public int Weapon1Acc;//����1��������
    public int Weapon2Acc;//����2��������

    [Header("�������")]
    public Text AccNum1Text;//���ڵ����������ı�
    public Text AccNum2Text;//���ڵ����������ı�
    public int AccNum1;//��������������
    public int AccNum2;//��������������
    public GameObject Weapon1Object;//��һ������
    public GameObject Weapon2Object;//�ڶ�������
    public bool Weapon1Attacked;//��һ���������غ��Ѿ�����
    public bool Weapon2Attacked;//�ڶ����������غ��Ѿ�����

    [Header("��ʾ�������")]
    public Text Hptext;//��Ŀֵ�ı�
    public Text Deftext;//��ֵ�ı�
    public Text Sptext;//����ֵ�ı�
    public Text Mptext;//����ֵ�ı�

    [Header("�ű��ڲ�����")]
    public int CardNum = 20;//��������������
    public int AttackManage;//������������ֵ
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UseCard()//ʹ�ÿ���
    {
        if (ChooseCards.Count == 1)
        {
            foreach (var i in ChooseCards)
                //ִ�п���Ч������������٣��������ƣ�
                Destroy(i);
            ChooseCards.Clear();
        }
    }
}
