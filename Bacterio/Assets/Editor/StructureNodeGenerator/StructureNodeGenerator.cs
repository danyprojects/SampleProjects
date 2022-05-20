using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BacterioEditor;
using Bacterio.Databases;
using Bacterio.Common;

public class StructureNodeGenerator : EditorWindow
{
    [MenuItem("Window/Custom/NodeEditor")]
    static void ShowEditor()
    {
        StructureNodeGenerator editor = EditorWindow.GetWindow<StructureNodeGenerator>();
        editor.name = "StructureNodeGenerator";
        editor.Show();
    }

    private class Node
    {
        public List<Connection> _connections;
        public Vector2 _position;
        public Rect _rect;
        public int _index;
    }

    private class Connection
    {
        public Rect _rect;
        public Node node1;
        public Node node2;
    }

    private const int WINDOW_PIXELS_TO_UNIT = 100;
    private const int WINDOW_UNITS = 6; //Render as if 6 units wide and 6 tall
    private const int LAYOUT_WIDTH = 350;
    private const int NODE_SIZE = 40;

    //Window stuff
    private StructureNodeGeneratorObject _editorObj = null;
    private StructureNodeGeneratorEditor _editor = null;
    private StructureDbId previousStructureDbId =StructureDbId.None;

    private KeyCode _pressedKey = KeyCode.None;
    private int _zoom = 1;


    //Drag variables
    private Vector2 _initialClick = Vector2.zero;
    private Vector2 _initialClickWithOffset = Vector2.zero;
    private Vector2 _offset = Vector2.zero;
    private Node _pressedNode = null;
    private bool _isDragging = false;
    private bool _drawDraggingLine = false;
    private Vector2 _dragLineEnd = Vector2.zero;

    private List<Node> _nodes = new List<Node>();
    private List<Connection> _connections = new List<Connection>();
    private int _nextIndex = 0;

    public void OnEnable()
    {
        _editorObj = ScriptableObject.CreateInstance<StructureNodeGeneratorObject>();
        _editor = (StructureNodeGeneratorEditor)Editor.CreateEditor(_editorObj);

        _editorObj._nodeTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/~Resources/UI/PictoIcons_128/Pictoicon_Ball.Png");
    }

    public void OnDisable()
    {
        Editor.DestroyImmediate(_editor);
        ScriptableObject.DestroyImmediate(_editorObj);
    }

    public void OnGUI()
    {
        if (previousStructureDbId != _editorObj._structureDbId)
        {
            previousStructureDbId = _editorObj._structureDbId;
            LoadNodes();
        }

        if (_editorObj._structureTexture != null)
        {
            //Get the importer to get the sprite pixel per unit ratio
            var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_editorObj._structureTexture));

            //Get the width and height of the rectangle to draw the texture in
            var width = (_editorObj._structureTexture.width / importer.spritePixelsPerUnit) * WINDOW_PIXELS_TO_UNIT * _zoom;
            var height = (_editorObj._structureTexture.height / importer.spritePixelsPerUnit) * WINDOW_PIXELS_TO_UNIT * _zoom;

            //Center the texture
            var pos = new Rect((position.width - width) / 2 + _offset.x, (position.height - height) / 2 + _offset.y, width, height);

            //Draw the sprite
            GUI.DrawTextureWithTexCoords(pos, _editorObj._structureTexture, new Rect(0, 0, 1, 1));
        }

        if (_editorObj._nodeTexture != null)
        {
            //Draw the connections
            foreach (var connection in _connections)
                DrawLine(connection.node1._position, connection.node2._position);

            //Draw the drag line
            if (_drawDraggingLine && _pressedNode != null)
                DrawLine(_pressedNode._position, _dragLineEnd);

            //Draw the nodes
            foreach (var node in _nodes)
            {
                var width = position.width / 2 + node._position.x * _zoom - NODE_SIZE * _zoom / 2;
                var height = position.height / 2 + node._position.y * _zoom - NODE_SIZE * _zoom / 2;
                var pos = new Rect(width + _offset.x, height + _offset.y, NODE_SIZE * _zoom, NODE_SIZE * _zoom);
                node._rect = pos;
                GUI.DrawTextureWithTexCoords(pos, _editorObj._nodeTexture, new Rect(0, 0, 1, 1));
            }

        }  

        //Draw inspector on the right
        _editor.Draw(position.width - LAYOUT_WIDTH, ClearNodes, GenerateData);

        Event e = Event.current;

        //Process scroll for zoom
        ProcessScrollWheel(e);
        ProcessMouse(e);
    }

    private void ProcessScrollWheel(Event e)
    {
        if (e == null || e.type != EventType.ScrollWheel)
            return;

        if (e.delta.y < 0)
            _zoom = Mathf.Min(_zoom + 1, 3);
        else if (e.delta.y > 0)
            _zoom = Mathf.Max(_zoom - 1, 1);

        e.Use();
    }

    private void ProcessMouse(Event e)
    {
        if (e == null)
            return;

        switch (e.type)
        {
            case EventType.MouseDown:
            {
                _pressedKey = e.button == 2 ? KeyCode.Mouse2 : (e.button == 1 ? KeyCode.Mouse1 : KeyCode.Mouse0);
                _initialClick = e.mousePosition;
                _initialClickWithOffset = e.mousePosition - _offset;
                if (CheckPressOnSprite(e.mousePosition, out Node node))
                    _pressedNode = node;
                e.Use();
            }
            break;
            case EventType.MouseUp:
            {
                if (_pressedKey == KeyCode.Mouse0 && _pressedNode == null && !_isDragging)
                    AddNode(e.mousePosition);
                else if(_pressedKey == KeyCode.Mouse1 && _pressedNode != null && _isDragging)
                {
                    if (CheckPressOnSprite(e.mousePosition, out Node node))
                        AddConnection(_pressedNode, node);
                }
                _pressedKey = KeyCode.None;
                _pressedNode = null;
                _isDragging = false;
                _drawDraggingLine = false;
                e.Use();
            }
            break;
            case EventType.MouseDrag:
            {
                _isDragging = true;
                if (_pressedKey == KeyCode.Mouse2)
                {
                    _offset = e.mousePosition - _initialClickWithOffset;
                    e.Use();
                }
                else if (_pressedKey == KeyCode.Mouse0)
                {
                    if (_pressedNode != null)
                    {
                        _pressedNode._position += (e.mousePosition - _initialClick) / _zoom;
                        _initialClick = e.mousePosition;
                        e.Use();
                    }
                }
                else if(_pressedKey == KeyCode.Mouse1)
                {
                    if(_pressedNode != null)
                    {
                        _drawDraggingLine = true;

                        //Snap to node if we're hovering another one
                        if (CheckPressOnSprite(e.mousePosition, out Node node) && node != _pressedNode)
                            _dragLineEnd = node._position;
                        else
                            _dragLineEnd = new Vector2(e.mousePosition.x - _offset.x - position.width / 2, e.mousePosition.y - _offset.y - position.height / 2) / _zoom;
                        e.Use();
                    }
                }
            }
            break;
        }
    }

    private void LoadNodes()
    {
        ClearNodes();

        if (_editorObj._structureDbId == StructureDbId.None)
            return;

        //load structures from database
        var structures = DatabaseGenerator.GetStructures().Structures;
        if ((int)_editorObj._structureDbId >= structures.Count)
            return;

        //get the structure
        var structure = structures[(int)_editorObj._structureDbId];

        if (structure._pathNodes == null)
            return;

        //add all the nodes
        foreach (var node in structure._pathNodes)        
            _nodes.Add(new Node() { _position = UnitsToNodePosition(node.position), _connections = new List<Connection>(), _index = _nextIndex++ });        

        //add all the connections
        for (int i = 0; i < structure._pathNodes.Length; i++)
        {
            var node = structure._pathNodes[i];
            //Only add connections that come later so we dont do double adds
            foreach (var connection in node.connections)
            {
                if (connection <= i)
                    continue;

                Connection conn = new Connection();
                conn.node1 = _nodes[i];
                conn.node2 = _nodes[connection];
                conn.node1._connections.Add(conn);
                conn.node2._connections.Add(conn);
                _connections.Add(conn);
            }
        }

        //try loading the sprite
        _editorObj._structureTexture = null;
        if (structure._spriteName != null && structure._spriteName != "")
        {
            var asset1 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/~Resources/UI/PictoIcons_128/" + structure._spriteName + ".Png");
            _editorObj._structureTexture = asset1;
        }

        //add remaining vars
        _editorObj._hasTerritory = structure._hasTerritory;
    }

    private void AddNode(Vector2 mousePos)
    {
        mousePos.x -= _offset.x;
        mousePos.y -= _offset.y;
        mousePos.x = mousePos.x - position.width / 2;
        mousePos.y = mousePos.y - position.height / 2;
        _nodes.Add(new Node() { _position = mousePos / _zoom, _connections = new List<Connection>(), _index = _nextIndex++ });
    }

    private void AddConnection(Node node1, Node node2)
    {
        //Check that we don't re-add the same connection
        foreach(var connection in node1._connections)
        {
            if (connection.node1._index == node2._index || connection.node2._index == node2._index)
                return;
        }

        Connection conn = new Connection();
        conn.node1 = node1;
        conn.node2 = node2;
        conn.node1._connections.Add(conn);
        conn.node2._connections.Add(conn);
        conn._rect = DrawLine(conn.node1._position, conn.node2._position);
        _connections.Add(conn);
    }

    private Rect DrawLine(Vector2 from, Vector2 to)
    {

       // width = position.width / 2 + node._position.x * _zoom - NODE_SIZE * _zoom / 2;
       // height = position.height / 2 + node._position.y * _zoom - NODE_SIZE * _zoom / 2;

        Rect line = new Rect(0, 0, 0, _zoom * 3);
        line.x = from.x * _zoom + _offset.x + position.width / 2;
        line.y = from.y * _zoom + _offset.y + position.height / 2;
        line.width = Vector2.Distance(from, to) * _zoom;

        var angle = Vector2.SignedAngle(Vector3.right, (to - from).normalized);
        //Rotate the GUI
        GUIUtility.RotateAroundPivot(angle, new Vector2(line.x, line.y));
        //Draw the line
        //position.width / 2 + node._position.x * _zoom - NODE_SIZE * _zoom / 2
        GUI.DrawTexture(line, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0, Color.green, 0, 0);

        //Rotate the GUI back to original
        GUIUtility.RotateAroundPivot(-angle, new Vector2(line.x, line.y));

        return line;
    }

    private bool CheckPressOnSprite(Vector2 mousePos, out Node pressedNode)
    {
        foreach (var node in _nodes)
        {
            if (node._rect.Contains(mousePos))
            {
                pressedNode = node;
                return true;
            }
        }

        pressedNode = null;

        return false;
    }

    //Methods to be called from inspector
    private void ClearNodes()
    {
        _nodes.Clear();
        _connections.Clear();
        _pressedNode = null;
        _nextIndex = 0;
    }

    private void GenerateData()
    {
        if (!ValidateExportData())
            return;

        Debug.Log("Exporting " + _nodes.Count + " nodes and " + _connections.Count + " connections");

        //Get the structure
        var structureSerialize = DatabaseGenerator.GetStructures();

        //Add empty structures until we match the ID
        if ((int)_editorObj._structureDbId >= structureSerialize.Structures.Count)        
            for (int i = structureSerialize.Structures.Count; i <= (int)_editorObj._structureDbId; i++)
                structureSerialize.Structures.Add(new StructureDb.StructureData());        

        var structure = structureSerialize.Structures[(int)_editorObj._structureDbId];

        structure._pathNodes = ExtractPathNodes().ToArray();
        structure._spriteName = _editorObj._structureTexture.name;
        structure._hasTerritory = _editorObj._hasTerritory;
        structureSerialize.Structures[(int)_editorObj._structureDbId] = structure;

        //Update the json file
        DatabaseGenerator.SaveJson(structureSerialize.JsonName, structureSerialize);
    }

    //Utility
    private List<Pathfinder.PathNode> ExtractPathNodes()
    {
        var nodes = new List<Pathfinder.PathNode>();

        foreach(var node in _nodes)
        {
            var pathNode = new Pathfinder.PathNode();
            pathNode.connections = GetConnectionsForNode(node).ToArray();
            pathNode.position = NodePositionToUnits(node);

            nodes.Add(pathNode);
        }

        return nodes;
    }

    private List<int> GetConnectionsForNode(Node node)
    {
        var connections = new List<int>();

        foreach(var connection in _connections)
        {
            if (connection.node1._index == node._index)
                connections.Add(connection.node2._index);
            else if (connection.node2._index == node._index)
                connections.Add(connection.node1._index);
        }

        return connections;
    }

    private Vector2 NodePositionToUnits(Node node)
    {
        //Invert Y because screen starts at top left but scene starts at bottom left
        return new Vector2(node._position.x / WINDOW_PIXELS_TO_UNIT, -node._position.y / WINDOW_PIXELS_TO_UNIT);
    }

    private Vector2 UnitsToNodePosition(Vector2 position)
    { 
        return new Vector2(position.x * WINDOW_PIXELS_TO_UNIT, -position.y * WINDOW_PIXELS_TO_UNIT); 
    }

    private bool ValidateExportData()
    {

        if (_editorObj._structureDbId == StructureDbId.None)
        {
            Debug.Log("Invalid structure id");
            return false;
        }



        return true;
    }
}