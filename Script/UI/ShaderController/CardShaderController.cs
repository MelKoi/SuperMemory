using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UI;


[ExecuteInEditMode()]
public class CardShaderController : MonoBehaviour
{
    [Header("卡牌牌面组件")]
    public Image cardGroundImage;
    public Image cardTypeImage;
    public Image cardBodyImage;
    public Text cardNameText;
    public Text cardDescriptionText;
    public Text cardCostText;
    public Material cardGround;
    public Material cardType;
    public Material cardBody;
    public Material cardName;
    public Material cardDescription;
    public Material cardCost;

    private Sprite sprite;

    [Header("溶解进度")]
    [Range(0,1f)]
    public float _ChangeAmount;

    void Start()
    {
        //cardGround = cardGroundImage.material;
        //cardType = cardTypeImage.material;
        //cardBody = cardBodyImage.material;
        //cardName = cardNameText.material;
        //cardDescription = cardDescriptionText.material;
        //cardCost = cardCostText.material;
        if (cardGroundImage != null)
        {
            cardGround = new Material(cardGroundImage.material);
            cardGroundImage.material = cardGround;
        }
        if (cardTypeImage != null)
        {
            cardType = new Material(cardTypeImage.material);
            cardTypeImage.material = cardType;
        }
        if (cardBodyImage != null)
        {
            cardBody = new Material(cardBodyImage.material);
            cardBodyImage.material = cardBody;
        }
        if (cardNameText != null)
        {
            cardName = new Material(cardNameText.material);
            cardNameText.material = cardName;
        }
        if (cardDescriptionText != null)
        {
            cardDescription = new Material(cardDescriptionText.material);
            cardDescriptionText.material = cardDescription;
        }
        if (cardCostText != null)
        {
            cardCost = new Material(cardCostText.material);
            cardCostText.material = cardCost;
        }

        if (cardGround == null || cardGround.shader == false || cardGround.shader.isSupported == false ||
            cardType == null || cardType.shader == false || cardType.shader.isSupported == false||
            cardBody == null || cardBody.shader == false || cardBody.shader.isSupported == false||
            cardName == null || cardName.shader == false || cardName.shader.isSupported == false ||
            cardDescription == null || cardDescription.shader == false || cardDescription.shader.isSupported == false ||
            cardCost == null || cardCost.shader == false || cardCost.shader.isSupported == false )
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (cardGround != null)
            cardGround.SetFloat("_ChangeAmount", _ChangeAmount);
        if (cardType != null)
            cardType.SetFloat("_ChangeAmount", _ChangeAmount);

        if (cardDescription != null)
        {
            if (_ChangeAmount <0.2f && _ChangeAmount >0.07f)
                cardDescription.SetFloat("_ChangeAmount", (_ChangeAmount-0.07f)/(0.2f-0.07f));
            else if(_ChangeAmount >=0.2f)
                cardDescription.SetFloat("_ChangeAmount", 1);
            else
                cardDescription.SetFloat("_ChangeAmount", 0);

        }


        if (cardName != null && cardCost != null)
        {
            if (_ChangeAmount > 0.85f)
            {
                cardCost.SetFloat("_ChangeAmount", _ChangeAmount);
                cardName.SetFloat("_ChangeAmount", _ChangeAmount);
            }
            else
            {
                cardCost.SetFloat("_ChangeAmount", 0);
                cardName.SetFloat("_ChangeAmount", 0);
            }
        }

        if (cardBody != null)
        {
            if (_ChangeAmount > 0.19f && _ChangeAmount < 0.535f)
                cardBody.SetFloat("_ChangeAmount", (_ChangeAmount - 0.19f) / (0.535f - 0.19f));
            else if (_ChangeAmount <=0.19f)
                cardBody.SetFloat("_ChangeAmount", 0);
            else
                cardBody.SetFloat("_ChangeAmount",1);
        }
            
    }

    //销毁时销毁材质实例
    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            // 运行时使用 Destroy
            if (cardGround != null) Destroy(cardGround);
            if (cardType != null) Destroy(cardType);
            if (cardBody != null) Destroy(cardBody);
            if (cardName != null) Destroy(cardName);
            if (cardDescription != null) Destroy(cardDescription);
            if (cardCost != null) Destroy(cardCost);
        }
        else
        {
            // 编辑器模式使用 DestroyImmediate
            if (cardGround != null) DestroyImmediate(cardGround);
            if (cardType != null) DestroyImmediate(cardType);
            if (cardBody != null) DestroyImmediate(cardBody);
            if (cardName != null) DestroyImmediate(cardName);
            if (cardDescription != null) DestroyImmediate(cardDescription);
            if (cardCost != null) DestroyImmediate(cardCost);
        }
    }

    //设置卡牌主图像
    public void SetMainTex(Material material,Sprite sprite)
    {
        if (material == null || sprite == null)
        {
            Debug.LogError("Material or Sprite is null!");
            return;
        }

        //获取Sprite的纹理
        Texture2D texture = sprite.texture;

        //设置材质的_MainTex
        material.SetTexture("_MainTex", texture);
    }

    //设置卡牌使用时特效颜色
    public void SetRampTex(Material material,Sprite sprite)
    {
        if (material == null || sprite == null)
        {
            Debug.LogError("Material or Sprite is null!");
            return;
        }

        //获取Sprite的纹理
        Texture2D texture = sprite.texture;

        //设置材质的_RampTex
        material.SetTexture("_RampTex", texture);
    }

    //实例化卡牌材质
    public void InitializeMaterials()
    {
        if (cardGroundImage != null)
        {
            cardGround = new Material(cardGroundImage.material);
            cardGroundImage.material = cardGround;
        }
        if (cardTypeImage != null)
        {
            cardType = new Material(cardTypeImage.material);
            cardTypeImage.material = cardType;
        }
        if (cardBodyImage != null)
        {
            cardBody = new Material(cardBodyImage.material);
            cardBodyImage.material = cardBody;
        }
        if (cardNameText != null)
        {
            cardName = new Material(cardNameText.materialForRendering);
            cardNameText.material = cardName;
        }
        if (cardDescriptionText != null)
        {
            cardDescription = new Material(cardDescriptionText.materialForRendering);
            cardDescriptionText.material = cardDescription;
        }
        if (cardCostText != null)
        {
            cardCost = new Material(cardCostText.materialForRendering);
            cardCostText.material = cardCost;
        }
    }
}
