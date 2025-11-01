using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : InteractableObj
{
    BoxCollider2D m_collider;
    SpriteRenderer[] m_sprites;

    protected override void OnAwake()
    {
        m_collider = GetComponent<BoxCollider2D>();
        m_sprites = gameObject.GetComponentsInChildren<SpriteRenderer>();        
    }
    public void ApplyState(bool isOn)
    {
        m_collider.enabled = !isOn;
        foreach(SpriteRenderer sprite in m_sprites)
        {
            sprite.enabled = !isOn;
        }
    }


}
