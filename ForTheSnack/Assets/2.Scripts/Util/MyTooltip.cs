using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MyTooltip : SingletonDontDestroy<MyTooltip>
{
    [SerializeField]
    GameObject m_panel;
    [SerializeField]
    Text m_text;

    GameObject m_parent;
    protected override void Awake()
    {
        base.Awake();

        m_parent = GameObject.FindObjectOfType<Panel_Graphics>().gameObject;
        m_panel = Resources.Load<GameObject>("Panel_Tooltip");
        m_text = m_panel.transform.GetChild(0).GetComponent<Text>();
        m_panel = Instantiate(m_panel, m_parent.transform);
        
    }

    public void Show(string message, Vector2 pos)
    {
        m_panel.SetActive(true);
        m_text.text = message;

        m_panel.GetComponent<RectTransform>().localPosition = pos;
    }

    public void Hide()
    {
        m_panel.SetActive(false);
    }

}
