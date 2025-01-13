using UnityEngine;

public class LookCamera : LookAt
{
    void Start()
    {
        m_LookAt = Camera.main.transform;
    }
}
