using UnityEngine;
using System.Collections;

public class ProjectileBehavior : MonoBehaviour
{
    public float lifespan = 3f;
    private Rigidbody2D rb;
    public int maxBounces = 3; // 最大反弹次数
    private int bounceCount = 0; // 当前反弹次数

    // 碰撞状态控制（与图层切换配合，双重保障）
    public bool canCollideWithPlayer = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 初始状态：不可与玩家碰撞
        canCollideWithPlayer = false;

        // 延迟启用玩家碰撞（与 PlayerAbilities 协程同步，避免单一点故障）
        Invoke("EnablePlayerCollision", 0.5f);

        // 生命周期管理
        Destroy(gameObject, lifespan);
    }

    // 启用玩家碰撞（供外部调用，同步状态）
    public void EnablePlayerCollision()
    {
        canCollideWithPlayer = true;
        Debug.Log($"投射物碰撞状态更新：可与玩家碰撞（对象: {gameObject.name}）");
    }

    void Update()
    {
        // 可视化调试：根据碰撞状态改变投射物颜色
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // 不可碰撞：红色；可碰撞：绿色
            spriteRenderer.color = canCollideWithPlayer ? Color.green : Color.red;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 调试信息：打印碰撞对象详情
        string otherLayer = LayerMask.LayerToName(other.gameObject.layer);
        Debug.Log($"投射物碰撞检测 - 对象: {other.gameObject.name}, 标签: {other.tag}, 图层: {otherLayer}, 碰撞状态: {canCollideWithPlayer}");

        // 1. 墙壁/地面碰撞处理
        if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            Debug.Log($"投射物碰撞墙壁/地面（反弹次数: {bounceCount + 1}/{maxBounces}）");
            BounceOffWall(other);
            bounceCount++;

            // 反弹次数超过限制，销毁投射物
            if (bounceCount >= maxBounces)
            {
                Debug.Log($"投射物反弹次数达到上限，销毁（对象: {gameObject.name}）");
                Destroy(gameObject);
            }
            return;
        }

        // 2. 玩家碰撞处理（双重条件：碰撞状态启用 + 距离检查）
        if (other.CompareTag("Player"))
        {
            // 条件1：碰撞状态已启用
            if (!canCollideWithPlayer)
            {
                Debug.LogWarning($"忽略玩家碰撞 - 碰撞状态未启用（对象: {gameObject.name}）");
                return;
            }

            // 条件2：距离检查（避免极近距离误触发）
            float distanceToPlayer = Vector3.Distance(transform.position, other.transform.position);
            if (distanceToPlayer <= 0.3f)
            {
                Debug.LogWarning($"忽略玩家碰撞 - 距离过近（距离: {distanceToPlayer:F2}m < 0.3m，对象: {gameObject.name}）");
                return;
            }

            // 满足所有条件：回收投射物
            Debug.Log($"玩家碰撞有效 - 回收投射物（距离: {distanceToPlayer:F2}m，对象: {gameObject.name}）");
            PlayerAbilities playerAbilities = other.GetComponent<PlayerAbilities>();
            if (playerAbilities != null)
            {
                playerAbilities.ReclaimProjectile(true);
            }
            Destroy(gameObject);
        }
    }

    // 投射物反弹逻辑
    void BounceOffWall(Collider2D wall)
    {
        if (rb == null)
        {
            Debug.LogError("投射物缺少 Rigidbody2D 组件，无法反弹！");
            return;
        }

        // 获取碰撞点法线（用于计算反弹方向）
        ContactPoint2D[] contacts = new ContactPoint2D[1];
        if (wall.GetContacts(contacts) > 0)
        {
            Vector2 normal = contacts[0].normal;
            Vector2 incomingVelocity = rb.velocity;

            // 计算反射速度（物理反弹公式）
            Vector2 reflectedVelocity = Vector2.Reflect(incomingVelocity, normal);
            rb.velocity = reflectedVelocity;

            Debug.Log($"投射物反弹 - 入射方向: {incomingVelocity}, 法线方向: {normal}, 反弹方向: {reflectedVelocity}");
        }
    }

    // Scene 视图可视化调试：绘制状态标记
    void OnDrawGizmos()
    {
        // 绘制碰撞状态球体（红色=不可碰撞，绿色=可碰撞）
        Gizmos.color = canCollideWithPlayer ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // 绘制状态文本（仅 Unity 编辑器可见）
#if UNITY_EDITOR
        string statusText = canCollideWithPlayer ? "可碰撞玩家" : "不可碰撞玩家";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, statusText);
#endif
    }
}