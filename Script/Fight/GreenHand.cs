using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GreenHand : BattleManager
{
    public GameObject IamgeAndText;
    public Image TalkImage;
    public Text TalkText;
    private int acc = 0;// 蓄能临时储存
    private bool hasAttackedThisTurn = false;
    // Start is called before the first frame update
    void Start()
    {
        GameStart();

    }

    public void GreenHandTeach()
    {

    }
    override public void StartEnemyTurn()//敌人的主要阶段
    {

    }
    override public IEnumerator EnemyAcc(GameObject Card)//敌方蓄能函数
    {

        yield return null;
    }
}
