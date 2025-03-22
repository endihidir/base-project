using UnityBase.BlackboardCore;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(BlackboardDataSO))]
public class BlackboardDataEditor : Editor
{
    private ReorderableList _entryList;

    private void OnEnable()
    {
        _entryList = new ReorderableList(serializedObject, serializedObject.FindProperty("entries"), true, true, true, true)
        {
            drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight), "Key");
                EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.3f + 10, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight), "Type");
                EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.6f + 5, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight), "Value");
            }
        };

        _entryList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = _entryList.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;

            var keyName = element.FindPropertyRelative("keyName");
            var valueType = element.FindPropertyRelative("valueType");
            var value = element.FindPropertyRelative("value");

            var keyNameRect = new Rect(rect.x, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
            var valueTypeRect = new Rect(rect.x + rect.width * 0.315f, rect.y, rect.width * 0.31f, EditorGUIUtility.singleLineHeight);
            var valueRect = new Rect(rect.x + rect.width * 0.64f, rect.y, rect.width * 0.36f, EditorGUIUtility.singleLineHeight);

            var enumValue = (AnyValue.ValueType)valueType.enumValueIndex;

            var fieldName = string.Concat(enumValue.ToString().ToLowerInvariant(), "Value");
            var fieldValue = value.FindPropertyRelative(fieldName);
            
            EditorGUI.PropertyField(valueRect, fieldValue, GUIContent.none);
            EditorGUI.PropertyField(keyNameRect, keyName, GUIContent.none);
            EditorGUI.PropertyField(valueTypeRect, valueType, GUIContent.none);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        _entryList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}