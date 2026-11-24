using UnityEngine;

// A plain white 1x1 unit square sprite made at runtime, so the gameplay needs no art files.
public static class GameSprites
{
    static Sprite square;

    public static Sprite Square
    {
        get
        {
            if (square == null)
            {
                var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                var px = new Color[16];
                for (int i = 0; i < px.Length; i++) px[i] = Color.white;
                tex.SetPixels(px);
                tex.filterMode = FilterMode.Point;
                tex.Apply();
                square = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }
            return square;
        }
    }

    public static SpriteRenderer Make(string name, Transform parent, Color color, Vector2 size, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Square;
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }
}
