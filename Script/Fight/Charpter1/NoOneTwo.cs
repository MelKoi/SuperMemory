using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NoOneTwo : BattleManager
{
    [Header("权重数据集")]
    private int Hide = 0;//不参与排序
    public int Behavior = 3;
    private Dictionary<string, int> actionWeights = new Dictionary<string, int>
    {
        {"UseCardAA", 8},
        {"UseCardCS", 5},
        {"WeaponAtt", 4},
        {"SkillUse", 2},
        {"WeaponAcc", 6}
    };

    private Dictionary<string, Func<IEnumerator>> actionMethods;

        

    [Header("双方部分上回合数据")]
    private int PlayerLastHP;
    private int PlayerLastMP;
    private int EnemyLastHP;

    [Header("临时储存")]
    private int acc = 0;//蓄能临时储存
    private bool hasAttackedThisTurn = false;//本回合是否进行过攻击,用于回合内判断行动模式
    private bool FirstEnemyTurn = true;

    private void Update()
    {
        if(hasAttackedThisTurn = false && (Enemy.Weapon1Acc >= 4 || Enemy.Weapon2Acc >= 4))//检测是否需要改变权重,以下同理
        {
            actionWeights["UseCardAA"] = 3;
        }
        else
        {
            actionWeights["UseCardAA"] = 8;
        }

        if(Enemy.hp - EnemyLastHP <= -3)
        {
            actionWeights["UseCardCS"] = 7;
        }
        else
        {
            actionWeights["UseCardCS"] = 5;
        }

        if(Enemy.Weapon1Acc >= 4 || Enemy.Weapon2Acc >= 4)
        {
            actionWeights["WeaponAtt"] = 5;
        }
        else
        {
            actionWeights["WeaponAtt"] = 6;
        }

        if(Player.hp < PlayerLastHP && Enemy.mp >= EnemyManager._PlayerData.PowerCost)
        {
            actionWeights["SkillUse"] = 8;
        }
        else
        {
            actionWeights["SkillUse"] = 2;
        }
    }
    private IEnumerator PerformAIActions()//总体的敌人AI
    {
        RectTransform ZeroPosition = ZeroPoint.GetComponent<RectTransform>();//获取战斗场地中点
        if(FirstEnemyTurn)//第一回合获取双方的基本数据
        {
            PlayerLastHP = Player.hp;
            PlayerLastMP = Player.mp;
            EnemyLastHP = EnemyManager._PlayerData.MaxHealth;
            FirstEnemyTurn = false;
        }

        var sortedActions = actionWeights.OrderByDescending(pair => pair.Value)
                                .ToList();

        if (Enemy.hp > EnemyManager._PlayerData.MaxHealth / 2)
            Behavior = 3;
        else
            Behavior = UnityEngine.Random.Range(3, 5);

        foreach (var action in sortedActions)//利用反射进行行为逻辑
        {
            if(Behavior == 0)
            {
                break;
            }
            //获取手牌
            //检索牌库内的手牌
            List<Transform> EnemyCards = new List<Transform>();
            Transform parent = EnemyManager.HandArea.transform;
            if (parent.childCount != 0)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    EnemyCards.Add(parent.GetChild(i));
                }
            }

            actionMethods = new Dictionary<string, Func<IEnumerator>>//获取方法
        {
            {"UseCardAA",() => UseCardAA()},
            {"UseCardCS",() => UseCardCS()},
            {"WeaponAtt",() => WeaponAtt()},
            {"SkillUse",() => SkillUse()},
            {"WeaponAcc",() => WeaponAcc()}
        };
            if (CanExecuteAction(action.Key) && actionMethods.TryGetValue(action.Key, out Func<IEnumerator> method))
            {
                yield return StartCoroutine(method());
                continue;
            }
            IEnumerator UseCardAA()
            {
                //使用攻击牌和行动牌
                GameObject AAcard = FindCardOfType(Type.攻击, EnemyCards);
                if (AAcard == null) AAcard = FindCardOfType(Type.行动, EnemyCards);

                if (AAcard != null && (Enemy.NowSp - int.Parse(AAcard.GetComponent<OneCardManager>().cardAsset.cost) >= 0))
                {
                    yield return StartCoroutine(UseCardWithAnimation(AAcard));
                    Behavior--;
                }

            }
            IEnumerator UseCardCS()
            {
                GameObject CScard = FindCardOfType(Type.对应, EnemyCards);
                if (CScard == null) CScard = FindCardOfType(Type.战技, EnemyCards);

                if (CScard != null && (Enemy.NowSp - int.Parse(CScard.GetComponent<OneCardManager>().cardAsset.cost) >= 0))
                {
                    yield return StartCoroutine(UseCardWithAnimation(CScard));
                    Behavior--;
                }
            }
            IEnumerator WeaponAtt()
            {
                if (TryWeaponAttack(out bool didAttack))
                {
                    if (didAttack) yield return new WaitForSeconds(5f); // 攻击动画等待
                    Behavior--;
                }
            }
            IEnumerator SkillUse()
            {
                if (!EnemyManager.EnemySkillIsUsed)
                {
                    if (Enemy.mp - EnemyManager._PlayerData.PowerCost >= 0)
                    {
                        Enemy.mp = Enemy.mp - EnemyManager._PlayerData.PowerCost;
                        foreach (var SkillEffect in EnemyManager._PlayerData.PowerEffect)
                            SkillEffect.ApplyEffect(this, EnemyManager, false);
                        Debug.Log("技能已经使用");
                        EnemyManager.EnemySkillIsUsed = !EnemyManager.EnemySkillIsUsed;
                        Behavior--;
                    }
                    else
                        Debug.Log("能量不足");
                    yield return null;
                }
            }
            IEnumerator WeaponAcc()
            {
                yield return StartCoroutine(EnemyAcc(parent.GetChild(0).gameObject));
                Behavior--;
            }
            bool CanExecuteAction(string actionName)
            {
                return true;// 实现条件检查
            }
        }
        EndTurn();
        if (!FirstEnemyTurn)
        {
            PlayerLastHP = Player.hp;
            PlayerLastMP = Player.mp;
            EnemyLastHP = Enemy.hp;
        }
        yield return null;
    }
    private IEnumerator UseCardWithAnimation(GameObject card)//敌方使用卡牌
    {
        card.GetComponent<CardPreview>().CanPreview = false;
        // 移动卡牌到屏幕中央的动画
        // 隐藏卡牌背面
        card.transform.DORotate(new Vector3(0, 90, 0), 1.0f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                card.GetComponent<OneCardManager>().CardBack.gameObject.SetActive(false);
                card.transform.DORotate(new Vector3(0, 180, 0), 1.0f);
            });

        card.transform.DOMove(new Vector3(960, 540, 0), 2.0f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                // 动画完成后执行
                if (card != null)
                {
                    // 使用卡牌
                    CardAsset cardAsset = card.GetComponent<OneCardManager>().cardAsset;
                    card.GetComponent<CardPreview>().CanPreview = true;
                    UseCard(cardAsset, card, Enemy);

                    Destroy(card);
                }
            });

        // 等待动画完成
        yield return new WaitForSeconds(2.0f);

        Behavior--;
    }
    private IEnumerator MoveCardToPosition(GameObject card, Vector3 targetPosition)
    {
        if (card == null) yield break; // 提前检查

        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPosition = card.transform.position;

        while (elapsed < duration && card != null)
        {
            card.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // 最终位置设置前再次检查
        if (card != null)
            card.transform.position = targetPosition;
    }

    private bool TryWeaponAttack(out bool didAttack)
    {
        didAttack = false;

        if (!Enemy.Weapon1)
        {
            acc = Enemy.Weapon1Acc;
            foreach (var damage in EnemyManager._PlayerWeapons[0].Accumulation)
            {
                if (acc >= damage.Acc)
                {
                    Enemy.Damage = damage.Value;
                }
            }

            if (Enemy.Damage != 0)
            {
                StartCoroutine(PerformWeaponAttack(true));
                didAttack = true; // 表示已开始攻击
                return true; // 表示尝试了攻击
            }
        }

        if (!Enemy.Weapon2)
        {
            acc = Enemy.Weapon2Acc;
            foreach (var damage in EnemyManager._PlayerWeapons[1].Accumulation)
            {
                if (acc >= damage.Acc)
                {
                    Enemy.Damage = damage.Value;
                }
            }

            if (Enemy.Damage != 0)
            {
                StartCoroutine(PerformWeaponAttack(false));
                didAttack = true; // 表示已开始攻击
                return true; // 表示尝试了攻击
            }
        }

        return false;
    }
    // 新增协程方法处理实际攻击逻辑
    private IEnumerator PerformWeaponAttack(bool isWeapon1)
    {
        // 触发攻击事件
        EnemyManager.BS.attEvent.RaiseEvent();

        // 等待攻击命中判定
        float timeout = 3f; // 超时时间
        float elapsed = 0f;

        while (!EnemyManager.BS.bulletController.hasHit && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!EnemyManager.BS.bulletController.hasHit)
        {
            Debug.LogWarning("攻击未命中或超时");
            Enemy.mp += acc;//处理攻击后逻辑
            acc = 0;
            Enemy.Damage = Enemy.Damage / 2;

            if (isWeapon1)
            {
                Enemy.Weapon1Acc = 0;
                Enemy.Weapon1 = true;
            }
            else
            {
                Enemy.Weapon2Acc = 0;
                Enemy.Weapon2 = true;
            }

            hasAttackedThisTurn = true;
            Behavior--;
            yield break;
        }

        // 执行攻击命中后的逻辑
        if (Purple.GetComponent<Image>().sprite == POpen)
        {
            foreach (var effect in CounterEffect)
            {
                effect.ApplyEffect(this, EnemyManager, true);
            }
            Purple.GetComponent<Image>().sprite = PClose;
        }
        foreach (var effect in EnemyManager.AttackEffect)
        {
            effect.ApplyEffect(this, EnemyManager, false);
        }
        Player.hp -= Enemy.Damage;
        Debug.Log($"对我方造成{Enemy.Damage}点伤害！");
        Enemy.mp += acc;//处理攻击后逻辑
        acc = 0;
        Enemy.Damage = 0;

        if (isWeapon1)
        {
            Enemy.Weapon1Acc = 0;
            Enemy.Weapon1 = true;
        }
        else
        {
            Enemy.Weapon2Acc = 0;
            Enemy.Weapon2 = true;
        }

        hasAttackedThisTurn = true;
        Behavior--;

    }
    private GameObject FindCardOfType(Type type, List<Transform> EnemyCards)//寻找相应的卡牌
    {
        foreach (Transform card in EnemyCards)
        {
            CardAsset cardAsset = card.gameObject.GetComponent<OneCardManager>().cardAsset;
            if (cardAsset.Type == type)
            {
                return card.gameObject;
            }
        }
        return null;
    }
    override public void StartEnemyTurn()//敌人的主要阶段
    {
        Debug.Log(_currentPhase);
        hasAttackedThisTurn = false;
        StartCoroutine(PerformAIActions());
        //实现敌方逻辑
        //测试时的暂定逻辑：攻击牌会马上使用，没有攻击牌时优先进行蓄能，蓄能会选择当前蓄能值更高的武器
        //武器达到1级蓄能则马上武器攻击，在本回合使用过武器攻击时，才会考虑使用非攻击牌，
        //一回合进行三次行动
        //不会进行翻滚对应
    }
    override public IEnumerator EnemyAcc(GameObject Card)//敌方蓄能函数
                                                         //在两把武器蓄能都为0，或者相同时取随机数（乐）
                                                         //当两把武器蓄能不同时，优先对蓄能等级高的武器进行蓄能。
    {
        Text targetWeapon = EnemyManager.Weapon1Acc;
        if (int.Parse(EnemyManager.Weapon1Acc.text)
            == int.Parse(EnemyManager.Weapon2Acc.text))
        {
            int Num = UnityEngine.Random.Range(1, 3);//生成1到2之间的随机整数
            if (Num == 1)
                targetWeapon = targetWeapon = EnemyManager.Weapon1Acc;
            else if (Num == 2)
                targetWeapon = targetWeapon = EnemyManager.Weapon2Acc;
        }
        else
        {
            if (int.Parse(EnemyManager.Weapon1Acc.text)
                > int.Parse(EnemyManager.Weapon2Acc.text))
                targetWeapon = targetWeapon = EnemyManager.Weapon1Acc;
            else
                targetWeapon = targetWeapon = EnemyManager.Weapon2Acc;
        }

        // 移动卡牌到武器位置的动画
        Vector3 weaponPosition = targetWeapon.transform.position;
        yield return StartCoroutine(MoveCardToPosition(Card.gameObject, weaponPosition));
        EnemyManager.BS.accEvent.RaiseEvent();
        // 增加蓄能值
        if (targetWeapon == EnemyManager.Weapon1Acc)
        {
            Enemy.Weapon1Acc++;
        }
        else
        {
            Enemy.Weapon2Acc++;
        }

        // 销毁卡牌或隐藏
        Destroy(Card);

        Behavior--;
        yield return null;
    }
}
