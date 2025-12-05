using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    #region 核心配置参数（Inspector面板可调）
    [Header("运动参数")]
    public float runSpeed = 5f;
    public float jumpForce = 8f;
    public int maxJumpCount = 2;

    [Header("速降参数")]
    [Tooltip("速降时重力缩放倍数（原重力*此值，建议3-5）")]
    public float descendGravityScale = 4f;
    [Tooltip("速降触发按键（默认左Ctrl）")]
    public KeyCode descendKey = KeyCode.LeftControl;

    [Header("变色功能参数")]
    [Tooltip("红色球体Sprite")]
    public Sprite ballRed;
    [Tooltip("蓝色球体Sprite")]
    public Sprite ballBlue;
    [Tooltip("变色触发按键（默认Shift）")]
    public KeyCode colorSwitchKey = KeyCode.LeftShift;

    [Header("落地检测参数")]
    public float groundCheckOffsetY = 0.3f;
    public float groundCheckDistance = 0.8f;
    public LayerMask groundLayer;
    #endregion

    #region 私有变量
    private Rigidbody2D rb;
    private SpriteRenderer sr; // 新增：Sprite渲染器引用
    private int currentJumpCount = 0;
    private bool isGrounded = false;
    private bool isDescending = false;
    private bool isTouchingGround = false;
    private float originalGravityScale;
    private bool isRed = true; // 新增：当前颜色状态（默认红色）
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>(); // 获取Sprite渲染器
        originalGravityScale = rb.gravityScale;

        // 容错：自动匹配Floor图层
        if (groundLayer.value == 0) groundLayer = LayerMask.GetMask("Floor");
        // 初始化小球颜色（默认红色）
        InitBallColor();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
    }

    void Update()
    {
        // 空引用容错：先检查GameManager是否存在
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.isGameOver || GameManager.Instance.isPaused) return;

        if (transform.position.y > 10f || transform.position.y < -10f)
        {
            GameManager.Instance.GameOver();
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            // 补充：死亡后视觉反馈
            if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.5f);
            enabled = false;
            return;
        }

        CheckPlayerInput(); // 包含变色输入检测
        isGrounded = CheckGroundedByRay() || isTouchingGround;

        // 落地/跳跃时重置速降状态+恢复原重力
        if (isGrounded || Input.GetKeyDown(KeyCode.Space))
        {
            isDescending = false;
            rb.gravityScale = originalGravityScale;
        }

        // 落地重置跳跃次数
        if (isGrounded) currentJumpCount = 0;
    }

    void FixedUpdate()
    {
        // 空引用容错
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.isGameOver || GameManager.Instance.isPaused)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        AutoRun();
        UpdateDescendLogic(); // 速降逻辑
    }

    #region 输入检测（新增变色触发）
    void CheckPlayerInput()
    {
        // 1. 跳跃逻辑
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

        // 2. 速降触发（按住生效）
        if (Input.GetKey(descendKey) && !isGrounded)
        {
            isDescending = true;
        }

        // 3. 变色触发（按一下切换，游戏中随时可触发）
        if (Input.GetKeyDown(colorSwitchKey))
        {
            ToggleBallColor();
        }
    }
    #endregion

    #region 核心功能逻辑
    // 自动奔跑
    void AutoRun()
    {
        Vector2 currentVel = rb.velocity;
        currentVel.x = runSpeed;
        rb.velocity = currentVel;
    }

    // 跳跃逻辑
    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        Invoke(nameof(ResetScale), 0.2f);
    }

    void ResetScale()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isGameOver)
        {
            transform.localScale = Vector3.one;
        }
    }

    // 速降逻辑
    void UpdateDescendLogic()
    {
        if (isDescending)
        {
            rb.gravityScale = descendGravityScale;
            rb.AddForce(Vector2.down * 5f, ForceMode2D.Impulse); // 额外下压力
        }
        else
        {
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

    // 初始化小球颜色
    void InitBallColor()
    {
        if (sr == null) return;
        // 优先使用配置的Sprite，无配置则提示
        if (isRed)
        {
            sr.sprite = ballRed != null ? ballRed : sr.sprite;
        }
        else
        {
            sr.sprite = ballBlue != null ? ballBlue : sr.sprite;
        }
        if (ballRed == null || ballBlue == null)
        {
            Debug.LogWarning("请在Inspector面板为小球配置红/蓝Sprite！");
        }
    }

    // 切换小球颜色（核心变色方法）
    void ToggleBallColor()
    {
        if (sr == null) return;

        if (ballRed == null || ballBlue == null)
        {
            Debug.LogError("请为PlayerController配置ballRed/ballBlue Sprite！");
            return;
        }

        // 切换颜色状态
        isRed = !isRed;
        // 更新Sprite
        sr.sprite = isRed ? ballRed : ballBlue;
        // 调试日志（可选）
        Debug.Log($"小球已切换为：{(isRed ? "红色" : "蓝色")}");
    }
    #endregion

    #region 碰撞检测 + 死亡判定（合并重复方法）
    // 合并后的唯一OnCollisionEnter2D方法（删除了重复的那一个）
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Floor"))
        {
            isTouchingGround = true;
            isDescending = false;
            rb.gravityScale = originalGravityScale;

            // 地板颜色匹配判定（核心死亡逻辑）
            CheckFloorColorMatch(other.collider);
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.collider.CompareTag("Floor"))
        {
            isTouchingGround = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance == null) return; // 空引用容错

        if (other.CompareTag("Death"))
        {
            GameManager.Instance.GameOver();
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.5f); // 半透明表示死亡
            enabled = false; // 禁用脚本，停止所有输入检测
        }
    }

    /// <summary>
    /// 检测地板颜色与玩家是否匹配，不匹配则死亡
    /// </summary>
    /// <param name="floorCollider">碰撞到的地板碰撞体</param>
    void CheckFloorColorMatch(Collider2D floorCollider)
    {
        if (GameManager.Instance == null || GameManager.Instance.isGameOver) return; // 避免重复触发

        SpriteRenderer floorSr = floorCollider.GetComponent<SpriteRenderer>();
        if (floorSr == null) return;

        // 判定规则：地板名称含"red"=红地板，含"blue"=蓝地板
        bool isFloorRed = floorSr.sprite.name.Contains("red");
        bool isFloorBlue = floorSr.sprite.name.Contains("blue");

        // 异色判定：红玩家踩蓝地板 / 蓝玩家踩红地板 → 死亡
        if ((isRed && isFloorBlue) || (!isRed && isFloorRed))
        {
            GameManager.Instance.GameOver();
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.5f);
            enabled = false;
        }
    }
    #endregion

    #region 调试可视化
    void OnDrawGizmos()
    {
        // 落地检测射线（绿=落地，红=未落地）
        Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y - groundCheckOffsetY);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * groundCheckDistance);

        // 速降状态射线（蓝色）
        if (isDescending)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 1f);
        }

        // 颜色状态可视化（红/蓝小球标记）
        Gizmos.color = isRed ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.2f);
    }
    #endregion
}