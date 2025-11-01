using UnityEngine;

public class PlayerSaveProxy : MonoBehaviour
{
    [SerializeField]
    Rigidbody2D m_rb;

    public Vector2 GetPosition() => (Vector2)(transform.position);
    public Vector2 GetVelocity() => m_rb ? m_rb.velocity : Vector2.zero;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable() => PlayerLocator.Registry(this);
    private void OnDisable() => PlayerLocator.Unregistry(this);

    public void ApplyPosition(Vector2 position)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);

        if(m_rb != null)
        {
            m_rb.position = position;
        }
    }
    public void ApplyVelocity(Vector2 velocity)
    {
        if(m_rb != null)
        {
            m_rb.velocity = velocity;
        }
    }
}
