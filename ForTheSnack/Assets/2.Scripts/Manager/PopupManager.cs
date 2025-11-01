using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : SingletonMonoBehaviour<PopupManager>
{

    [SerializeField] GameObject m_panel;
    [SerializeField] Text m_text;
    [SerializeField] Button m_button;
    Transform m_parent;
    protected override void Awake()
    {
        base.Awake();
        m_parent = PanelController.Instance.gameObject.GetComponent<RectTransform>().transform;
        m_panel = Resources.Load<GameObject>("Panel_Popup");
        
        
    }

    protected override void Start()
    {
        base.Start();
        var cat = FindObjectOfType<CatController>();
        cat.OnSurprised += Show;
    }

    private void OnApplicationQuit()
    {
        var cat = FindObjectOfType<CatController>();
        cat.OnSurprised -= Show;
    }

    public void Show(string text)
    {
        m_panel = Instantiate(m_panel, m_parent);

        m_text = m_panel.GetComponentInChildren<Text>();
        m_button = m_panel.GetComponentInChildren<Button>();

        m_text.text = text;

        m_button.onClick.AddListener(Hide);


        m_panel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

    }

    public void Hide()
    {
        m_panel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    
}
