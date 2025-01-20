using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstHandManager : MonoBehaviour
{
    public BattleManager bm;
    public EnemyManager em;
    public int TurnNum = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {

    }
    public void BanPlayerWeapon()
    {
        for (int i = 0; i < 3; i++)//查找到玩家选项中对应的武器
        {
            if (!bm.ChooseWeapon[i].WeaponName.Equals("秉烛"))
            {
                if (bm.Weapon1 == null) bm.Weapon1 = bm.ChooseWeapon[i];
                else if (bm.Weapon1 != null) bm.Weapon2 = bm.ChooseWeapon[i];
            }
            else if (bm.ChooseWeapon[i].WeaponName.Equals("秉烛")) continue;
        }
        bm.ChooseFirstDrewButton[0].GetComponentInChildren<Text>().text = bm.Weapon1.WeaponName;
        bm.ChooseFirstDrewButton[1].GetComponentInChildren<Text>().text = bm.Weapon2.WeaponName;
        bm.Weapon1Object.GetComponent<WeaponCardManager>().weaponAsset = bm.Weapon1;
        bm.Weapon2Object.GetComponent<WeaponCardManager>().weaponAsset = bm.Weapon2;

    }
    public void EnemyAction()
    {
        TurnNum++;
        switch (TurnNum)
        {
            case 1: 
                em.ChooseCards.Add(em.enemyHand.transform.GetChild(0).gameObject);
                em.UseCard();
                em.ChooseCards.Add(em.enemyHand.transform.GetChild(1).gameObject);
                em.UseCard();
                bm.TurnEnd();
                break;
            case 2:

                break;

            case 3:
                break;

        }
    }
}