using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class PlayerCtrl : MonoBehaviour
{
    private readonly float erf = 0.01f;
    [InspectorLabel("冲刺/蹬墙后锁定水平移动时间")]
    public float LockHorizontalMoveTime;
    [InspectorLabel("跳跃时长")]
    public float MaxJumpTime; private float CurJumpTime;
    [InspectorLabel("土狼时间")]
    public float WolfTime; private float EscapeGroundTime;
    [InspectorLabel("基本设置")]
    public float MoveSpeed;
    public float DashImpulse;
    public float JumpForce;
    public Vector2 WallSlideForce;
    public Vector2 WallJumpImpulse;
    private Vector2 Direction;
    private bool inSky, canDash;
    private float LockMoveTime;
    Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        UpdateFaceDirection();
        LockMoveTime -= Time.deltaTime;
        if (GroundDetect())
        {
            CurJumpTime = 0f;
            inSky = false;
            canDash = true;
            EscapeGroundTime = 0f;
        }
        if (!GroundDetect())
        {
            EscapeGroundTime += Time.deltaTime;
            if (EscapeGroundTime >= WolfTime)
                inSky = true;
        }
        if (WallDetect() && inSky && !(rb.velocity.y >= erf))
        {
            rb.AddForce(WallSlideForce);
        }
        if (LockMoveTime <= 0f)
            HorizontalMove();
        WallJump();
        Jump();
        Dash();
    }
    private void HorizontalMove() => rb.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * MoveSpeed, rb.velocity.y);
    private void UpdateFaceDirection()
    {
        if (Mathf.Abs(rb.velocity.x) >= erf)
            Direction = new Vector2(rb.velocity.x, 0).normalized;
    }
    private void Jump()
    {
        if (Input.GetKey(KeyCode.Space) && CurJumpTime <= MaxJumpTime && !inSky)
        {
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Force);
            CurJumpTime += Time.deltaTime;
        }
        if (!Input.GetKey(KeyCode.Space) && CurJumpTime >= erf)
            inSky = true;
    }
    private void Dash()
    {
        if (!canDash) return;
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Vector2 dir = rb.velocity.normalized;
            if (dir.magnitude <= erf) dir = Direction;
            rb.velocity = new Vector2(dir.x, dir.y / 2.0f) * DashImpulse;
            canDash = false;
            LockMoveTime = LockHorizontalMoveTime;
        }
    }
    private void WallJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && inSky && WallDetect())
        {
            rb.AddForce(new Vector2(-WallJumpImpulse.x * Direction.x, WallJumpImpulse.y), ForceMode2D.Impulse);
            LockMoveTime = LockHorizontalMoveTime;
        }
    }
    private bool GroundDetect()
    {
        Ray2D ray1 = new Ray2D(transform.position + Vector3.left * 0.4f, Vector2.down);
        Ray2D ray2 = new Ray2D(transform.position + Vector3.right * 0.4f, Vector2.down);
        Debug.DrawRay(ray1.origin, ray1.direction, Color.red);
        Debug.DrawRay(ray2.origin, ray2.direction, Color.red);
        var hits1 = Physics2D.RaycastAll(ray1.origin, ray1.direction, 0.6f);
        var hits2 = Physics2D.RaycastAll(ray2.origin, ray2.direction, 0.6f);
        foreach (var hit in hits1)
            if (hit.transform.CompareTag("Ground"))
                return true;
        foreach (var hit in hits2)
            if (hit.transform.CompareTag("Ground"))
                return true;
        return false;
    }
    private bool WallDetect()
    {
        Ray2D ray = new Ray2D(transform.position + Vector3.up * 0.4f, Direction);
        Debug.DrawRay(ray.origin, ray.direction, Color.yellow);
        var hits = Physics2D.RaycastAll(ray.origin, ray.direction, 0.6f);
        foreach (var hit in hits)
            if (hit.transform.CompareTag("Wall"))
                return true;
        return false;
    }
}
