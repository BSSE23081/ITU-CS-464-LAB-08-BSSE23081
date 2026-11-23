using System;
using UnityEngine;

// Tap / click / space to jump. Publishes Jumped; the jump sound is not played from here.
public class PlayerController : MonoBehaviour
{
    public GameController game;
    public float groundY = -2.5f;        // top surface of the ground
    public float jumpVelocity = 13f;
    public float gravity = 36f;

    public event Action Jumped;

    float vy;
    bool grounded = true;
    const float HalfSize = 0.5f;

    public Vector2 Position { get { return transform.position; } }

    // the square sprite is created in code, so the visuals are built at runtime
    void Awake()
    {
        GameSprites.Make("Glow", transform, new Color(Neon.Cyan.r, Neon.Cyan.g, Neon.Cyan.b, 0.3f), Vector2.one * 1.35f, 9);
        GameSprites.Make("Body", transform, Neon.Cyan, Vector2.one, 10);
    }

    void Start()
    {
        transform.position = new Vector3(transform.position.x, groundY + HalfSize, 0f);
    }

    void Update()
    {
        if (grounded && game != null && game.Running && GameInput.JumpPressed())
        {
            vy = jumpVelocity;
            grounded = false;
            Jumped?.Invoke();
        }

        if (!grounded)
        {
            vy -= gravity * Time.deltaTime;
            var p = transform.position;
            p.y += vy * Time.deltaTime;
            if (p.y <= groundY + HalfSize)
            {
                p.y = groundY + HalfSize;
                grounded = true;
                vy = 0f;
            }
            transform.position = p;
        }
    }
}
