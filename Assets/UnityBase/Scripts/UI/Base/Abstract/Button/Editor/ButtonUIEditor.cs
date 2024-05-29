#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace UnityBase.UI.ButtonCore
{
    [CustomEditor(typeof(ButtonUI), true)]
    public class ButtonUIEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ButtonUI button = (ButtonUI)target;
            
            button?.EditorInitialize();

            /*if (button is IButtonUI buttonUI)
            {
                if (string.IsNullOrEmpty(buttonUI.GetButtonId()))
                {
                    buttonUI.SetButtonId(GUID.Generate().ToString());
                    EditorUtility.SetDirty(button);
                }
            }*/
        }
    }
}
#endif