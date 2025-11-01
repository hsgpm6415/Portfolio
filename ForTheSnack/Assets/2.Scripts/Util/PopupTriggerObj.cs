using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupTriggerObj : MonoBehaviour
{
    public PopupTriggerObjType m_type;

    [TextArea]
    public string m_message;

    [SerializeField]
    bool m_isTriggered = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Cat") || m_isTriggered) return;
        m_isTriggered = true;

        Debug.Log(collision.gameObject.name);
        if(m_type == PopupTriggerObjType.Cucumber)
        {
            var cat = collision.GetComponent<CatController>();
            SoundManager.Instance.PlaySFX("TouchedCucumber");
            cat.OnTriggeredCumcumber();
        }
        else if(m_type == PopupTriggerObjType.Snack)
        {
            var cat = collision.GetComponent<CatController>();
            cat.OnEndTriggered();
        }
        else
        {
            PopupManager.Instance.Show(m_message);
        }
        
        gameObject.SetActive(false);
    }
}



public enum PopupTriggerObjType
{
    None,
    Tomato,
    FishFillet,
    Cucumber,
    Snack,
    FishSteak,
    Max
}