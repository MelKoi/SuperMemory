using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Runtime.CompilerServices;

public class NoONE : BattleManager
{

    private int Behavior = 3;//行动次数
    private int acc = 0;// 蓄能临时储存
    private bool hasAttackedThisTurn = false;
    private IEnumerator PerformAIActions()//总体的敌人AI
    {
        RectTransform ZeroPosition = ZeroPoint.GetComponent<RectTransform>();//获取战斗场地中点
        while (Behavior > 0)//3次行动，0，1，2
        {
            //获取手牌
            //检索牌库内的手牌
            List<Transform> EnemyCards = new List<Transform>();
            Transform parent = EnemyManager.HandArea.transform;
            if (parent.childCount == 0)
            {
                Behavior--;//?
                continue;
            }
            for (int i = 0; i < parent.childCount; i++)
            {
                EnemyCards.Add(parent.GetChild(i));
            }
            //优先使用攻击牌
           GameObject attackCard = FindCardOfType(Type.攻击, EnemyCards);
            if (attackCard != null)
            {
                yield return StartCoroutine(UseCardWithAnimation(attackCard));
                continue;
            }

            // 2. 检查武器是否可以攻击
            if (TryWeaponAttack(out bool didAttack))
            {
                if (didAttack) yield return new WaitForSeconds(0.5f); // 攻击动画等待
                continue;
            }

            // 3. 如果有武器攻击过，考虑使用非攻击牌
            if (hasAttackedThisTurn)
            {
                GameObject nonAttackCard = FindNonAttackCard(EnemyCards);
                if (nonAttackCard != null)
                {
                    yield return StartCoroutine(UseCardWithAnimation(nonAttackCard));
                    continue;
                }
            }

            // 4. 默认行为：增加蓄能
            yield return StartCoroutine(EnemyAcc(parent.GetChild(0).gameObject));
        }

        // 三次行动结束后调用回合结束
        EndTurn();
    }

    private IEnumerator UseCardWithAnimation(GameObject card)
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
    private bool TryWeaponAttack(out bool didAttack)//武器攻击
    {
        didAttack = false;

        if (Enemy.Weapon1 == false)
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
                if (this.Purple)//如果对方已经使用过对应牌
                {
                    foreach (var effect in CounterEffect)
                    {
                        effect.ApplyEffect(this, EnemyManager);
                    }
                    Purple.SetActive(false);
                }
                Player.hp = Player.hp - Enemy.Damage;
                Debug.Log("对我方造成" + Enemy.Damage + "点伤害！");
                Enemy.mp = Enemy.mp + acc;
                acc = Enemy.Weapon1Acc = 0;
                Enemy.Damage = 0;
                Enemy.Weapon1 = true;
                hasAttackedThisTurn = true;
                Behavior--;
                didAttack = true;
                return true;
            }

        }
        if (Enemy.Weapon2 == false)
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
                if (this.Purple)//如果对方已经使用过对应牌
                {
                    foreach (var effect in CounterEffect)
                    {
                        effect.ApplyEffect(this, EnemyManager);
                    }
                }
                Player.hp = Player.hp - Enemy.Damage;
                Debug.Log("对我方造成" + Enemy.Damage + "点伤害！");
                Enemy.mp = Enemy.mp + acc;
                acc = Enemy.Weapon2Acc = 0;
                Enemy.Damage = 0;
                Enemy.Weapon2 = true;
                hasAttackedThisTurn = true;
                Behavior--;
                didAttack = true;
                return true;
            }
        }
        return false;
    }
    private GameObject FindNonAttackCard(List<Transform> EnemyCards)//寻找非攻击牌
    {
        foreach (Transform card in EnemyCards)
        {
            CardAsset cardAsset = card.gameObject.GetComponent<OneCardManager>().cardAsset;
            if (cardAsset.Type != Type.攻击)
            {
                return card.gameObject;
            }
        }
        return null;
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
        Behavior = 3;
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
            int Num = Random.Range(1, 3);//生成1到2之间的随机整数
            if (Num == 1)
                targetWeapon = targetWeapon = EnemyManager.Weapon1Acc;
            else if (Num == 2)
                targetWeapon = targetWeapon = EnemyManager.Weapon2Acc;
        }
        else
        {
            if(int.Parse(EnemyManager.Weapon1Acc.text) 
                > int.Parse(EnemyManager.Weapon2Acc.text))
                targetWeapon = targetWeapon = EnemyManager.Weapon1Acc;
            else
                targetWeapon = targetWeapon = EnemyManager.Weapon2Acc;
        }

        // 移动卡牌到武器位置的动画
        Vector3 weaponPosition = targetWeapon.transform.position;
        yield return StartCoroutine(MoveCardToPosition(Card.gameObject, weaponPosition));
        // 增加蓄能值
        if(targetWeapon == EnemyManager.Weapon1Acc)
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
