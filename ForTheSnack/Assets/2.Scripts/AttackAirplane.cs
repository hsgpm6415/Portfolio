using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAirplane : MonoBehaviour
{
    BoxCollider2D m_collider2D;
    void Awake()
    {
        m_collider2D = GetComponent<BoxCollider2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Cat"))
        {
            m_collider2D.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Finish"))
        {
            m_collider2D.isTrigger = false;
        }
    }
}
