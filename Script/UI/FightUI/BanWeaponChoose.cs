using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BanWeaponChoose : MonoBehaviour
{
    public bool Ban;

    public Button W1;
    public Button W2;
    public Button W3;

    private Button CentreButton;



    private void Start()
    {
        CentreButton = W1;
    }

    public void ChangeWeaponLast()
    {
        Vector3 temp;

        switch(CentreButton.name)
        {
            case "W1":
                temp = W1.transform.position;
                if(Ban)
                {
                    W1.transform.position = W2.transform.position;
                    W2.transform.position = W3.transform.position;
                    W3.transform.position = temp;
                    CentreButton = W3;

                }
                else
                {
                    W1.transform.position = W2.transform.position;
                    W2.transform.position = temp;
                    CentreButton = W2;
                }
                    break;
            case "W2":
                temp = W2.transform.position;
                if(Ban)
                {
                    W2.transform.position = W3.transform.position;
                    W3.transform.position = W1.transform.position;
                    W1.transform.position = temp;
                    CentreButton = W1;
                }
                else
                {
                    W2.transform.position = W1.transform.position;
                    W1.transform.position = temp;
                    CentreButton = W1;
                }
                break;
            case "W3":
                temp = W3.transform.position;
                W3.transform.position = W1.transform.position;
                W1.transform.position = W2.transform.position;
                W2.transform.position = temp;
                CentreButton = W2;
                break;
        }
    }
    public void ChangeWeaponNext()
    {
        Vector3 temp;

        switch (CentreButton.name)
        {
            case "W1":
                temp = W1.transform.position;
                if(Ban)
                {
                    W1.transform.position = W3.transform.position;
                    W3.transform.position = W2.transform.position;
                    W2.transform.position = temp;
                    CentreButton = W2;
                }
                else
                {
                    W1.transform.position = W2.transform.position;
                    W2.transform.position = temp;
                    CentreButton = W2;
                }
                    break;
            case "W2":
                temp = W2.transform.position;
                if (Ban)
                {
                    W2.transform.position = W1.transform.position;
                    W1.transform.position = W3.transform.position;
                    W3.transform.position = temp;
                    CentreButton = W3;
                }
                else
                {
                    W2.transform.position = W1.transform.position;
                    W1.transform.position = temp;
                    CentreButton = W1;
                }
                    
                    break;
            case "W3":
                temp = W3.transform.position;
                W3.transform.position = W2.transform.position;
                W2.transform.position = W1.transform.position;
                W1.transform.position = temp;
                CentreButton = W1;
                break;
        }
    }
}
