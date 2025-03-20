using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDissolve : MonoBehaviour
{
    public Material dissolve;//����
    public float dissolveSpeed = 0.5f;

    private float dissolveAmount = 0f;

    void Start()
    {
        // ȷ�����Ƶ� Image ���ʹ���ܽ����
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
