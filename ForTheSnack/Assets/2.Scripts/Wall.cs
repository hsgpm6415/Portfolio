using UnityEngine;


public class Wall : MonoBehaviour
{
    public void SetCameraPos()
    {
        Camera.main.transform.position = transform.position + Vector3.back * 10f;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Foot"))
        {
            SetCameraPos();
        }
    }
}
