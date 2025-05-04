using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ChangeCharactorManager : MonoBehaviour
{
    [Header("广播")]
    public ChangeFightCharactorSO changeFightCharactorEvent;

    public CharactorAsset charactor;
    //点击启用
    public void OnClickCharactorCard()
    {
        
            charactor = GetComponent<CharacterCardManager>().charactorasset;
            changeFightCharactorEvent.RaiseChangeFightCharactorEvent(charactor);
        
    }
}
