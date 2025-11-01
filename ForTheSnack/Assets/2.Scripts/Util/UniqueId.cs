using System;
using UnityEngine;

public class UniqueId : MonoBehaviour
{
    [SerializeField]
    string id;
    public string Id { get { return id; } }


#if UNITY_EDITOR
    void OnValidate()
    {
        if(string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
        }
    }
#endif
}
