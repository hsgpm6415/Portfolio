using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelController : SingletonDontDestroy<PanelController>
{
    [SerializeField]
    GameObject m_menuPanel;
    [SerializeField]
    GameObject m_introPanel;

    [SerializeField]
    Selectable m_selectable;

    protected override void Awake()
    {
        base.Awake();
        m_menuPanel     = transform.GetChild(0).gameObject;
        m_introPanel    = transform.GetChild(1).gameObject;
    }
    protected override void Start()
    {
        GameManager.Instance.OnPauseChanged += Show;
        Show(GameManager.Instance.IsPause);
    }
    private void OnDisable()
    {
        GameManager.Instance.OnPauseChanged -= Show;
    }
    void Show(bool pause)
    {
        m_menuPanel.SetActive(pause);
        if(pause && EventSystem.current && m_selectable)
        {
            EventSystem.current.SetSelectedGameObject(m_selectable.gameObject);
        }
    }
}
