using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GreenHand : BattleManager
{
    public GameObject IamgeAndText;
    public Image TalkImage;
    public Text TalkText;
    private int acc = 0;// ������ʱ����
    private bool hasAttackedThisTurn = false;
    // Start is called before the first frame update
    void Start()
    {
        GameStart();

    }

    public void GreenHandTeach()
    {

    }
    override public void StartEnemyTurn()//���˵���Ҫ�׶�
    {

    }
    override public IEnumerator EnemyAcc(GameObject Card)//�з����ܺ���
    {

        yield return null;
    }
}
