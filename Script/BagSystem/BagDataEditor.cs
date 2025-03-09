#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BagAsset))]
public class BagDataEditor : Editor
{
    private BagAsset bagAsset;

    private void OnEnable()
    {
        bagAsset = target as BagAsset;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 显示默认配置
        EditorGUILayout.PropertyField(serializedObject.FindProperty("initialCharacters"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("initialWeapons"));

        // 运行时数据展示
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("运行时数据", EditorStyles.boldLabel);

        if (bagAsset.runtimeBagData != null)
        {
            DrawRuntimeData();
        }

        else
        {
            EditorGUILayout.HelpBox("运行时数据未初始化", MessageType.Warning);
        }

        // 操作按钮
        EditorGUILayout.Space();
        if (GUILayout.Button("初始化运行时数据"))
        {
            bagAsset.InitializeRuntimeData();
            EditorUtility.SetDirty(bagAsset);
        }

        if (GUILayout.Button("保存到文件"))
        {
            bagAsset.SaveData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRuntimeData()
    {
        EditorGUI.indentLevel++;

        EditorGUILayout.LabelField("角色状态:");
        foreach (var state in bagAsset.runtimeBagData.characterStates)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(state.asset, typeof(CharactorAsset), false);
            state.owned = EditorGUILayout.Toggle("拥有", state.owned);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("武器状态:");
        foreach (var state in bagAsset.runtimeBagData.weaponStates)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(state.asset, typeof(WeaponAsset), false);
            state.owned = EditorGUILayout.Toggle("拥有", state.owned);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUI.indentLevel--;
    }
}
#endif