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
            Transform Area = bm.SkillArea;
            if (Card == null)
                Debug.Log("Card");
            if (Area == null)
                Debug.Log("Area");
            SkillCardChange(Card, Area);
        }
       else if (bm._currentPhase == GamePhase.enemyAction)
        {
            Debug.Log("�з�ս�����Ѿ�ʹ�ã�");
            Transform Area = em.SkillArea;
            // ���� Area �еĵ�һ��������
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
        // ����ԭʼ Card
        if (Card != null)
        {
            Destroy(Card);
        }
    }
    public interface ICardEffect
    {
        void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user);
    }
    //���ԼӼ�Ч��
    public class AttributeModifierEffect : ICardEffect
    {
        public enum AttributeType { Hp, Sp, Mp }
        public AttributeType TargetAttribute;
        public int ModifierValue;

        public void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user)
        {
            switch (TargetAttribute)
            {
                case AttributeType.Hp:
                    user.hp += ModifierValue;
                    break;
                case AttributeType.Sp:
                    user.NowSp += ModifierValue;
                    break;
                case AttributeType.Mp:
                    user.mp += ModifierValue;
                    break;
            }

            // ����UI
            if (user == battleManager.Player)
            {
                battleManager.UpdateUI(battleManager.HpText, battleManager.MpText, battleManager.SpText,
                    battleManager.Weapon1Acc, battleManager.Weapon2Acc, user);
            }
            else
            {
                battleManager.UpdateUI(enemyManager.HpText, enemyManager.MpText, enemyManager.SpText,
                    enemyManager.Weapon1Acc, enemyManager.Weapon2Acc, user);
            }
        }
    }
    //���ٿ�������
    public class CostReductionEffect : ICardEffect
    {
        public int ReductionAmount;

        public void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user)
        {
            // ��һ��ȫ�ֱ����洢��ʱ���ļ���
            user.TemporaryCostReduction = ReductionAmount;
        }
    }
    //����ĳ���ж�
    public class DisableActionEffect : ICardEffect
    {
        public void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user)
        {
            if (battleManager._currentPhase == GamePhase.playerReady)
            {
                battleManager._currentPhase = GamePhase.playerAction;
                battleManager.EndTurn();
            }
        }
    }
    //����һ����
    public class DiscardCardEffect : ICardEffect
    {
        public void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user)
        {
            // �������һ������
            if (user == battleManager.Player)
            {
                if (battleManager.HandArea.transform.childCount > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, battleManager.HandArea.transform.childCount);
                    GameObject cardToDiscard = battleManager.HandArea.transform.GetChild(randomIndex).gameObject;
                    Destroy(cardToDiscard);
                }
            }
            else
            {
                if (enemyManager.HandArea.transform.childCount > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, enemyManager.HandArea.transform.childCount);
                    GameObject cardToDiscard = enemyManager.HandArea.transform.GetChild(randomIndex).gameObject;
                    Destroy(cardToDiscard);
                }
            }
        }
    }
}
