using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Stick : InteractableObj, ISaveable
{
    [SerializeField]
    StickColorType m_stickColorType;

    [SerializeField]
    Sprite m_on;
    [SerializeField]
    Sprite m_off;
    [SerializeField]
    Laser m_laser;

    bool m_isOn;

    UniqueId m_uniqueId;
    SpriteRenderer m_sprRenderer;

    public string SaveId => m_uniqueId.Id;
    protected override void OnAwake()
    {
        m_uniqueId = GetComponent<UniqueId>();

        if(m_on == null || m_off == null || m_laser == null)
        {
            switch(m_stickColorType)
            {
                case StickColorType.Red:
                    {
                        m_on = Resources.Load<Sprite>("SwitchRedOn");
                        m_off = Resources.Load<Sprite>("SwitchRedOff");
                        m_laser = GameObject.FindGameObjectWithTag("Laser_Red").GetComponent<Laser>();
                    }
                    break;
                case StickColorType.Green:
                    {
                        m_on = Resources.Load<Sprite>("SwitchGreenOn");
                        m_off = Resources.Load<Sprite>("SwitchGreenOff");
                        m_laser = GameObject.FindGameObjectWithTag("Laser_Green").GetComponent<Laser>();
                    }
                    break;
                case StickColorType.Yellow:
                    {
                        m_on = Resources.Load<Sprite>("SwitchYellowOn");
                        m_off = Resources.Load<Sprite>("SwitchYellowOff");
                        m_laser = GameObject.FindGameObjectWithTag("Laser_Yellow").GetComponent<Laser>();
                    }
                    break;
            }
        }

        m_isOn = false;
        m_type = InteractableObjType.Stick;
        m_sprRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    void OnEnable() => SaveResistry.Resistry(this);
    void OnDisable() => SaveResistry.Unresistry(this);


    public void Toggle()
    {
        SoundManager.Instance.PlaySFX("Stick");
        m_isOn = !m_isOn;
        ApplyState(m_isOn);
    }

    void ApplyState(bool isOn)
    {
        if (isOn) m_sprRenderer.sprite = m_on;
        else if (!isOn) m_sprRenderer.sprite = m_off;

        m_laser.ApplyState(isOn);
    }

    public StickSave CaptureTyped()
    {
        var data = new StickSave() { id = SaveId, isOn = m_isOn };
        return data;
    }

    public void RestoreTyped(StickSave save)
    {
        m_isOn = save.isOn;
        ApplyState(m_isOn);
    }

    enum StickColorType
    {
        None = -1,
        Red,
        Green,
        Yellow,
        Max
    }
}

