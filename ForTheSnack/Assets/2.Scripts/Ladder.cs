using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : InteractableObj
{
    [SerializeField]
    bool m_isEntered;
    CatController m_cat;

    void Start()
    {
        m_type = InteractableObjType.Ladder;
        m_isEntered = false;
        m_cat = GameManager.Instance.Cat;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("ClimbSensor")) return;
        
        if(m_cat == null) m_cat = GameManager.Instance.Cat;

        m_isEntered = true;
        StartCoroutine(Coroutine_PressedUpArrow());
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("ClimbSensor")) return;

        m_isEntered = false;
    }
    public IEnumerator Coroutine_PressedUpArrow()
    {
        while (!m_cat.IsGround)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (m_isEntered)
                {
                    m_cat.HangOnALadder(this);
                }
                yield break;
            }
            yield return null;
        }
        yield break;
    }
}
