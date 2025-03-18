using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Item", menuName = "New Item", order = 6)]
public class ItemAsset : ScriptableObject
{
    [Header("General info")]
    public string itemName;  //道具名
    public Sprite itemImage;  //道具图片

    [TextArea(2, 3)]//填写框，最少2行，最多3行
    public string description;//卡牌描述
}
