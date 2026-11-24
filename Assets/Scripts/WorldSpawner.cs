using System.Collections.Generic;
using UnityEngine;

// Spawns obstacles, coins and the occasional repair pickup, scrolls them toward the player
// and does the overlap tests. It tells PlayerHealth / CoinWallet what happened.
public class WorldSpawner : MonoBehaviour
{
    public GameController game;
    public PlayerController player;
    public PlayerHealth health;
    public CoinWallet wallet;

    enum Kind { Obstacle, Coin, Repair }

    class Item
    {
        public Transform t;
        public Kind kind;
        public Vector2 half;
        public SpriteRenderer[] renderers;
        public bool used;
    }

    readonly List<Item> items = new List<Item>();
    float timer = 1.5f;
    const float SpawnX = 4.6f, DespawnX = -5f;

    void Update()
    {
        if (game == null || !game.Running) return;
        float dt = Time.deltaTime, speed = game.Speed;

        timer -= dt;
        if (timer <= 0f)
        {
            Spawn();
            timer = Random.Range(8f, 12.5f) / speed;   // gap measured in distance, so it scales with speed
        }

        Vector2 p = player.Position;
        for (int i = items.Count - 1; i >= 0; i--)
        {
            var it = items[i];
            it.t.position += Vector3.left * speed * dt;
            if (it.t.position.x < DespawnX) { Remove(i); continue; }
            if (it.used) continue;

            float dx = Mathf.Abs(it.t.position.x - p.x), dy = Mathf.Abs(it.t.position.y - p.y);
            if (dx > it.half.x + 0.4f || dy > it.half.y + 0.4f) continue;

            switch (it.kind)
            {
                case Kind.Coin:
                    wallet.Add(1);
                    Remove(i);
                    break;
                case Kind.Repair:
                    health.Heal(1);
                    Remove(i);
                    break;
                case Kind.Obstacle:
                    if (health.TakeDamage(1))
                    {
                        it.used = true;   // one hit per obstacle
                        foreach (var r in it.renderers) { var c = r.color; c.a = 0.25f; r.color = c; }
                    }
                    break;
            }
        }
    }

    void Spawn()
    {
        float ground = player.groundY;
        float h = Random.value < 0.5f ? 1.0f : 1.35f;
        // obstacles are shrunk a little for the hit test so near misses feel fair
        Add(Kind.Obstacle, new Vector3(SpawnX, ground + h / 2f, 0), new Vector2(0.8f, h), Neon.Red, new Vector2(0.3f, h / 2f - 0.1f));

        // a reward arc over the obstacle (collected while jumping)
        if (Random.value < 0.75f)
            for (int i = -1; i <= 1; i++)
                Add(Kind.Coin, new Vector3(SpawnX + i * 0.9f, ground + h + 0.7f, 0), new Vector2(0.45f, 0.45f), Neon.Gold, new Vector2(0.3f, 0.3f));

        // a row on the ground after it (collected just by running through)
        if (Random.value < 0.6f)
            for (int i = 0; i < 4; i++)
                Add(Kind.Coin, new Vector3(SpawnX + 3.2f + i * 0.9f, ground + 0.5f, 0), new Vector2(0.45f, 0.45f), Neon.Gold, new Vector2(0.3f, 0.3f));

        // repair pickup at running height, so you can just run into it
        if (Random.value < 0.3f && health.Current < health.maxHealth)
            Add(Kind.Repair, new Vector3(SpawnX + 6.8f, ground + 0.5f, 0), new Vector2(0.6f, 0.6f), Neon.Green, new Vector2(0.35f, 0.35f));
    }

    void Add(Kind kind, Vector3 pos, Vector2 size, Color color, Vector2 half)
    {
        var root = new GameObject(kind.ToString());
        root.transform.SetParent(transform, false);
        root.transform.position = pos;
        var glow = GameSprites.Make("Glow", root.transform, new Color(color.r, color.g, color.b, 0.3f), size * 1.35f, 4);
        var body = GameSprites.Make("Body", root.transform, color, size, 5);
        if (kind != Kind.Obstacle) root.transform.rotation = Quaternion.Euler(0, 0, 45f);   // diamonds
        items.Add(new Item { t = root.transform, kind = kind, half = half, renderers = new[] { body, glow } });
    }

    void Remove(int index)
    {
        Destroy(items[index].t.gameObject);
        items.RemoveAt(index);
    }
}
