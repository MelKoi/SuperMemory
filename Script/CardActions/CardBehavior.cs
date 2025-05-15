using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        foreach (var effect in Card.GetComponent<OneCardManager>().cardAsset.Effects)
        {
            if (bm._currentPhase == GamePhase.playerAction)
                bm.AttackEffect.Add(effect);
            else if (bm._currentPhase == GamePhase.enemyAction)
                em.AttackEffect.Add(effect);
        }
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
            effect.ApplyEffect(bm, em,false);
        }
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
        if(bm._currentPhase == GamePhase.playerAction)
            if(bm.Purple.GetComponent<Image>().sprite == bm.PClose)
            {
                bm.Purple.GetComponent<Image>().sprite = bm.POpen;
                foreach(var effect in Card.GetComponent<OneCardManager>().cardAsset.Effects)
                    bm.CounterEffect.Add(effect);
            }
            else
            {
                bm.CounterEffect.Clear();
                foreach (var effect in Card.GetComponent<OneCardManager>().cardAsset.Effects)
                    bm.CounterEffect.Add(effect);
            }
        else if(bm._currentPhase == GamePhase.enemyAction)
            if(em.Purple.GetComponent<Image>().sprite == bm.PClose)
            {
                em.Purple.GetComponent<Image>().sprite = bm.PClose;
                foreach (var effect in Card.GetComponent<OneCardManager>().cardAsset.Effects)
                    em.CounterEffect.Add(effect);
            }
            else
            {
                em.CounterEffect.Clear();
                foreach (var effect in Card.GetComponent<OneCardManager>().cardAsset.Effects)
                    em.CounterEffect.Add(effect);
            }

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
            bm.SkillEffect.Clear();
            foreach(var effect in Card.GetComponent<OneCardManager>().cardAsset.Effects)
                bm.SkillEffect.Add(effect);
        }
       else if (bm._currentPhase == GamePhase.enemyAction)
        {
            Debug.Log("敌方战技牌已经使用！");
            Transform Area = em.SkillArea;
            // 销毁 Area 中的第一个子物体
            SkillCardChange(Card, Area);
            foreach (var effect in Card.GetComponent<OneCardManager>().cardAsset.Effects)
                em.SkillEffect.Add(effect);
        }
    }
    private void SkillCardChange(GameObject Card,Transform Area)
    {

        foreach (Transform child in Area)
        {
            if (child != null)
            {   //希望能先将卡牌溶解掉再销毁
                child.GetComponent<CardAnimationController>().isGreenCard =false;
                Destroy(child.gameObject);
            }
        }
        //测试将原来的牌移动到战技区域
        //Card.transform.SetParent(Area,true);
        //Card.GetComponent<CardDragContral>().canDrug = false;
        //Card.transform.localPosition = Vector3.zero;
        //Card.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
        Card.GetComponent<CardAnimationController>().useFinish = true;
        Card.GetComponent<CardAnimationController>().isGreenCard = true;
        CardAsset asset = Card.GetComponent<OneCardManager>().cardAsset;
        GameObject Skill = Instantiate(Card, Area.position, Quaternion.identity, Area);
        OneCardManager card = Skill.GetComponent<OneCardManager>();
        CardShaderController cardShaderController = Skill.GetComponent<CardShaderController>();
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
        Skill.GetComponent<CardDragContral>().canDrug = false;
        Skill.transform.localPosition = Vector3.zero;
        Skill.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
    }
}