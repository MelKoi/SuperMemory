using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BanWeaponChoose : MonoBehaviour
{
    public bool Ban;

    public Button W1;
    public Button W2;
    public Button W3;

    public BattleManager bs;
    



    private void Start()
    {
        bs.CentreButton = W1;
    }
    public void ChangeWeaponLast()
    {
        Vector3 temp;
        switch (bs.CentreButton.name)
        {
            case "W1":
                temp = W1.transform.position;
                if(Ban)
                {
                    W1.transform.position = W2.transform.position;
                    W2.transform.position = W3.transform.position;
                    W3.transform.position = temp;
                    bs.CentreButton = W3;
                    bs.BanDescription.text = bs.EnemyManager._PlayerAllWeapons[2].description;
                }
                else
                {
                    W1.transform.position = W2.transform.position;
                    W2.transform.position = temp;
                    bs.CentreButton = W2;
                    bs.FirstDescription.text = bs.PlayerWeapons[1].description;
                }
                    break;
            case "W2":
                temp = W2.transform.position;
                if(Ban)
                {
                    W2.transform.position = W3.transform.position;
                    W3.transform.position = W1.transform.position;
                    W1.transform.position = temp;
                    bs.CentreButton = W1;
                    bs.BanDescription.text = bs.EnemyManager._PlayerAllWeapons[0].description;
                }
                else
                {
                    W2.transform.position = W1.transform.position;
                    W1.transform.position = temp;
                    bs.CentreButton = W1;
                    bs.FirstDescription.text = bs.PlayerWeapons[0].description;
                }
                break;
            case "W3":
                temp = W3.transform.position;
                W3.transform.position = W1.transform.position;
                W1.transform.position = W2.transform.position;
                W2.transform.position = temp;
                bs.CentreButton = W2;
                bs.BanDescription.text = bs.EnemyManager._PlayerAllWeapons[1].description;
                break;
        }
    }
    public void ChangeWeaponNext()
    {
        Vector3 temp;

        switch (bs.CentreButton.name)
        {
            case "W1":
                temp = W1.transform.position;
                if(Ban)
                {
                    W1.transform.position = W3.transform.position;
                    W3.transform.position = W2.transform.position;
                    W2.transform.position = temp;
                    bs.CentreButton = W2;
                    bs.BanDescription.text = bs.EnemyManager._PlayerAllWeapons[1].description;
                }
                else
                {
                    W1.transform.position = W2.transform.position;
                    W2.transform.position = temp;
                    bs.CentreButton = W2;
                    bs.FirstDescription.text = bs.PlayerWeapons[1].description;
                }
                    break;
            case "W2":
                temp = W2.transform.position;
                if (Ban)
                {
                    W2.transform.position = W1.transform.position;
                    W1.transform.position = W3.transform.position;
                    W3.transform.position = temp;
                    bs.CentreButton = W3;
                    bs.BanDescription.text = bs.EnemyManager._PlayerAllWeapons[2].description;
                }
                else
                {
                    W2.transform.position = W1.transform.position;
                    W1.transform.position = temp;
                    bs.CentreButton = W1;
                    bs.FirstDescription.text = bs.PlayerWeapons[0].description;
                }
                    
                    break;
            case "W3":
                temp = W3.transform.position;
                W3.transform.position = W2.transform.position;
                W2.transform.position = W1.transform.position;
                W1.transform.position = temp;
                bs.CentreButton = W1;
                bs.BanDescription.text = bs.EnemyManager._PlayerAllWeapons[0].description;
                break;
        }
    }
}
