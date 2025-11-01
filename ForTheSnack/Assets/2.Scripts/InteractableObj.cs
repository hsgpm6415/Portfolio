using UnityEngine;

public enum InteractableObjType
{
    None = -1,
    Obstacle,
    Cucumber,
    Ladder,
    Rope,
    Water,
    Snack,
    SwayingFloor,
    MovingWalk,
    Stick,
    Max
}


public abstract class InteractableObj : MonoBehaviour
{
    #region [Component]
    protected Rigidbody2D m_rigid2D;
    protected Collider2D m_collider2D;
    #endregion

    #region [Variable]
    protected InteractableObjType m_type;
    public InteractableObjType Type { get { return m_type; } set { m_type = value; } }
    #endregion

    void Awake()
    {
        m_type = InteractableObjType.None;
        OnAwake();
    }

    void Start()
    {
        OnStart();
    }

    

    protected virtual void OnAwake() { }


    protected virtual void OnStart() { }
}
