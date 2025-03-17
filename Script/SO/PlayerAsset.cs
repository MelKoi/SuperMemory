using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "MyPlayer", menuName = "NewPlayer", order = 1)]
public class PlayerAsset : ScriptableObject
{
    public int Player;//角色，0代表主角

    public CharactorAsset CharacterAsset;//人物

    public int hp;
    public int mp;
    public int maxSp;
    public int NowSp;

    public WeaponAsset[] WeaponAsset = new WeaponAsset[3];//主要使用的武器

    public int Weapon1Acc;//武器1蓄能
    public int Weapon2Acc;//武器2蓄能
    public bool Weapon1;//武器一已经攻击
    public bool Weapon2;//武器二已经攻击
    public int TemporaryCostReduction;//减少的卡牌消耗
}
