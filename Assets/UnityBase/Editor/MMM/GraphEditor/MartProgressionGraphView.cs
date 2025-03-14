using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using UnityEditor.UIElements;

public class MartProgressionGraphView : GraphView
{
    public MartProgressionGraphView()
    {
        this.AddManipulator(new ContentZoomer()
        {
            minScale = .01f,
            maxScale = 1,
        });
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());


        var grid = new GridBackground();
        grid.StretchToParentSize();
        Insert(0, grid);

        style.flexGrow = 1;
    }


    private void AutoLayoutNodes()
    {
        Dictionary<Node, Vector2> positions = new Dictionary<Node, Vector2>();
        Dictionary<int, int> depthCounter = new Dictionary<int, int>(); // Tracks the number of nodes per depth level

        float xSpacing = 600f; // Space between depth levels
        float ySpacing = 450f; // Space between nodes at the same depth

        Queue<(Node node, int depth)> queue = new Queue<(Node, int)>();
        HashSet<Node> visited = new HashSet<Node>();

        // Identify root nodes (nodes that have no incoming edges)
        foreach (var node in nodes.Where(n => !edges.Any(e => e.input.node == n)))
        {
            queue.Enqueue((node, 0));
            visited.Add(node);
        }

        while (queue.Count > 0)
        {
            var (node, depth) = queue.Dequeue();

            // Ensure the depthCounter dictionary has an entry for this depth
            if (!depthCounter.ContainsKey(depth))
                depthCounter[depth] = 0;

            // Compute node position
            Vector2 position = new Vector2(depth * xSpacing, depthCounter[depth] * ySpacing);
            positions[node] = position;

            // Increment depth counter to prevent overlap
            depthCounter[depth]++;

            // Process connected nodes
            var children = edges
                .Where(e => e.output.node == node)
                .Select(e => e.input.node)
                .ToList();

            foreach (var child in children)
            {
                if (!visited.Contains(child))
                {
                    queue.Enqueue((child, depth + 1));
                    visited.Add(child);
                }
            }
        }

        Span<Color> depthColors = stackalloc Color[]
        {
            Color.green,
            Color.red,
            Color.blue,
            Color.cyan,
            Color.magenta,
            Color.yellow,
            Color.black,
            Color.gray,
        };

        // Apply calculated positions to the nodes
        foreach (var pair in positions)
        {
            pair.Key.SetPosition(new Rect(pair.Value, pair.Key.GetPosition().size));
            pair.Key.style.borderTopColor = depthColors[Mathf.FloorToInt(pair.Value.y / ySpacing) % 8];
            pair.Key.style.borderBottomColor = depthColors[Mathf.FloorToInt(pair.Value.y / ySpacing) % 8];
            pair.Key.style.borderLeftColor = depthColors[Mathf.FloorToInt(pair.Value.y / ySpacing) % 8];
            pair.Key.style.borderRightColor = depthColors[Mathf.FloorToInt(pair.Value.y / ySpacing) % 8];
        }
    }


    private string _lastQueryString = string.Empty;
    private List<Node> _queryResult = new List<Node>();
    private int _nextFocusIndex = 0;

    public void FocusOnNodes(string search)
    {
        if (_lastQueryString.Equals(search) && _queryResult.Count != 0)
        {
            var node = _queryResult[_nextFocusIndex];
            _nextFocusIndex = (_nextFocusIndex + 1) % _queryResult.Count;
            
            ClearSelection();
            AddToSelection(node);
            FrameSelection();
            //UpdateViewTransform( -node.GetPosition().center, Vector3.one * .5f);
        }
        else
        {
            _lastQueryString = search;
            _queryResult = nodes.Where(n => n.title.Contains(search, StringComparison.InvariantCultureIgnoreCase)).ToList();
            _nextFocusIndex = 0;
            if (_queryResult.Count == 0)
                return;
            
            var node = _queryResult[_nextFocusIndex];
            _nextFocusIndex = (_nextFocusIndex + 1) % _queryResult.Count;

            ClearSelection();
            AddToSelection(node);
            FrameSelection();
            //UpdateViewTransform(-node.GetPosition().center, Vector3.one * .5f);
        }
    }

    public void LoadNodesFromScene()
    {
        //_treeCount.Clear();

        /*ClearGraph();

        var createdNodes = new Dictionary<GameObject, ProgressionNode>();

        const int horizontalSpacing = 520;
        const int verticalSpacing = 450;*/

        //var rootNodes = BuildTree(GetMasterControllers());
        
        /*var rootNodes = BuildTree(GameObject.FindObjectsOfType<MatActivateController>());
        int nextRoot = 0;

        foreach (var rootNode in rootNodes)
        {
            PlaceNode(rootNode, 0, nextRoot, 0, horizontalSpacing, verticalSpacing, createdNodes);
            nextRoot += 1;
        }*/

        AutoLayoutNodes();
    }

    private void PlaceNode(TreeNode<GameObject> treeNode, int depth, int rootStart, int yOffset, int horizontalSpacing, int verticalSpacing, Dictionary<GameObject, ProgressionNode> createdNodes)
    {
        if (!createdNodes.ContainsKey(treeNode.Value))
        {
            var position = new Vector2((rootStart + 0) * horizontalSpacing, depth * verticalSpacing);
            var node = CreateNode(treeNode.Value.name, position);
            AddElement(node);
            createdNodes[treeNode.Value] = node;
            yOffset++; // Increment offset to prevent overlapping in the same column
        }

        var currentNode = createdNodes[treeNode.Value];

        // Compute starting Y position for children
        int childYOffset = yOffset;

        foreach (var child in treeNode.Children)
        {
            PlaceNode(child, depth + 1, rootStart, 0, horizontalSpacing, verticalSpacing, createdNodes);

            var childNode = createdNodes[child.Value];
            var edge = new Edge
            {
                output = currentNode.outputContainer.ElementAt(0) as Port,
                input = childNode.inputContainer.ElementAt(0) as Port
            };

            edge.output.Connect(edge);
            edge.input.Connect(edge);

            AddElement(edge);
        }

        // Adjust main offset after all children are placed
        yOffset = childYOffset;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();

        foreach (var port in ports)
        {
            if (startPort == port || startPort.node == port.node || startPort.direction == port.direction) continue;

            var isAlreadyConnected = port.connections.Any(connection =>
                connection.input == startPort || connection.output == startPort);

            if (isAlreadyConnected) continue;

            compatiblePorts.Add(port);
        }

        return compatiblePorts;
    }

    private List<TreeNode<GameObject>> BuildTree(/*MasterController[] controllers*/)
    {
        /*var nodeDictionary = new Dictionary<GameObject, TreeNode<GameObject>>();

        foreach (var controller in controllers)
        {
            if (!nodeDictionary.ContainsKey(controller.gameObject))
                nodeDictionary[controller.gameObject] = new TreeNode<GameObject>(controller.gameObject);

            // var orderedList = controller.objectToActivateList.Where(x=> x != null).OrderBy(x => x.TryGetComponent(out MatActivateController matActivateController));
            if (controller is MatActivateController matActivateController)
            {
                foreach (var obj in matActivateController.objectToActivateList)
                {
                    if (obj == null) continue;
                    if (!obj.TryGetComponent<MatActivateController>(out var mac)) continue;

                    if (!nodeDictionary.ContainsKey(obj))
                        nodeDictionary[obj] = new TreeNode<GameObject>(obj);

                    nodeDictionary[controller.gameObject].Children.Add(nodeDictionary[obj]);
                }
            }
        }

        var list = nodeDictionary.Values
            .Where(node => !nodeDictionary.Values.Any(other => other.Children.Contains(node))).ToList();

        return list;*/
        return default;
    }


    private ProgressionNode CreateNode(string nodeName, Vector2 position)
    {
        /*var matActivateController = GameObject.Find(nodeName)?.GetComponent<MatActivateController>();
        var node = new ProgressionNode
        {
            title = nodeName,
            Guid = Guid.NewGuid().ToString(),
            masterController = GameObject.Find(nodeName)?.GetComponent<MasterController>(),
        };

        node.SetPosition(new Rect(position, new Vector2(150, 200)));


        var hasOutput = matActivateController != null;

        return node.Draw(hasOutput);*/

        return default;
    }

    public override void AddToSelection(ISelectable selectable)
    {
        /*if (selectable is not Node node) return;

        var nodeObject = Object.FindObjectsOfType<MasterController>(true)
            .FirstOrDefault(x => x.name.Equals(node.title));

        if (!nodeObject) return;

        Selection.activeObject = nodeObject;
        EditorGUIUtility.PingObject(nodeObject);

        base.AddToSelection(selectable);*/
    }

    public override EventPropagation DeleteSelection()
    {
        foreach (var element in selection)
        {
            if (element is Edge edge)
            {
                OnEdgeDeleted(edge);
            }
            else if (element is Node node)
            {
                OnNodeDeleted(node);
            }
        }

        return base.DeleteSelection();
    }

    private void OnEdgeDeleted(Edge edge)
    {
        var outputNodeTitle = edge.output.node.title;
        /*var inputNodeTitle = edge.input.node.title;

        var outputObject = GameObject.Find(outputNodeTitle);
        var inputObject = GameObject.Find(inputNodeTitle);

        if (!outputObject ||
            !outputObject.TryGetComponent<MatActivateController>(out var outputMatActivateController) ||
            outputMatActivateController.objectToActivateList == null) return;

        outputMatActivateController.objectToActivateList = outputMatActivateController.objectToActivateList
            .Where(obj => !obj.Equals(inputObject)).ToArray();

        EditorUtility.SetDirty(outputMatActivateController);*/
    }

    private void OnNodeDeleted(Node node)
    {
        /*var nodeObject = GameObject.Find(node.title);

        if (!nodeObject) return;

        var matActivateControllers = Object.FindObjectsOfType<MatActivateController>();

        foreach (var controller in matActivateControllers)
        {
            if (controller.objectToActivateList == null) continue;

            controller.objectToActivateList = controller.objectToActivateList.Where(obj => obj != nodeObject).ToArray();

            EditorUtility.SetDirty(controller);
        }*/
    }

    private void ClearGraph()
    {
        foreach (var element in graphElements.ToList())
        {
            RemoveElement(element);
        }
    }

    /*private MasterController[] GetMasterControllers() => Object.FindObjectsOfType<MasterController>(true)
        .Where(x => x.GetComponentInParent<Map3DController>(true) is null).ToArray();*/

    /*private IDictionary<GameObject, int> _treeCount = new Dictionary<GameObject, int>();

    private int CalculateTreeDepth(Dictionary<GameObject, TreeNode<GameObject>> rootNodes, GameObject startNode, int depth)
    {
        if (rootNodes.TryGetValue(startNode, out var treeNode))
        {
            var totalDepth = 0;

            foreach (var treeNodeChild in treeNode.Children)
            {
                totalDepth += CalculateTreeDepth(rootNodes, treeNodeChild.Value, depth + 1);
            }

            _treeCount.TryAdd(startNode, totalDepth);
        }

        return depth;
    }*/
}

public class ProgressionNode : Node
{
    public string Guid { get; set; }
    //public MasterController masterController;
    public ProgressionNode()
    {
        capabilities = Capabilities.Movable | Capabilities.Selectable | Capabilities.Resizable;
    }

    public ProgressionNode Draw(bool hasOutput)
    {
        var inputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi,
            typeof(GameObject));
        inputPort.portName = "Input";
        inputPort.AddManipulator(new EdgeConnector<Edge>(new EdgeConnectorListener()));
        inputContainer.Add(inputPort);

        if (hasOutput)
        {
            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Multi,
                typeof(GameObject));
            outputPort.portName = "Output";
            outputPort.AddManipulator(new EdgeConnector<Edge>(new EdgeConnectorListener()));
            outputContainer.Add(outputPort);
        }


        /*if (masterController is MatActivateController matActivateController)
        {
            {
                var butt1 = new Button(() =>
                {
                    if (matActivateController.secondObjectToHighlight == null && matActivateController.objectToActivateList.Length > 0)
                    {
                        matActivateController.secondObjectToHighlight = matActivateController.objectToActivateList[0];
                    }
                    else if(matActivateController.secondObjectToHighlight != null &&  matActivateController.objectToActivateList.Length > 0)
                    {
                        var indexOf = Array.IndexOf(matActivateController.objectToActivateList, matActivateController.secondObjectToHighlight);
                        var nextIndex = (indexOf + 1) % matActivateController.objectToActivateList.Length;
                        matActivateController.secondObjectToHighlight = matActivateController.objectToActivateList[nextIndex];
                        this.Q("butt_obj_highlight_2").GetFirstOfType<ObjectField>().value = matActivateController.secondObjectToHighlight;
                        EditorUtility.SetDirty(matActivateController);
                    }
                })
                {
                    text = "Cycle Object To Highlight 2"
                };
                
                Insert(1, butt1);
            }
            {
                var butt1 = new Button(() =>
                {
                    if (matActivateController.objectToHighlight == null && matActivateController.objectToActivateList.Length > 0)
                    {
                        matActivateController.objectToHighlight = matActivateController.objectToActivateList[0];
                    }
                    else if(matActivateController.objectToHighlight != null &&  matActivateController.objectToActivateList.Length > 0)
                    {
                        var indexOf = Array.IndexOf(matActivateController.objectToActivateList, matActivateController.objectToHighlight);
                        var nextIndex = (indexOf + 1) % matActivateController.objectToActivateList.Length;
                        matActivateController.objectToHighlight = matActivateController.objectToActivateList[nextIndex];
                        this.Q("butt_obj_highlight").GetFirstOfType<ObjectField>().value = matActivateController.objectToHighlight; 
                        EditorUtility.SetDirty(matActivateController);
                    }
                })
                {
                    text = "Cycle Object To Highlight"
                };
                
                
                
                Insert(1, butt1);
            }

            {
                var listView = new ListView();
                var items = matActivateController.objectToActivateList.ToList();
                listView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
                listView.fixedItemHeight = 40;
                listView.itemsSource = items;
                listView.showAddRemoveFooter = true;

                listView.makeItem = () => new ObjectField()
                {
                    allowSceneObjects = true,
                    objectType = typeof(MasterController),
                };
                //listView.reorderMode = ListViewReorderMode.Animated;
                listView.bindItem = (element, i) =>
                {
                    if (element is ObjectField objectField)
                    {
                        objectField.value = items[i];
                        objectField.RegisterValueChangedCallback(evt =>
                        {
                            if (evt.newValue == null)
                            {
                                return;
                            }
                            if (evt.newValue is GameObject gameObject)
                            {
                                matActivateController.objectToActivateList[i] = gameObject;
                                EditorUtility.SetDirty(matActivateController);
                            }
                            else if (evt.newValue is MasterController master)
                            {
                                matActivateController.objectToActivateList[i] = master.gameObject;
                                EditorUtility.SetDirty(matActivateController);
                            }
                            else
                            {
                                Debug.LogError("Object type " + evt.newValue.GetType());
                            }
                        });
                    }
                };
                listView.itemsAdded += ints =>
                {
                    listView.style.height = 40 * items.Count + 50;
                    matActivateController.objectToActivateList = (listView.itemsSource as List<GameObject>)?.ToArray();
                    EditorUtility.SetDirty(matActivateController);
                    Debug.LogError("itemsAdded " + listView.itemsSource.Count);
                };

                listView.itemsRemoved += ints =>
                {
                    listView.style.height = 40 * items.Count + 50;
                    matActivateController.objectToActivateList = (listView.itemsSource as List<GameObject>)?.ToArray();
                    EditorUtility.SetDirty(matActivateController);
                    Debug.LogError("itemsRemoved " + listView.itemsSource.Count);
                };
                /*listView.itemsSourceChanged += () =>
                {
                    listView.style.height = 40 * items.Count + 50;
                    listView.Rebuild();
                    Debug.LogError("itemsSourceChanged " + listView.itemsSource.Count);
                };#1#

                
                listView.style.height = 40 * items.Count + 50;
                Insert(1, listView);
            }

            {
                var objectField = new ObjectField("Second Object To Highlight")
                {
                    name = "butt_obj_highlight_2"
                };;
                objectField.value = matActivateController.secondObjectToHighlight;
                objectField.RegisterValueChangedCallback(evt =>
                {
                    matActivateController.secondObjectToHighlight = (GameObject)evt.newValue;
                    EditorUtility.SetDirty(matActivateController);
                });

                Insert(1, objectField);
            }
            {
                var objectField = new ObjectField("Object To Highlight")
                {
                    name = "butt_obj_highlight"
                };
                objectField.value = matActivateController.objectToHighlight;
                objectField.RegisterValueChangedCallback(evt =>
                {
                    matActivateController.objectToHighlight = (GameObject)evt.newValue;
                    EditorUtility.SetDirty(matActivateController);
                });

                Insert(1, objectField);
            }

            {
                var priceField = new IntegerField("Price");
                priceField.value = matActivateController.objectTypeReq;
                priceField.RegisterValueChangedCallback(evt =>
                {
                    var evtNewValue = evt.newValue;
                    matActivateController.objectTypeReq = evtNewValue;
                    EditorUtility.SetDirty(matActivateController);
                });

                Insert(1, priceField);
            }
            
            {
                var sectionField = new IntegerField("Required Section Id");
                sectionField.value = matActivateController.unlockSectionId;
                sectionField.RegisterValueChangedCallback(evt =>
                {
                    var evtNewValue = evt.newValue;
                    matActivateController.unlockSectionId = evtNewValue;
                    EditorUtility.SetDirty(matActivateController);
                });

                Insert(1, sectionField);
            }
            {
                var sectionField = new IntegerField("Required Section Expansion");
                sectionField.value = matActivateController.unlockExpansionLevel;
                sectionField.RegisterValueChangedCallback(evt =>
                {
                    var evtNewValue = evt.newValue;
                    matActivateController.unlockExpansionLevel = evtNewValue;
                    EditorUtility.SetDirty(matActivateController);
                });

                Insert(1, sectionField);
            }
        }

        if (masterController is StoreTileController storeTileController)
        {
            {
                var objectField = new ObjectField("zoomOutHighglightTransform");
                objectField.value = storeTileController.zoomOutHighglightTransform;
                objectField.RegisterValueChangedCallback(evt =>
                {
                    storeTileController.zoomOutHighglightTransform = ((GameObject)evt.newValue).transform;
                    EditorUtility.SetDirty(storeTileController);
                });

                Insert(1, objectField);
            }
        }*/

        RefreshExpandedState();
        RefreshPorts();

        return this;
    }
}

public class TreeNode<T>
{
    public T Value { get; }
    public List<TreeNode<T>> Children { get; }

    public TreeNode(T value)
    {
        Value = value;
        Children = new List<TreeNode<T>>();
    }
}