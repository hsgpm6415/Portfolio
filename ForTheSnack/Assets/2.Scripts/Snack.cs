using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snack : InteractableObj
{

    protected override void OnAwake()
    {
        m_type = InteractableObjType.Snack;
    }

    public void PlayEndTrigger()
    {
        //SoundManager.Instance.PlaySFX("End1"); 
    }

/*
    IEnumerator Coroutine_Play()
    {
        
    }

    IEnumerator Coroutine_PlaySFX(string name)
    {
        
    }*/

}
