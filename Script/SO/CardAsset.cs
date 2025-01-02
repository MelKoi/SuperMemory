using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TargetingOptions//卡牌能指定的目标
{
    NoTarget,
    All,
    Enemy,
    Self
}
[CreateAssetMenu(fileName = "MyCard", menuName = "New Card",order = 2)]//创建结果文件夹为MC，被利用的文件夹为NC

public class CardAsset : ScriptableObject
{
    [Header("General info")]//普遍信息
    public WeaponAsset WeaponAsset; //卡牌的归属,没有就说明是场景卡牌
    public string CardName;//卡牌的名字
    [TextArea(2, 3)]//填写框，最少2行，最多3行
    public string description;//卡牌描述
    public Sprite cardPic;//卡牌图案
    public Sprite TypePic;//卡牌种类
    public string cost;//花费   
}
