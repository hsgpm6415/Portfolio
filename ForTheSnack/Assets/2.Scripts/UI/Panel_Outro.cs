using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_Outro : SingletonDontDestroy<Panel_Outro>
{

    protected override void Awake()
    {
        base.Awake();

    }

    protected override void Start()
    {
        gameObject.SetActive(false);
    }

    
}
