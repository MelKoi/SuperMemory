using UnityEngine;

static class BattleUiSprites
{
    static Sprite circle;
    static Sprite square;

    public static Sprite Circle => circle != null ? circle : (circle = CreateCircle());
    public static Sprite Square => square != null ? square : (square = CreateSquare());

    static Sprite CreateSquare()
    {
        var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var pixels = new Color[16];
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.hideFlags = HideFlags.HideAndDontSave;
        var sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    static Sprite CreateCircle()
    {
        const int size = 128;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        var radius = (size - 1) * 0.5f;
        var center = new Vector2(radius, radius);
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var distance = Vector2.Distance(new Vector2(x, y), center);
                var alpha = Mathf.Clamp01(radius - distance + 0.75f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.hideFlags = HideFlags.HideAndDontSave;
        var sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }
}
