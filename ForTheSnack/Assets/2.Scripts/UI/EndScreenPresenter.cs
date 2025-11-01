using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class EndScreenPresenter : MonoBehaviour
{
    [SerializeField] CanvasGroup m_group;
    [SerializeField] RectTransform m_contentWrapper;


    [Header("Anim")]
    [SerializeField] AnimationCurve m_ease;
    [SerializeField] float m_fadeInTime = 0.45f;
    [SerializeField] float m_fadeOutTime = 0.7f;
    [SerializeField] float m_offScreenFactor;
    Vector2 m_targetPos;

    void Awake()
    {
        m_group = GetComponent<CanvasGroup>();
        m_contentWrapper = transform.GetChild(1).GetComponent<RectTransform>();

        m_ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
        m_targetPos = m_contentWrapper.anchoredPosition;
        m_group.alpha = 0f;
        m_group.interactable = false;
        m_group.blocksRaycasts = false;

        m_fadeInTime = 0.45f;
        m_offScreenFactor = 1f;
    }

    void Start()
    {
        Panel_Ending.Instance.OnEnded += ShowContentWrapper;
    }

    void OnApplicationQuit()
    {
        Panel_Ending.Instance.OnEnded -= ShowContentWrapper;
    }

    public void ShowContentWrapper()
    {
        m_group.alpha = 1f;
        

        var rootRect = (RectTransform)m_group.transform;
        float offY = -rootRect.rect.height * m_offScreenFactor;

        m_contentWrapper.anchoredPosition = new Vector2(m_targetPos.x, offY);

        StartCoroutine(Coroutine_FadeInContent(offY, m_targetPos.y));
    }

    IEnumerator Coroutine_FadeInContent(float startY, float endY)
    {
        float t = 0f;

        while(t < m_fadeInTime)
        {
            t += Time.unscaledDeltaTime;
            float k = m_ease.Evaluate(t / m_fadeInTime);
            float y = Mathf.Lerp(startY, endY, k);
            m_contentWrapper.anchoredPosition = new Vector2(m_targetPos.x, y);
            yield return null;
        }

        m_contentWrapper.anchoredPosition = new Vector2(m_targetPos.x, endY);

        m_group.interactable = true;
        m_group.blocksRaycasts = true;

    }

    IEnumerator Coroutine_FadeOutContent()
    {
        var t = 0f;
        var currentAlpha = m_group.alpha;
        while(t < m_fadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            var k = (t / m_fadeOutTime);
            k = k * k * (3 - 2 * k);
            m_group.alpha = Mathf.Lerp(currentAlpha, 0f, k);
            yield return null;
        }
        
    }

    public void Hide()
    {
        m_group.interactable = false;
        m_group.blocksRaycasts = false;
        StartCoroutine(Coroutine_FadeOutContent());
        SoundManager.Instance.PlaySFX("ButtonSFX");
        GameProgressService.Instance.EndAndExitToTitle();
    }
    
}
