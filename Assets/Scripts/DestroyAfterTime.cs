using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float time = 0.1f;

    void Start()
    {
        Destroy(gameObject, time);
    }
}