using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TargetingOptions//������ָ����Ŀ��
{
    NoTarget,
    All,
    Enemy,
    Self
}
[CreateAssetMenu(fileName = "MyCard", menuName = "New Card",order = 2)]//��������ļ���ΪMC�������õ��ļ���ΪNC

public class CardAsset : ScriptableObject
{
    [Header("General info")]//�ձ���Ϣ
    public WeaponAsset WeaponAsset; //���ƵĹ���,û�о�˵���ǳ�������
    public string CardName;//���Ƶ�����
    [TextArea(2, 3)]//��д������2�У����3��
    public string description;//��������
    public Sprite cardPic;//����ͼ��
    public Sprite TypePic;//��������
    public string cost;//����   
}
