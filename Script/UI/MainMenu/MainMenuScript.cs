using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using JetBrains.Annotations;
using TMPro;


public class MainMenuScript : MonoBehaviour
{
    public Button StartGameBotton;//点击开始游戏的按钮
    public TMP_Text StartGameText;

    public Button NewGameBotton;//新游戏的按钮
    public TMP_Text NewGameText;

    public Button LoadGameBotton;//读取游戏的按钮
    public TMP_Text LoadGameText;

    public Button SettingBotton;//设置的按钮
    public TMP_Text SettingText;

    public Button QuitGameBotton;//退出游戏的按钮
    public TMP_Text QuittGameText;

    public Image MainMenuPanel;//放置其他东西的面板
    public void StartGame()
    {
        StartGameText.DOFade(0, 0.6f);
        DOTween.Kill(StartGameBotton);
        StartGameBotton.gameObject.SetActive(false);
        MainMenuPanel.gameObject.SetActive(true);

        NewGameText.DOFade(1, 1.0f);
        NewGameBotton.GetComponent<RectTransform>().DOAnchorPosX(610, 1.0f);
        LoadGameText.DOFade(1, 1.0f);
        LoadGameBotton.GetComponent<RectTransform>().DOAnchorPosX(610, 1.0f);
        SettingText.DOFade(1, 1.0f);
        SettingBotton.GetComponent<RectTransform>().DOAnchorPosX(610, 1.0f);
        QuittGameText.DOFade(1, 1.0f);
        QuitGameBotton.GetComponent<RectTransform>().DOAnchorPosX(610, 1.0f);
    }
}
