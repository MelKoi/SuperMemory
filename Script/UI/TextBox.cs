using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TextBox : MonoBehaviour
    //�ı���ʾ�ű��������ı������壬ʵ��������������ʾ��
    //����������ȫ����ʾ���Լ���ʾ��Ϻ�����������ʼ��ʾ��һ��
{
    public Text text;
    public TextAsset textAsset;

    public int index;
    private bool TextShowFinish;//�Ƿ���ɴ���
    private bool cancelTyping = false;//ȡ������    

    List<string> strings = new List<string>();
    void  Awake()
    {
        GetTxt(textAsset);
    }
    private void OnEnable()
    {
        TextShowFinish = true;
        StartCoroutine(ShowText());
    }
    void Update()
    {
        if(Input.GetMouseButtonDown(0) && index == strings.Count)//�ı�������ͽ���
        {
            index = 0;
            return;
        }
        if (Input.GetMouseButtonDown(0))//û�в�����Ͳ�����һ��
        {
            if(TextShowFinish && !cancelTyping)
                StartCoroutine(ShowText());
            else if(!TextShowFinish)
                cancelTyping = !cancelTyping;
        }
    }
    public void GetTxt(TextAsset textAsset)//��ȡ�ı�
    {
        index = 0;

        var str = textAsset.text.Split('\n');

       foreach(var list in str)
        {
            strings.Add(list);
        }
    }
    IEnumerator ShowText()//ʵ������������ʾ��Ч��
    {
        TextShowFinish = false;
        text.text = "";
        //for(int i=0; i<strings[index].Length; i++)
        //{
        //   text.text += strings[index][i];

        //     yield return new WaitForSeconds(0.05f);
        //}
        int letter = 0;
        while(!cancelTyping && letter < strings[index].Length-1)
        {
            text.text += strings[index][letter];
            letter++;
            yield return new WaitForSeconds(0.05f);
        }
        text.text = strings[index];
        cancelTyping= false;
        TextShowFinish=true;
        index++;
    }
}
