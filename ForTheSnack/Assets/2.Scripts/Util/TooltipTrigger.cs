using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea] public string m_message;
    public void OnPointerEnter(PointerEventData eventData)
    {
        MyTooltip.Instance.Show(m_message, new Vector2(0f, -200f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MyTooltip.Instance.Hide();
        
    }

   
}
