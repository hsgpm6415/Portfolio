using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Panel_Intro : SingletonDontDestroy<Panel_Intro>
{
    [SerializeField]
    Text m_textFirst;
    [SerializeField]
    Text m_textSecond;
    [SerializeField]
    Text m_textThird;


    public event Action<bool> OnIntroFinished;

    protected override void Awake()
    {
        base.Awake();
        m_textFirst     = transform.GetChild(0).GetComponent<Text>();
        m_textSecond    = transform.GetChild(1).GetComponent<Text>();
        m_textThird     = transform.GetChild(2).GetComponent<Text>();
    }

    protected override void Start()
    {
        GameProgressService.Instance.OnIntroStarted += HandleStartIntroEvent;
        GameProgressService.Instance.OnIntroFinished += HandleFinishIntroEvent;
        gameObject.SetActive(false);
    }

    void OnApplicationQuit()
    {
        GameProgressService.Instance.OnIntroStarted -= HandleStartIntroEvent;
        GameProgressService.Instance.OnIntroFinished -= HandleFinishIntroEvent;
    }

    void HandleStartIntroEvent(bool isContinue)
    {
        gameObject.SetActive(true);
        StartCoroutine(Coroutine_FadeInOutText(m_textFirst, m_textSecond, m_textThird, isContinue));
    }

    void HandleFinishIntroEvent()
    {
        gameObject.SetActive(false);
    }
    IEnumerator Coroutine_Show(Text text)
    {
        float time = 1.5f;
        float t = 0f;
        var color = text.color;

        while(t < time)
        {
            t += Time.unscaledDeltaTime;
            float k = t / time;
            k = k * k * (3 - 2 * k);
            color.a = Mathf.Lerp(0f, 1f, k);
            text.color = color;
            yield return null;
        }
    }
    IEnumerator Coroutine_Hide(Text text)
    {
        float time = 2.5f;
        float t = 0f;
        var color = text.color;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float k = t / time;
            k = k * k * (3 - 2 * k);
            color.a = Mathf.Lerp(1f, 0f, k);
            text.color = color;
            yield return null;
        }
    }
    IEnumerator Coroutine_FadeInOutText(Text text1, Text text2, Text text3, bool isContinue)
    {
        yield return StartCoroutine(Coroutine_Show(text1));
        yield return StartCoroutine(Coroutine_Hide(text1));

        yield return StartCoroutine(Coroutine_Show(text2));
        yield return StartCoroutine(Coroutine_Hide(text2));

        yield return StartCoroutine(Coroutine_Show(text3));
        yield return StartCoroutine(Coroutine_Hide(text3));

        OnIntroFinished?.Invoke(isContinue);
    }
}
