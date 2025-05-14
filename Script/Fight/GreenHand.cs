using DG.Tweening;
using DialogueEditor;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GreenHand : BattleManager
{
    //public GameObject IamgeAndText;
    //public Image TalkImage;
    //public Text TalkText;
    private int acc = 0;// 蓄能临时储存
    private bool hasAttackedThisTurn = false;
    private int TurnNum = 0;//回合数：0,1,2分别进行不同的教程；1，2，3则用于控制敌方的基本行动

    [Header("新手教程组件")]
    [SerializeField] private GameObject GreenHandPanel;//教程面板
    public ConversationManager ConversationManager;//对话管理组件
    public int currentDialog = 1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            StartCoroutine(PlayerHide());
        }
        switch (_currentPhase)
        {
            case GamePhase.playerReady:
                PlayerReady();
                if(TurnNum == 0 || TurnNum == 1 || TurnNum == 2)
                {
                    GreenHandPanel.SetActive(true);
                    StartDialog();
                }
                    
                break;
            case GamePhase.enemyReady:
                EnemyReady();
                break;
            case GamePhase.enemyAction:
                if (!hasEnemyTurnStarted)
                {
                    int cardsNum = HandArea.transform.childCount;
                    for (int i = 0; i < cardsNum; i++)
                        HandArea.transform.GetChild(i).GetComponent<CardDragContral>().canDrug = false;
                    StartEnemyTurn();
                    hasEnemyTurnStarted = true;
                }
                break;
            case GamePhase.gameEnd:
                Debug.Log("对局结束");
                GameOver.SetActive(true);
                //这里放回到对话的方法或者函数
                return;
        }

        if (Player.Weapon1Acc != int.Parse(Weapon1Acc.text)
            || Player.Weapon2Acc != int.Parse(Weapon2Acc.text)
            || Player.hp != int.Parse(HpText.text)
            || Player.NowSp != int.Parse(SpText.text)
            || Player.mp != int.Parse(MpText.text))
        {
            UpdateUI(HpText, MpText, SpText, Weapon1Acc, Weapon2Acc, Player);
        }
        if (Enemy.Weapon1Acc != int.Parse(EnemyManager.Weapon1Acc.text)
            || Enemy.Weapon2Acc != int.Parse(EnemyManager.Weapon2Acc.text)
            || Enemy.hp != int.Parse(EnemyManager.HpText.text)
            || Enemy.NowSp != int.Parse(EnemyManager.SpText.text)
            || Enemy.mp != int.Parse(EnemyManager.MpText.text))
        {
            UpdateUI(EnemyManager.HpText, EnemyManager.MpText, EnemyManager.SpText, EnemyManager.Weapon1Acc,
            EnemyManager.Weapon2Acc, Enemy);
        }
        if (Player.hp <= 0 || Enemy.hp <= 0)
        {
            _currentPhase = GamePhase.gameEnd;
        }
    }
    private IEnumerator PerformAIActions()//总体的敌人AI
    {
        RectTransform ZeroPosition = ZeroPoint.GetComponent<RectTransform>();//获取战斗场地中点
        List<Transform> EnemyCards = new List<Transform>();
        Transform parent = EnemyManager.HandArea.transform;
        TurnNum++;
        switch (TurnNum)
        {
            case 1://第一个回合，敌人会出牌和蓄能
                for (int i = 0; i < parent.childCount; i++)
                {
                    EnemyCards.Add(parent.GetChild(i));
                }
                yield return StartCoroutine(UseCardWithAnimation(EnemyCards[0].gameObject));

                yield return new WaitForSeconds(0.8f);

                for (int i = 0; i < 3; i++)
                    yield return StartCoroutine(EnemyAcc(parent.GetChild(0).gameObject));
                break;
            case 2://敌人会相应进行攻击
                if (TryWeaponAttack(out bool didAttack))
                {
                    if (didAttack) yield return new WaitForSeconds(5f); // 攻击动画等待
                }
                break;
            case 3://敌人会使用技能
                EnemyUseSkill();
                break;
            case 4://弹窗，敌人认输
                Enemy.hp = 0;
                break;
        }
        EndTurn();
           
    }
    public void StartDialog()
    {
        StartCoroutine(StartConversation());
    }
    IEnumerator StartConversation()
    {
        yield return null;

        string dialogNum = "GreenPart" + currentDialog.ToString();
        NPCConversation nPCConversation = GameObject.Find(dialogNum).GetComponent<NPCConversation>();
        ConversationManager.StartConversation(nPCConversation);
        currentDialog++;
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
    override public void StartEnemyTurn()//敌人的主要阶段
    {
        Debug.Log(_currentPhase);
        hasAttackedThisTurn = false;
        StartCoroutine(PerformAIActions());
    }
    override public IEnumerator EnemyAcc(GameObject Card)//敌方蓄能函数
    {
        //敌方蓄能函数
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
                if (int.Parse(EnemyManager.Weapon1Acc.text)
                    > int.Parse(EnemyManager.Weapon2Acc.text))
                    targetWeapon = targetWeapon = EnemyManager.Weapon1Acc;
                else
                    targetWeapon = targetWeapon = EnemyManager.Weapon2Acc;
            }

            // 移动卡牌到武器位置的动画
            Vector3 weaponPosition = new Vector3(targetWeapon.transform.position.x, targetWeapon.transform.position.y, Card.transform.position.z);
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

            yield return null;
        }
    }
    private IEnumerator UseCardWithAnimation(GameObject card)
    {
        card.GetComponent<CardPreview>().CanPreview = false;
        // 移动卡牌到屏幕中央的动画
        // 隐藏卡牌背面
        
        card.GetComponent<OneCardManager>().CardBack.gameObject.SetActive(false);
            

        card.transform.DOMove(new Vector3(960, 540, card.transform.position.z), 2.0f)
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


    }
    
}
