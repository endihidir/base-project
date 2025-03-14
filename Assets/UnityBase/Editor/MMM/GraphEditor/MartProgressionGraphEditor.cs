using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MartProgressionGraphEditor : EditorWindow
{
    private MartProgressionGraphView graphView;

    [MenuItem("Tools/Mart Progression Visualizer")]
    public static void OpenGraphWindow()
    {
        var window = GetWindow<MartProgressionGraphEditor>();
        window.titleContent = new GUIContent("Mart Progression");
    }

    private void OnEnable()
    {
        graphView = new MartProgressionGraphView
        {
            name = "Mart Progression"
        };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);

        AddCustomToolbar();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }

    private void AddCustomToolbar()
    {
        var toolbar = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row
            }
        };

        var clearProgressionButton = new Button(() =>
        {
            if (EditorUtility.DisplayDialog("Warning", "This will clear all mat activate connections", "ok", "no"))
            {
                /*var matActivates = FindObjectsOfType<MatActivateController>();
                for (var i = 0; i < matActivates.Length; i++)
                {
                    matActivates[i].objectToActivateList = matActivates[i].objectToActivateList.Where(
                        o => o != null && o.GetComponent<MatActivateController>() == null
                    ).ToArray();
                    EditorUtility.SetDirty(matActivates[i]);
                }*/
            }
        })
        {
            text = "Clear Flow"
        };

        var loadButton = new Button(() =>
        {
            graphView.LoadNodesFromScene();
        })
        {
            text = "Visualize Mart Progression"
        };

        var searchField = new TextField("Search:")
        {
            multiline = false,
            maxLength = 64,
        };
        
        searchField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                graphView.FocusOnNodes( searchField.value );    
            }
        });
        searchField.RegisterValueChangedCallback(evt =>
        {
            graphView.FocusOnNodes( searchField.value );
        });
        
        toolbar.Add(loadButton);
        toolbar.Add(searchField);
        toolbar.Add(clearProgressionButton);
        rootVisualElement.Add(toolbar);
    }
}