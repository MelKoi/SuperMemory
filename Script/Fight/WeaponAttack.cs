using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponAttack : MonoBehaviour
{
    [System.Serializable]
    public struct Syn//格挡值对应的减少关系
    {
        public int injured;//伤害阈值
        public int synHarm;//格挡值衰减
    }
    public List<Syn> syn;

    public BattleManager battleManager;
    public float cooldown = 0.5f;//冷却时间
   



    public PlayerAsset Enemy;
    public PlayerAsset Player;
    public GameObject Weapon1;//第一武器
    public GameObject Weapon2;//第二武器
    public int Acc;//蓄能
    public WeaponAsset Weapon;//武器

    [SerializeField]
    private Text Point;//提醒
    // Start is called before the first frame update
    void Start()
    {
        battleManager = GetComponent<BattleManager>();
        Enemy = battleManager.Enemy;
        Player = battleManager.Player;
        GameObject PointObject = transform.Find("Place/Point").gameObject;
        Point = PointObject.GetComponent<Text>();
    }
    // Update is called once per frame
    void Update()
    {
       if(Input.GetKeyDown(KeyCode.X))//按下X触发第一个武器攻击
        {
            Weapon = Weapon1.transform.Find("WeaponCardPrefeb(Clone)").GetComponent<WeaponCardManager>().weaponAsset;
            StartCoroutine(Weapon1AttackEnemyCoroutine(Enemy, Player)); 
        }
       else if(Input.GetKeyDown(KeyCode.C))//按下C触发第二个武器攻击
        {
            Weapon = Weapon2.transform.Find("WeaponCardPrefeb(Clone)").GetComponent<WeaponCardManager>().weaponAsset;
            StartCoroutine(Weapon2AttackEnemyCoroutine(Enemy, Player));
        }
    }
    // 攻击敌方
    public IEnumerator Weapon1AttackEnemyCoroutine(PlayerAsset attacked, PlayerAsset attack)
    {
        EnemyManager enemyManager = GetComponent<EnemyManager>();
        if (attack.Weapon1)//如果本次我方/敌方行动阶段已经攻击过
        {
            StartCoroutine(Pointed());//触发提示面板
            yield break;
        }
        Acc = attack.Weapon1Acc;//获取蓄能
           
        if (attacked.NowHp != 0 && !attack.Weapon1isCooldown)//如果敌方的HP不等于零且武器1还没有正在攻击（攻击进入冷却）
        {
            foreach (var damage in Weapon.Accumulation)
            {
                if (Acc >= damage.Acc)
                {
                    attack.Weapon1isCooldown = true;
                    attack.Damage = damage.Value;
                }
            }
            if (attack.Damage == 0)
            {
               StartCoroutine(AccLow());
                yield break;
            }
            bool hasHit;
            bool HitShilder;
            Sprite purple;
            List<CardEffectAsset> att;
            List<CardEffectAsset> coun;
            if (attack == Player)
            {
                battleManager.BS.attEvent.RaiseEvent();
                // 等待攻击命中判定
                float timeout = 1f; // 超时时间
                float elapsed = 0f;

                while (elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
               
                battleManager.LastAttWeapon = 1;//变更玩家最后攻击的武器
                hasHit = battleManager.BS.bulletController.hasHit;
                HitShilder = battleManager.BS.bulletController.HitShilder;
                purple = enemyManager.Purple.GetComponent<Image>().sprite;
                att = battleManager.AttackEffect;
                coun = battleManager.CounterEffect;

            }
            else
            {
                enemyManager.BS.attEvent.RaiseEvent();
                // 等待攻击命中判定
                float timeout = 1f; // 超时时间
                float elapsed = 0f;

                while (elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                
                hasHit = enemyManager.BS.bulletController.hasHit;
                HitShilder = enemyManager.BS.bulletController.HitShilder;
                purple = battleManager.Purple.GetComponent<Image>().sprite;
                att = enemyManager.AttackEffect;
                coun = enemyManager.CounterEffect; 
            }
            Debug.Log(hasHit);
            Debug.Log(HitShilder);
            if (HitShilder)//攻击到盾牌
            {
                Debug.LogWarning("攻击被格挡");
                attack.Damage = attack.Damage * 3 / 4;
            }
            if (attacked.Injured)
            {
                attack.Damage = attack.Damage * 5 / 4;
            }
            if (purple == battleManager.POpen && HitShilder)//如果对方已经使用过对应牌
            {
                foreach (var effect in coun)//调用对应牌的效果
                {
                    effect.ApplyEffect(battleManager, enemyManager, true);
                }
                purple = battleManager.PClose;
            }
            attack.NowMp = attack.NowMp + Acc;
            Acc = 0;
            attack.Weapon1Acc = Acc;
            attack.Weapon1 = true;
            foreach (var effect in att)
            {
                effect.ApplyEffect(battleManager, enemyManager, false);
            }
            int TemporarySyn = 0;
            foreach (var synChange in syn)//变动格挡值
            { 
                if (attack.Damage > synChange.injured)
                    TemporarySyn = synChange.synHarm;
            }
            if(attacked.NowSynchronization != 0)//如果格挡值不为零则受到格挡值损伤
            {
                attacked.NowSynchronization -= TemporarySyn;
            }
            if(attacked.NowSynchronization == 0 && !attacked.Injured)//如果格挡值归零且不为破绽模式
            {
                attacked.Injured = true;
            }
            attacked.NowHp = attacked.NowHp - attack.Damage;
            attack.NowSynchronization += (attack.MaxSynchronization * 3 / 20);
            Debug.Log($"使用 {Weapon.WeaponName} 对敌方造成" + attack.Damage + "点伤害！");
            attack.NowSynchronization = (int)(attack.NowSynchronization * 1.15f);
            attack.Damage = 0;
            yield return new WaitForSeconds(cooldown);
            attack.Weapon1isCooldown = false;
        }
    }
    private IEnumerator Weapon2AttackEnemyCoroutine(PlayerAsset attacked, PlayerAsset attack)
    {
        EnemyManager enemyManager = GetComponent<EnemyManager>();
        if (attack.Weapon2)//如果本次我方/敌方行动阶段已经攻击过
        {
            StartCoroutine(Pointed());//触发提示面板
            yield break;
        }
        Acc = attack.Weapon2Acc;//获取蓄能

        if (attacked.NowHp != 0 && !attack.Weapon2isCooldown)//如果敌方的HP不等于零且武器1还没有正在攻击（攻击进入冷却）
        {
            foreach (var damage in Weapon.Accumulation)
            {
                if (Acc >= damage.Acc)
                {
                    attack.Weapon2isCooldown = true;
                    attack.Damage = damage.Value;
                }
            }
            if (attack.Damage == 0)
            {
                StartCoroutine(AccLow());
                yield break;
            }
            bool hasHit;
            bool HitShilder;
            Sprite purple;
            List<CardEffectAsset> att;
            List<CardEffectAsset> coun;
            if (attack == Player)
            {
                battleManager.LastAttWeapon = 2;//变更玩家最后攻击的武器
                battleManager.BS.attEvent.RaiseEvent();
                hasHit = battleManager.BS.bulletController.hasHit;
                HitShilder = battleManager.BS.bulletController.HitShilder;
                purple = enemyManager.Purple.GetComponent<Image>().sprite;
                att = battleManager.AttackEffect;
                coun = battleManager.CounterEffect;
                
            }
            else
            {
                enemyManager.BS.attEvent.RaiseEvent();
                hasHit = enemyManager.BS.bulletController.hasHit;
                HitShilder = enemyManager.BS.bulletController.HitShilder;
                purple = battleManager.Purple.GetComponent<Image>().sprite;
                att = enemyManager.AttackEffect;
                coun = enemyManager.CounterEffect;
                

            }


            // 等待攻击命中判定
            float timeout = 2f; // 超时时间
            float elapsed = 0f;

            while (!hasHit && !HitShilder && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            Debug.Log(hasHit);
            Debug.Log(HitShilder);

            if (HitShilder)//攻击到盾牌
            {
                Debug.LogWarning("攻击被格挡");
                attack.Damage = attack.Damage * 3 / 4;
            }
            if (attacked.Injured)
            {
                attack.Damage = attack.Damage * 5 / 4;
            }
            if (purple == battleManager.POpen && HitShilder)//如果对方已经使用过对应牌
            {
                foreach (var effect in coun)//调用对应牌的效果
                {
                    effect.ApplyEffect(battleManager, enemyManager, true);
                }
                purple = battleManager.PClose;
            }
            attack.NowMp = attack.NowMp + Acc;
            Acc = 0;
            attack.Weapon2Acc = Acc;
            attack.Weapon2 = true;
            foreach (var effect in att)
            {
                effect.ApplyEffect(battleManager, enemyManager, false);
            }
            int TemporarySyn = 0;
            foreach (var synChange in syn)//变动格挡值
            {
                if (attack.Damage > synChange.injured)
                    TemporarySyn = synChange.synHarm;
            }
            if (attacked.NowSynchronization != 0)//如果格挡值不为零则受到格挡值损伤
            {
                attacked.NowSynchronization -= TemporarySyn;
            }
            if (attacked.NowSynchronization == 0 && !attacked.Injured)//如果格挡值归零且不为破绽模式
            {
                attacked.Injured = true;
            }
            attacked.NowHp = attacked.NowHp - attack.Damage;
            attack.NowSynchronization += (attack.MaxSynchronization * 3 / 20);
            Debug.Log($"使用 {Weapon.WeaponName} 对敌方造成" + attack.Damage + "点伤害！");
            attack.NowSynchronization = (int)(attack.NowSynchronization * 1.15f);
            attack.Damage = 0;
            yield return new WaitForSeconds(cooldown);
            attack.Weapon2isCooldown = false;
        }
    }
    private IEnumerator Pointed()
    {
        Point.gameObject.SetActive(true);
        Point.text = "此武器本回合已经用于攻击";
        yield return new WaitForSeconds(0.8f);
        Point.gameObject.SetActive(false);
    }
    private IEnumerator AccLow()
    {
        Point.gameObject.SetActive(true);
        Point.text = "武器蓄能不足";
        yield return new WaitForSeconds(0.8f);
        Point.gameObject.SetActive(false);
    }
}
