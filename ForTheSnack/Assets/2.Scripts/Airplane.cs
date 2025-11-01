using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airplane : MonoBehaviour
{
    [SerializeField]
    float m_speed;

    [SerializeField]
    FlyingDir m_dir;

    [SerializeField]
    float m_respawnDelay;

    [SerializeField]
    float m_toggleSecond;

    Rigidbody2D m_rigid2D;
    Vector3 m_startPosition;
    SpriteRenderer[] m_sprites;
    BoxCollider2D m_collider;
    WaitForSeconds m_wait;

    [SerializeField]
    Vector2 m_curDir;
    WaitForSeconds m_toggleWait;

    void Awake()
    {
        m_rigid2D = GetComponent<Rigidbody2D>();
        m_sprites = GetComponentsInChildren<SpriteRenderer>();
        m_collider = GetComponent<BoxCollider2D>();
        m_startPosition = transform.position;
        m_wait = new WaitForSeconds(m_respawnDelay);
        m_toggleWait = new WaitForSeconds(m_toggleSecond);
        
    }

    void Start()
    {
        if(m_dir == FlyingDir.Bilateral) StartCoroutine(Coroutine_ToggleDir());
    }

    void FixedUpdate()
    {
        if(m_dir == FlyingDir.Left)
        {
            m_rigid2D.MovePosition(m_rigid2D.position + Vector2.left * m_speed * Time.fixedDeltaTime);
        }
        else if (m_dir == FlyingDir.Right)
        {
            m_rigid2D.MovePosition(m_rigid2D.position + Vector2.right * m_speed * Time.fixedDeltaTime);
        }
        else if(m_dir == FlyingDir.Bilateral)
        {
            m_rigid2D.MovePosition(m_rigid2D.position + m_curDir * m_speed * Time.fixedDeltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Finish"))
        {
            StartCoroutine(Coroutine_Hide());
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Finish"))
        {
            StartCoroutine(Coroutine_Hide());
        } 
    }

    IEnumerator Coroutine_Hide()
    {
        m_collider.enabled = false;
        foreach (var sprite in m_sprites)
        {
            sprite.enabled = false;
        }

        yield return m_wait;

        transform.position = m_startPosition;
        m_collider.enabled = true;

        foreach (var sprite in m_sprites)
        {
            sprite.enabled = true;
        }
    }

    IEnumerator Coroutine_ToggleDir() // ¿Ô´Ù °¬´Ù
    {
        while (true)
        {
            yield return m_toggleWait;
            if (m_curDir == Vector2.left) m_curDir = Vector2.right;
            else if (m_curDir == Vector2.right) m_curDir = Vector2.left;

            foreach (var sprite in m_sprites)
            {
                sprite.flipX = !sprite.flipX;
            }
        }
    }
}

[Serializable]
public enum FlyingDir
{
    None,
    Left,
    Right,
    Bilateral,
    Max,
}