using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DateInfo : MonoBehaviour
{
    public TextMeshProUGUI time;
    public TextMeshProUGUI weak;

    private void Start()
    {
        UpdateDateTime();
    }
    private void Update()
    {
        UpdateDateTime();
    }
    void UpdateDateTime()
    {
        //获取当前系统时间
        DateTime now = DateTime.Now;

        //格式化时间和星期
        string currentTime = now.ToString("HH:mm"); // 时间格式：小时:分钟
        string dayOfWeek = now.DayOfWeek.ToString(); // 星期几

        //将时间和星期显示在 TextMeshPro 上
        time.text = currentTime;
        weak.text = dayOfWeek ;
    }
}
