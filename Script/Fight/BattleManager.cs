using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.UI;

public enum GamePhase
{
    gameStart,playerAction,playerReady,enemyAction
}
public class BattleManager : MonoBehaviour
{
    [Header("������������")]
    public GameObject hand;//������
    public GameObject cardPrefeb;//����Ԥ��
    public WeaponAsset[] ChooseWeapon = new WeaponAsset[3];//�����ǰԤѡ��3������
    public List<GameObject> ChooseCards;//���ִ����Ҫ����ʱѡ��Ŀ���

    [Header("��ҿ��ƵĻ�������")]
    public CharactorAsset charactor;//������ҵ�����
    public WeaponAsset Weapon1;//�洢��ҵĵ�һ������
    public WeaponAsset Weapon2;//�洢��ҵĵڶ�������
    public  List<CardAsset> shuffledCards;//�����Ŀ���

    [Header("�����ϸ����")]
    public string power;//���ѡ������＼��
    public int speed;//��ҵ��ٶ�
    public int HandCardNum;//��ҵ���������
    public int Hp;
    public int Def;
    public int Sp;
    public int Mp;
    public int Weapon1Acc;//����1��������
    public int Weapon2Acc;//����2��������

    [Header("�������")]
    public Text AccNum1Text;//���ڵ����������ı�
    public Text AccNum2Text;//���ڵ����������ı�
    public int AccNum1;//��������������
    public int AccNum2;//��������������
    public GameObject Weapon1Object;//��һ������
    public GameObject Weapon2Object;//�ڶ�������
    public bool Weapon1Attacked;//��һ���������غ��Ѿ�����
    public bool Weapon2Attacked;//�ڶ����������غ��Ѿ�����

    [Header("��ʾ�������")]
    public GameObject WeaponChoosePanel;//ѡ����������Ļ���
    public GameObject DrewChoosePanel;//ѡ���һ�γ�ȡ�Ļ���
    public Button[] ChooseWeaponButton = new Button[3];//������ѡ���İ�ť
    public Button[] ChooseFirstDrewButton = new Button[2];//ѡ���鰴ť
    public Text Hptext;//��Ŀֵ�ı�
    public Text Deftext;//��ֵ�ı�
    public Text Sptext;//����ֵ�ı�
    public Text Mptext;//����ֵ�ı�
 

    [Header("�ű��ڲ�����")]
    public int CardNum = 20;//��������������
    public GamePhase gamePhase = GamePhase.gameStart;//��ʼΪ��Ϸ��ʼ
    private WeaponAsset FirstDrewWeapon;//��һ����ȡ������
    public int AttackManage;//������������ֵ
    public EnemyManager enemyManager;
    void Start()
    {
       GameStart();
    }
    void Update()
    {
        if (gamePhase == GamePhase.playerReady)
            PlayerReady();
        else if (gamePhase == GamePhase.enemyAction)
            EnemyAction();
    }
    public int DrowCards(GameObject hand,int CardNum,int HandCardNum, List<CardAsset> shuffledCards)//ÿ�غϿ�ʼ�鿨����
        //�����ֱ�Ϊ����ɫ���������ҷ�&�з����������е������������������ޡ���ǰӵ�е���������
    {
        int HandNum =HandCardNum - hand.transform.childCount;//��ȡ��Ҫ�����ֵĿ��Ƹ���
        if(HandNum < 0)//�������Ƴ�������ʱ������
            HandNum = 0;
        for (int i = 0; i < HandNum; i++)
        {
            System.Random rng = new System.Random();//��ȡ�����
            int k = rng.Next(CardNum);
            cardPrefeb.GetComponent<OneCardManager>().cardAsset = shuffledCards[k];
            shuffledCards.RemoveAt(k);
            CardNum--;
            if (hand != null)
            {
                GameObject handcard = Instantiate(cardPrefeb, hand.transform, false);
            }
            if (shuffledCards.Count == 0)//������ƿ�Ҫ���꣬���ʣ����ƾ�ֹͣ����
                break;
        }
        return CardNum;
    }
    //��Ϸ��ʼ���غϿ�ʼ���غ��У��غϽ���
    //��Ϸ��ʼ����ȡ�����ƣ����ѡ�����ţ���һ���ƿ⣬ѡ������֮һ����ȡx�����ơ�
    public void ReadDeck()//��ȡ�������
    {
        HandCardNum = charactor.HandCardNum;
        Hp = charactor.MaxHealth;
        Def = charactor.StartDef;
        Sp = charactor.MaxSp;
        Mp = 0;
        speed = charactor.speed;
    }
    public void GameStart()//��Ϸ��ʼ����������ݵĸ����
    {
        //��ҴӶ��ֵ�����������ѡ��һ�����ã�������ʱʹ���Լ��Ľ���ѡ��
        for(int i = 0;i < 3; i++)
        {
             ChooseWeaponButton[i].GetComponentInChildren<Text>().text = ChooseWeapon[i].WeaponName;
        }
        ReadDeck();
        Hptext.text = Hp.ToString();
        Deftext.text = Def.ToString();
        Sptext.text = Sp.ToString();
        Mptext.text = Mp.ToString();
        Weapon1Attacked = false;
        Weapon2Attacked = false;
        AttackManage = 0;
        EnemyStart();
        gamePhase = GamePhase.playerAction;
    }
    public void EnemyStart()//��Ϸ��ʼ���ڵ������ݵĸ����
    {
        enemyManager.HandCardNum = enemyManager.charactor.HandCardNum;
        enemyManager.Hp = enemyManager.charactor.MaxHealth;
        enemyManager.Def = enemyManager.charactor.StartDef;
        enemyManager.Sp = enemyManager.charactor.MaxSp;
        enemyManager.Mp = 0;
        enemyManager.speed = enemyManager.charactor.speed;
        enemyManager.Hptext.text = enemyManager.Hp.ToString();
        enemyManager.Deftext.text = enemyManager.Def.ToString();
        enemyManager.Sptext.text = enemyManager.Sp.ToString();
        enemyManager.Mptext.text = enemyManager.Mp.ToString();
        enemyManager.Weapon1Attacked = false;
        enemyManager.Weapon2Attacked = false;
        enemyManager.AttackManage = 0;
        enemyManager.shuffledCards = new List<CardAsset>(enemyManager.Weapon1.Allcard);
        enemyManager.CardNum = DrowCards(enemyManager.enemyHand, enemyManager.CardNum, enemyManager.HandCardNum,enemyManager.shuffledCards);
    }
    public void TurnEnd()//�غϽ�������
    {
        if(gamePhase == GamePhase.playerAction)
        {
            gamePhase = GamePhase.enemyAction;
        }
        else if(gamePhase == GamePhase.enemyAction)
        {
            Weapon1Attacked = false;
            Weapon2Attacked = false;
            AttackManage = 0;
            gamePhase = GamePhase.playerReady;
        }
    }
    public void EnemyAction()//���˻غϣ���Ҫ��д����AI��
    {
       TurnEnd();
    }
    public void PlayerReady()//��һغ�
    {
        if (shuffledCards.Count == 0)//��һ���������ƿ�û�վͳ��һ��������,���˾ͳ�ڶ���������.
        {
            if(FirstDrewWeapon.WeaponName.Equals(Weapon1.WeaponName))
                shuffledCards = new List<CardAsset>(Weapon2.Allcard);
            else
                shuffledCards = new List<CardAsset>(Weapon1.Allcard);
        }
        Sp = charactor.MaxSp;//����������
        Sptext.text = Sp.ToString();
        CardNum =  DrowCards(hand,CardNum, HandCardNum, shuffledCards);
        gamePhase = GamePhase.playerAction;
    }
    public void ChooseWeapons(Text text)
    {
        string name = text.text;//��ȡ��ǰ��ť��Ӧ��������
        for(int i = 0; i < 3; i++)//���ҵ����ѡ���ж�Ӧ������
        {
            if (!ChooseWeapon[i].WeaponName.Equals(name))
            {
                if (Weapon1 == null) Weapon1 = ChooseWeapon[i];
                else if (Weapon1 != null) Weapon2 = ChooseWeapon[i];
            }
            else if (ChooseWeapon[i].WeaponName.Equals(name)) continue;
        }
        ChooseFirstDrewButton[0].GetComponentInChildren<Text>().text = Weapon1.WeaponName;
        ChooseFirstDrewButton[1].GetComponentInChildren<Text>().text = Weapon2.WeaponName;
        Weapon1Object.GetComponent<WeaponCardManager>().weaponAsset = Weapon1;
        Weapon2Object.GetComponent<WeaponCardManager>().weaponAsset= Weapon2;
        WeaponChoosePanel.SetActive(false);
    }//ѡ������
    public void ChooseFirstDrew(Text text)//ѡ���ȳ�ȡ�Ŀ���
    {
        string name = text.text;//��ȡ��ǰ��ť��Ӧ��������
        if (name.Equals(Weapon1.WeaponName)) FirstDrewWeapon = Weapon1;
        else FirstDrewWeapon = Weapon2;
        shuffledCards = new List<CardAsset>(FirstDrewWeapon.Allcard);
        CardNum = DrowCards(hand, CardNum, HandCardNum, shuffledCards);
        DrewChoosePanel.SetActive(false);
    }
    public void UseCard()//ʹ�ÿ���
    {
        if(ChooseCards.Count == 1)
        {
            foreach (var i in ChooseCards)
                //ִ�п���Ч������������٣��������ƣ�
                Destroy(i);
            ChooseCards.Clear();
        }
    }
    public void WeaponAccmulation(GameObject AccWeapon)//��������
    {
        if(AccWeapon == Weapon1Object)
        {
            if(ChooseCards.Count <= Weapon1Object.GetComponent<WeaponCardManager>().weaponAsset.OnceAccumulation)//���ѡ�������С�ڵ�������ĵ�������
            {
                AccNum1 = AccNum1 + ChooseCards.Count;//�������������Ŀ���
                AccNum1Text.text = AccNum1.ToString();//���������ı�
                foreach (var i in ChooseCards)//�ݻٱ��������ܵĿ���
                    Destroy(i);
                ChooseCards.Clear();//ѡ����������
            }
            else
            {
                Debug.Log("�������޷��������ܵ�ǰ����");
            }
        }
        else if(AccWeapon == Weapon2Object)
        {
            if (ChooseCards.Count <= Weapon2Object.GetComponent<WeaponCardManager>().weaponAsset.OnceAccumulation)//���ѡ�������С�ڵ�������ĵ�������
            {
                AccNum2 = AccNum2 + ChooseCards.Count;
                AccNum2Text.text = AccNum2.ToString();
                foreach (var i in ChooseCards)
                    Destroy(i);
                ChooseCards.Clear();
            }
            else
            {
                Debug.Log("�������޷��������ܵ�ǰ����");
            }
        }

    }
    public void WeaponAttack(GameObject AccWeapon)//���ƹ���
    {
        //���¹�����ť���ȼ��������������WeaponAsset�е����ܵȼ�ƥ�䣬ѡ���ݵ����һ�����ܵȼ���
        //�����й���������ֵ���㣬ˢ���ı����ݣ��ѹ���boolֵ��Ϊtrue

        if(AccWeapon == Weapon1Object)//����if����Ӧ�������������ݺ��ı�����Ϊ0
        {
            if(!Weapon1Attacked)
            {
                for (int i = 0; i < 3; i++)//ƥ�����ܣ����Ҵ���˺�
                    if (AccNum1 >= AccWeapon.GetComponent<WeaponCardManager>().weaponAsset.Accumulation[i].Acc)
                    {
                        AttackManage = AccWeapon.GetComponent<WeaponCardManager>().weaponAsset.Accumulation[i].Value;
                    }
                AttackToEnemy(AttackManage);
                Debug.Log("������" + AttackManage + "���˺�");
                if (AttackManage == 0)//�����ܲ���ʱ�򲻳��˺�
                {
                    Debug.Log("���ܲ��㣬�޷�����˺�");
                    return;
                }
                Mp = Mp + AccNum1;
                Mptext.text = Mp.ToString();
                AccNum1 = 0;//������Ҫ��ֵ��������Ϊ���غ��Թ���״̬
                AccNum1Text.text = AccNum1.ToString();
                Weapon1Attacked = true;
                AttackManage = 0;
            }
            else
            {
                Debug.Log("���������غ��Ѿ����ڹ���");
            }
        }
        else if(AccWeapon == Weapon2Object) 
        { 
            if (!Weapon2Attacked)
            {
                for (int i = 0; i < 3; i++)//ƥ�����ܣ����Ҵ���˺�
                    if (AccNum2 >= AccWeapon.GetComponent<WeaponCardManager>().weaponAsset.Accumulation[i].Acc)
                    {
                        AttackManage = AccWeapon.GetComponent<WeaponCardManager>().weaponAsset.Accumulation[i].Value;
                    }
                AttackToEnemy(AttackManage);
                Debug.Log("������" + AttackManage + "���˺�");
                if (AttackManage == 0)
                {
                    Debug.Log("���ܲ��㣬�޷�����˺�");
                    return;
                }
                Mp = Mp + AccNum2;
                Mptext.text = Mp.ToString();
                AccNum2 = 0;
                AccNum2Text.text = AccNum2.ToString();
                Weapon2Attacked = true;
                AttackManage = 0;
            }
            else
            {
                Debug.Log("���������غ��Ѿ����ڹ���");
            }
        }
     }
    public void AttackToEnemy(int attack)//���к��ж��˺�
    {
        if(gamePhase == GamePhase.playerAction)
        {
            if(enemyManager.Def >= attack)
            {
                enemyManager.Def -= attack;
                enemyManager.Deftext.text = enemyManager.Def.ToString();
            }
            else
            {
                enemyManager.Hp -= attack;
                enemyManager.Hptext.text = enemyManager.Hp.ToString();
            }
        }
        else if(gamePhase == GamePhase.enemyAction)
        {
            if (Def >= attack)
            {
                Def -= attack;
                Deftext.text = Def.ToString();
            }
            else
            {
                Hp -= attack;
                Hptext.text = Hp.ToString();
            }
        }
    }
}
