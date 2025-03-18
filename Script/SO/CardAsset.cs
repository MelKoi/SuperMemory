using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Type
{
    攻击,行动,对应,战技
}
public enum Used
{
    我方,敌方
}
[CreateAssetMenu(fileName = "MyCard", menuName = "New Card", order = 2)]//创建结果文件夹为MC，被利用的文件夹为NC
public class CardAsset : ScriptableObject
{
    [Header("General info")]//普遍信息
    public string CardName;//卡牌的名字
    [TextArea(2, 3)]//填写框，最少2行，最多3行
    public string description;//卡牌描述
    public Sprite cardPic;//卡牌图案
    public Type Type;
    public Sprite TypePic;//卡牌种类图案
    public string cost;//花费

    [Header("卡面效果")]
     public List<CardEffectAsset> Effects = new List<CardEffectAsset>();  
}
public class CardState
{
    public int TemporaryCost; // 临时消耗 
    public bool IsInPlayArea; // 是否在展开区
}
