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
        Debug.Log("�����Ƶ�Ч������������һ�ι�����");
        Destroy(Card);//���ٿ���
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
        Debug.Log("���ж����Ѿ�ʹ�ã�");
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
            effectName = "�ӳ�Ч���Ѿ�������"
        };

        //��Ч����ӵ�������
        CardEffectManager.Instance.AddDelayedEffect(effect);

        //���ٿ���
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
            Debug.Log("���ս�����Ѿ�ʹ�ã�");
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
            Debug.Log("�з�ս�����Ѿ�ʹ�ã�");
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
