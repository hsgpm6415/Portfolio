using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : SingletonDontDestroy<LoadSceneManager>
{

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 빌드 인덱스를 사용하는 enum 기반 로딩. 활성화 직후 onActivated가 1회 호출된다.
    /// </summary>
    public SceneLoadHandle LoadScene(SceneType sceneType, Action onActivated = null, bool allowSceneActivation = true)
    {
        var async = SceneManager.LoadSceneAsync((int)sceneType, LoadSceneMode.Single);
        async.allowSceneActivation = allowSceneActivation;
        HookCompleted(async, onActivated);
        return new SceneLoadHandle(async);
    }

    private static void HookCompleted(AsyncOperation op, Action onActivated)
    {
        if (onActivated == null) return;

        if (op.isDone)
        {
            onActivated?.Invoke();
        }
        else
        {
            void Handler(AsyncOperation _)
            {
                op.completed -= Handler;
                onActivated.Invoke();
            }

            op.completed += Handler;
        }

    }
}
public enum SceneType
{
    None = -1,
    Start,
    Intro,
    Main,
    Max
}

public readonly struct SceneLoadHandle
{
    public AsyncOperation Operation { get; }

    public SceneLoadHandle(AsyncOperation operation) => Operation = operation;

    public bool IsDone => Operation.isDone;
    public float Progress => Operation.progress;

    /// <summary>
    /// allowSceneActivation=false로 시작했다면, 이 메서드로 활성화를 트리거한다.
    /// </summary>
    public void Activate()
    {
        if(!Operation.allowSceneActivation)
        {
            Operation.allowSceneActivation = true;
        }
    }

}
