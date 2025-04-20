using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Game Scene/GameSceneSO")]
public class GameSceneSO : ScriptableObject
{
    public SceneType sceneType;
    public AssetReference sceneRenference;
}
public enum SceneType
{
    Location, Menu
}