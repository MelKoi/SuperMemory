using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "My Character", menuName ="New Charactor", order = 1)]

public class CharactorAsset : ScriptableObject
{
    public int MaxHealth;//生命

    public int StartDef;//格挡值

    public int MaxSp;//最大精力

    public int speed;//速度

    [TextArea(2, 3)]//填写框，最少2行，最多3行
    public string description;//人物描述

    [TextArea(2, 3)]//填写框，最少2行，最多3行
    public string Skill;//人物性能

    public int HandCardNum;//手牌数

    public string[] Power;//技能

    public Sprite HeroImage;//图片

    public Sprite[] PowerImage;//技能图片
}
