using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;


public class ScenesLoader : MonoBehaviour
{
    [Header("事件监听")]
    public SceneLoadEventSO loadEventSO;
    public VoidEventSO newGameEvent;

    [Header("广播")]
    public VoidEventSO afterSceneLoadedEvent;
    public VoidEventSO afterSceneLoadedStartDialogEvent;
    public FadeEventSO fadeEvent;
    public SceneLoadEventSO unLoadedEvent;
    [SerializeField] public GameSceneSO currentLoadedScene;

    [Header("场景")]
    public GameSceneSO menuScene;
    public GameSceneSO firstLoadScene;

    private GameSceneSO sceneToLoad;
    private bool fadeScreen;
    private bool isLoading;
    public float fadeDuration = 1f;

    private void Start()
    {
        loadEventSO.RaiseLoadRequestEvent(menuScene, true);
    }
    private void OnEnable()
    {
        loadEventSO.LoadRequestEvent += OnLoadRequestEvent;
        newGameEvent.OnEventRaised += NewGame;
    }
    private void OnDisable()
    {
        loadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
        newGameEvent.OnEventRaised -= NewGame;
    }

    private void NewGame()
    {
        sceneToLoad = firstLoadScene;
        loadEventSO.RaiseLoadRequestEvent(sceneToLoad, true);
    }

    private void OnLoadRequestEvent(GameSceneSO locationToLoad, bool fadeScreen)
    {
        if (isLoading)
            return;
        isLoading = true;
        sceneToLoad = locationToLoad;
        this.fadeScreen = fadeScreen;
        if(currentLoadedScene != null)
        {
            StartCoroutine(UnLoadPreviousScene());
        }
        else
        {
            LoadNewScene();
        }
    }
    //
    public IEnumerator UnLoadPreviousScene()
    {
        if (fadeScreen)
        {
            //变黑
            fadeEvent.FadeIn(fadeDuration);
        }
        //等待场景完全变黑
        yield return new WaitForSeconds(fadeDuration);

        //通知场景尚未转换，可在此加载其他事项
        unLoadedEvent.RaiseLoadRequestEvent(sceneToLoad, true);

        yield return currentLoadedScene.sceneRenference.UnLoadScene();
        //加载新场景
        LoadNewScene();
    }

    //加载新场景
    private void LoadNewScene()
    {
        var loadingOption = sceneToLoad.sceneRenference.LoadSceneAsync(LoadSceneMode.Additive, true);
        loadingOption.Completed += OnLoadCompleted;
    }
    //加载完毕
    private void OnLoadCompleted(AsyncOperationHandle<SceneInstance> handle)
    {
        currentLoadedScene = sceneToLoad;
        if (fadeScreen)
        {
            //变透明 
            fadeEvent.FadeOut(fadeDuration);
        }

        isLoading = false;
        //场景加载完成后事件
        if (currentLoadedScene.sceneType != SceneType.Menu)
        {
            afterSceneLoadedEvent.RaiseEvent();
        }
        if (currentLoadedScene.sceneType == SceneType.Theater) {
            afterSceneLoadedStartDialogEvent.RaiseEvent();
        }
    }
}
