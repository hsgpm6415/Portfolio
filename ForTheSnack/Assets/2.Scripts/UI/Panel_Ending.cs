using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Panel_Ending : SingletonDontDestroy<Panel_Ending>
{
    [SerializeField]
    TextMeshProUGUI m_playTimeText;

    [SerializeField]
    GameObject m_background;
    double m_playTime;

    [SerializeField]
    Button m_okButton;

    public event Action OnEnded;
    protected override void Awake()
    {
        base.Awake();
        m_playTime = 0L;
        m_background = transform.GetChild(0).gameObject;
        m_playTimeText = transform.GetChild(1).GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>();
        m_okButton = GetComponentInChildren<Button>();
        m_okButton.onClick.AddListener(OnClicked);
    }

    protected override void Start()
    {
        
    }

    void OnApplicationQuit()
    {
        
    }

    public void Init(SceneType sceneType)
    {
        if(sceneType == SceneType.Main)
        {
            var cat = FindObjectOfType<CatController>();
            cat.OnSnackAte += HandlerSnackAteEvent;
        }    
    }

    public void HandlerSnackAteEvent()
    {
        StartCoroutine(Coroutine_Show());
        var cat = FindObjectOfType<CatController>();
        cat.OnSnackAte -= HandlerSnackAteEvent;
    }

    void Hide() => GetComponent<EndScreenPresenter>().Hide();

    IEnumerator Coroutine_Show()
    {
        m_playTime = MenuManager.Instance.Elapsed;

        int sec = (int)Math.Floor(m_playTime);
        
        var hour = sec / 3600;
        var min = (sec % 3600) / 60;
        var s = sec % 60;

        m_playTimeText.SetText("{0:00}:{1:00}:{2:00}", hour, min, s);

        var img = m_background.GetComponent<Image>();
        var color = img.color;
        float target = 1.0f;
        float t = 0f;

        while (t <= target)
        {
            t += Time.unscaledDeltaTime;
            var k = (t / target);
            k = k * k * (3 - 2 * k);
            color.a = Mathf.Lerp(0f, 0.95f, k);
            img.color = color;
            yield return null;
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        color.a = 0.95f;
        OnEnded.Invoke();
    }
    
    void OnClicked() => Hide();

}
