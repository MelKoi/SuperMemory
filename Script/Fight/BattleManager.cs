using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public enum GamePhase
{
    gameStart,playerAction,enemyAction,playerReady,enemyReady
}
public class BattleManager : MonoBehaviour
{
    private Dictionary<CardAsset, CardState> cardStates = new Dictionary<CardAsset, CardState>();

    [Header("双方角色")]
    public PlayerAsset Player;//我方
    public PlayerAsset Enemy;//敌方

    [Header("玩家数据")]
    public CharactorAsset PlayerData;//角色卡
    public WeaponAsset[] PlayerWeapons = new WeaponAsset[2];//禁用后武器卡
    public WeaponAsset[] PlayerAllWeapons = new WeaponAsset[3];//初始武器卡
    public WeaponAsset DrewWeapon;//第一个抽的武器

    [Header("敌方数据引用")]
    public EnemyManager EnemyManager;//敌方管理器
    
    [Header("卡牌组件")]
    public GameObject HandArea;//手牌区域
    public GameObject CardPrefab;//卡牌预制体
    public Transform SkillArea;//战技展开区域

    [Header("UI组件")]
    public Text HpText;//我方生命
    public Text SpText;//我方体力
    public Text MpText;//我方能量
    public Text Weapon1Acc;//武器1蓄能
    public Text Weapon2Acc;//武器2蓄能

    public GameObject BanEnemyWeapon;//ban武器界面
    public GameObject FirstDrew;//选择抽取武器界面
    public GameObject ChooseCardsPanel;//初始战斗场地

    public List<CardAsset> _currentDeck;//牌库
    public GamePhase _currentPhase;//回合情况
    public GameObject ChooseCards;//选中的卡牌
    public CardPool PlayerPool;//对象池

    public void DrowCards(int amount)//抽取卡牌
    {
        for (int i = 0; i < amount; i++)
        {
            if (_currentDeck.Count == 0)
            {
                Debug.LogWarning("牌库已空");
                return;
            }

            int randomIndex = UnityEngine.Random.Range(0, _currentDeck.Count);
            CreateCard(_currentDeck[randomIndex], HandArea.transform);
            _currentDeck.RemoveAt(randomIndex);
        }
    }
    private void CreateCard(CardAsset asset, Transform parent)
    {
        GameObject newCard = Instantiate(CardPrefab, parent);
        newCard.GetComponent<OneCardManager>().ReadCardFromAsset(asset);
    }

    //UI更新
    public void UpdateUI(Text Hptext, Text Mptext, Text Sptext, Text Weapon1Acc, Text Weapon2Acc ,PlayerAsset player)
    {
        Hptext.text = player.hp.ToString();
        Mptext.text = player.mp.ToString();
        Sptext.text = player.maxSp.ToString();
        Weapon1Acc.text = player.Weapon1Acc.ToString();
        Weapon2Acc.text = player.Weapon2Acc.ToString();
    }
    public void GameStart()//游戏开始
    {
        
        //读取玩家各种数据
        PlayerData = Player.CharacterAsset;
        for(int i = 0; i < 3; i++)
        {
            PlayerAllWeapons[i] = Player.WeaponAsset[i];
        }
        Player.hp = PlayerData.MaxHealth;
        Player.maxSp = PlayerData.MaxSp;
        Player.NowSp = Player.maxSp;
        Player.mp = 0;
        PlayerPool = new CardPool();
        _currentDeck = new List<CardAsset>();

        //读取敌方各种数据
        EnemyManager._PlayerData = Enemy.CharacterAsset;
        for (int i = 0; i < 3; i++)
        {
           EnemyManager._PlayerAllWeapons[i] = Enemy.WeaponAsset[i];
        }
        Enemy.hp = EnemyManager._PlayerData.MaxHealth;
        Enemy.maxSp = EnemyManager._PlayerData.MaxSp;
        Enemy.NowSp = Enemy.maxSp;
        Enemy.mp = 0;
        EnemyManager.EnemyPool = new CardPool();
        EnemyManager._currentDeck = new List<CardAsset>();

        //统一更新UI
        UpdateUI(HpText, MpText, SpText,Weapon1Acc,Weapon2Acc,Player);
        UpdateUI(EnemyManager.HpText, EnemyManager.MpText, EnemyManager.SpText, EnemyManager.Weapon1Acc, 
            EnemyManager.Weapon2Acc,Enemy);
        for(int i = 0;i < 3; i++)
        {
            BanEnemyWeapon.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Text>().text = EnemyManager._PlayerAllWeapons[i].WeaponName;
        } 
    }

    public void PlayerReady()//玩家准备阶段
    {
        Debug.Log(_currentPhase);
        Player.maxSp = PlayerData.MaxSp;
        UpdateUI(HpText, MpText, SpText, Weapon1Acc, Weapon2Acc, Player);
        _currentPhase = GamePhase.playerAction;
        DrowCards(PlayerData.HandCardNum - HandArea.transform.childCount);
    }
    
    public void EndTurn()//回合结束
    {
        if (_currentPhase == GamePhase.playerAction)
        {
            _currentPhase = GamePhase.enemyReady;
        }
        else if(_currentPhase == GamePhase.enemyAction)
        {
            _currentPhase = GamePhase.playerReady;
        }
    }

    public void EnemyReady()
    {
        Debug.Log(_currentPhase);
        Enemy.NowSp = Enemy.maxSp;
        UpdateUI(EnemyManager.HpText, EnemyManager.MpText, EnemyManager.SpText, EnemyManager.Weapon1Acc,
            EnemyManager.Weapon2Acc, Enemy);
        _currentPhase = GamePhase.enemyAction;
    }
    public void ChooseWeapons(Text text)//我方ban对方一个武器牌
    {
        foreach(WeaponAsset i in EnemyManager._PlayerAllWeapons)
        {
            int j = 0;
            if (!i.WeaponName.Equals(text.text))
            {
                EnemyManager._PlayerWeapons[j] = i;
                j++;
            }
        }
        FirstDrew.SetActive(true);
        for (int i = 0; i < 2; i++)
        {
            FirstDrew.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Text>().text = PlayerWeapons[i].WeaponName;
        }
        BanEnemyWeapon.SetActive(false);
    }
    public void ChooseFirstDrew(Text text)//选择第一抽取的武器
    {
        foreach(WeaponAsset weapon in PlayerWeapons)
        {
            if (weapon.WeaponName.Equals(text.text))
            {
                DrewWeapon = weapon;
                break;
            }
        }
        foreach(CardAsset card in DrewWeapon.Allcard)//拷贝卡牌
        {
            _currentDeck.Add(card);
            cardStates[card] = new CardState
            {
                TemporaryCost = int.Parse(card.cost),
                IsInPlayArea = false
            };
        }
        DrowCards(PlayerData.HandCardNum);
        FirstDrew.SetActive(false);
        ChooseCardsPanel.SetActive(false);
        _currentPhase = GamePhase.playerReady;
    }
  
    public void UseCard(CardAsset card, GameObject cardObject)//使用卡牌
    {
        var behaviour = CardBehaviourFactory.Create(card);
        behaviour.Onplay(this, EnemyManager, cardObject);

        int nowsp = 0;
        if (nowsp < 0)
        {
            Debug.Log("当前体力不够使用此牌");
            return;
        }
        
        //PlayerData.Sp = nowsp;
        //SpText.text = PlayerData.Sp.ToString();
        Destroy(cardObject); //销毁卡牌
    }
    public void WeaponAttack(GameObject AccWeapon)//武器攻击
    {
        GameObject accText = AccWeapon.transform.GetChild(3).gameObject;
        int AccNum = int.Parse(accText.GetComponent<Text>().text);
        //本回合攻击过的武器，bool值为true
        if (!AccWeapon.GetComponent<WeaponCardManager>().weaponAsset.WeaponAttacked)
        {
            for (int i = 0; i < 3; i++)//匹配蓄能等级
                if (AccNum >= AccWeapon.GetComponent<WeaponCardManager>().weaponAsset.Accumulation[i].Acc)
                {
                    //AttackManage = AccWeapon.GetComponent<WeaponCardManager>().weaponAsset.Accumulation[i].Value;
                }
            //Debug.Log("你打出了" + AttackManage + "点伤害");
            //if (AttackManage == 0)//如果没到蓄能等级，无法打出伤害
            //{
            //    Debug.Log("此武器蓄能不足以发动攻击");
            //    return;
            //}
            //Mp = Mp + AccNum;
            //Mptext.text = Mp.ToString();
            AccNum = 0;//重置蓄能
            accText.GetComponent<Text>().text = AccNum.ToString();
            AccWeapon.GetComponent<WeaponCardManager>().weaponAsset.WeaponAttacked = true;
            //AttackManage = 0;
        }
        else
        {
            Debug.Log("此武器本回合已经攻击过了");
        }
    }
}

public static class CardBehaviourFactory
{
    public static CardBehaviour Create(CardAsset cardAsset)
    {
        // 直接使用ScriptableObject中的Type字段
        switch (cardAsset.Type)
        {
            case Type.攻击:
                return new AttackCardBehaviour(cardAsset);
            case Type.战技:
                return new SkillCardBehaviour(cardAsset);
            case Type.行动:
                return new ActionCardBehaviour(cardAsset);
            case Type.对应:
                return new CounterCardBehaviour(cardAsset);
            default:
                throw new ArgumentException("未知卡牌类型");
        }
    }
}
