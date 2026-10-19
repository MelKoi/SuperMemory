using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class ChineseFonts
{
    public const string FontResourcePath = "Fonts/NotoSansSC-Regular";
    public const string TmpResourcePath = "Fonts/NotoSansSC-Regular SDF";

    static Font legacyFont;

    public static Font Legacy
    {
        get
        {
            if (legacyFont == null)
            {
                legacyFont = Resources.Load<Font>(FontResourcePath);
            }

            return legacyFont;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InstallTextMeshProFallback()
    {
        TMP_FontAsset chineseFont = Resources.Load<TMP_FontAsset>(TmpResourcePath);
        TMP_Settings settings = TMP_Settings.LoadDefaultSettings();
        if (chineseFont == null || settings == null)
        {
            return;
        }

        List<TMP_FontAsset> fallbacks = TMP_Settings.fallbackFontAssets;
        if (fallbacks == null)
        {
            TMP_Settings.fallbackFontAssets = new List<TMP_FontAsset> { chineseFont };
            return;
        }

        if (!fallbacks.Contains(chineseFont))
        {
            fallbacks.Insert(0, chineseFont);
        }
    }

    public static bool IsBuiltinFont(Font font)
    {
        if (font == null)
        {
            return true;
        }

        string fontName = font.name;
        return fontName == "LegacyRuntime" || fontName == "Arial" || fontName == "LiberationSans";
    }

    public static void Apply(Text text)
    {
        Font font = Legacy;
        if (text == null || font == null || !IsBuiltinFont(text.font))
        {
            return;
        }

        text.font = font;
    }

    public static void Apply(TextMesh textMesh)
    {
        Font font = Legacy;
        if (textMesh == null || font == null || !IsBuiltinFont(textMesh.font))
        {
            return;
        }

        textMesh.font = font;
    }
}
