using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GreenHand : BattleManager
{
    //public GameObject IamgeAndText;
    //public Image TalkImage;
    //public Text TalkText;
    private int acc = 0;// ������ʱ����
    private bool hasAttackedThisTurn = false;
    private int TurnNum = 0;//�غ���
    // Start is called before the first frame update

    private IEnumerator PerformAIActions()//����ĵ���AI
    {
        RectTransform ZeroPosition = ZeroPoint.GetComponent<RectTransform>();//��ȡս�������е�
        List<Transform> EnemyCards = new List<Transform>();
        Transform parent = EnemyManager.HandArea.transform;
        TurnNum++;
        switch (TurnNum)
        {
            case 1:
                for (int i = 0; i < parent.childCount; i++)
                {
                    EnemyCards.Add(parent.GetChild(i));
                }
                yield return StartCoroutine(UseCardWithAnimation(EnemyCards[0].gameObject));

                yield return new WaitForSeconds(0.8f);

                for (int i = 0; i < 3; i++)
                    yield return StartCoroutine(EnemyAcc(parent.GetChild(0).gameObject));
                break;
            case 2:
                if (TryWeaponAttack(out bool didAttack))
                {
                    if (didAttack) yield return new WaitForSeconds(5f); // ���������ȴ�
                }
                break;
            case 3:
                EnemyUseSkill();
                break;
            case 4:
                Enemy.hp = 0;
                break;
        }
        EndTurn();
           
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

    }
    public void GreenHandTeach()
    {

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
    override public void StartEnemyTurn()//���˵���Ҫ�׶�
    {
        Debug.Log(_currentPhase);
        hasAttackedThisTurn = false;
        StartCoroutine(PerformAIActions());
    }
    override public IEnumerator EnemyAcc(GameObject Card)//�з����ܺ���
    {
        //�з����ܺ���
        //�������������ܶ�Ϊ0��������ͬʱȡ��������֣�
        //�������������ܲ�ͬʱ�����ȶ����ܵȼ��ߵ������������ܡ�
        {
            Text targetWeapon = EnemyManager.Weapon1Acc;
            if (int.Parse(EnemyManager.Weapon1Acc.text)
                == int.Parse(EnemyManager.Weapon2Acc.text))
            {
                int Num = Random.Range(1, 3);//����1��2֮����������
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

            yield return null;
        }
    }
    private IEnumerator UseCardWithAnimation(GameObject card)
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


    }
}
