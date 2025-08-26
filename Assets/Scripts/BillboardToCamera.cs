using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    private Transform cam;
    private void Awake()
    {
        if (Camera.main != null) cam = Camera.main.transform;
    }
    private void LateUpdate()
    {
        if (cam == null) return;
        transform.rotation = Quaternion.LookRotation(transform.position - cam.position);
    }
}