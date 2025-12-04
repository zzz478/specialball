using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    #region 操作配置参数（Inspector面板可调）
    [Header("运动参数")]
    public float runSpeed = 5f;
    public float jumpForce = 8f;
    public int maxJumpCount = 2;

    [Header("速降参数（重点！）")]
    [Tooltip("速降时重力缩放倍数（原重力*此值，建议3-5）")]
    public float descendGravityScale = 4f;
    [Tooltip("速降触发按键（默认左Ctrl）")]
    public KeyCode descendKey = KeyCode.LeftControl;

    [Header("落地检测参数")]
    public float groundCheckOffsetY = 0.3f;
    public float groundCheckDistance = 0.8f;
    public LayerMask groundLayer;
    #endregion

    #region 私有变量
    private Rigidbody2D rb;
    private int currentJumpCount = 0;
    private bool isGrounded = false;
    private bool isDescending = false; // 速降状态
    private bool isTouchingGround = false;
    private float originalGravityScale; // 保存初始重力缩放
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 保存初始重力缩放（用于速降后恢复）
        originalGravityScale = rb.gravityScale;
        // 容错：自动匹配Floor图层
        if (groundLayer.value == 0) groundLayer = LayerMask.GetMask("Floor");
    }

    void Update()
    {
        // 游戏结束/暂停时禁用所有操作
        if (GameManager.Instance.isGameOver || GameManager.Instance.isPaused) return;

        CheckPlayerInput();
        // 双检测判定落地
        isGrounded = CheckGroundedByRay() || isTouchingGround;

        // 落地/跳跃时重置速降状态+恢复原重力
        if (isGrounded || Input.GetKeyDown(KeyCode.Space))
        {
            isDescending = false;
            rb.gravityScale = originalGravityScale;
        }

        // 落地重置跳跃次数
        if (isGrounded) currentJumpCount = 0;

        // 调试日志（可选，取消注释查看速降状态）
        // Debug.Log($"速降状态：{isDescending} | 落地状态：{isGrounded}");
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.isGameOver || GameManager.Instance.isPaused)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        // 自动奔跑
        AutoRun();
        // 速降逻辑（持续生效）
        UpdateDescendLogic();
    }

    #region 输入检测（优化速降触发）
    void CheckPlayerInput()
    {
        // 跳跃逻辑（保持正常）
        if (Input.GetKeyDown(KeyCode.Space) && !isDescending)
        {
            if (currentJumpCount < maxJumpCount)
            {
                Jump();
                currentJumpCount++;
                isGrounded = false;
                isTouchingGround = false;
            }
        }

        // 速降触发：按住按键+空中状态（GetKey持续检测，而非GetKeyDown）
        if (Input.GetKey(descendKey) && !isGrounded)
        {
            isDescending = true;
        }
    }
    #endregion

    #region 运动逻辑（核心修复速降）
    void AutoRun()
    {
        Vector2 currentVel = rb.velocity;
        currentVel.x = runSpeed;
        rb.velocity = currentVel;
    }

    void Jump()
    {
        // 重置Y轴速度，保证跳跃高度一致
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // 速降逻辑：修改重力缩放（比AddForce更直观，手感更明显）
    void UpdateDescendLogic()
    {
        if (isDescending)
        {
            // 提高重力缩放实现快速下落
            rb.gravityScale = descendGravityScale;
            // 可选：额外加瞬时下压力，强化速降效果
            rb.AddForce(Vector2.down * 5f, ForceMode2D.Impulse);
        }
        else
        {
            // 非速降时恢复原重力
            rb.gravityScale = originalGravityScale;
        }
    }

    // 射线落地检测
    bool CheckGroundedByRay()
    {
        Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y - groundCheckOffsetY);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null && rb.velocity.y <= 0f;
    }
    #endregion

    #region 碰撞器落地检测
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Floor"))
        {
            isTouchingGround = true;
            // 落地立即关闭速降
            isDescending = false;
            rb.gravityScale = originalGravityScale;
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.collider.CompareTag("Floor"))
        {
            isTouchingGround = false;
        }
    }
    #endregion

    #region 死亡判定
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Death"))
        {
            GameManager.Instance.GameOver();
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
    }
    #endregion

    #region 调试可视化
    void OnDrawGizmos()
    {
        Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y - groundCheckOffsetY);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * groundCheckDistance);
        // 速降状态可视化：场景视图显示蓝色射线（速降时）
        if (isDescending)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 1f);
        }
    }
    #endregion
}