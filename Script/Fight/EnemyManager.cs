using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
    [Header("敌人区域数据")]
    public GameObject enemyHand;//敌人手牌区
    public WeaponAsset[] ChooseWeapon;//赛前预选的3个武器
    public List<GameObject> ChooseCards;//玩家执行主要动作时选择的卡牌

    [Header("敌人卡牌基本数据")]
    public CharactorAsset charactor;//敌人的角色
    public WeaponAsset Weapon1;//第一个武器
    public WeaponAsset Weapon2;//第二个武器
    public List<CardAsset> shuffledCards;//武器的卡组
    

    [Header("敌人详细数据")]
    public string power;//敌人的角色技能
    public int speed;//敌人角色的速度
    public int HandCardNum;//手牌上限
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
    public Text Hptext;//剧目值文本
    public Text Deftext;//格挡值文本
    public Text Sptext;//精力值文本
    public Text Mptext;//法力值文本

    [Header("脚本内部数据")]
    public int CardNum = 20;//单个卡组总数量
    public int AttackManage;//发动攻击的数值
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UseCard()//使用卡牌
    {
        if (ChooseCards.Count == 1)
        {
            foreach (var i in ChooseCards)
                //执行卡牌效果，随后卡牌销毁（进入弃牌）
                Destroy(i);
            ChooseCards.Clear();
        }
    }
}
