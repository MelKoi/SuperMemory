using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.UI;

public enum GamePhase
{
    gameStart,playerAction,enemyAction
}
public class BattleManager : MonoBehaviour
{
    [Header("玩家区域的数据")]
    public GameObject hand;//手牌区
    public GameObject cardPrefeb;//手牌预设
    public WeaponAsset[] ChooseWeapon = new WeaponAsset[3];//玩家赛前预选的3个武器
    public List<GameObject> ChooseCards;//玩家执行主要动作时选择的卡牌

    [Header("玩家卡牌的基本数据")]
    public CharactorAsset charactor;//储存玩家的人物
    public WeaponAsset Weapon1;//存储玩家的第一个武器
    public WeaponAsset Weapon2;//存储玩家的第二个武器
    public  List<CardAsset> shuffledCards;//第一个武器的卡组

    [Header("玩家详细数据")]
    public string power;//玩家选择的人物技能
    public int speed;//玩家的速度
    public int HandCardNum;//玩家的手牌上限
    public int Hp;
    public int Def;
    public int Sp;
    public int Mp;
    public int Weapon1Acc;//武器1蓄能数量
    public int Weapon2Acc;//武器2蓄能数量

    [Header("武器组件")]
    public Text AccNum1Text;//现在的武器蓄能文本
    public Text AccNum2Text;//现在的武器蓄能文本
    public int AccNum1;//现在武器的蓄能
    public int AccNum2;//现在武器的蓄能
    public GameObject Weapon1Object;//第一个武器
    public GameObject Weapon2Object;//第二个武器
    public bool Weapon1Attacked;//第一个武器本回合已经攻击
    public bool Weapon2Attacked;//第二个武器本回合已经攻击

    [Header("显示数据组件")]
    public GameObject WeaponChoosePanel;//选择武器界面的画布
    public GameObject DrewChoosePanel;//选择第一次抽取的画布
    public Button[] ChooseWeaponButton = new Button[3];//武器三选二的按钮
    public Button[] ChooseFirstDrewButton = new Button[2];//选卡组按钮
    public Text Hptext;//剧目值文本
    public Text Deftext;//格挡值文本
    public Text Sptext;//精力值文本
    public Text Mptext;//法力值文本
 

    [Header("脚本内部数据")]
    private int CardNum = 20;//单个卡组总数量
    public GamePhase gamePhase = GamePhase.gameStart;//初始为游戏开始
    private WeaponAsset FirstDrewWeapon;//第一个抽取的武器
    public int AttackManage;//发动攻击的数值
    void Start()
    {
       GameStart();
    }
    public void DrowCards(WeaponAsset Weapon)//还需添加抽卡前检查玩家有几张手牌的功能（玩家只能大于等于当前的手牌数）
    {
        shuffledCards = new List<CardAsset>(Weapon.Allcard);
        for (int i = 0; i < HandCardNum; i++)
        {
            System.Random rng = new System.Random();//获取随机数
            int k = rng.Next(CardNum);
            cardPrefeb.GetComponent<OneCardManager>().cardAsset = shuffledCards[k];
            shuffledCards.RemoveAt(k);
            CardNum--;
            if (hand != null)
            {
                GameObject handcard = Instantiate(cardPrefeb, hand.transform, false);
            }
        }
    }
    //游戏开始，回合开始，回合中，回合结束
    //游戏开始：抽取武器牌，玩家选择三张；玩家获得牌库，选择其中之一；抽取x张手牌。
    public void ReadDeck()//读取玩家数据
    {
        HandCardNum = charactor.HandCardNum;
        Hp = charactor.MaxHealth;
        Def = charactor.StartDef;
        Sp = charactor.MaxSp;
        Mp = 0;
        speed = charactor.speed;
    }
    public void GameStart()//游戏开始的各项工作
    {
        //玩家从对手的三个武器中选出一个禁用（这里暂时使用自己的进行选择）
        for(int i = 0;i < 3; i++)
        {
             ChooseWeaponButton[i].GetComponentInChildren<Text>().text = ChooseWeapon[i].WeaponName;
        }
        ReadDeck();
        Hptext.text = Hp.ToString();
        Deftext.text = Def.ToString();
        Sptext.text = Sp.ToString();
        Mptext.text = Mp.ToString();
        Weapon1Attacked = false;
        Weapon2Attacked = false;
        AttackManage = 0;
        gamePhase = GamePhase.playerAction;
    }
    public void TurnEnd()//回合结束处理
    {
        if(gamePhase == GamePhase.playerAction)
        {
            gamePhase = GamePhase.enemyAction;
        }
        else if(gamePhase == GamePhase.enemyAction)
        {
            Weapon1Attacked = false;
            Weapon2Attacked = false;
            AttackManage = 0;
            gamePhase = GamePhase.playerAction;
        }
    }
    public void ChooseWeapons(Text text)
    {
        string name = text.text;//获取当前按钮对应的武器名
        for(int i = 0; i < 3; i++)//查找到玩家选项中对应的武器
        {
            if (!ChooseWeapon[i].WeaponName.Equals(name))
            {
                if (Weapon1 == null) Weapon1 = ChooseWeapon[i];
                else if (Weapon1 != null) Weapon2 = ChooseWeapon[i];
            }
            else if (ChooseWeapon[i].WeaponName.Equals(name)) continue;
        }
        ChooseFirstDrewButton[0].GetComponentInChildren<Text>().text = Weapon1.WeaponName;
        ChooseFirstDrewButton[1].GetComponentInChildren<Text>().text = Weapon2.WeaponName;
        Weapon1Object.GetComponent<WeaponCardManager>().weaponAsset = Weapon1;
        Weapon2Object.GetComponent<WeaponCardManager>().weaponAsset= Weapon2;
        WeaponChoosePanel.SetActive(false);
    }
    public void ChooseFirstDrew(Text text)//选择先抽取的卡组
    {
        string name = text.text;//获取当前按钮对应的武器名
        if (name.Equals(Weapon1.WeaponName)) FirstDrewWeapon = Weapon1;
        else FirstDrewWeapon = Weapon2;
        DrowCards(FirstDrewWeapon);
        DrewChoosePanel.SetActive(false);
    }
    public void UseCard()
    {
        if(ChooseCards.Count == 1)
        {
            foreach (var i in ChooseCards)
                //执行卡牌效果，随后卡牌销毁（进入弃牌）
                Destroy(i);
            ChooseCards.Clear();
        }
    }
    public void WeaponAccmulation(GameObject AccWeapon)//卡牌蓄能
    {
        if(AccWeapon == Weapon1Object)
        {
            if(ChooseCards.Count <= Weapon1Object.GetComponent<WeaponCardManager>().weaponAsset.OnceAccumulation)//如果选择的手牌小于等于允许的单次蓄能
            {
                AccNum1 = AccNum1 + ChooseCards.Count;//加入蓄能数量的卡牌
                AccNum1Text.text = AccNum1.ToString();//更新蓄能文本
                foreach (var i in ChooseCards)//摧毁被用来蓄能的卡牌
                    Destroy(i);
                ChooseCards.Clear();//选择的手牌清空
            }
            else
            {
                Debug.Log("此武器无法单次蓄能当前数量");
            }
        }
        else if(AccWeapon == Weapon2Object)
        {
            if (ChooseCards.Count <= Weapon2Object.GetComponent<WeaponCardManager>().weaponAsset.OnceAccumulation)//如果选择的手牌小于等于允许的单次蓄能
            {
                AccNum2 = AccNum2 + ChooseCards.Count;
                AccNum2Text.text = AccNum2.ToString();
                foreach (var i in ChooseCards)
                    Destroy(i);
                ChooseCards.Clear();
            }
            else
            {
                Debug.Log("此武器无法单次蓄能当前数量");
            }
        }

    }
    public void WeaponAttack(GameObject AccWeapon)
    {
        //按下攻击按钮后，先检测蓄能数量，与WeaponAsset中的蓄能等级匹配，选出≥的最后一档蓄能等级，
        //随后进行攻击，蓄能值归零，刷新文本内容，已攻击bool值变为true

        if(AccWeapon == Weapon1Object)//两个if将对应的武器蓄能数据和文本更新为0
        {
            if(!Weapon1Attacked)
            {
                for (int i = 0; i < 3; i++)//匹配蓄能，并且打出伤害
                    if (AccNum1 >= AccWeapon.GetComponent<WeaponCardManager>().weaponAsset.Accumulation[i].Acc)
                    {
                        AttackManage = AccWeapon.GetComponent<WeaponCardManager>().weaponAsset.Accumulation[i].Value;
                    }
                Debug.Log("你打出了" + AttackManage + "点伤害");
                if (AttackManage == 0)//当蓄能不足时打不出伤害
                {
                    Debug.Log("蓄能不足，无法造成伤害");
                    return;
                }
                AccNum1 = 0;//调整必要数值，且设置为本回合以攻击状态
                AccNum1Text.text = AccNum1.ToString();
                Weapon1Attacked = true;
                AttackManage = 0;
            }
            else
            {
                Debug.Log("此武器本回合已经用于攻击");
            }
        }
        else if(AccWeapon == Weapon2Object) 
        { 
            if (!Weapon2Attacked)
            {
                for (int i = 0; i < 3; i++)//匹配蓄能，并且打出伤害
                    if (AccNum2 >= AccWeapon.GetComponent<WeaponCardManager>().weaponAsset.Accumulation[i].Acc)
                    {
                        AttackManage = AccWeapon.GetComponent<WeaponCardManager>().weaponAsset.Accumulation[i].Value;
                    }
                Debug.Log("你打出了" + AttackManage + "点伤害");
                if (AttackManage == 0)
                {
                    Debug.Log("蓄能不足，无法造成伤害");
                    return;
                }
                AccNum2 = 0;
                AccNum2Text.text = AccNum2.ToString();
                Weapon2Attacked = true;
                AttackManage = 0;
            }
            else
            {
                Debug.Log("此武器本回合已经用于攻击");
            }
        }
     }
}
