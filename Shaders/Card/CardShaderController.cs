using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UI;


[ExecuteInEditMode()]
public class CardShaderController : MonoBehaviour
{
    public Image cardGroundImage;
    public Image cardTypeImage;
    public Image cardBodyImage;
    private Material cardGround;
    private Material cardType;
    private Material cardBody;

    [Range(0,1f)]
    public float _ChangeAmount;

    void Start()
    {
        cardGround = cardGroundImage.material;
        cardType = cardTypeImage.material;
        cardBody = cardBodyImage.material;

        if (cardGround == null || cardGround.shader == false || cardGround.shader.isSupported == false ||
            cardType == null || cardType.shader == false || cardType.shader.isSupported == false||
            cardBody == null || cardBody.shader == false || cardBody.shader.isSupported == false)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        cardGround.SetFloat("_ChangeAmount", _ChangeAmount);
        cardType.SetFloat("_ChangeAmount", _ChangeAmount);
        if (_ChangeAmount > 0.19f && _ChangeAmount < 0.535f)
            cardBody.SetFloat("_ChangeAmount", (_ChangeAmount - 0.19f) / (0.535f - 0.19f));
        else if (_ChangeAmount <=0.19f)
            cardBody.SetFloat("_ChangeAmount", 0);
        else
            cardBody.SetFloat("_ChangeAmount",1);
    }
}
