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
        foreach (var effect in Card.GetComponent<OneCardManager>().cardAsset.Effects)
        {
            if (bm._currentPhase == GamePhase.playerAction)
                effect.ApplyEffect(bm, em, bm.Player);
            else if (bm._currentPhase == GamePhase.enemyAction)
                effect.ApplyEffect(bm, em, bm.Enemy);
        }
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
            Transform Area = bm.SkillArea;
            if (Card == null)
                Debug.Log("Card");
            if (Area == null)
                Debug.Log("Area");
            SkillCardChange(Card, Area);
        }
       else if (bm._currentPhase == GamePhase.enemyAction)
        {
            Debug.Log("敌方战技牌已经使用！");
            Transform Area = em.SkillArea;
            // 销毁 Area 中的第一个子物体
            SkillCardChange(Card, Area);
        }
    }
    private void SkillCardChange(GameObject Card,Transform Area)
    {

        foreach (Transform child in Area)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }

        GameObject Skill = Instantiate(Card, Area.position, Quaternion.identity, Area);
        Skill.GetComponent<CardDragContral>().canDrug = false;
        Skill.transform.localPosition = Vector3.zero;
        Skill.transform.localScale =new Vector3(0.75f,0.75f,1f);
        // 销毁原始 Card
        if (Card != null)
        {
            Destroy(Card);
        }
    }
}