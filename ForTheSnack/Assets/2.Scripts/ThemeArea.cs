using System;
using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class ThemeArea : MonoBehaviour
{

    public ThemeProfile m_theme;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Foot"))
        {
            ThemeManager.Instance.NotifyEnter(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Foot"))
        {
            ThemeManager.Instance.NotifyExit(this);
        }
    }
}
