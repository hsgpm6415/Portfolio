using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : InteractableObj
{
    CatController m_cat;

    void Awake()
    {
        m_collider2D = GetComponent<BoxCollider2D>();
        m_cat = GameManager.Instance.Cat;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Cat")) return;

        if(m_cat == null) m_cat = GameManager.Instance.Cat;

        m_cat.IsWet = true;
    }

}
