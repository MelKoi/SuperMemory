using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TextBox : MonoBehaviour
    //文本显示脚本，放在文本框父物体，实现了文字逐字显示，
    //按下鼠标左键全部显示，以及显示完毕后按下鼠标左键开始显示下一句
{
    public Text text;
    public TextAsset textAsset;

    public int index;
    private bool TextShowFinish;//是否完成打字
    private bool cancelTyping = false;//取消打字    

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
        if(Input.GetMouseButtonDown(0) && index == strings.Count)//文本播放完就结束
        {
            index = 0;
            return;
        }
        if (Input.GetMouseButtonDown(0))//没有播放完就播放下一句
        {
            if(TextShowFinish && !cancelTyping)
                StartCoroutine(ShowText());
            else if(!TextShowFinish)
                cancelTyping = !cancelTyping;
        }
    }
    public void GetTxt(TextAsset textAsset)//获取文本
    {
        index = 0;

        var str = textAsset.text.Split('\n');

       foreach(var list in str)
        {
            strings.Add(list);
        }
    }
    IEnumerator ShowText()//实现文字逐字显示的效果
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
