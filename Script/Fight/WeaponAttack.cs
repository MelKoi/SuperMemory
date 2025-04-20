using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponAttack : MonoBehaviour, IPointerClickHandler
{
    public BattleManager battleManager;
    public float doubleClickThreshold = 0.3f;//双击时间阈值(s)
    public float cooldown = 1f;//冷却时间
    private bool isCooldown = false;//是否在冷却

    public bool isAttacted = false;
    private float lastClickTime; // 上次点击时间
    public PlayerAsset Enemy;
    public PlayerAsset Player;
    public int Acc;//蓄能
    public WeaponAsset Weapon;//武器

    [SerializeField]
    private Text Point;//提醒
    // Start is called before the first frame update
    void Start()
    {
        Weapon = gameObject.GetComponent<WeaponCardManager>().weaponAsset;
        battleManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<BattleManager>();
        Enemy = battleManager.Enemy;
        Player = battleManager.Player;
        GameObject PointObject = battleManager.transform.Find("Place/Point").gameObject;
        Point = PointObject.GetComponent<Text>();
    }
    // Update is called once per frame
    void Update()
    {

    }
    // 实现 IPointerClickHandler 接口（适用于UI元素）
    public void OnPointerClick(PointerEventData eventData)
    {
        // 检测是否为双击
        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            // 触发攻击逻辑
            AttackEnemy(Enemy, Player);
                
        }
        lastClickTime = Time.time;
    }
    // 攻击敌方
    private void AttackEnemy(PlayerAsset attacked,PlayerAsset attack)
    {
        BattleManager battleManager = transform.parent.parent.parent.GetComponent<BattleManager>();
        EnemyManager enemyManager = transform.parent.parent.parent.GetComponent<EnemyManager>();
        if (transform.parent.name.Equals("WeaponCard1"))
        {
            if(attack.Weapon1)
            {
                StartCoroutine(Pointed());
                return;
            }
            Acc = attack.Weapon1Acc;
        }
        else
        {
            if (attack.Weapon2)
            {
                StartCoroutine(Pointed());
                return;
            }
            Acc = attack.Weapon2Acc;
        }
        if (attacked.hp != 0)
        {
            foreach (var damage in Weapon.Accumulation)
            {
                if (Acc >= damage.Acc)
                {
                    Player.Damage = damage.Value;
                }
            }
            if (Player.Damage == 0)
                return;
            battleManager.BS.attEvent.RaiseEvent();
            if (!battleManager.BS.AttTouch)
                return;
            else
            {
                if (enemyManager.Purple.activeSelf == true)//如果对方已经使用过对应牌
                {
                    foreach (var effect in enemyManager.CounterEffect)//调用对应牌的效果
                    {
                        effect.ApplyEffect(battleManager, enemyManager);
                    }
                    enemyManager.Purple.gameObject.SetActive(false);
                }
                attacked.hp = attacked.hp - Player.Damage;
                Debug.Log($"使用 {gameObject.name} 对敌方造成" + Player.Damage + "点伤害！");
                foreach (var effect in battleManager.AttackEffect)
                {
                    effect.ApplyEffect(battleManager, enemyManager);
                }
                attack.mp = attack.mp + Acc;
                Acc = 0;
                Player.Damage = 0;
                if (transform.parent.name.Equals("WeaponCard1"))
                {
                    attack.Weapon1Acc = Acc;
                    attack.Weapon1 = true;
                }
                else
                {
                    attack.Weapon2Acc = Acc;
                    attack.Weapon2 = true;
                }
                StartCoroutine(CooldownTimer());
            }
        }
    }
    private IEnumerator CooldownTimer()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isCooldown = false;
    }
    private IEnumerator Pointed()
    {
        Point.gameObject.SetActive(true);
        Point.text = "此武器本回合已经用于攻击";
        yield return new WaitForSeconds(0.8f);
        Point.gameObject.SetActive(false);
    }
}
