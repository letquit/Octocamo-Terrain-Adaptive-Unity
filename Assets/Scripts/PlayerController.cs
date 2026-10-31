using UnityEngine;

// 教程版简单角色控制器（站立/趴下/移动），Unity 6 适配：
// 只覆盖水平速度，保留垂直速度，这样重力能让机器人正常落到地形上。
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public Collider standingCollider;
    public Collider proneCollider;

    private bool isMoving = false;
    private bool isProne = false;

    private Animator animator;
    private Rigidbody rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            isProne = !isProne;
            if (animator != null)
            {
                animator.SetBool("IsProne", isProne);
            }
            if (standingCollider != null)
            {
                standingCollider.enabled = !isProne;
            }
            if (proneCollider != null)
            {
                proneCollider.enabled = isProne;
            }
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(x, 0, z) * moveSpeed * (isProne ? 0.4f : 1.0f);
        Vector3 worldMove = transform.TransformVector(move);

        if (rb != null)
        {
            // Unity 6 用 linearVelocity；只改水平分量，保留重力产生的竖直速度
            rb.linearVelocity = new Vector3(worldMove.x, rb.linearVelocity.y, worldMove.z);
        }

        isMoving = Mathf.Abs(x) > 0.05f || Mathf.Abs(z) > 0.05f;
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}
