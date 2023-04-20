﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using GraphViewPort = UnityEditor.Experimental.GraphView.Port;
using GraphViewEdge = UnityEditor.Experimental.GraphView.Edge;
using GraphViewSearchWindow = UnityEditor.Experimental.GraphView.SearchWindow;

namespace CFEngine.Editor
{
    public interface IGraphMovable
    {
        void OnMoved ();
    }
    public interface IGraphResizable
    {
        void OnStartResize ();
        void OnResized ();
    }

    /// <summary>
    /// Graph view that contains the nodes, edges, etc. 
    /// </summary>
    public class CanvasView : GraphView
    {
        private static string styleDir = string.Format ("{0}/Styles", AssetsConfig.instance.EngineResPath);
        public static VisualTreeAsset LoadUXML (string text)
        {
            string path = string.Format ("{0}/{1}.uxml",
                styleDir, text);
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset> (path);
        }
        public static StyleSheet LoadStyle (string text)
        {
            string path = string.Format ("{0}/{1}.uss",
                styleDir, text);
            return AssetDatabase.LoadAssetAtPath<StyleSheet> (path);
        }

        /// <summary>
        /// Title displayed in the bottom left of the canvas
        /// </summary>
        public string Title
        {
            get
            {
                return m_Title.text;
            }
            set
            {
                m_Title.text = value;
            }
        }

        Label m_Title;

        List<CommentView> m_CommentViews = new List<CommentView> ();

        GraphEditor m_GraphEditor;
        Graph m_Graph;
        protected SerializedObject m_SerializedGraph;

        protected SearchWindow m_Search;
        EditorWindow m_EditorWindow;

        protected EdgeConnectorListener m_EdgeListener;

        HashSet<ICanDirty> m_Dirty = new HashSet<ICanDirty> ();

        Vector2 m_LastMousePosition;

        public CanvasView (EditorWindow window, string back)
        {
            m_EditorWindow = window;

            styleSheets.Add (Resources.Load<StyleSheet> ("Styles/CanvasView"));
            AddToClassList ("canvasView");

            m_EdgeListener = new EdgeConnectorListener (this);
            m_Search = ScriptableObject.CreateInstance<SearchWindow> ();
            m_Search.AddSearchProvider (new DefaultSearchProvider ());
            m_Search.target = this;

            SetupZoom (0.05f, ContentZoomer.DefaultMaxScale);

            this.AddManipulator (new ContentDragger ());
            this.AddManipulator (new SelectionDragger ());
            this.AddManipulator (new RectangleSelector ());
            this.AddManipulator (new ClickSelector ());

            // Add event handlers for shortcuts and changes
            RegisterCallback<KeyDownEvent> (OnGraphKeydown);
            RegisterCallback<MouseMoveEvent> (OnGraphMouseMove);

            graphViewChanged = OnGraphViewChanged;

            RegisterCallback<AttachToPanelEvent> (c => { Undo.undoRedoPerformed += OnUndoRedo; });
            RegisterCallback<DetachFromPanelEvent> (c => { Undo.undoRedoPerformed -= OnUndoRedo; });

            nodeCreationRequest = (ctx) => OpenSearch (ctx.screenMousePosition);

            // Add handlers for (de)serialization
            serializeGraphElements = OnSerializeGraphElements;
            canPasteSerializedData = OnTryPasteSerializedData;
            unserializeAndPaste = OnUnserializeAndPaste;

            RegisterCallback<GeometryChangedEvent> (OnFirstResize);

            m_Title = new Label (back);
            m_Title.AddToClassList ("canvasViewTitle");
            Add (m_Title);

            // Add a grid renderer *behind* content containers
            Insert (0, new GridBackground ());
        }

        void OnUndoRedo ()
        {
            Debug.Log ("Undo/Redo");
            Refresh ();
        }

        void OnGraphMouseMove (MouseMoveEvent evt)
        {
            m_LastMousePosition = evt.mousePosition;
        }

        /// <summary>
        /// Event handler to frame the graph view on initial layout
        /// </summary>
        void OnFirstResize (GeometryChangedEvent evt)
        {
            UnregisterCallback<GeometryChangedEvent> (OnFirstResize);
            FrameAll ();
        }

        GraphViewChange OnGraphViewChanged (GraphViewChange change)
        {
            if (m_SerializedGraph == null)
            {
                return change;
            }

            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    // TODO: Move/optimize
                    if (element is NodeView node)
                    {
                        UpdateCommentLink (node);
                    }
                }

                // Moved nodes will update their underlying models automatically.
                EditorUtility.SetDirty (m_Graph);
            }

            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is NodeView node)
                    {
                        RemoveNode (node);
                    }
                    else if (element is GraphViewEdge edge)
                    {
                        RemoveEdge (edge, true);
                    }
                    else if (element is CommentView comment)
                    {
                        RemoveComment (comment);
                    }

                    if (element is ICanDirty canDirty)
                    {
                        m_Dirty.Remove (canDirty);
                    }
                }
            }

            return change;
        }

        void OnGraphKeydown (KeyDownEvent evt)
        {
            // C: Add a new comment around the selected nodes (or just at mouse position)
            if (evt.keyCode == KeyCode.C && !evt.ctrlKey && !evt.commandKey)
            {
                AddComment ();
            }
        }

        public void Load (Graph graph)
        {
            m_Graph = graph;
            m_SerializedGraph = graph.serializedGraph;
            // Clear ();
            AddCustomNode (graph);
            AddNodeViews (graph.nodes);
            AddCommentViews (graph.comments);

            // Reset the lookup to a new set of whitelisted modules
            m_Search.ClearModules ();

            var attrs = graph.GetType ().GetCustomAttributes (true);
            foreach (var attr in attrs)
            {
                if (attr is IncludeModulesAttribute modulesAttr)
                {
                    foreach (var module in modulesAttr.modules)
                    {
                        m_Search.AddModule (module);
                    }
                }
            }
            AddCustomModule ();
        }

        /// <summary>
        /// Create a new node from reflection data and insert into the Graph.
        /// </summary>
        internal void AddNodeFromSearch (
            AbstractNode node,
            Vector2 screenPosition,
            PortView connectedPort = null
        )
        {
            var serializedNodesArr = GetNodeProperty (node); //m_SerializedGraph.FindProperty ("nodes");
            if (serializedNodesArr == null)
                return;
            // Calculate where to place this node on the graph
            var windowRoot = m_EditorWindow.rootVisualElement;
            var windowMousePosition = m_EditorWindow.rootVisualElement.ChangeCoordinatesTo (
                windowRoot.parent,
                screenPosition - m_EditorWindow.position.position
            );

            var graphMousePosition = contentViewContainer.WorldToLocal (windowMousePosition);

            // Track undo and add to the graph
            Undo.RegisterCompleteObjectUndo (m_Graph, $"Add Node {node.name}");
            // Debug.Log($"+node {node.name}");

            node.graphPosition = graphMousePosition;

            int index = OnAddNode (node);
            if (index < 0)
                return;
            m_Graph.AddNode (node);
            m_SerializedGraph.Update ();
            EditorUtility.SetDirty (m_Graph);

            // var nodeIdx = m_Graph.nodes.IndexOf (node);
            var serializedNode = serializedNodesArr.GetArrayElementAtIndex (index);

            // Add a node to the visual graph
            var editorType = NodeReflection.GetNodeEditorType (node.GetType ());
            var element = Activator.CreateInstance (editorType) as NodeView;
            element.Initialize (node, serializedNode, m_EdgeListener);
            OnPostAddNode (node, element);
            AddElement (element);

            // If there was a provided existing port to connect to, find the best 
            // candidate port on the new node and connect. 
            if (connectedPort != null)
            {
                var edge = new GraphViewEdge ();

                if (connectedPort.direction == Direction.Input)
                {
                    edge.input = connectedPort;
                    edge.output = element.GetCompatibleOutputPort (connectedPort);
                }
                else
                {
                    edge.output = connectedPort;
                    edge.input = element.GetCompatibleInputPort (connectedPort);
                }

                AddEdge (edge, false);
            }

            Dirty (element);
        }

        /// <summary>
        /// Remove a node from both the canvas view and the graph model
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode (NodeView node)
        {
            Undo.RegisterCompleteObjectUndo (m_Graph, $"Delete Node {node.name}");

            // Debug.Log ($"-node {node.name}");

            if (node.comment != null)
            {
                node.comment.RemoveElement (node);
            }

            m_Graph.RemoveNode (node.target);
            OnRemoveNode (node.target);
            m_SerializedGraph.Update ();
            EditorUtility.SetDirty (m_Graph);

            RemoveElement (node);
        }

        /// <summary>
        /// Add a new edge to both the canvas view and the underlying graph model
        /// </summary>
        public void AddEdge (GraphViewEdge edge, bool registerAsNewUndo)
        {
            if (edge.input == null || edge.output == null) return;

            if (registerAsNewUndo)
            {
                Undo.RegisterCompleteObjectUndo (m_Graph, "Add Edge");
            }

            // Handle single connection ports on either end. 
            var edgesToRemove = new List<GraphViewEdge> ();
            if (edge.input.capacity == GraphViewPort.Capacity.Single)
            {
                foreach (var conn in edge.input.connections)
                {
                    edgesToRemove.Add (conn);
                }
            }

            if (edge.output.capacity == GraphViewPort.Capacity.Single)
            {
                foreach (var conn in edge.output.connections)
                {
                    edgesToRemove.Add (conn);
                }
            }

            foreach (var edgeToRemove in edgesToRemove)
            {
                RemoveEdge (edgeToRemove, false);
            }

            var input = edge.input as PortView;
            var output = edge.output as PortView;

            Debug.Log ($"+edge {input.portName} to {output.portName}");

            // Connect the ports in the model
            m_Graph.AddEdge (input.target, output.target);
            m_SerializedGraph.Update ();
            EditorUtility.SetDirty (m_Graph);

            // Add a matching edge view onto the canvas
            var newEdge = input.ConnectTo (output);
            AddElement (newEdge);

            // Dirty the affected node views
            Dirty (input.node as NodeView);
            Dirty (output.node as NodeView);
        }

        /// <summary>
        /// Remove an edge from both the canvas view and the underlying graph model
        /// </summary>
        public void RemoveEdge (GraphViewEdge edge, bool registerAsNewUndo)
        {
            var input = edge.input as PortView;
            var output = edge.output as PortView;

            if (registerAsNewUndo)
            {
                Undo.RegisterCompleteObjectUndo (m_Graph, "Remove Edge");
            }

            Debug.Log ($"-edge {input.portName} to {output.portName}");

            // Disconnect the ports in the model
            m_Graph.RemoveEdge (input.target, output.target);
            m_SerializedGraph.Update ();
            EditorUtility.SetDirty (m_Graph);

            // Remove the edge view
            edge.input.Disconnect (edge);
            edge.output.Disconnect (edge);
            edge.input = null;
            edge.output = null;
            RemoveElement (edge);

            // Dirty the affected node views
            Dirty (input.node as NodeView);
            Dirty (output.node as NodeView);
        }

        /// <summary>
        /// Resync nodes and edges on the canvas with the modified graph.
        /// </summary>
        void Refresh ()
        {
            // TODO: Smart diff - if we start seeing performance issues. 
            // It gets complicated due to how we bind serialized objects though.

            // For now, we just nuke everything and start over.

            // Clear serialized graph first so that change events aren't undo tracked
            m_SerializedGraph = null;

            DeleteElements (graphElements.ToList ());

            Load (m_Graph);
        }

        /// <summary>
        /// Mark a node and all dependents as dirty for the next refresh. 
        /// </summary>
        /// <param name="node"></param>
        public void Dirty (ICanDirty element)
        {
            m_Dirty.Add (element);

            // TODO: Not the best place for this.
            EditorUtility.SetDirty (m_Graph);

            element.OnDirty ();

            // Also dirty outputs if a NodeView
            if (element is NodeView node)
            {
                foreach (var port in node.outputs)
                {
                    foreach (var conn in port.connections)
                    {
                        Dirty (conn.input.node as NodeView);
                    }
                }
            }
        }

        /// <summary>
        /// Dirty all nodes on the canvas for a complete refresh
        /// </summary>
        public void DirtyAll ()
        {
            graphElements.ForEach ((element) =>
            {
                if (element is ICanDirty cd)
                {
                    cd.OnDirty ();
                    m_Dirty.Add (cd);
                }
            });
        }

        public void Update ()
        {
            // Propagate change on dirty elements
            foreach (var element in m_Dirty)
            {
                element.OnUpdate ();
            }

            m_Dirty.Clear ();
        }

        public void UpdateCommentLink (NodeView node)
        {
            if (node.comment != null)
            {
                // Keep existing connection
                if (node.comment.OverlapsElement (node))
                {
                    Debug.Log ("keep conn");
                    return;
                }

                Debug.Log ("Remove old");
                node.comment.RemoveElement (node);
            }

            // Find new comment associations
            foreach (var comment in m_CommentViews)
            {
                Debug.Log ("Try overlap");
                if (comment.OverlapsElement (node))
                {
                    Debug.Log ("Found");
                    comment.AddElement (node);
                    return;
                }
            }
        }

        public void OpenSearch (Vector2 screenPosition, PortView connectedPort = null)
        {
            m_Search.sourcePort = connectedPort;
            GraphViewSearchWindow.Open (new SearchWindowContext (screenPosition), m_Search);
        }

        protected NodeView AddNodeView (AbstractNode node, SerializedProperty nodeArraySP, int index)
        {
            var serializedNode = nodeArraySP.GetArrayElementAtIndex (index);

            var editorType = NodeReflection.GetNodeEditorType (node.GetType ());
            var element = Activator.CreateInstance (editorType) as NodeView;

            element.Initialize (node, serializedNode, m_EdgeListener);
            AddElement (element);
            return element;
        }
        protected void SyncEdge (Dictionary<AbstractNode, NodeView> nodeMap)
        {
            foreach (var node in nodeMap)
            {
                foreach (var port in node.Key.Ports)
                {
                    if (!port.isInput)
                    {
                        continue;
                    }
                    port.HydratePorts();

                    foreach (var conn in port.Connections)
                    {
                        if (conn == null)
                        {
                            continue;
                        }
                        var connectedNode = conn.node;
                        if (connectedNode == null)
                        {
                            Debug.LogError (
                                $"Could not connect `{node.Value.title}:{port.name}`: " +
                                $"Connected node no longer exists."
                            );
                            continue;
                        }

                        // Only add if the linked node is in the collection
                        // TODO: This shouldn't be a problem
                        if (!nodeMap.ContainsKey (connectedNode))
                        {
                            Debug.LogError (
                                $"Could not connect `{node.Value.title}:{port.name}` -> `{connectedNode.name}:{conn.name}`. " +
                                $"Target node does not exist in the NodeView map"
                            );
                            continue;
                        }

                        var inPort = node.Value.GetInputPort (port.name);
                        var outPort = nodeMap[connectedNode].GetOutputPort (conn.name);

                        if (inPort == null)
                        {
                            Debug.LogError (
                                $"Could not connect `{node.Value.title}:{port.name}` -> `{connectedNode.name}:{conn.name}`. " +
                                $"Input port `{port.name}` no longer exists."
                            );
                        }
                        else if (outPort == null)
                        {
                            Debug.LogError (
                                $"Could not connect `{connectedNode.name}:{conn.name}` to `{node.Value.name}:{port.name}`. " +
                                $"Output port `{conn.name}` no longer exists."
                            );
                        }
                        else
                        {
                            var edge = inPort.ConnectTo (outPort);
                            AddElement (edge);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Append views for a set of nodes
        /// </summary>
        void AddNodeViews (List<AbstractNode> nodes, bool selectOnceAdded = false, bool centerOnMouse = false)
        {
            var serializedNodesArr = GetNodeProperty (); //FindProperty ("nodes");
            if (serializedNodesArr == null || nodes.Count == 0)
                return;

            // Add views of each node from the graph
            var nodeMap = new Dictionary<AbstractNode, NodeView> ();
            // TODO: Could just be a list with index checking. 

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var graphIdx = m_Graph.nodes.IndexOf (node);

                if (graphIdx < 0)
                {
                    Debug.LogError ("Cannot add NodeView: Node is not indexed on the graph");
                }
                else
                {
                    var element = AddNodeView (node, serializedNodesArr, graphIdx);

                    nodeMap.Add (node, element);
                    Dirty (element);

                    if (selectOnceAdded)
                    {
                        AddToSelection (element);
                    }
                }
            }

            if (centerOnMouse)
            {
                var bounds = GetBounds (nodeMap.Values);
                var worldPosition = contentViewContainer.WorldToLocal (m_LastMousePosition);
                var delta = worldPosition - bounds.center;

                foreach (var node in nodeMap)
                {
                    node.Value.SetPosition (new Rect (node.Key.graphPosition + delta, Vector2.one));
                }
            }

            SyncEdge (nodeMap);
        }

        /// <summary>
        /// Append views for comments from a Graph
        /// </summary>
        void AddCommentViews (IEnumerable<Comment> comments)
        {
            foreach (var comment in comments)
            {
                var commentView = new CommentView (comment);
                m_CommentViews.Add (commentView);
                AddElement (commentView);
                Dirty (commentView);
            }
        }

        /// <summary>
        /// Calculate the bounding box for a set of elements
        /// </summary>
        Rect GetBounds (IEnumerable<ISelectable> items)
        {
            var contentRect = Rect.zero;

            foreach (var item in items)
            {
                if (item is GraphElement ele)
                {
                    var boundingRect = ele.GetPosition ();
                    boundingRect.width = Mathf.Max (boundingRect.width, 1);
                    boundingRect.height = Mathf.Max (boundingRect.height, 1);

                    boundingRect = ele.parent.ChangeCoordinatesTo (contentViewContainer, boundingRect);

                    if (contentRect.width < 1 || contentRect.height < 1)
                    {
                        contentRect = boundingRect;
                    }
                    else
                    {
                        contentRect = RectUtils.Encompass (contentRect, boundingRect);
                    }
                }
            }

            return contentRect;
        }

        /// <summary>
        /// Add a new comment to the canvas and the associated Graph
        /// 
        /// If there are selected nodes, this'll encapsulate the selection with
        /// the comment box. Otherwise, it'll add at defaultPosition.
        /// </summary>
        void AddComment ()
        {
            Undo.RegisterCompleteObjectUndo (m_Graph, "Add Comment");

            Debug.Log ("+comment");

            // Pad out the bounding box a bit more on the selection
            var padding = 30f; // TODO: Remove hardcoding

            var bounds = GetBounds (selection);

            if (bounds.width < 1 || bounds.height < 1)
            {
                Vector2 worldPosition = contentViewContainer.WorldToLocal (m_LastMousePosition);
                bounds.x = worldPosition.x;
                bounds.y = worldPosition.y;

                // TODO: For some reason CSS minWidth/minHeight isn't being respected. 
                // Maybe I need to wait for CSS to load before setting bounds?
                bounds.width = 150 - padding * 2;
                bounds.height = 100 - padding * 3;
            }

            bounds.x -= padding;
            bounds.y -= padding * 2;
            bounds.width += padding * 2;
            bounds.height += padding * 3;

            var comment = new Comment ();
            comment.text = "New Comment";
            comment.graphRect = bounds;

            var commentView = new CommentView (comment);
            commentView.onResize += Dirty;

            // Add to the model
            m_Graph.comments.Add (comment);
            m_SerializedGraph.Update ();
            EditorUtility.SetDirty (m_Graph);

            m_CommentViews.Add (commentView);
            AddElement (commentView);

            Dirty (commentView);
        }

        /// <summary>
        /// Remove a comment from both the canvas view and the graph model
        /// </summary>
        /// <param name="comment"></param>
        public void RemoveComment (CommentView comment)
        {
            Undo.RegisterCompleteObjectUndo (m_Graph, "Delete Comment");

            Debug.Log ($"-comment {comment.target.text}");

            // Remove from the model
            m_Graph.comments.Remove (comment.target);
            m_SerializedGraph.Update ();
            EditorUtility.SetDirty (m_Graph);

            RemoveElement (comment);
            m_CommentViews.Remove (comment);
        }

        /// <summary>
        /// Handler for deserializing a node from a string payload
        /// </summary>
        /// <param name="operationName"></param>
        /// <param name="data"></param>
        private void OnUnserializeAndPaste (string operationName, string data)
        {
            Undo.RegisterCompleteObjectUndo (m_Graph, "Paste Subgraph");

            var cpg = CopyPasteGraph.Deserialize (data);

            foreach (var node in cpg.nodes)
            {
                m_Graph.AddNode (node);
            }

            foreach (var comment in cpg.comments)
            {
                m_Graph.comments.Add (comment);
            }

            m_SerializedGraph.Update ();
            EditorUtility.SetDirty (m_Graph);

            // Add views for all the new elements
            ClearSelection ();
            AddNodeViews (cpg.nodes, true, true);
            AddCommentViews (cpg.comments);

            ScriptableObject.DestroyImmediate (cpg);
        }

        private bool OnTryPasteSerializedData (string data)
        {
            return CopyPasteGraph.CanDeserialize (data);
        }

        /// <summary>
        /// Serialize a selection to support cut/copy/duplicate
        /// </summary>
        private string OnSerializeGraphElements (IEnumerable<GraphElement> elements)
        {
            return CopyPasteGraph.Serialize (elements);
        }

        public override List<GraphViewPort> GetCompatiblePorts (GraphViewPort startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<GraphViewPort> ();
            var startPortView = startPort as PortView;

            ports.ForEach ((port) =>
            {
                var portView = port as PortView;
                if (portView.IsCompatibleWith (startPortView))
                {
                    compatiblePorts.Add (portView);
                }
            });

            return compatiblePorts;
        }

        /// <summary>
        /// Replacement of the base AddElement() to undo the hardcoded
        /// border style that's overriding USS files. 
        /// Should probably report this as dumb. 
        /// 
        /// See: https://github.com/Unity-Technologies/UnityCsReference/blob/02d565cf3dd0f6b15069ba976064c75dc2705b08/Modules/GraphViewEditor/Views/GraphView.cs#L1222
        /// </summary>
        /// <param name="graphElement"></param>
        public new void AddElement (GraphElement graphElement)
        {
            var borderBottomWidth = graphElement.style.borderBottomWidth;
            base.AddElement (graphElement);

            if (graphElement.IsResizable ())
            {
                graphElement.style.borderBottomWidth = borderBottomWidth;
            }
        }
        protected virtual SerializedProperty GetNodeProperty ()
        {
            return m_SerializedGraph.FindProperty ("nodes");
        }

        protected virtual SerializedProperty GetNodeProperty (AbstractNode node)
        {
            return m_SerializedGraph.FindProperty ("nodes");
        }

        protected virtual void AddCustomNode (Graph graph) { }

        protected virtual void AddCustomModule ()
        {

        }

        protected virtual int OnAddNode (AbstractNode node)
        {
            return -1;
        }
        protected virtual void OnPostAddNode (AbstractNode node, NodeView nodeView)
        {

        }
        protected virtual void OnRemoveNode (AbstractNode node)
        {

        }
    }
}