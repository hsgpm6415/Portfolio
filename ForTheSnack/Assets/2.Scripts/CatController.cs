using System;
using UnityEngine;
using UnityEngine.Rendering;
using Unity;
using UnityEditor;
using System.Collections;
public class CatController : MonoBehaviour
{
    const float MAX_JUMPFORCE = 15f;
    const float MAX_JUMPFORCE_WHEN_WET = 13f;

    #region [Variable]
    [SerializeField]
    bool m_readyToJump;
    bool m_isClimbedLadder;
    bool m_isClimbedRope;
    [SerializeField]
    bool m_isGround;
    bool m_isWet;
    [SerializeField]
    bool m_onMovingWalk;
    public bool m_surprised;
    public bool m_ateSnack;

    [SerializeField]
    float m_jumpValue;
    float m_leftValue;
    float m_rightValue;
    float m_upValue;
    float m_downValue;
    float m_movingWalkSpeed;
    float m_jumpBufferTime;
    float m_jumpBufferCounter;
    float m_coyoteTime;
    [SerializeField ] float m_coyoteCounter;

    Vector2 m_moveVec;
    Vector3 m_lookToLeft;
    Vector3 m_lookToRight;
    [SerializeField]
    JumpState m_jumpState;
    [SerializeField]
    CatState m_catState;

    GameObject m_climbableObject;
    Stick m_stick;

    RaycastHit2D m_hit;
    #endregion

    #region [Component]
    Rigidbody2D m_rigid2D;
    Animator m_animator;
    SlopeSlider2D m_slopeSlider;
    #endregion

    #region [Check Ground]
    [Header("Ground Check BoxCast")]
    public Transform m_footPoint;           // 박스 중심 (발 중앙)
    Vector2 m_boxSize;
    public float m_extraDistance = 0.02f;   // 충돌 “놓침” 보정 거리
    public LayerMask m_groundLayer;
    #endregion

    #region [Property]
    public bool IsGround { get { return m_isGround; } set { m_isGround = value; } }
    public bool IsWet { get { return m_isWet; } set { m_isWet = value; } }
    #endregion

    public event Action<string> OnSurprised;
    public event Action OnSnackAte;
    void Awake()
    {
        m_rigid2D = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();        
        m_slopeSlider = GetComponent<SlopeSlider2D>();
        m_boxSize = new Vector2(0.88f, 0.05f);
        Init();
    }
    void FixedUpdate()
    {
        bool wasGounded = m_isGround;

        m_hit = Physics2D.BoxCast(m_footPoint.position, m_boxSize, 180f, Vector2.down, m_extraDistance, m_groundLayer);

        if (!m_slopeSlider.Sliding) m_isGround = m_hit.collider != null;
        if (m_slopeSlider.Sliding) m_isGround = false;

        if(wasGounded && !m_isGround)
        {
            m_coyoteCounter = m_coyoteTime;
        }

        Movement();
    }
    void Update()
    {
        if(!m_isGround && m_coyoteCounter > 0f)
        {
            m_coyoteCounter -= Time.unscaledDeltaTime;
        }
        InputAction();
        SetState();
    }
    void LateUpdate()
    {
        Rendering();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (m_footPoint == null) return;
        Gizmos.color = Color.cyan;
        
        Gizmos.DrawWireCube(
            m_footPoint.position + Vector3.down * m_extraDistance,
            m_boxSize
        );
    }
#endif

    #region [Trigger/Collision]
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Stick"))
        {
            m_stick = collision.GetComponent<Stick>();
        }
        if(collision.CompareTag("Floor"))
        {
            if (m_surprised)
            {
                StartCoroutine(Coroutine_ShowPopup());
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // 바닥
        if (collision.CompareTag("Floor") || collision.CompareTag("SwayingFloor"))
        {
            m_readyToJump = false;
            m_jumpValue = 0f;
            m_jumpState = JumpState.Jumping;
            m_catState = CatState.Jump;
            m_animator.SetInteger("JumpState", (int)m_jumpState);
        }

        // 무빙 워크
        if (collision.CompareTag("MovingWalk"))
        {
            m_movingWalkSpeed = 0f;
            m_onMovingWalk = false;
            m_readyToJump = false;
            m_jumpValue = 0f;
            m_jumpState = JumpState.Jumping;
            m_catState = CatState.Jump;
            m_animator.SetInteger("JumpState", (int)m_jumpState);
        }

        // 물
        if (collision.CompareTag("Water"))
        {
            m_isWet = false;
        }

        // 사다리
        if (collision.CompareTag("Ladder"))
        {
            m_animator.SetBool("IsClimbed", false);
            m_rigid2D.gravityScale = 1f;
            m_isClimbedLadder = false;
            m_upValue = 0f;
            m_downValue = 0f;
            m_climbableObject = null;
            
        }

        // 루프
        if (collision.CompareTag("Rope"))
        {
            m_animator.SetBool("IsClimbed", false);
            m_rigid2D.gravityScale = 1f;
            m_isClimbedRope = false;
            m_upValue = 0f;
            m_downValue = 0f;
            m_climbableObject = null;
            
        }

        // 레버
        if(collision.CompareTag("Stick"))
        {
            m_stick = null;
        }

    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Airplane"))
        {
            m_rigid2D.gravityScale      = 1f;
            m_upValue                   = 0f;
            m_downValue                 = 0f;
            m_isClimbedRope             = false;
            m_climbableObject           = null;
        }
    }

    #endregion

    #region [Private Func]
    void Init()
    {
        m_jumpValue         = 0.0f;
        m_leftValue         = 0.0f;
        m_rightValue        = 0.0f;
        m_upValue           = 0.0f;
        m_downValue         = 0.0f;
        m_movingWalkSpeed   = 0.0f;
        m_jumpBufferTime    = 0.1f;
        m_jumpBufferCounter = 0.0f;
        m_coyoteTime        = 0.08f;
        m_coyoteCounter     = 0.0f;

        m_isGround          = true;
        m_readyToJump       = false;
        m_isClimbedLadder   = false;
        m_isClimbedRope     = false;
        m_onMovingWalk      = false;
        m_surprised         = false;
        m_ateSnack          = false;

        m_lookToLeft        = new Vector3(0f, 0f, 0f);
        m_lookToRight       = new Vector3(0f, 180f, 0f);

        m_jumpState         = JumpState.None;
        m_catState          = CatState.Idle;

        m_climbableObject   = null;
        m_stick             = null;
    }
    void InputAction()
    {
        // 키 눌렀을 때
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (m_isWet) m_leftValue = -10f;
            else m_leftValue = -3.15f;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (m_isWet) m_rightValue = 10f;
            else m_rightValue = 3.15f;
        }
        // 키 땠을 때
        if (Input.GetKeyUp(KeyCode.LeftArrow))          m_leftValue = 0f;
        if (Input.GetKeyUp(KeyCode.RightArrow))         m_rightValue = 0f;
        // 루프
        if (m_isClimbedRope)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))      m_upValue = 3.15f;
            if (Input.GetKeyDown(KeyCode.DownArrow))    m_downValue = -3.15f;
            if (Input.GetKeyUp(KeyCode.UpArrow))        m_upValue = 0f;
            if (Input.GetKeyUp(KeyCode.DownArrow))      m_downValue = 0f;
        }
        // 점프
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 땅일 때
            if (m_isGround)
            {
                m_readyToJump = true;
                if (!m_onMovingWalk) m_rigid2D.velocity = Vector2.zero;
                m_jumpValue = 0f;
                m_jumpState = JumpState.ReadyToJump;
                m_animator.SetInteger("JumpState", (int)m_jumpState);
                m_coyoteCounter = 0;
            }
            // 루프 일 때
            else if (m_isClimbedRope)
            {
                m_jumpState = JumpState.Jumping;
                m_animator.SetInteger("JumpState", (int)m_jumpState);
                m_isClimbedRope = false;
                m_upValue = 0f;
                m_downValue = 0f;
                m_rigid2D.velocity = Vector2.zero;
                m_rigid2D.AddForce(new Vector2((m_leftValue + m_rightValue), 13.5f), ForceMode2D.Impulse);
                m_rigid2D.gravityScale = 1f;
            }
            // 땅이 아닐 때 점프 키를 눌렀을 경우의 유예 시간
            else if(!m_isGround)
            {
                if (m_coyoteCounter > 0f) m_isGround = true;
                else if(m_coyoteCounter <= 0f) StartCoroutine(Coroutine_JumpBufferCounter());
            }
        }
        // 점프 키를 누르고 있을 때
        if (Input.GetKey(KeyCode.Space) && m_isGround && m_readyToJump)
        {
            m_jumpValue += Time.deltaTime * 40f;

            var maxJumpForce = m_isWet ? MAX_JUMPFORCE_WHEN_WET : MAX_JUMPFORCE;

            if (m_jumpValue <= maxJumpForce) return;
            else
            {
                m_rigid2D.AddForce(new Vector2((m_leftValue + m_rightValue), maxJumpForce), ForceMode2D.Impulse);
                SoundManager.Instance.PlaySFX("JumpLong");

                m_jumpValue = 0f;
                m_readyToJump = false;
            }

        }

        // 점프 키를 땠을 때
        if (Input.GetKeyUp(KeyCode.Space) && m_readyToJump && !m_slopeSlider.Sliding)
        {
            m_rigid2D.AddForce(new Vector2((m_leftValue + m_rightValue), m_jumpValue), ForceMode2D.Impulse);

            if (m_jumpValue <= 10.0f) SoundManager.Instance.PlaySFX("JumpShort");
            else SoundManager.Instance.PlaySFX("JumpLong");

            m_jumpValue = 0f;
            m_readyToJump = false;
        }

        // ========== Stick 상호작용
        if(Input.GetKeyDown(KeyCode.F))
        {
            if (m_stick == null) return;
            else
            {
                m_stick.Toggle();
            }
        }
    }
    void Movement()
    {
        // 오이 트리거 || 엔딩 트리거
        if (m_surprised || m_ateSnack) return; 
        
        if (m_isGround && !m_readyToJump && !m_slopeSlider.Sliding)     // Default 상태
        {
            if(!m_onMovingWalk)
            {
                m_moveVec.Set((m_leftValue + m_rightValue), m_rigid2D.velocity.y);
                m_rigid2D.velocity = m_moveVec;
            }
            else
            {
                m_moveVec.Set((m_leftValue + m_rightValue + m_movingWalkSpeed), m_rigid2D.velocity.y);
                m_rigid2D.velocity = m_moveVec;
            }
        }
        else if(m_isGround && m_readyToJump && m_onMovingWalk)          // 무빙워크에서 점프 준비중일 때
        {
            m_moveVec.Set(m_movingWalkSpeed, m_rigid2D.velocity.y);
            m_rigid2D.velocity = m_moveVec;
        }
        else if (m_isClimbedRope)                                       //rope
        {
            transform.position = new Vector3(m_climbableObject.transform.position.x, transform.position.y, 0f);
            m_moveVec.Set(m_rigid2D.velocity.x, (m_upValue + m_downValue));
            m_rigid2D.velocity = m_moveVec;
        }
        else if (m_isClimbedLadder)                                     //Ladder
        {
            transform.position = new Vector3(m_climbableObject.transform.position.x, transform.position.y, 0f);
            m_moveVec.Set(0f, 3.15f);
            m_rigid2D.velocity = m_moveVec;
        }
    }
    void Rendering()
    {
        if (m_surprised || m_ateSnack) return;

        if (m_rigid2D.velocity.x > 0f && !m_slopeSlider.Sliding && !m_readyToJump) transform.rotation = Quaternion.Euler(m_lookToRight);

        if (m_rigid2D.velocity.x < 0f && !m_slopeSlider.Sliding && !m_readyToJump) transform.rotation = Quaternion.Euler(m_lookToLeft);

        if (m_jumpState == JumpState.Falling) m_animator.SetInteger("JumpState", (int)m_jumpState);

        if (m_catState == CatState.Walk && !m_slopeSlider.Sliding) m_animator.SetBool("IsWalk", true);
        else if (m_catState == CatState.Idle && !m_slopeSlider.Sliding) m_animator.SetBool("IsWalk", false);

        if (m_jumpState == JumpState.None) m_animator.SetInteger("JumpState", (int)m_jumpState);
    }
    void SetState()
    {
        if (m_rigid2D.velocity.y < 0f || m_slopeSlider.Sliding)
        {
            m_catState = CatState.Fall;
            m_jumpState = JumpState.Falling;
            return;
        }


        if ((m_leftValue != 0f || m_rightValue != 0f))
        {
            if (!m_slopeSlider.Sliding) m_catState = CatState.Walk;
        }
        else if ((m_leftValue == 0f && m_rightValue == 0f))
        {
            if (!m_slopeSlider.Sliding) m_catState = CatState.Idle;
        }



        if (m_jumpState != JumpState.None)
        {
            if (m_jumpState != JumpState.ReadyToJump && m_isGround && m_rigid2D.velocity.y == 0) m_jumpState = JumpState.None;
        }
    }
    IEnumerator Coroutine_ShowPopup()
    {
        m_catState = CatState.None;
        m_animator.SetBool("IsSurprised", false);
        m_surprised = false;
        yield return new WaitForSeconds(0.7f);
        OnSurprised?.Invoke("Cat Hate Cucumber");
    }
    IEnumerator Coroutine_StartEndingProcess()
    {
        m_rigid2D.velocity = Vector2.zero;
        m_jumpValue = 0f;

        yield return new WaitForSecondsRealtime(0.15f);
        SoundManager.Instance.PlaySFX("End1");
        yield return new WaitForSecondsRealtime(0.93f);
        m_rigid2D.AddForce(new Vector2(0f, 7.5f), ForceMode2D.Impulse);
        SoundManager.Instance.PlaySFX("JumpShort");
        yield return new WaitForSecondsRealtime(0.8f);
        m_rigid2D.AddForce(new Vector2(0f, 7.5f), ForceMode2D.Impulse);
        SoundManager.Instance.PlaySFX("JumpShort");

        yield return new WaitForSecondsRealtime(0.8f);

        var img = transform.GetChild(2).GetComponent<SpriteRenderer>();
        var color = img.color;
        float fadeInTime = 1f;
        float fadeOutTime = 1f;
        float t = 0f;

        while(t < fadeInTime)
        {
            t += Time.unscaledDeltaTime;
            var k = t / fadeInTime;
            k = k * k * (3 - 2 * k);
            color.a = Mathf.Lerp(0f, 1f, k);
            img.color = color;
            yield return null;
        }
        color.a = 1f;
        img.color = color;

        yield return new WaitForSecondsRealtime(1.3f);

        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            var k = t / fadeInTime;
            k = k * k * (3 - 2 * k);
            color.a = Mathf.Lerp(1f, 0f, k);
            img.color = color;
            yield return null;
        }
        color.a = 0f;
        img.color = color;

        OnSnackAte.Invoke();
    }
    IEnumerator Coroutine_JumpBufferCounter()
    {
        m_jumpBufferCounter = m_jumpBufferTime;
        while(true)
        {
            yield return null;
            m_jumpBufferCounter -= Time.unscaledDeltaTime;

            if (m_isGround) break;
        }

        if(m_jumpBufferCounter > 0.0f)
        {
            m_readyToJump = true;
            if (!m_onMovingWalk) m_rigid2D.velocity = Vector2.zero;
            m_jumpValue = 0f;
            m_jumpState = JumpState.ReadyToJump;
            m_animator.SetInteger("JumpState", (int)m_jumpState);
        }
        m_jumpBufferCounter = 0.0f;
    }
    #endregion
    #region [Public Func]
    public void HangOnARope(Rope rope)
    {
        if (rope == null) return;
        m_rigid2D.velocity = Vector2.zero;
        m_rigid2D.gravityScale = 0f;
        transform.position = new Vector3(rope.transform.position.x, transform.position.y, 0f);

        m_isClimbedRope = true;
        m_jumpState = JumpState.None;
        m_animator.SetInteger("JumpState", (int)m_jumpValue);
        m_animator.SetBool("IsClimbed", true);

        m_climbableObject = rope.gameObject;
    }
    public void HangOnALadder(Ladder ladder)
    {
        if(ladder == null) return;
        m_rigid2D.velocity = Vector2.zero;
        m_rigid2D.gravityScale = 0f;
        transform.position = new Vector3(ladder.transform.position.x, transform.position.y, 0f);

        m_isClimbedLadder = true;
        m_jumpState = JumpState.None;
        m_animator.SetInteger("JumpState", (int)m_jumpValue);
        m_animator.SetBool("IsClimbed", true);

        m_climbableObject = ladder.gameObject;
    }
    public void OnMovingWalk(MovingWalk movingWalk)
    {
        if(movingWalk == null) return;

        m_onMovingWalk = true;
        if(movingWalk.Arrow == MovingWalkArrow.Left)
        {
            m_movingWalkSpeed = -1 * movingWalk.Speed;
        }
        else if(movingWalk.Arrow == MovingWalkArrow.Right)
        {
            m_movingWalkSpeed = movingWalk.Speed;
        }
    }

    public void OnTriggeredCumcumber()
    {
        m_surprised = true;
        m_catState = CatState.Jump;
        m_animator.SetBool("IsSurprised", true);
        m_rigid2D.velocity = Vector2.zero;
        m_moveVec.Set(-7.0f, 5.0f);
        m_rigid2D.velocity = m_moveVec;
    }

    public void OnEndTriggered()
    {
        m_ateSnack = true;
        StartCoroutine(Coroutine_StartEndingProcess());
    }
    #endregion
}

public enum JumpState
{
    None = 0,
    ReadyToJump,
    Jumping,
    Falling,
    Max
}

public enum CatState
{
    None,
    Idle,
    Walk,
    Jump,
    Fall,
    Surprised,
    Max
}