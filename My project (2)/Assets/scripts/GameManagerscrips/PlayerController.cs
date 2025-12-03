using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    #region 配置参数（可在Inspector面板调整）
    [Header("运动参数")]
    [Tooltip("自动奔跑的X轴速度")]
    public float runSpeed = 5f;
    [Tooltip("跳跃力度")]
    public float jumpForce = 8f;

    [Header("资源引用")]
    [Tooltip("红色球体Sprite")]
    public Sprite ballRed;
    [Tooltip("蓝色球体Sprite")]
    public Sprite ballBlue;
    #endregion

    #region 私有变量
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isRed = true; // 初始颜色为红色
    private int jumpCount = 0; // 跳跃计数（控制二连跳）
    private int maxJumpCount = 2; // 最大跳跃次数（二连跳）
    private bool isDescending = false; // 速降状态标记
    private bool isGrounded = false; // 落地状态标记
    #endregion

    void Awake()
    {
        // 获取组件引用
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        // 初始化Sprite颜色
        sr.sprite = isRed ? ballRed : ballBlue;
    }

    void Update()
    {
        // 若游戏结束/暂停，禁用所有输入
        if (GameManager.Instance.isGameOver || GameManager.Instance.isPaused) return;

        // 检测输入指令
        InputDetection();
        // 落地状态检测（通过射线检测，避免碰撞体穿透问题）
        GroundCheck();
    }

    void FixedUpdate()
    {
        // 若游戏结束/暂停，停止运动
        if (GameManager.Instance.isGameOver || GameManager.Instance.isPaused)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 实现自动向右奔跑（固定X轴速度）
        AutoRun();
        // 速降逻辑执行
        if (isDescending) FastDescend();
    }

    #region 输入检测模块
    void InputDetection()
    {
        // 1. 跳跃/二连跳（空格触发，落地重置计数，速降时不可跳跃）
        if (Input.GetKeyDown(KeyCode.Space) && !isDescending)
        {
            if (jumpCount < maxJumpCount)
            {
                Jump();
                jumpCount++;
            }
        }

        // 2. 速降（左Ctrl触发，仅空中可触发，落地重置状态）
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isGrounded)
        {
            isDescending = true;
        }
        if (isGrounded) isDescending = false;

        // 3. 变色（Shift触发，切换红/蓝并同步Sprite）
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            ToggleColor();
        }
    }
    #endregion

    #region 运动逻辑模块
    /// <summary>
    /// 自动向右奔跑（固定X轴速度，不改变Y轴速度）
    /// </summary>
    void AutoRun()
    {
        Vector2 currentVelocity = rb.velocity;
        currentVelocity.x = runSpeed;
        rb.velocity = currentVelocity;
    }

    /// <summary>
    /// 跳跃逻辑
    /// </summary>
    void Jump()
    {
        // 重置Y轴速度，避免叠加重力影响跳跃高度
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    /// <summary>
    /// 速降逻辑（增大重力，快速下落）
    /// </summary>
    void FastDescend()
    {
        rb.AddForce(Vector2.down * 15f, ForceMode2D.Force);
    }

    /// <summary>
    /// 落地检测（向下发射短射线，判断是否接触地板）
    /// </summary>
    void GroundCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, LayerMask.GetMask("Floor"));
        if (hit.collider != null && hit.collider.CompareTag("Floor"))
        {
            isGrounded = true;
            jumpCount = 0; // 落地重置跳跃计数
        }
        else
        {
            isGrounded = false;
        }
    }
    #endregion

    #region 颜色逻辑模块
    /// <summary>
    /// 切换球体颜色（红/蓝）
    /// </summary>
    void ToggleColor()
    {
        isRed = !isRed;
        sr.sprite = isRed ? ballRed : ballBlue;
    }
    #endregion

    #region 碰撞与死亡判定
    void OnCollisionEnter2D(Collision2D other)
    {
        // 检测与地板的碰撞，判断颜色是否匹配
        if (other.collider.CompareTag("Floor"))
        {
            SpriteRenderer floorSr = other.collider.GetComponent<SpriteRenderer>();
            if (floorSr == null) return;

            // 地板红色 vs 球体蓝色，或地板蓝色 vs 球体红色，触发死亡
            bool isFloorRed = floorSr.sprite.name.Contains("red");
            if ((isFloorRed && !isRed) || (!isFloorRed && isRed))
            {
                GameManager.Instance.GameOver();
                // 死亡后禁用所有输入（通过GameManager状态控制，此处可额外冻结刚体）
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 若触碰死亡触发器（如掉落边界），触发死亡
        if (other.CompareTag("Death"))
        {
            GameManager.Instance.GameOver();
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
    }
    #endregion
}