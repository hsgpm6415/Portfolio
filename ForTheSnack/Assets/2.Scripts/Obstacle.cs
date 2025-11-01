using UnityEngine;

public class Obstacle : InteractableObj
{

    void Awake()
    {
        m_type = InteractableObjType.Obstacle;
        m_rigid2D = GetComponent<Rigidbody2D>();
        m_collider2D = GetComponent<Collider2D>();
        
    }
    protected override void OnStart()
    {
        
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Cat")) return;
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
                    SoundManager.Instance.PlaySFX("Hit");
                }
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






    }
}
