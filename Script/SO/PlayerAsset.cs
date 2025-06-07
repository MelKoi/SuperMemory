using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "MyPlayer", menuName = "NewPlayer", order = 1)]
public class PlayerAsset : ScriptableObject
{
    public int Player;//角色，0代表主角

    public CharactorAsset CharacterAsset;//人物

    public int MaxHp;//生命值
    public int NowHp;
    public int MaxSynchronization;//架势条
    public int NowSynchronization;
    public int NowMp;

    public WeaponAsset[] WeaponAsset = new WeaponAsset[3];//主要使用的武器


    public WeaponAsset WeaponOne;//武器1
    public WeaponAsset WeaponTwo;//武器2
    public int Weapon1Acc;//武器1蓄能
    public int Weapon2Acc;//武器2蓄能
    public bool Weapon1;//武器一已经攻击
    public bool Weapon2;//武器二已经攻击
    public bool Weapon1isCooldown = false;//是否在冷却
    public bool Weapon2isCooldown = false;
    public int TemporaryCostReduction;//减少的卡牌消耗
    public int Damage;//攻击的伤害
    public bool Injured = false;//是否进入重伤
    public int InjuredTurn = 0;//进入重伤的回合,到1清零

}
