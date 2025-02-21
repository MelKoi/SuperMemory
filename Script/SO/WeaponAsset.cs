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
    [Header("General info")]//普遍信息
    public string WeaponName;//武器的名字
    public int Wight;
    [TextArea(2, 3)]//填写框，最少2行，最多3行
    public string description;//卡牌描述
    public int OnceAccumulation;//单次蓄力
    [System.Serializable]
    public struct AccumulationPair//蓄力伤害,哈希结构（？）
    {
        public int Acc; //蓄力
        public int Value; //伤害
    }
    public List<AccumulationPair> Accumulation;

    public List<CardAsset> Allcard;

    [Header("Inside Imformation")]//内部信息
    public bool WeaponAttacked = false;
}
