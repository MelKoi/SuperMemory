using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDissolve : MonoBehaviour
{
    public Material dissolve;//材质
    public float dissolveSpeed = 0.5f;

    private float dissolveAmount = 0f;

    void Start()
    {
        // 确保卡牌的 Image 组件使用溶解材质
        Image cardImage = GetComponent<Image>();
        dissolveAmount = 0f;
        if (cardImage != null)
        {
            cardImage.material = dissolve;
        }
    }

    void Update()
    {
        if (dissolveAmount < 1f)
        {
            dissolveAmount += Time.deltaTime * dissolveSpeed;
            dissolve.SetFloat("_ClipRate", dissolveAmount);
        }
    }
}
