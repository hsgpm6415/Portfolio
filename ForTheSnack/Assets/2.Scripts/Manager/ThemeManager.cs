using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeManager : SingletonDontDestroy<ThemeManager>
{
    
    public ThemeProfile m_defaultTheme;

    public float m_enterDwell = 0.15f;              // 진입 후 전환까지 최소 체류 시간
    public float m_exitLinger = 0.25f;              // 이탈 후 실제 제거까지 유예 시간
    public float m_minSwitchInterval = 0.45f;       // 전환 쿨다운

    public event Action<ThemeProfile> OnThemeChanged;

    class Entry { public float m_enterTime; public bool m_eligible; }
    readonly Dictionary<ThemeArea, Entry> m_entryDic = new ();
    readonly Dictionary<ThemeArea, Coroutine> m_exitTimerDic = new();

    ThemeProfile m_current;
    float m_lastSwitch;

    public ThemeArea m_currentThemeArea;

    bool m_isLoad;

    public bool IsLoad { get { return m_isLoad; } set { m_isLoad = value; } }

    protected override void Awake()
    {
        base.Awake();
        m_isLoad = false;
        m_lastSwitch = 0f;
        m_current = m_defaultTheme;
    }

    #region [Public Func]
    public void NotifyEnter(ThemeArea area)
    {
        Debug.Log(area.m_theme);
        if(m_exitTimerDic.TryGetValue(area, out var coroutine))
        {
            StopCoroutine(coroutine);
            m_exitTimerDic.Remove(area);
        }

        float now = Time.unscaledTime;
        m_entryDic[area] = new Entry() { m_enterTime = now, m_eligible = false };
        StartCoroutine(Coroutine_Dwell(area));
    }

    public void NotifyExit(ThemeArea area)
    {
        if (m_exitTimerDic.ContainsKey(area)) return;
        m_exitTimerDic[area] = StartCoroutine(Coroutine_Linger(area));
    }

    public void ReevaluateAt(Collider2D playerCol)
    {
        // 1) 물리 최신화(프레임 넘기지 않고 바로 검사할 때)
        Physics2D.SyncTransforms();

        // 2) 현재 활성 영역 초기화(필요시 linger 무시)
        m_entryDic.Clear();

        // 3) 플레이어 주변 ThemeArea만 필터링해서 찾기
        var results = new List<Collider2D>(8);
        var filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Wall")); // ThemeArea 전용 레이어 권장
        filter.useTriggers = true; // 사용 가능하면 켬

        var count = playerCol.OverlapCollider(filter, results);
        for (int i = 0; i < count; i++)
        {
            var area = results[i].GetComponent<ThemeArea>();
            if (area == null) continue;

            // dwell/쿨다운 무시하고 즉시 후보로
            m_entryDic[area] = new Entry { m_enterTime = Time.unscaledTime, m_eligible = true };
        }

        // 4) 쿨다운 무시 평가
        Evaluate(force: true);
    }

    #endregion

    #region [Private Func]
    void Evaluate(bool force = false)
    {
        if (!force && Time.unscaledTime - m_lastSwitch < m_minSwitchInterval)
        {
            Debug.Log("skip");
            return;
        }

        ThemeArea bestArea = null;
        float bestTime = float.NegativeInfinity;

        foreach(var kv in m_entryDic)
        {
            var area = kv.Key;
            var entry = kv.Value;

            if (!entry.m_eligible) continue;
            if(entry.m_enterTime > bestTime)
            {
                bestTime = entry.m_enterTime;
                bestArea = area;
            }
        }

        var next = (bestArea != null) ? bestArea.m_theme : m_defaultTheme;

        if (next == m_current) return;

        m_current = next;
        m_currentThemeArea = bestArea;
        m_lastSwitch = Time.unscaledTime;
        OnThemeChanged?.Invoke(m_current);

    }

    
    #endregion


    #region [Coroutine]
    IEnumerator Coroutine_Dwell(ThemeArea area)
    {
        yield return new WaitForSecondsRealtime(m_enterDwell);
        if(m_entryDic.TryGetValue(area, out var entry))
        {
            entry.m_eligible = true;
            Evaluate();
        }
    }

    IEnumerator Coroutine_Linger(ThemeArea area)
    {
        yield return new WaitForSecondsRealtime (m_exitLinger);
        m_entryDic.Remove(area);
        m_exitTimerDic.Remove (area);
        Evaluate();
    }

    #endregion
}
