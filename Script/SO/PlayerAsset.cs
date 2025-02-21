using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "MyPlayer", menuName = "NewPlayer", order = 1)]
public class PlayerAsset : ScriptableObject
{
    public int Player;//��ɫ��0��������

    public CharactorAsset CharacterAsset;//����

    public int hp;
    public int mp;
    public int maxSp;
    public int NowSp;

    public WeaponAsset[] WeaponAsset = new WeaponAsset[3];//��Ҫʹ�õ�����

    public int Weapon1Acc;//����1����
    public int Weapon2Acc;//����2����
}
