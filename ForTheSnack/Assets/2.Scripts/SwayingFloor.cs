using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SwayingFloor : InteractableObj
{
    [SerializeField]
    float m_second;

    [SerializeField]
    bool m_hasTrigger;

    [SerializeField]
    bool m_isFalling;

    SpriteRenderer m_sprRenderer;
    Animator m_animator;
    WaitForSeconds m_delay;
    Vector3 m_startPosition;



    protected override void OnAwake()
    {
        m_hasTrigger = false;

        m_sprRenderer   = GetComponentInChildren<SpriteRenderer>();
        m_animator      = GetComponent<Animator>();
        m_rigid2D       = GetComponent<Rigidbody2D>();

        m_collider2D    = GetComponent<BoxCollider2D>();
        m_delay         = new WaitForSeconds(m_second);
        m_startPosition = transform.position;
        m_type          = InteractableObjType.SwayingFloor;
    }

    private void OnEnable()
    {
        
        
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_hasTrigger) return;

        if (!m_hasTrigger) m_hasTrigger = true;

        if (collision.CompareTag("Foot") || collision.CompareTag("ClimbSensor"))
        {
            m_animator.SetTrigger("Swaying");
        }

    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!m_hasTrigger) return;
        if (m_hasTrigger) m_hasTrigger = false;

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_isFalling)
        {
            StartCoroutine(Coroutine_Hide());
            return;
        }


        if (collision.transform.CompareTag("Cat"))
        {
            ContactPoint2D contact = collision.contacts[0]; //2D 물리적 충돌 시 발생하는 모든 접촉점 중 첫번째
            var normal = contact.normal;                    //법선 벡터
            var rigid2D = collision.collider.GetComponent<Rigidbody2D>();

            var cir = collision.collider as CircleCollider2D;

            if (rigid2D != null)
            {
                if (normal.y < 0f)  // 고양이가 사물의 위에 있을 때, 사물이 고양이 아래에 있다.
                {
                    if (cir != null)
                    {
                        if (normal.x > 0)
                        {
                            rigid2D.AddForce(Vector2.left * 5f, ForceMode2D.Impulse);
                        }
                        else if (normal.x < 0)
                        {
                            rigid2D.AddForce(Vector2.right * 5f, ForceMode2D.Impulse);
                        }
                    }
                    SoundManager.Instance.PlaySFX("Hit");
                    return;
                }

                if (normal.x > 0)
                {
                    if (rigid2D.velocity.x < 0) return;
                    rigid2D.AddForce(Vector2.left * 5f, ForceMode2D.Impulse);
                    Debug.Log("플레이어가 오브젝트의 왼쪽에 부딪쳤다!");
                }
                else if (normal.x < 0)
                {
                    if (rigid2D.velocity.x > 0) return;
                    rigid2D.AddForce(Vector2.right * 5f, ForceMode2D.Impulse);
                    Debug.Log("플레이어가 오브젝트의 오른쪽에 부딪쳤다!");
                }
                SoundManager.Instance.PlaySFX("Hit");
            }
            else
            {
                Debug.Log("RigidBody is null");
            }
            return;
        }
    }

    IEnumerator Coroutine_Hide()
    {
        m_collider2D.enabled = false;
        m_sprRenderer.enabled = false;

        yield return m_delay;

        transform.position = m_startPosition;
        transform.rotation = Quaternion.identity;
        m_collider2D.enabled = true;
        m_sprRenderer.enabled = true;
        m_isFalling = false;

        m_rigid2D.bodyType = RigidbodyType2D.Static;
        m_rigid2D.constraints = RigidbodyConstraints2D.FreezeAll;

    }

    public void Anim_Falling()
    {
        m_isFalling = true;
        m_rigid2D.bodyType = RigidbodyType2D.Dynamic;
        m_rigid2D.constraints = RigidbodyConstraints2D.FreezePositionX;
        m_rigid2D.AddForce(Vector2.down * 10f, ForceMode2D.Impulse);    
        return;
    }
}
