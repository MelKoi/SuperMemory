using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/ChangeFightCharactorSO")]
public class ChangeFightCharactorSO : ScriptableObject
{
    public UnityAction<CharactorAsset> ChangeFightCharactorEvent;

    public void RaiseChangeFightCharactorEvent(CharactorAsset charactorAsset)
    {
        ChangeFightCharactorEvent?.Invoke(charactorAsset);
    }
}
