using UnityEngine;

// 简单的第三人称跟随相机：自动找到场景里的 PlayerController 作为跟随目标。
public class SimpleFollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3.5f, -6f);
    public float lookHeight = 1.2f;
    public float smoothTime = 0.15f;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null)
            {
                target = pc.transform;
            }
            if (target == null)
            {
                return;
            }
        }

        Vector3 desired = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}
