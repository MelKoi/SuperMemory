using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

 public enum WeaponType
    {
        Light,Heavy,Special
    }
[CreateAssetMenu(fileName = "MyWeapon", menuName = "New Weapon", order = 3)]
public class WeaponAsset : ScriptableObject
{
    [Header("General info")]//�ձ���Ϣ
    public string WeaponName;//����������
    public int Wight;
    [TextArea(2, 3)]//��д������2�У����3��
    public string description;//��������
    public int OnceAccumulation;//��������
    [System.Serializable]
    public struct AccumulationPair//�����˺�
    {
        public int Acc; //����
        public int Value; //�˺�
    }
    public List<AccumulationPair> Accumulation;

    public List<CardAsset> Allcard;
}
