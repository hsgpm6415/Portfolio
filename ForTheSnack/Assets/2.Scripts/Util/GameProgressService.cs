using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
/// <summary>
/// 게임 진행(플레이어 위치/오브젝트 상태)의 저장/로드/새 게임을 총괄하는 오케스트레이터.
/// - 파일 I/O는 SaveSystem이 담당(JSON)
/// - 대상 수집/복원은 각 컴포넌트(CaptureTyped/RestoreTyped)
/// - 씬 활성화 직후 onActivated 콜백에서 1프레임 유예 + PlayerLocator 대기 후 복원
/// </summary>
public sealed class GameProgressService : SingletonDontDestroy<GameProgressService>
{
    [SerializeField]
    PlayerSaveProxy m_playerProxy;
    [SerializeField]
    bool m_applyVelocityOnLoad;
    public bool IsBusy { get; private set; }
    public bool HasSave => SaveSystem.Exists();

    public event Action OnSavingStarted;
    public event Action OnSavingFinished;
    public event Action OnLoadingStarted;
    public event Action OnLoadingFinished;
    public event Action<bool> OnIntroStarted;
    public event Action OnIntroFinished;
    protected override void Awake()
    {
        base.Awake();
        m_applyVelocityOnLoad = true;
    }

    protected override void Start()
    {
        base.Start();
        Panel_Intro.Instance.OnIntroFinished += HandleAfterIntroEvent;
    }

    void OnApplicationQuit()
    {
        Panel_Intro.Instance.OnIntroFinished -= HandleAfterIntroEvent;
    }

    public void SaveProgress()
    {
        OnSavingStarted?.Invoke();

        var data = new GameProgressData();

        m_playerProxy = PlayerLocator.Current ?? FindObjectOfType<PlayerSaveProxy>();

        data.elapsed = MenuManager.Instance.Elapsed;

        if (m_playerProxy != null)
        {
            var pos = m_playerProxy.GetPosition();
            var vel = m_playerProxy.GetVelocity();

            data.playerPosX = pos.x;
            data.playerPosY = pos.y;
            data.playerVelX = vel.x;
            data.playerVelY = vel.y;
        }

        foreach (var s in SaveResistry.All)
        {
            if (s is SpecificPoint sp)
            {
                data.specificPoints.Add(sp.CaptureTyped());
            }
        }

        foreach (var s in SaveResistry.All)
        {
            if (s is Stick stick)
            {
                data.sticks.Add(stick.CaptureTyped());
            }
        }

        SaveSystem.Write(data);
        OnSavingFinished?.Invoke();

    }

    public void SaveAndExitToTitle()
    {
        if (IsBusy) return;
        StartCoroutine(Coroutine_SaveAndExitToTitle());
    }
    /// <summary>세이브가 있으면 로드/복원.</summary>
    public void LoadProgress()
    {
        if (IsBusy) return;
        StartCoroutine(Coroutine_LoadProgress());
    }
    /// <summary>세이브를 삭제하고 시작 씬으로 새 게임.</summary>
    public void NewGame()
    {
        if (IsBusy) return;
        StartCoroutine(Coroutine_NewGame());
    }
    public void EndAndExitToTitle()
    {
        if (IsBusy) return;
        StartCoroutine(Coroutine_EndAndExitToTitle());
    }
    #region [Coroutine]

    private IEnumerator Coroutine_EndAndExitToTitle()
    {
        IsBusy = true;
        SaveSystem.Delete();
        var handle = LoadSceneManager.Instance.LoadScene(
            SceneType.Start,
            onActivated: () => { StartCoroutine(Coroutine_AfterTitleActivated()); }
        );

        yield return handle.Operation;
    }


    private IEnumerator Coroutine_SaveAndExitToTitle()
    {
        IsBusy = true;

        SaveProgress();

        var handle = LoadSceneManager.Instance.LoadScene(
            SceneType.Start,
            onActivated: () => { StartCoroutine(Coroutine_AfterTitleActivated()); }
        );

        yield return handle.Operation;
    }


    private IEnumerator Coroutine_LoadProgress()
    {
        IsBusy = true;
        OnLoadingStarted?.Invoke();
        
        if (!HasSave)
        {
            Debug.LogWarning("[GameProgressService] No save file to load.");
            IsBusy = false;
            OnLoadingFinished.Invoke();
            yield break;
        }

        yield return null;

        var handle = LoadSceneManager.Instance.LoadScene(
            SceneType.Intro, 
            onActivated: () => StartCoroutine(Coroutine_AfterContinueActivated())
        );

        while (!handle.IsDone) yield return null;

        yield return handle.Operation;
    }

    private IEnumerator Coroutine_NewGame()
    {
        IsBusy = true;
        try
        {
            SaveSystem.Delete();
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }

        Time.timeScale = 1.0f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;



        var handle = LoadSceneManager.Instance.LoadScene(
            SceneType.Intro, 
            onActivated: () => { StartCoroutine(Coroutine_AfterNewGameActivated());
         });

        while (!handle.IsDone) yield return null;

        yield return handle.Operation;
    }

    private IEnumerator Coroutine_AfterContinueActivated()
    {
        yield return null;
        OnIntroStarted.Invoke(true);
        IsBusy = false;

    }

    /// <summary>
    /// 씬 활성화 직후(LoadSceneManager 콜백) 호출되어, 한 프레임 유예 + PlayerLocator 대기 후 저장 데이터를 복원한다.
    /// </summary>
    private IEnumerator Coroutine_AfterActivatedAndReady(GameProgressData data)
    {
        yield return null;

        yield return Coroutine_WaitForPlayerReady(2f);

        if(PlayerLocator.Current == null)
        {
            Debug.LogError("[GameProgressService] Player not ready after scene activation. Abort restore.");
            OnIntroFinished?.Invoke();
            OnLoadingFinished?.Invoke();
            IsBusy = false;
            yield break;
        }

        RestoreWorldState(data);

        m_playerProxy = PlayerLocator.Current;
        GameManager.Instance.Init(SceneType.Main);
        MenuManager.Instance.InitMenu(SceneType.Main);
        Panel_Ending.Instance.Init(SceneType.Main);
        m_playerProxy.ApplyPosition(new Vector2(data.playerPosX, data.playerPosY));
        if (m_applyVelocityOnLoad)  m_playerProxy.ApplyVelocity(new Vector2(data.playerVelX, data.playerVelY));
        else                        m_playerProxy.ApplyVelocity(Vector2.zero);


        var col = GameManager.Instance.Cat.transform.GetChild(0).GetComponent<Collider2D>();
        ThemeManager.Instance.ReevaluateAt(col);
        OnIntroFinished?.Invoke();
        OnLoadingFinished?.Invoke();
        IsBusy = false;
    }
    /// <summary>
    /// New Game 시 씬 활성화 직후 호출. 한 프레임 유예만 하고 종료.
    /// (필요하면 여기서 초기 위치 배치/카메라 리셋 등을 추가)
    /// </summary>
    private IEnumerator Coroutine_AfterNewGameActivated()
    {
        yield return null;
        OnIntroStarted?.Invoke(false);
        IsBusy = false;
    }

    private IEnumerator Coroutine_AfterTitleActivated()
    {
        yield return null;

        GameManager.Instance.Init(SceneType.Start);
        MenuManager.Instance.InitMenu(SceneType.Start);
        Panel_Ending.Instance.Init(SceneType.Start);
        SettingService.Instance.InitSetting();

        IsBusy = false;
    }

    private IEnumerator Coroutine_WaitForPlayerReady(float timeoutSeconds)
    {
        float t = 0f;
        while (PlayerLocator.Current == null && t < timeoutSeconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }
    
    private IEnumerator Coroutine_AtferIntroActivated()
    {
        yield return null;
        GameManager.Instance.Init(SceneType.Main);
        MenuManager.Instance.InitMenu(SceneType.Main);
        Panel_Ending.Instance.Init(SceneType.Main);
        OnIntroFinished?.Invoke();
        IsBusy = false;
        
    }
    private IEnumerator Coroutine_AfterIntro(bool isContinue)
    {
        IsBusy = true;
        yield return null;
        if (isContinue)
        {
            var data = SaveSystem.Read();
            var handle = LoadSceneManager.Instance.LoadScene(
                SceneType.Main,
                onActivated: () => StartCoroutine(Coroutine_AfterActivatedAndReady(data))
            );
            while (!handle.IsDone) yield return null;

            yield return handle.Operation;
        }
        else
        {
            var handle = LoadSceneManager.Instance.LoadScene(
                SceneType.Main,
                onActivated: () => {
                StartCoroutine(Coroutine_AtferIntroActivated());
            });

            while (!handle.IsDone) yield return null;

            yield return handle.Operation;
        }
    }

    #endregion

    private void RestoreWorldState(GameProgressData data)
    {
        foreach (var s in data.specificPoints)
        {
            if (SaveResistry.TryGet(s.id, out ISaveable a) && a is SpecificPoint spComp)
                spComp.RestoreTyped(s);
        }
        foreach (var s in data.sticks)
        {
            if (SaveResistry.TryGet(s.id, out ISaveable a) && a is Stick stickComp)
                stickComp.RestoreTyped(s);
        }

        MenuManager.Instance.SetElapsed(data.elapsed);
        
    }

    void HandleAfterIntroEvent(bool isContinue)
    {
        StartCoroutine(Coroutine_AfterIntro(isContinue));
    }
}
