using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "MyCharacter", menuName ="New Charactor", order = 4)]

public class CharactorAsset : ScriptableObject
{
    public string Cardname;//角色名字

    public Sprite BianKuang;//卡牌边框

    public int MaxHealth;//生命

    public int MaxMp;//最大精力

    public int MaxSynchronization;//最大同步率

    public int speed;//速度

    [TextArea(2, 3)]//填写框，最少2行，最多3行
    public string description;//人物描述

    public int HandCardNum;//手牌数

    public string Power;//技能

    [TextArea(2, 3)] public string PowerDescription;//技能描述

    public Sprite HeroImage;//图片

    public Sprite PowerImage;//技能图片

    public List<CardEffectAsset> PowerEffect;//技能效果

    public int PowerCost;//技能消耗
}
