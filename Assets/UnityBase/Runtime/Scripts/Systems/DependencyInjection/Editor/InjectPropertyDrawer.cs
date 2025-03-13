using UnityEditor;
using UnityEngine;

namespace DependencyInjection
{
    [CustomPropertyDrawer(typeof(InjectAttribute))]
    public class InjectPropertyDrawer : PropertyDrawer
    {
        private Texture2D icon;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UnityBase/Scripts/DesignPatterns/InUse/DependencyInjection/Editor/icon.png");
            
            if (icon != null) 
            {
                label.image = icon;
            }

            EditorGUI.PropertyField(position, property, label);
        }
    }
}