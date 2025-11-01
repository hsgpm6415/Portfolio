using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeSlider2D : MonoBehaviour
{
    public float m_slideStartAngle = 15f;
    public float m_slideEndAngle = 8f;
    public float m_slideForce = 5f;

    Rigidbody2D m_rigid2D;
    [SerializeField]
    bool m_isSliding;

    public bool Sliding { get { return m_isSliding; } }

    void Awake()
    {
        m_rigid2D = GetComponent<Rigidbody2D>();
        m_isSliding = false;
    }


    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Floor")) return;

        foreach (var contact in collision.contacts)
        {
            var angle = Vector2.Angle(contact.normal, Vector2.up);

            if (!m_isSliding && angle >= m_slideStartAngle)
            {
                Debug.Log("Sliding Start");
                m_isSliding = true;
            }
            else if(m_isSliding && angle <= m_slideEndAngle)
            {
                Debug.Log("Sliding End");
                m_isSliding = false;
            }
            
            if(m_isSliding)
            {
                Vector2 slideDir = new Vector2(contact.normal.y, -contact.normal.x).normalized;

                if(Vector2.Dot(slideDir, Vector2.down) < 0) slideDir = -slideDir;

                m_rigid2D.AddForce(slideDir * m_slideForce, ForceMode2D.Force);
            }

            break;
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = m_isSliding ? Color.red : Color.green;
        // 현재 속도를 화살표로 표시
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)m_rigid2D.velocity * 0.2f);
    }

    void Update()
    {
        if(m_isSliding && m_rigid2D.velocity.y == 0) m_isSliding = false;
    }
}
