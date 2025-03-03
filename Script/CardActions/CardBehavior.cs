using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public abstract class CardBehaviour : MonoBehaviour
{
    public abstract void Onplay(BattleManager bm,EnemyManager em,GameObject Card);
}
public class AttackCardBehaviour : CardBehaviour
{
    private CardAsset _card;
    public AttackCardBehaviour(CardAsset card)
    {
        _card = card;
    }
    public override void Onplay(BattleManager bm, EnemyManager em, GameObject Card)
    {
        Debug.Log("攻击牌的效果将作用于下一次攻击！");
        Destroy(Card);//销毁卡牌
    }
}
public class ActionCardBehaviour : CardBehaviour
{
    private CardAsset _card;
    public ActionCardBehaviour(CardAsset card)
    {
        _card = card;
    }
    public override void Onplay(BattleManager bm, EnemyManager em, GameObject Card)
    {
        Debug.Log("该行动牌已经使用！");
        Destroy(Card);
    }
}
public class CounterCardBehaviour : CardBehaviour
{
    private CardAsset _card;
    public CounterCardBehaviour(CardAsset card)
    {
        _card = card;
    }
    public override void Onplay(BattleManager bm, EnemyManager em, GameObject Card)
    {
        bm.Purple.SetActive(true);
        DelayedCardEffect effect = new DelayedCardEffect
        {
            effectName = "延迟效果已经发动！"
        };

        //将效果添加到管理器
        CardEffectManager.Instance.AddDelayedEffect(effect);

        //销毁卡牌
        Destroy(Card);
    }
}
public class SkillCardBehaviour : CardBehaviour
{
    private CardAsset _card;
    public SkillCardBehaviour(CardAsset card)
    {
        _card = card;
    }
    public override void Onplay(BattleManager bm, EnemyManager em, GameObject Card)
    {
       if(bm._currentPhase == GamePhase.playerAction)
        {
            Debug.Log("玩家战技牌已经使用！");
            Transform Area = bm.gameObject.transform.Find("Place/GreenCard");
            if(Area.childCount == 0)
            {
                SkillCardChange(Card, Area);
            }
            else
            {
                Destroy(Area.GetChild(0));
                SkillCardChange(Card, Area);
            }
        }
       else if (bm._currentPhase == GamePhase.enemyAction)
        {
            Debug.Log("敌方战技牌已经使用！");
            Transform Area = bm.gameObject.transform.Find("Place/Enemy/GreenCard");
            if (Area.childCount == 0)
            {
                SkillCardChange(Card, Area);
            }
            else
            {
                Destroy(Area.GetChild(0));
                SkillCardChange(Card, Area);
            }
        }
    }
    public void SkillCardChange(GameObject Card,Transform Area)
    {
        GameObject Skill = Instantiate(Card, Area.position, Quaternion.identity, Area);
        Skill.GetComponent<CardDragContral>().canDrug = false;
        Skill.transform.localPosition = Vector3.zero;
        Skill.transform.localScale = Vector3.one;
        Destroy(Card);
    }
}
