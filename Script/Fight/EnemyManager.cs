using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
    [Header("敌方数据")]
    public CharactorAsset _PlayerData;//角色卡
    public WeaponAsset[] _PlayerWeapons = new WeaponAsset[2];//禁用后武器卡
    public WeaponAsset[] _PlayerAllWeapons = new WeaponAsset[3];//初始武器卡
    public WeaponAsset DrewWeapon;//第一个抽的武器

    [Header("敌方区域")]
    public GameObject HandArea;//手牌区域
    public GameObject CardPrefab;//卡牌预制体
    public Transform SkillArea;//战技展开区域

    [Header("UI组件")]
    public Text HpText;//生命
    public Text SpText;//体力
    public Text MpText;//能量
    public Text Weapon1Acc;//武器1蓄能
    public Text Weapon2Acc;//武器2蓄能

    public List<CardAsset> _currentDeck;//牌库
    public CardPool EnemyPool;//对象池
}
