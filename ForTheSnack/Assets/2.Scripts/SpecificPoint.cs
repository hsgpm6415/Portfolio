using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpecificPoint : MonoBehaviour, ISaveable
{
    [SerializeField]
    Collider2D m_backTileCollidier;
    [SerializeField]
    Ladder m_ladder;
    [SerializeField]
    bool m_isTriggered;
    BoxCollider2D m_ladderCollider;
    UniqueId m_uniqueId;
    public string SaveId => m_uniqueId.Id;

    void Awake()
    {
        m_uniqueId = GetComponent<UniqueId>();

        if (m_ladder == null)
            m_ladder = transform.parent.GetComponentInChildren<Ladder>();

        if (m_backTileCollidier == null)
            m_backTileCollidier = transform.parent.Find("BackTiles").GetComponent<Collider2D>();

        m_ladderCollider = m_ladder.GetComponent<BoxCollider2D>();


    }

    void OnEnable() => SaveResistry.Resistry(this);
    void OnDisable() => SaveResistry.Unresistry(this);

    void Start()
    {
        m_ladderCollider.enabled = true;
        m_backTileCollidier.enabled = false;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_isTriggered || !collision.CompareTag("Foot")) return;

        m_isTriggered = true;
        ApplyState(m_isTriggered);
    }


    void ApplyState(bool isTriggered)
    {
        m_backTileCollidier.enabled = isTriggered;
        m_ladderCollider.enabled = !isTriggered;
    }

    public SpecificPointSave CaptureTyped()
    {
        var data = new SpecificPointSave() { id = SaveId, triggered = m_isTriggered };
        return data;
    }


    public void RestoreTyped(SpecificPointSave data)
    {
        m_isTriggered = data.triggered;
        ApplyState(m_isTriggered);
    }

}
