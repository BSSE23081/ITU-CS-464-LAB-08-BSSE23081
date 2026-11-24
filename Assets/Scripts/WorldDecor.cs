using System.Collections.Generic;
using UnityEngine;

// Ground and scrolling grid lines, built at runtime (the square sprite is generated in code,
// so it can't be saved into the scene). The lines scroll at the game speed to show movement.
public class WorldDecor : MonoBehaviour
{
    public GameController game;
    public float groundY = -2.5f;

    readonly List<Transform> lines = new List<Transform>();
    const float Spacing = 2f, Count = 8;

    void Awake()
    {
        var floor = GameSprites.Make("GroundFill", transform, Color.Lerp(Neon.BG, Neon.Pink, 0.08f), new Vector2(30, 6), 0);
        floor.transform.position = new Vector3(0, groundY - 3f, 0);
        var edge = GameSprites.Make("GroundLine", transform, Neon.Pink, new Vector2(30, 0.1f), 2);
        edge.transform.position = new Vector3(0, groundY - 0.05f, 0);

        for (int i = 0; i < Count; i++)
        {
            var l = GameSprites.Make("GridLine", transform, new Color(Neon.Pink.r, Neon.Pink.g, Neon.Pink.b, 0.25f), new Vector2(0.05f, 6), 1);
            l.transform.position = new Vector3(-8 + i * Spacing, groundY - 3f, 0);
            lines.Add(l.transform);
        }
    }

    void Update()
    {
        float speed = (game != null && game.Running) ? game.Speed : 0f;
        foreach (var l in lines)
        {
            var p = l.position;
            p.x -= speed * Time.deltaTime;
            if (p.x < -8f) p.x += Count * Spacing;
            l.position = p;
        }
    }
}
