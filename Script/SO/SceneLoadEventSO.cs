using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/SceneLoadEventSO")]
public class SceneLoadEventSO : ScriptableObject
{
    public UnityAction<GameSceneSO, bool> LoadRequestEvent;
    
    /// <summary>
    /// 场景切换请求
    /// </summary>
    /// <param name="locationToLoad">需要加载的场景</param>
    /// <param name="fadeScreen">是否需要渐黑</param>
    public void RaiseLoadRequestEvent(GameSceneSO locationToLoad,bool fadeScreen)
    {
        LoadRequestEvent?.Invoke(locationToLoad, fadeScreen);
    }
}
