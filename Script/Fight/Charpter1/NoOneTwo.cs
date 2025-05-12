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
    [Header("Ȩ�����ݼ�")]
    private int Hide = 0;//����������
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

        

    [Header("˫�������ϻغ�����")]
    private int PlayerLastHP;
    private int PlayerLastMP;
    private int EnemyLastHP;

    [Header("��ʱ����")]
    private int acc = 0;//������ʱ����
    private bool hasAttackedThisTurn = false;//���غ��Ƿ���й�����,���ڻغ����ж��ж�ģʽ
    private bool FirstEnemyTurn = true;

    private void Update()
    {
        if(hasAttackedThisTurn = false && (Enemy.Weapon1Acc >= 4 || Enemy.Weapon2Acc >= 4))//����Ƿ���Ҫ�ı�Ȩ��,����ͬ��
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
    private IEnumerator PerformAIActions()//����ĵ���AI
    {
        RectTransform ZeroPosition = ZeroPoint.GetComponent<RectTransform>();//��ȡս�������е�
        if(FirstEnemyTurn)//��һ�غϻ�ȡ˫���Ļ�������
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

        foreach (var action in sortedActions)//���÷��������Ϊ�߼�
        {
            if(Behavior == 0)
            {
                break;
            }
            //��ȡ����
            //�����ƿ��ڵ�����
            List<Transform> EnemyCards = new List<Transform>();
            Transform parent = EnemyManager.HandArea.transform;
            if (parent.childCount != 0)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    EnemyCards.Add(parent.GetChild(i));
                }
            }

            actionMethods = new Dictionary<string, Func<IEnumerator>>//��ȡ����
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
                //ʹ�ù����ƺ��ж���
                GameObject AAcard = FindCardOfType(Type.����, EnemyCards);
                if (AAcard == null) AAcard = FindCardOfType(Type.�ж�, EnemyCards);

                if (AAcard != null && (Enemy.NowSp - int.Parse(AAcard.GetComponent<OneCardManager>().cardAsset.cost) >= 0))
                {
                    yield return StartCoroutine(UseCardWithAnimation(AAcard));
                    Behavior--;
                }

            }
            IEnumerator UseCardCS()
            {
                GameObject CScard = FindCardOfType(Type.��Ӧ, EnemyCards);
                if (CScard == null) CScard = FindCardOfType(Type.ս��, EnemyCards);

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
                    if (didAttack) yield return new WaitForSeconds(5f); // ���������ȴ�
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
                        Debug.Log("�����Ѿ�ʹ��");
                        EnemyManager.EnemySkillIsUsed = !EnemyManager.EnemySkillIsUsed;
                        Behavior--;
                    }
                    else
                        Debug.Log("��������");
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
                return true;// ʵ���������
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
    private IEnumerator UseCardWithAnimation(GameObject card)//�з�ʹ�ÿ���
    {
        card.GetComponent<CardPreview>().CanPreview = false;
        // �ƶ����Ƶ���Ļ����Ķ���
        // ���ؿ��Ʊ���
        card.transform.DORotate(new Vector3(0, 90, 0), 1.0f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                card.GetComponent<OneCardManager>().CardBack.gameObject.SetActive(false);
                card.transform.DORotate(new Vector3(0, 180, 0), 1.0f);
            });

        card.transform.DOMove(new Vector3(960, 540, 0), 2.0f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                // ������ɺ�ִ��
                if (card != null)
                {
                    // ʹ�ÿ���
                    CardAsset cardAsset = card.GetComponent<OneCardManager>().cardAsset;
                    card.GetComponent<CardPreview>().CanPreview = true;
                    UseCard(cardAsset, card, Enemy);

                    Destroy(card);
                }
            });

        // �ȴ��������
        yield return new WaitForSeconds(2.0f);

        Behavior--;
    }
    private IEnumerator MoveCardToPosition(GameObject card, Vector3 targetPosition)
    {
        if (card == null) yield break; // ��ǰ���

        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPosition = card.transform.position;

        while (elapsed < duration && card != null)
        {
            card.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // ����λ������ǰ�ٴμ��
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
                didAttack = true; // ��ʾ�ѿ�ʼ����
                return true; // ��ʾ�����˹���
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
                didAttack = true; // ��ʾ�ѿ�ʼ����
                return true; // ��ʾ�����˹���
            }
        }

        return false;
    }
    // ����Э�̷�������ʵ�ʹ����߼�
    private IEnumerator PerformWeaponAttack(bool isWeapon1)
    {
        // ���������¼�
        EnemyManager.BS.attEvent.RaiseEvent();

        // �ȴ����������ж�
        float timeout = 3f; // ��ʱʱ��
        float elapsed = 0f;

        while (!EnemyManager.BS.bulletController.hasHit && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!EnemyManager.BS.bulletController.hasHit)
        {
            Debug.LogWarning("����δ���л�ʱ");
            Enemy.mp += acc;//���������߼�
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

        // ִ�й������к���߼�
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
        Debug.Log($"���ҷ����{Enemy.Damage}���˺���");
        Enemy.mp += acc;//���������߼�
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
    private GameObject FindCardOfType(Type type, List<Transform> EnemyCards)//Ѱ����Ӧ�Ŀ���
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
    override public void StartEnemyTurn()//���˵���Ҫ�׶�
    {
        Debug.Log(_currentPhase);
        hasAttackedThisTurn = false;
        StartCoroutine(PerformAIActions());
        //ʵ�ֵз��߼�
        //����ʱ���ݶ��߼��������ƻ�����ʹ�ã�û�й�����ʱ���Ƚ������ܣ����ܻ�ѡ��ǰ����ֵ���ߵ�����
        //�����ﵽ1�����������������������ڱ��غ�ʹ�ù���������ʱ���Żῼ��ʹ�÷ǹ����ƣ�
        //һ�غϽ��������ж�
        //������з�����Ӧ
    }
    override public IEnumerator EnemyAcc(GameObject Card)//�з����ܺ���
                                                         //�������������ܶ�Ϊ0��������ͬʱȡ��������֣�
                                                         //�������������ܲ�ͬʱ�����ȶ����ܵȼ��ߵ������������ܡ�
    {
        Text targetWeapon = EnemyManager.Weapon1Acc;
        if (int.Parse(EnemyManager.Weapon1Acc.text)
            == int.Parse(EnemyManager.Weapon2Acc.text))
        {
            int Num = UnityEngine.Random.Range(1, 3);//����1��2֮����������
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

        // �ƶ����Ƶ�����λ�õĶ���
        Vector3 weaponPosition = targetWeapon.transform.position;
        yield return StartCoroutine(MoveCardToPosition(Card.gameObject, weaponPosition));
        EnemyManager.BS.accEvent.RaiseEvent();
        // ��������ֵ
        if (targetWeapon == EnemyManager.Weapon1Acc)
        {
            Enemy.Weapon1Acc++;
        }
        else
        {
            Enemy.Weapon2Acc++;
        }

        // ���ٿ��ƻ�����
        Destroy(Card);

        Behavior--;
        yield return null;
    }
}
