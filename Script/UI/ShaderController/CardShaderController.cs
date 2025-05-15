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
    [Header("材质")]
    public Material cardGround;
    public Material cardType;
    public Material cardBody;
    public Material cardName;
    public Material cardDescription;
    public Material cardCost;

    private bool _materialsInitialized = false;
    private Sprite sprite;

    [Header("溶解进度")]
    [Range(0,1f)]
    public float _ChangeAmount;

    
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
        if (_materialsInitialized) return;

        // 使用安全实例化方法
        cardGround = CreateMaterialInstance(cardGroundImage);
        cardType = CreateMaterialInstance(cardTypeImage);
        cardBody = CreateMaterialInstance(cardBodyImage);
        cardName = CreateMaterialInstance(cardNameText);
        cardDescription = CreateMaterialInstance(cardDescriptionText);
        cardCost = CreateMaterialInstance(cardCostText);

        _materialsInitialized = true;

        #if UNITY_EDITOR
        // 编辑器专用调试
        Debug.Log("Materials initialized");
        #endif
    }

    private Material CreateMaterialInstance(Graphic graphic)
    {
        if (graphic == null) return null;

        Material mat = new Material(graphic.materialForRendering)
        {
            name = $"{graphic.name}_Instance",
            hideFlags = HideFlags.DontSave
        };
        graphic.material = mat;
        return mat;
    }


}
