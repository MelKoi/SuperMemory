using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

[InitializeOnLoad]
public static class ChineseFontSetup
{
    const string FontPath = "Assets/Resources/Fonts/NotoSansSC-Regular.otf";
    const string TmpFontPath = "Assets/Resources/Fonts/NotoSansSC-Regular SDF.asset";
    const string SettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
    const string SampleCharacters = "中文字体测试";

    static bool importQueued;
    static bool batchRequested;
    static int retries;

    static ChineseFontSetup()
    {
        if (Application.isBatchMode)
        {
            return;
        }

        ObjectFactory.componentWasAdded += OnComponentAdded;
        EditorApplication.delayCall += EnsureFonts;
    }

    public static void RunBatch()
    {
        batchRequested = true;
        EnsureFonts();
    }

    static void OnComponentAdded(Component component)
    {
        switch (component)
        {
            case Text text:
                ChineseFonts.Apply(text);
                break;
            case TextMesh textMesh:
                ChineseFonts.Apply(textMesh);
                break;
            case TMP_Text tmpText:
                ApplyTmpFont(tmpText);
                break;
        }
    }

    static void EnsureFonts()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            Retry();
            return;
        }

        if (!File.Exists(FontPath))
        {
            return;
        }

        if (!File.Exists(SettingsPath))
        {
            ImportEssentialResources();
            return;
        }

        CreateAndAssign();
    }

    static void Retry()
    {
        if (retries++ > 40)
        {
            Debug.LogError("中文字体初始化超时。");
            FinishBatch(1);
            return;
        }

        EditorApplication.delayCall += EnsureFonts;
    }

    static void FinishBatch(int exitCode)
    {
        if (batchRequested)
        {
            EditorApplication.Exit(exitCode);
        }
    }

    static void ImportEssentialResources()
    {
        if (importQueued)
        {
            return;
        }

        string packagePath = FindEssentialPackage();
        if (string.IsNullOrEmpty(packagePath))
        {
            Debug.LogWarning("未找到 TextMeshPro 基础资源包，中文 TMP 字体还不能生成。");
            FinishBatch(1);
            return;
        }

        importQueued = true;
        AssetDatabase.importPackageCompleted += OnPackageImported;
        AssetDatabase.ImportPackage(packagePath, false);
    }

    static void OnPackageImported(string packageName)
    {
        if (!File.Exists(SettingsPath))
        {
            return;
        }

        AssetDatabase.importPackageCompleted -= OnPackageImported;
        importQueued = false;
        EditorApplication.delayCall += CreateAndAssign;
    }

    static string FindEssentialPackage()
    {
        string cache = Path.Combine(Directory.GetCurrentDirectory(), "Library", "PackageCache");
        if (!Directory.Exists(cache))
        {
            return null;
        }

        string[] directories = Directory.GetDirectories(cache, "com.unity.ugui@*");
        for (int i = 0; i < directories.Length; i++)
        {
            string candidate = Path.Combine(directories[i], "Package Resources", "TMP Essential Resources.unitypackage");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    static void CreateAndAssign()
    {
        Font source = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (source == null)
        {
            AssetDatabase.ImportAsset(FontPath, ImportAssetOptions.ForceUpdate);
            source = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        }

        if (source == null)
        {
            Debug.LogWarning("中文字体还没有导入完成。");
            Retry();
            return;
        }

        VerifyLegacyFont(source);

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
        if (fontAsset == null)
        {
            fontAsset = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
            if (fontAsset == null || fontAsset.material == null || fontAsset.material.shader == null)
            {
                Debug.LogError("无法从 Noto Sans SC 创建 TextMeshPro 动态字体。");
                FinishBatch(1);
                return;
            }

            fontAsset.name = "NotoSansSC-Regular SDF";
            if (!fontAsset.TryAddCharacters(SampleCharacters, out string missing) || !string.IsNullOrEmpty(missing))
            {
                Debug.LogError("TextMeshPro 没能生成中文字形：" + missing);
            }

            AssetDatabase.CreateAsset(fontAsset, TmpFontPath);
            if (fontAsset.material != null)
            {
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            Texture2D atlas = fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0
                ? fontAsset.atlasTextures[0]
                : null;
            if (atlas != null)
            {
                AssetDatabase.AddObjectToAsset(atlas, fontAsset);
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(TmpFontPath);
            fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
        }

        if (fontAsset == null)
        {
            return;
        }

        AssignTextMeshProSettings(fontAsset);
        if (!batchRequested)
        {
            AssignOpenScenes(fontAsset);
        }

        Debug.Log("中文字体已就绪：UI Text / TextMesh 使用 Noto Sans SC，TextMeshPro 使用动态字体并已加入回退列表。");
        FinishBatch(0);
    }

    static void VerifyLegacyFont(Font source)
    {
        source.RequestCharactersInTexture(SampleCharacters, 32, FontStyle.Normal);
        if (!source.HasCharacter('中'))
        {
            Debug.LogError("UI Text 没能从 Noto Sans SC 取到中文字形。");
        }
    }

    static void AssignTextMeshProSettings(TMP_FontAsset fontAsset)
    {
        if (TMP_Settings.LoadDefaultSettings() == null)
        {
            FinishBatch(1);
            return;
        }

        bool changed = false;
        TMP_FontAsset currentDefault = TMP_Settings.defaultFontAsset;
        if (currentDefault == null || currentDefault.name.StartsWith("LiberationSans"))
        {
            TMP_Settings.defaultFontAsset = fontAsset;
            changed = true;
        }

        List<TMP_FontAsset> fallbacks = TMP_Settings.fallbackFontAssets;
        if (fallbacks == null)
        {
            TMP_Settings.fallbackFontAssets = new List<TMP_FontAsset> { fontAsset };
            changed = true;
        }
        else if (!fallbacks.Contains(fontAsset))
        {
            fallbacks.Insert(0, fontAsset);
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        EditorUtility.SetDirty(TMP_Settings.instance);
        AssetDatabase.SaveAssets();
    }

    static void AssignOpenScenes(TMP_FontAsset fontAsset)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                continue;
            }

            bool changed = false;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Text[] texts = roots[rootIndex].GetComponentsInChildren<Text>(true);
                for (int textIndex = 0; textIndex < texts.Length; textIndex++)
                {
                    if (!ChineseFonts.IsBuiltinFont(texts[textIndex].font))
                    {
                        continue;
                    }

                    ChineseFonts.Apply(texts[textIndex]);
                    EditorUtility.SetDirty(texts[textIndex]);
                    changed = true;
                }

                TextMesh[] textMeshes = roots[rootIndex].GetComponentsInChildren<TextMesh>(true);
                for (int textIndex = 0; textIndex < textMeshes.Length; textIndex++)
                {
                    if (!ChineseFonts.IsBuiltinFont(textMeshes[textIndex].font))
                    {
                        continue;
                    }

                    ChineseFonts.Apply(textMeshes[textIndex]);
                    EditorUtility.SetDirty(textMeshes[textIndex]);
                    changed = true;
                }

                TMP_Text[] tmpTexts = roots[rootIndex].GetComponentsInChildren<TMP_Text>(true);
                for (int textIndex = 0; textIndex < tmpTexts.Length; textIndex++)
                {
                    TMP_Text tmpText = tmpTexts[textIndex];
                    if (tmpText.font != null && !tmpText.font.name.StartsWith("LiberationSans"))
                    {
                        continue;
                    }

                    tmpText.font = fontAsset;
                    EditorUtility.SetDirty(tmpText);
                    changed = true;
                }
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }
        }
    }

    static void ApplyTmpFont(TMP_Text tmpText)
    {
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
        if (fontAsset == null || tmpText == null)
        {
            return;
        }

        if (tmpText.font != null && !tmpText.font.name.StartsWith("LiberationSans") && tmpText.font != fontAsset)
        {
            return;
        }

        tmpText.font = fontAsset;
    }
}
