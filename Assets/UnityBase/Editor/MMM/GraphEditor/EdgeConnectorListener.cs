using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EdgeConnectorListener : IEdgeConnectorListener
{
    public void OnDrop(GraphView graphView, Edge edge)
    {
        /*var outputNodeTitle = edge.output.node.title;
        var inputNodeTitle = edge.input.node.title;

        var outputObject = GameObject.Find(outputNodeTitle);
        
        if (!outputObject || !outputObject.TryGetComponent<MatActivateController>(out var outputMatActivateController))
        {
            Debug.LogError($"Output Node '{outputNodeTitle}' is not a valid GameObject with MatActivateController.");
            return;
        }

        var inputObject = GameObject.Find(inputNodeTitle);
        if (!inputObject)
        {
            Debug.LogError($"Input Node '{inputNodeTitle}' is not a valid GameObject.");
            return;
        }

        if (outputMatActivateController.objectToActivateList == null)
        {
            outputMatActivateController.objectToActivateList = new[] { inputObject };
        }
        else
        {
            if (!Array.Exists(outputMatActivateController.objectToActivateList, obj => obj == inputObject))
            {
                var newArray = new GameObject[outputMatActivateController.objectToActivateList.Length + 1];
                Array.Copy(outputMatActivateController.objectToActivateList, newArray, outputMatActivateController.objectToActivateList.Length);
                newArray[^1] = inputObject;
                outputMatActivateController.objectToActivateList = newArray;
                EditorUtility.SetDirty(outputMatActivateController);
            }
            else
            {
                Debug.LogError($"Input Node '{inputNodeTitle}' is already in Output Node '{outputNodeTitle}' GameObject array.");
            }
        }*/
    }

    public void OnDropOutsidePort(Edge edge, Vector2 position)
    {
        //Debug.LogError("Edge dropped outside a valid port.");
    }
}