using DG.Tweening;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderKeywordFilter;
using UnityEditor.VersionControl;
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
    private Dictionary<CardAsset, CardState> EnemycardStates = new Dictionary<CardAsset, CardState>();

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
    public Transform SkillArea;//战技展开区域
    public GameObject CharacterArea;//人物牌区域

    [Header("卡牌预制体")]
    public GameObject CardPrefab;//卡牌预制体
    public GameObject CharacterPrefab;//人物卡牌预制体
    public GameObject WeaponPrefeb;//武器卡牌预制体

    [Header("UI组件")]
    public Text HpText;//我方生命
    public Text SpText;//我方体力
    public Text MpText;//我方能量
    public Text Weapon1Acc;//武器1蓄能
    public Text Weapon2Acc;//武器2蓄能
    public GameObject Purple;//对应卡是否使用
    public GameObject ZeroPoint;//战斗场地中心

    [Header("功能性变量")]
    public GameObject BanEnemyWeapon;//ban武器界面
    public GameObject FirstDrew;//选择抽取武器界面
    public GameObject ChooseCardsPanel;//初始战斗场地
    public List<CardEffectAsset> AttackEffect = new List<CardEffectAsset>();//攻击牌效果池
    public List<CardEffectAsset> SkillEffect = new List<CardEffectAsset>();//战技牌效果
    public List<CardEffectAsset> CounterEffect = new List<CardEffectAsset>();//对应牌效果池
    public List<CardAsset> _currentDeck;//牌库
    public GamePhase _currentPhase;//回合情况
    public bool hasEnemyTurnStarted = false;//进入敌人的回合
    public CardPool PlayerPool;//对象池

    void Start()
    {
        GameStart();
    }
    void Update()
    {
        switch (_currentPhase)
        {
            case GamePhase.playerReady:
                PlayerReady();
                break;
            case GamePhase.enemyReady:
                EnemyReady();
                break;
            case GamePhase.enemyAction:
                if (!hasEnemyTurnStarted)
                {
                    StartEnemyTurn();
                    hasEnemyTurnStarted = true;
                }
                break;
        }
        if (Player.Weapon1Acc != int.Parse(Weapon1Acc.text)
            || Player.Weapon2Acc != int.Parse(Weapon2Acc.text)
            || Player.hp != int.Parse(HpText.text)
            || Player.NowSp != int.Parse(SpText.text)
            || Player.mp != int.Parse(MpText.text))
        {
            UpdateUI(HpText, MpText, SpText, Weapon1Acc, Weapon2Acc, Player);
        }
        if (Enemy.Weapon1Acc != int.Parse(EnemyManager.Weapon1Acc.text)
            || Enemy.Weapon2Acc != int.Parse(EnemyManager.Weapon2Acc.text)
            || Enemy.hp != int.Parse(EnemyManager.HpText.text)
            || Enemy.NowSp != int.Parse(EnemyManager.SpText.text)
            || Enemy.mp != int.Parse(EnemyManager.MpText.text))
        {
            UpdateUI(EnemyManager.HpText, EnemyManager.MpText, EnemyManager.SpText, EnemyManager.Weapon1Acc,
            EnemyManager.Weapon2Acc, Enemy);
        }
    }
    public void DrowCards(int amount, GameObject HandArea, List<CardAsset> _currentDeck)//抽取卡牌
    {
        if (_currentDeck.Count == 0)//本次抽卡时发现牌库为空
            if (_currentPhase == GamePhase.playerReady)//如果是玩家抽卡发现的
            {
                foreach (WeaponAsset second in PlayerWeapons)
                    if (!second.WeaponName.Equals(DrewWeapon.WeaponName))//检索玩家拥有的第二把武器
                    {
                        DrewWeapon = second;//更新抽取的武器
                        break;
                    }
                foreach (CardAsset card in DrewWeapon.Allcard)//拷贝卡牌
                {
                    _currentDeck.Add(card);
                    cardStates[card] = new CardState
                    {
                        TemporaryCost = int.Parse(card.cost),
                        IsInPlayArea = false
                    };
                }
            }
            else if (_currentPhase == GamePhase.enemyReady)//如果是敌人抽卡发现的
            {
                foreach (WeaponAsset second in EnemyManager._PlayerWeapons)
                    if (!second.WeaponName.Equals(EnemyManager.DrewWeapon.WeaponName))//检索敌人拥有的第二把武器
                    {
                        EnemyManager.DrewWeapon = second;//更新抽取的武器
                        break;
                    }
                foreach (CardAsset card in EnemyManager.DrewWeapon.Allcard)//拷贝卡牌
                {
                    EnemyManager._currentDeck.Add(card);
                    EnemycardStates[card] = new CardState
                    {
                        TemporaryCost = int.Parse(card.cost),
                        IsInPlayArea = false
                    };
                }

            }

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
        CardShaderController cardShaderController = newCard.GetComponent<CardShaderController>();
        OneCardManager card = newCard.GetComponent<OneCardManager>();
        newCard.GetComponent<OneCardManager>().ReadCardFromAsset(asset);
        newCard.GetComponent<OneCardManager>().cardAsset = asset;
        cardShaderController.InitializeMaterials();
        cardShaderController.SetMainTex(cardShaderController.cardGround, card.CardImage.sprite);
        cardShaderController.SetMainTex(cardShaderController.cardType, card.CardType.sprite);
        cardShaderController.SetMainTex(cardShaderController.cardBody, card.CardPic.sprite);
        cardShaderController.SetRampTex(cardShaderController.cardGround, asset.Ramp);
        cardShaderController.SetRampTex(cardShaderController.cardType, asset.Ramp);
        cardShaderController.SetRampTex(cardShaderController.cardBody, asset.Ramp);
        cardShaderController.SetRampTex(cardShaderController.cardName, asset.Ramp);
        cardShaderController.SetRampTex(cardShaderController.cardDescription, asset.Ramp);
        cardShaderController.SetRampTex(cardShaderController.cardCost, asset.Ramp);
        if (newCard.transform.parent.name.Equals("EnemyHand"))
            newCard.GetComponent<OneCardManager>().CardBack.SetActive(true);

    }
    private void CreateCharacter(CharactorAsset character, Transform parent)
    {
        GameObject newCharacter = Instantiate(CharacterPrefab, parent);
        newCharacter.GetComponent<CharacterCardManager>().charactorasset = character;
        newCharacter.GetComponent<CharacterCardManager>().ReadCardFromAsset(character);
    }
    private void CreateWeapon(WeaponAsset weapon, Transform parent)
    {
        GameObject newWeapon = Instantiate(WeaponPrefeb, parent);
        newWeapon.GetComponent<WeaponCardManager>().weaponAsset = weapon;
        newWeapon.GetComponent<WeaponCardManager>().ReadCardFromAsset(weapon);
    }
    //UI更新
    public void UpdateUI(Text Hptext, Text Mptext, Text Sptext, Text Weapon1Acc, Text Weapon2Acc, PlayerAsset player)
    {
        Hptext.text = player.hp.ToString();
        Mptext.text = player.mp.ToString();
        Sptext.text = player.NowSp.ToString();
        Weapon1Acc.text = player.Weapon1Acc.ToString();
        Weapon2Acc.text = player.Weapon2Acc.ToString();
    }
    public void GameStart()//游戏开始
    {
        //读取玩家各种数据
        PlayerData = Player.CharacterAsset;
        for (int i = 0; i < 3; i++)
        {
            PlayerAllWeapons[i] = Player.WeaponAsset[i];
        }
        Player.hp = PlayerData.MaxHealth;
        Player.maxSp = PlayerData.MaxSp;
        Player.NowSp = Player.maxSp;
        PlayerPool = new CardPool();
        _currentDeck = new List<CardAsset>();
        Player.Weapon1Acc = Player.Weapon2Acc = Player.mp = 0;
        Player.Weapon1 = Player.Weapon2 = false;
        CreateCharacter(PlayerData, transform.Find("Place/CharacterCard"));


        //读取敌方各种数据
        EnemyManager._PlayerData = Enemy.CharacterAsset;
        for (int i = 0; i < 3; i++)
        {
            EnemyManager._PlayerAllWeapons[i] = Enemy.WeaponAsset[i];
        }
        Enemy.hp = EnemyManager._PlayerData.MaxHealth;
        Enemy.maxSp = EnemyManager._PlayerData.MaxSp;
        Enemy.NowSp = Enemy.maxSp;
        EnemyManager.EnemyPool = new CardPool();
        EnemyManager._currentDeck = new List<CardAsset>();
        Enemy.Weapon1Acc = Enemy.Weapon2Acc = Enemy.mp = 0;
        Enemy.Weapon1 = Enemy.Weapon2 = false;
        CreateCharacter(EnemyManager._PlayerData, transform.Find("Place/Enemy/CharacterCard"));

        for(int i = 0; i < 3; i++)
        {
            BanEnemyWeapon.transform.GetChild(i).gameObject.GetComponent<Image>().sprite = EnemyManager._PlayerAllWeapons[i].CardFace;
        }

        //统一更新UI
        UpdateUI(HpText, MpText, SpText, Weapon1Acc, Weapon2Acc, Player);
        UpdateUI(EnemyManager.HpText, EnemyManager.MpText, EnemyManager.SpText, EnemyManager.Weapon1Acc,
            EnemyManager.Weapon2Acc, Enemy);
        for (int i = 0; i < 3; i++)
        {
            BanEnemyWeapon.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Text>().text = EnemyManager._PlayerAllWeapons[i].WeaponName;
        }
    }
    public void PlayerReady()//玩家准备阶段
    {
        hasEnemyTurnStarted = false;//敌方回合结束
        Debug.Log(_currentPhase);
        Player.NowSp = Player.maxSp;//恢复体力
        if (SkillEffect != null)
            foreach (var effect in SkillEffect)
            {
                effect.ApplyEffect(this, EnemyManager);
            }
        UpdateUI(HpText, MpText, SpText, Weapon1Acc, Weapon2Acc, Player);
        DrowCards(PlayerData.HandCardNum - HandArea.transform.childCount, HandArea, _currentDeck);
        _currentPhase = GamePhase.playerAction;
    }
    public void EndTurn()//回合结束
    {
        if (_currentPhase == GamePhase.playerAction)
        {
            //重置武器攻击状态
            Player.Weapon1 = false;
            Player.Weapon2 = false;
            _currentPhase = GamePhase.enemyReady;
        }
        else if (_currentPhase == GamePhase.enemyAction)
        {
            //重置武器攻击状态
            Enemy.Weapon1 = false;
            Enemy.Weapon2 = false;
            _currentPhase = GamePhase.playerReady;
        }
    }
    public void EnemyReady()//敌方准备阶段
    {
       
        Debug.Log(_currentPhase);
        Enemy.NowSp = Enemy.maxSp;//恢复体力
        if (EnemyManager.SkillEffect != null)
            foreach (var effect in EnemyManager.SkillEffect)
            {
                effect.ApplyEffect(this, EnemyManager);
            }
        UpdateUI(EnemyManager.HpText, EnemyManager.MpText, EnemyManager.SpText, EnemyManager.Weapon1Acc,
            EnemyManager.Weapon2Acc, Enemy);
        DrowCards(EnemyManager._PlayerData.HandCardNum - EnemyManager.HandArea.transform.childCount, EnemyManager.HandArea, EnemyManager._currentDeck);
        _currentPhase = GamePhase.enemyAction;
    }
    public void ChooseWeapons(Text text)//我方ban对方一个武器牌
    {
        int j = 0;
        foreach (WeaponAsset i in EnemyManager._PlayerAllWeapons)
        {
            if (!i.WeaponName.Equals(text.text))
            {
                EnemyManager._PlayerWeapons[j] = i;
                j++;
            }
        }
        EnemyManager.DrewWeapon = EnemyManager._PlayerWeapons[0];
        FirstDrew.SetActive(true);
        for (int i = 0; i < 2; i++)
        {
            FirstDrew.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Text>().text = PlayerWeapons[i].WeaponName;
            FirstDrew.transform.GetChild(i).gameObject.GetComponent<Image>().sprite = PlayerWeapons[i].CardFace;
        }

            

        BanEnemyWeapon.SetActive(false);
        CreateWeapon(PlayerWeapons[0], transform.Find("Place/WeaponCard1"));
        CreateWeapon(PlayerWeapons[1], transform.Find("Place/WeaponCard2"));
        CreateWeapon(EnemyManager._PlayerWeapons[0], transform.Find("Place/Enemy/WeaponCard1"));
        CreateWeapon(EnemyManager._PlayerWeapons[1], transform.Find("Place/Enemy/WeaponCard2"));
    }
    public void ChooseFirstDrew(Text text)//选择第一抽取的武器
    {
        foreach (WeaponAsset weapon in PlayerWeapons)
        {
            if (weapon.WeaponName.Equals(text.text))
            {
                DrewWeapon = weapon;
                break;
            }
        }
        foreach (CardAsset card in DrewWeapon.Allcard)//拷贝卡牌
        {
            _currentDeck.Add(card);
            cardStates[card] = new CardState
            {
                TemporaryCost = int.Parse(card.cost),
                IsInPlayArea = false
            };
        }
        foreach (CardAsset card in EnemyManager.DrewWeapon.Allcard)//拷贝卡牌
        {
            EnemyManager._currentDeck.Add(card);
            EnemycardStates[card] = new CardState
            {
                TemporaryCost = int.Parse(card.cost),
                IsInPlayArea = false
            };
        }
        DrowCards(PlayerData.HandCardNum, HandArea, _currentDeck);
        DrowCards(EnemyManager._PlayerData.HandCardNum, EnemyManager.HandArea, EnemyManager._currentDeck);
        FirstDrew.SetActive(false);
        ChooseCardsPanel.SetActive(false);
        _currentPhase = GamePhase.playerReady;
    }
    public virtual void StartEnemyTurn()//敌人的主要阶段
    {
        Debug.Log(_currentPhase);
        EndTurn();
        //实现敌方逻辑
    }
    public virtual IEnumerator EnemyAcc(GameObject Card)
    {
        Debug.Log("蓄能！");
        yield return null;
        EndTurn();
        //实现敌方逻辑
    }
    public void UseCard(CardAsset card, GameObject cardObject, PlayerAsset User)//使用卡牌
    {
        int nowsp;
        if (User.TemporaryCostReduction != 0)
        {
            nowsp = User.NowSp + User.TemporaryCostReduction - int.Parse(card.cost);
            User.TemporaryCostReduction = 0;
        }
        else
            nowsp = User.NowSp - int.Parse(card.cost);
        if (nowsp < 0)
        {
            Debug.Log("当前体力不够使用此牌");
            return;
        }
        var behaviour = CardBehaviourFactory.Create(card);
        behaviour.Onplay(this, EnemyManager, cardObject);
        if(_currentPhase == GamePhase.playerAction)
            Destroy(cardObject);
        User.NowSp = nowsp;
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