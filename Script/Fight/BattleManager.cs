﻿using DG.Tweening;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GamePhase
{
    gameStart,AllReady,playerAction,enemyAction,playerReady,enemyReady,playerEnd,enemyEnd,gameEnd
}
public class BattleManager : MonoBehaviour
{
    private Dictionary<CardAsset, CardState> cardStates = new Dictionary<CardAsset, CardState>();
    private Dictionary<CardAsset, CardState> EnemycardStates = new Dictionary<CardAsset, CardState>();

    [Header("开头动画所需")]
    public Animation BackGroud;//背景
    public GameObject ChooseWeapon;//选择武器界面
    public GameObject Place;//战斗场地

    [Header("双方角色")]
    public PlayerAsset Player;//我方
    public PlayerAsset Enemy;//敌方

    [Header("玩家数据")]
    public CharactorAsset PlayerData;//角色卡
    public WeaponAsset[] PlayerWeapons = new WeaponAsset[2];//禁用后武器卡
    public WeaponAsset[] PlayerAllWeapons = new WeaponAsset[3];//初始武器卡
    public WeaponAsset DrewWeapon;//第一个抽的武器
    public bool Weapon1CardEmpety = false;//第一个武器卡组被抽空
    public bool Weapon2CardEmpety = false;//第二个武器卡组被抽空

    [Header("敌方数据引用")]
    public EnemyManager EnemyManager;//敌方管理器

    [Header("卡牌组件")]
    public GameObject HandArea;//手牌区域
    public Transform SkillArea;//战技展开区域
    public GameObject CharacterArea;//人物牌区域

    [Header("卡牌预制体")]
    public GameObject CardPrefab;//卡牌预制体
    public GameObject CharacterPrefeb;//人物卡牌预制体
    public GameObject WeaponPrefeb;//武器卡牌预制体

    [Header("UI组件")]
    public TextMeshProUGUI HpText;//我方生命
    public TextMeshProUGUI MpText;//我方能量
    public Text SynchronizationText;//我方架势条
    public Text Weapon1Acc;//武器1蓄能
    public Text Weapon2Acc;//武器2蓄能
    public GameObject Purple;//对应卡是否使用
    public GameObject ZeroPoint;//战斗场地中心
    public Button PowerButton;//技能按钮
    public Button EndTurnButton;//回合结束按钮
    public Image PlayerPower;//玩家的技能图片
    public Image EnemyPower;//敌人的技能图片
    public Sprite POpen;//对应牌起效
    public Sprite PClose;//对应牌失效
    public GameObject GameOver;//游戏结束
    public Button CentreButton;//选择武器的中心按钮
    public Text BanDescription;//Ban掉敌方武器时的武器描述
    public Text FirstDescription;//选择首选抽取武器时的武器描述

    [Header("功能性变量")]
    public GameObject BanEnemyWeapon;//ban武器界面
    public GameObject FirstDrew;//选择抽取武器界面
    public List<CardEffectAsset> AttackEffect = new List<CardEffectAsset>();//攻击牌效果池
    public List<CardEffectAsset> SkillEffect = new List<CardEffectAsset>();//战技牌效果
    public List<CardEffectAsset> CounterEffect = new List<CardEffectAsset>();//对应牌效果池
    public List<CardAsset> _currentDeck;//牌库
    public GamePhase _currentPhase;//回合情况
    public bool hasEnemyTurnStarted = false;//进入敌人的回合
    public CardPool PlayerPool;//对象池
    public bool PlayerSkillIsUsed = false;//玩家技能已经使用
    public bool addDialogFlag = false;//？
    public BagDataManager bagDataManager;//获取背包
    [Header("广播")]
    public SceneLoadEventSO sceneLoadEvent;
    [Header("事件监听")]
    public BattleListen BS;
    public int LastAttWeapon;//监听玩家最后一次使用于攻击的武器
    [Header("场景")]
    public GameSceneSO room;
    [Header("通关奖励")]
    public List<CharactorAsset> passCharactorReward;
    public List<WeaponAsset> passWeaponReward;

    void Start()
    {
        GameStart();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z) && Player.NowMp >= 5 && !Player.Injured)//按下z执行玩家格挡，对于韧性条的根据伤害伤害衰减：标准：原始伤害5以下为1档，7以下为2档，7以上为3档
        {
            Player.NowMp = Player.NowMp - 5;
            PlayerHide();
        }
        switch (_currentPhase)//根据回合阶段进行具体的操作
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
                    int cardsNum = HandArea.transform.childCount;
                    for (int i = 0; i < cardsNum; i++)
                        HandArea.transform.GetChild(i).GetComponent<CardDragContral>().canDrug = false;
                    StartEnemyTurn();
                    hasEnemyTurnStarted = true;
                }
                break;
            case GamePhase.playerEnd:
                PlayerEnd(); break;
            case GamePhase.enemyEnd:
                EnemyEnd(); break;
            case GamePhase.gameEnd:
                Debug.Log("对局结束");
                if (Enemy.NowHp <= 0 && !addDialogFlag)
                {
                    addDialogFlag = true;
                    GetPassReward();
                    GameObject.Find("FightAndDialogController").GetComponent<FightAndDialogController>().currentDialog++;
                    GameObject.Find("FightAndDialogController").GetComponent<FightAndDialogController>().currentFight++;
                }
                GameOver.SetActive(true);
                return;
        }
        if(_currentPhase != GamePhase.gameStart)
        {
            if (Player.Weapon1Acc != int.Parse(Weapon1Acc.text)
           || Player.Weapon2Acc != int.Parse(Weapon2Acc.text)
           || Player.NowMp != int.Parse(HpText.text)
           || Player.NowMp != int.Parse(MpText.text)
           || Player.NowSynchronization != int.Parse(SynchronizationText.text))
            {
                UpdateUI(HpText, MpText, SynchronizationText, Weapon1Acc, Weapon2Acc, Player);
            }
            if (Enemy.Weapon1Acc != int.Parse(EnemyManager.Weapon1Acc.text)
                || Enemy.Weapon2Acc != int.Parse(EnemyManager.Weapon2Acc.text)
                || Enemy.NowHp != int.Parse(EnemyManager.HpText.text)
                || Enemy.NowMp != int.Parse(EnemyManager.MpText.text)
                || Enemy.NowSynchronization != int.Parse(EnemyManager.SynchronizationText.text))
            {
                UpdateUI(EnemyManager.HpText, EnemyManager.MpText, EnemyManager.SynchronizationText, EnemyManager.Weapon1Acc,
                EnemyManager.Weapon2Acc, Enemy);
            }
            if (Player.NowSynchronization > Player.MaxSynchronization)
                Player.NowSynchronization = Player.MaxSynchronization;
            if (Enemy.NowSynchronization > Enemy.MaxSynchronization)
                Enemy.NowSynchronization = Enemy.MaxSynchronization;
            if ((Player.NowHp <= 0 || Enemy.NowHp <= 0) || ((Weapon1CardEmpety && Weapon2CardEmpety) ||
                (EnemyManager.Weapon1CardEmpety && EnemyManager.Weapon2CardEmpety)))//任何一方生命值归零或者手牌抽完判负
            {
                _currentPhase = GamePhase.gameEnd;
            }
        }
    }
    public void DrowCards(int amount, GameObject HandArea, List<CardAsset> _currentDeck)//抽取卡牌
    {
        if (_currentDeck.Count == 0)//本次抽卡时发现牌库为空
            if (_currentPhase == GamePhase.playerReady)//如果是玩家抽卡发现的
            {
                if(!Weapon1CardEmpety)//确定是第几把武器抽空了
                    Weapon1CardEmpety = true;
                else
                    Weapon2CardEmpety = true;
                    foreach (WeaponAsset second in PlayerWeapons)
                    {
                        if (!second.WeaponName.Equals(DrewWeapon.WeaponName))//检索玩家拥有的第二把武器
                        {
                            DrewWeapon = second;//更新抽取的武器
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
            }
            else if (_currentPhase == GamePhase.enemyReady)//如果是敌人抽卡发现的
            {
                if (!EnemyManager.Weapon1CardEmpety)//确定是第几把武器抽空了
                    EnemyManager.Weapon1CardEmpety = true;
                else
                    EnemyManager.Weapon2CardEmpety = true;
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
            if(HandArea != null)
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
        {
            newCard.GetComponent<OneCardManager>().CardBack.SetActive(true);
            newCard.GetComponent<CardPreview>().CanPreview = false;
        }


    }
    private void CreateCharacter(CharactorAsset character, Transform parent)
    {
        GameObject newCharacter = Instantiate(CharacterPrefeb, parent);
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
    public void UpdateUI(TextMeshProUGUI Hptext, TextMeshProUGUI Mptext, Text SynchronizationText, Text Weapon1Acc, Text Weapon2Acc, PlayerAsset player)
    {
        Hptext.text = player.NowHp.ToString();
        Mptext.text = player.NowMp.ToString();
        SynchronizationText.text = player.NowSynchronization.ToString();
        Weapon1Acc.text = player.Weapon1Acc.ToString();
        Weapon2Acc.text = player.Weapon2Acc.ToString();
    }
    public void GameStart()//游戏开始
    {
        //读取玩家各种数据
        PlayerData = Player.CharacterAsset;
        //获取背包管理器
        bagDataManager = GameObject.Find("Bag").GetComponent<BagDataManager>();
        for (int i = 0; i < 3; i++)
        {
            PlayerAllWeapons[i] = Player.WeaponAsset[i];
        }
        Player.MaxHp = Player.NowHp = PlayerData.MaxHealth;//玩家游戏开始时的生命值
        Player.MaxSynchronization = Player.NowSynchronization = PlayerData.MaxSynchronization;//玩家的架势条最大值
        PlayerPool = new CardPool();//初始化玩家的卡牌对象池
        _currentDeck = new List<CardAsset>();//初始化玩家可手牌list
        Player.Weapon1Acc = Player.Weapon2Acc = Player.NowMp = 0;//玩家游戏开始时的两把武器蓄能，能量值
        Player.Weapon1 = Player.Weapon2 = false;//将两把武器设置为可攻击状态
        CreateCharacter(PlayerData, transform.Find("Place/CharacterCard"));//创建玩家卡
        HpText = transform.Find("Place/CharacterCard/CharacterCard(Clone)/HP/HPText").GetComponent<TextMeshProUGUI>();
        MpText = transform.Find("Place/CharacterCard/CharacterCard(Clone)/MP/MPText").GetComponent<TextMeshProUGUI>();
        PlayerPower.sprite = PlayerData.PowerImage;//创建玩家技能
        Purple.GetComponent<Image>().sprite = PClose;//将对应牌状态设置为否

        //读取敌方各种数据
        EnemyManager._PlayerData = Enemy.CharacterAsset;
        for (int i = 0; i < 3; i++)
        {
            EnemyManager._PlayerAllWeapons[i] = Enemy.WeaponAsset[i];
        }
        Enemy.MaxHp = Enemy.NowHp = EnemyManager._PlayerData.MaxHealth;
        Enemy.MaxSynchronization = Enemy.NowSynchronization = EnemyManager._PlayerData.MaxSynchronization;
        Enemy.NowMp = 10;
        EnemyManager.EnemyPool = new CardPool();
        EnemyManager._currentDeck = new List<CardAsset>();
        Enemy.Weapon1Acc = Enemy.Weapon2Acc =0;
        Enemy.Weapon1 = Enemy.Weapon2 = false;
        CreateCharacter(EnemyManager._PlayerData, transform.Find("Place/Enemy/CharacterCard"));
        EnemyManager.HpText = transform.Find("Place/Enemy/CharacterCard/CharacterCard(Clone)/HP/HPText").GetComponent<TextMeshProUGUI>();
        EnemyManager.MpText = transform.Find("Place/Enemy/CharacterCard/CharacterCard(Clone)/MP/MPText").GetComponent<TextMeshProUGUI>();
        EnemyPower.sprite = EnemyManager._PlayerData.PowerImage;
        EnemyManager.Purple.GetComponent<Image>().sprite = PClose;

        for (int i = 1; i <= 3; i++)
        {
            BanEnemyWeapon.transform.GetChild(i + 1).gameObject.GetComponent<Image>().sprite = EnemyManager._PlayerAllWeapons[i - 1].CardFace;
        }

        //统一更新UI
        UpdateUI(HpText, MpText, SynchronizationText, Weapon1Acc, Weapon2Acc, Player);
        UpdateUI(EnemyManager.HpText, EnemyManager.MpText, EnemyManager.SynchronizationText, EnemyManager.Weapon1Acc,
            EnemyManager.Weapon2Acc, Enemy);
        for (int i = 1; i <= 3; i++)//将敌方武器赋给ban武器界面
        {
            BanEnemyWeapon.transform.GetChild(i + 1).GetChild(0).gameObject.GetComponent<Text>().text = EnemyManager._PlayerAllWeapons[i - 1].WeaponName;
        }
        BanDescription.GetComponent<Text>().text = EnemyManager._PlayerAllWeapons[0].description;
        StartCoroutine(PlayGameStartAnimation());
    }
    public void PlayerReady()//玩家准备阶段
    {
        hasEnemyTurnStarted = false;//敌方回合结束
        Debug.Log(_currentPhase);//显示回合阶段
        PowerButton.interactable = true;
        EndTurnButton.interactable = true;//将两个功能性按钮设置为可按下状态
        Player.NowMp += 10;//恢复体力,每个回合恢复10点，玩家每次攻击会将卡牌数量添加，不超过上线（MaxMp）
        PlayerSkillIsUsed = false;//重置玩家技能使用情况
        if (SkillEffect != null)
            foreach (var effect in SkillEffect)//先结算当前我方展开的战技牌
            {
                effect.ApplyEffect(this, EnemyManager, false);
            }
        if (EnemyManager.SkillEffect != null)
            foreach (var effect in EnemyManager.SkillEffect)//再结算当前敌方展开的战技牌，下方敌人同理
            {
                effect.ApplyEffect(this, EnemyManager, true);
            }
        if (EnemyManager.CounterEffect.Count != 0)//检测敌方是否有对应牌的保险代码，经测试决定是否可以删掉
            EnemyManager.Purple.GetComponent<Image>().sprite = POpen;
        if (Enemy.Injured)//重置敌方韧性条
        {
            Enemy.Injured = false;
            Enemy.NowSynchronization = Enemy.MaxSynchronization / 2;
        }
        else if (!Player.Weapon1 && !Player.Weapon2)//如果本回合没有攻击，敌方架势条恢复25%
            Enemy.NowSynchronization += (Enemy.MaxSynchronization / 4);
        Player.Damage = 0;//重置伤害
        UpdateUI(HpText, MpText, SynchronizationText, Weapon1Acc, Weapon2Acc, Player);//更新UI
        //重置武器攻击状态
        Enemy.Weapon1 = false;
        Enemy.Weapon2 = false;
        _currentPhase = GamePhase.playerAction;
        int cardsNum = HandArea.transform.childCount;
        for (int i = 0; i < cardsNum; i++)
            HandArea.transform.GetChild(i).GetComponent<CardDragContral>().canDrug = true;
    }
    public void EndTurn()//回合结束
    {
        if (_currentPhase == GamePhase.playerAction)
        {
            
            _currentPhase = GamePhase.playerEnd;
        }
        else if (_currentPhase == GamePhase.enemyAction)
        {
           
            _currentPhase = GamePhase.enemyEnd;
        }
    }
    public void EnemyReady()//敌方准备阶段
    {

        Debug.Log(_currentPhase);
        PowerButton.interactable = false;
        EndTurnButton.interactable = false;//将两个功能性按钮设置为不可控制状态
        Enemy.NowMp += 10;//恢复体力
        if (EnemyManager.SkillEffect != null)
            foreach (var effect in EnemyManager.SkillEffect)
            {
                effect.ApplyEffect(this, EnemyManager, false);
            }
        if (SkillEffect != null)
            foreach (var effect in SkillEffect)
            {
                effect.ApplyEffect(this, EnemyManager, true);
            }
        Enemy.Damage = 0;
        if(Player.Injured)
        {
            Player.Injured = false;
            Player.NowSynchronization = Player.MaxSynchronization / 2;
        }
        else if (!Player.Weapon1 && !Player.Weapon2)//如果本回合没有攻击，敌方架势条恢复25%
            Enemy.NowSynchronization += (Enemy.MaxSynchronization / 4);
        if (CounterEffect.Count != 0)
            Purple.GetComponent<Image>().sprite = POpen;
        UpdateUI(EnemyManager.HpText, EnemyManager.MpText, EnemyManager.SynchronizationText, EnemyManager.Weapon1Acc,
            EnemyManager.Weapon2Acc, Enemy);
        //重置武器攻击状态
        Player.Weapon1 = false;
        Player.Weapon2 = false;
        _currentPhase = GamePhase.enemyAction;
    }
    public void PlayerEnd()//玩家的结束阶段
    {
        DrowCards(PlayerData.HandCardNum - HandArea.transform.childCount, HandArea, _currentDeck);//抽卡
        _currentPhase = GamePhase.enemyReady;
    }
    public void EnemyEnd()//敌方的结束阶段
    {
        DrowCards(EnemyManager._PlayerData.HandCardNum - EnemyManager.HandArea.transform.childCount, EnemyManager.HandArea, EnemyManager._currentDeck);
        _currentPhase = GamePhase.playerReady;
    }
    public void ChooseWeapons(Text text)//我方ban对方一个武器牌
    {
        int j = 0, k = 0;
        int random = new System.Random().Next(0, 3);
        for (int i = 0; i < 3; i++)
        {
            if (i != random)
            {
                PlayerWeapons[k] = PlayerAllWeapons[i];
                k++;
            }
        }
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
        for (int i = 1; i <= 2; i++)
        {
            FirstDrew.transform.GetChild(i + 1).GetChild(0).gameObject.GetComponent<Text>().text = PlayerWeapons[i - 1].WeaponName;
            FirstDrew.transform.GetChild(i + 1).gameObject.GetComponent<Image>().sprite = PlayerWeapons[i - 1].CardFace;
        }
        FirstDescription.GetComponent<Text>().text = PlayerWeapons[0].description;
        BanEnemyWeapon.SetActive(false);
        Enemy.WeaponOne = EnemyManager._PlayerWeapons[0];
        Enemy.WeaponTwo = EnemyManager._PlayerWeapons[1];
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
        Place.SetActive(true);
        Player.WeaponOne = PlayerWeapons[0];
        Player.WeaponTwo = PlayerWeapons[1];
        FirstDrew.SetActive(false);
        ChooseWeapon.SetActive(false);
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
            nowsp = User.NowMp + User.TemporaryCostReduction - int.Parse(card.cost);
            User.TemporaryCostReduction = 0;
        }
        else
            nowsp = User.NowMp - int.Parse(card.cost);
        if (nowsp < 0)
        {
            nowsp = User.NowMp;
            Debug.Log("当前体力不够使用此牌");
            return;
        }
        var behaviour = CardBehaviourFactory.Create(card);
        behaviour.Onplay(this, EnemyManager, cardObject);
        if (_currentPhase == GamePhase.playerAction)
            Destroy(cardObject);
        User.NowMp = nowsp;
    }

    public void PlayerUseSkill()
    {
        if (!PlayerSkillIsUsed)
        {
            if (Player.NowMp - PlayerData.PowerCost >= 0)
            {
                Player.NowMp = Player.NowMp - PlayerData.PowerCost;
                foreach (var SkillEffect in PlayerData.PowerEffect)
                    SkillEffect.ApplyEffect(this, EnemyManager, false);
                Debug.Log("技能已经使用");
                PlayerSkillIsUsed = !PlayerSkillIsUsed;
            }
            else
                Debug.Log("能量不足");
        }
    }
    public void EnemyUseSkill()
    {
        if (Enemy.NowMp - EnemyManager._PlayerData.PowerCost >= 0)
        {
            Enemy.NowMp = Enemy.NowMp - EnemyManager._PlayerData.PowerCost;
            //foreach (var SkillEffect in EnemyManager._PlayerData.PowerEffect)
            //SkillEffect.ApplyEffect(this, EnemyManager, false);
            Debug.Log("技能已经使用");
            EnemyManager.EnemySkillIsUsed = !EnemyManager.EnemySkillIsUsed;
        }
        else
            Debug.Log("能量不足");
    }

    IEnumerator PlayGameStartAnimation()//对战开始动画
    {
        BackGroud.Play();
        yield return new WaitForSeconds(3.5f);

        ChooseWeapon.SetActive(true);

    }
    public void PlayerHide()//玩家格挡
    {
        BS.hideEvent.RaiseEvent();
    }

    public void BackToRoom()
    {
        sceneLoadEvent.RaiseLoadRequestEvent(room, true);
    }//返回房间

    public void GetPassReward()
    {
        foreach (var charactor in passCharactorReward)
        {
            if (charactor != null)
            {
                bagDataManager.AcquireCharactor(charactor);
            }
        }
        foreach (var weapon in passWeaponReward)
        {
            if (weapon != null)
            {
                bagDataManager.AcquireWeapon(weapon);
            }
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