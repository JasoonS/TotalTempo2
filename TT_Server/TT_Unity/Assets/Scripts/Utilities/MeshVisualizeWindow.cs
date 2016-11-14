using UnityEngine;
using UnityEditor;

public class MeshVisualizeWindow : EditorWindow
{
    [MenuItem("Window/Mesh visualizer")]
    public static void OpenWindow()
    {
        GetWindow<MeshVisualizeWindow>().Focus();
    }

    private Mesh _selectedMesh;
    private Vector3[] _selectedVertices, _selectedNormals;
    private Color[] _selectedColors, _randomColors;

    private int[] _selectedTriangles;

    private int _startVertIndex, _endVertIndex, _startTriIndex, _endTriIndex;

    private bool _shouldVisualizeVertices = true, _shouldVisualizeTriangles = false;

    private bool _useVertexColors = true;

    private bool _hasVertexColors { get { return _selectedMesh.colors != null && _selectedMesh.colors.Length > 0; } }

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;

        init();
        _randomColors = generateColors(255);
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    void OnSelectionChange()
    {
        init();
        //Force to repaint UI, otherwise the new values arent visible until you interact with the window 
        Repaint();
    }

    //Initialises the variables for the current selection.

    private void init()
    {
        //Reset to default

        _selectedMesh = null;

        if (Selection.activeGameObject == null)
            return;

        MeshFilter mFilter;
        SkinnedMeshRenderer mSkinRender;

        if ((mFilter = Selection.activeGameObject.GetComponent<MeshFilter>()) != null)
            _selectedMesh = mFilter.sharedMesh;

        if ((mSkinRender = Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>()) != null)
            _selectedMesh = mSkinRender.sharedMesh;

        if (_selectedMesh == null || (_selectedVertices != null && _selectedMesh.vertices.Length == _selectedVertices.Length))
            return;

        _selectedVertices = _selectedMesh.vertices;
        _selectedNormals = _selectedMesh.normals;
        _selectedTriangles = _selectedMesh.triangles;
        _selectedColors = validateColors(_selectedMesh.colors);

        if (!_hasVertexColors || !_useVertexColors)
            _selectedColors = generateColors(_selectedVertices.Length);

        _startVertIndex = 0;

        _endVertIndex = _selectedVertices.Length;
        _endTriIndex = _selectedTriangles.Length / 3;
    }

    private Color[] validateColors(Color[] colors)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i].a < .5f)
                colors[i].a = 1f;
        }

        return colors;
    }

    private Color[] generateColors(int size)
    {
        Color[] cols = new Color[size];

        for (int i = 0; i < cols.Length; i++)
            cols[i] = new Color(Random.value, Random.value, Random.value);

        return cols;
    }

    #region GUI 

    void OnGUI()
    {
        if (_selectedMesh == null)
        {
            EditorGUILayout.HelpBox("Please select a meshfilter or skinnedmeshrenderer!", MessageType.Error);

            return;
        }

        drawGeneralGUI();

        EditorGUILayout.Space();

        drawVertexGUI();

        EditorGUILayout.Space();

        drawTriangleGUI();

        if (GUI.changed)
            SceneView.RepaintAll();
    }

    private void drawGeneralGUI()
    {
        EditorGUILayout.LabelField("General settings", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField(string.Format("Vertex count: {0}, triangle count: {1}", _selectedVertices.Length, _selectedMesh.triangles.Length / 3));

        if (_hasVertexColors)
        {
            _useVertexColors = EditorGUILayout.Toggle("Use vertex colors", _useVertexColors);

            if (GUI.changed && _useVertexColors)
                _selectedColors = validateColors(_selectedMesh.colors);
            else if (GUI.changed && !_useVertexColors)
                _selectedColors = generateColors(_selectedVertices.Length);
        }

        EditorGUI.indentLevel--;
    }
    private void drawVertexGUI()
    {
        _shouldVisualizeVertices = EditorGUILayout.BeginToggleGroup("Show vertices", _shouldVisualizeVertices);

        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("Visualise from index: " + _startVertIndex + " to " + _endVertIndex);

        //Lame, MinMaxSlider only has a float version!

        float tmpStart = _startVertIndex, tmpEnd = _endVertIndex;

        EditorGUILayout.MinMaxSlider(ref tmpStart, ref tmpEnd, 0, _selectedVertices.Length);

        _startVertIndex = (int)tmpStart;
        _endVertIndex = (int)tmpEnd;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Min");

        if (GUILayout.Button("-"))
            _startVertIndex = Mathf.Clamp(_startVertIndex - 1, 0, _endVertIndex);
        if (GUILayout.Button("+"))
            _startVertIndex = Mathf.Clamp(_startVertIndex + 1, 0, _endVertIndex);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Max");

        if (GUILayout.Button("-"))
            _endVertIndex = Mathf.Clamp(_endVertIndex - 1, _startVertIndex, _selectedVertices.Length);
        if (GUILayout.Button("+"))
            _endVertIndex = Mathf.Clamp(_endVertIndex + 1, _startVertIndex, _selectedVertices.Length);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndToggleGroup();

        EditorGUI.indentLevel--;
    }

    private void drawTriangleGUI()
    {
        _shouldVisualizeTriangles = EditorGUILayout.BeginToggleGroup("Show triangles", _shouldVisualizeTriangles);

        EditorGUILayout.LabelField("Visualise from index: " + _startTriIndex + " to " + _endTriIndex);
        EditorGUI.indentLevel++;

        //Lame, MinMaxSlider only has a float version! 
        float tmpStart = _startTriIndex, tmpEnd = _endTriIndex;
        EditorGUILayout.MinMaxSlider(ref tmpStart, ref tmpEnd, 0, _selectedTriangles.Length / 3f);
        _startTriIndex = (int)tmpStart;
        _endTriIndex = (int)tmpEnd;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Min");

        if (GUILayout.Button("-"))
            _startTriIndex = Mathf.Clamp(_startTriIndex - 1, 0, _endTriIndex);
        if (GUILayout.Button("+"))
            _startTriIndex = Mathf.Clamp(_startTriIndex + 1, 0, _endTriIndex);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Max");

        if (GUILayout.Button("-"))
            _endTriIndex = Mathf.Clamp(_endTriIndex - 1, _startTriIndex, _selectedTriangles.Length / 3);
        if (GUILayout.Button("+"))
            _endTriIndex = Mathf.Clamp(_endTriIndex + 1, _startTriIndex, _selectedTriangles.Length / 3);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndToggleGroup();
        EditorGUI.indentLevel--;
    }

    #endregion

    #region Scene 
    private void OnSceneGUI(SceneView sceneView)
    {
        if (_selectedMesh == null || Selection.activeGameObject == null)
            return;

        //Everything we do is in local space

        Handles.matrix = Selection.activeGameObject.transform.localToWorldMatrix;

        if (_shouldVisualizeVertices)
            visualizeVertices();

        if (_shouldVisualizeTriangles)
            visualizeTriangles();

    }

    private void visualizeVertices()
    {
        for (int i = _startVertIndex; i < _endVertIndex; i++)
        {
            Handles.color = _selectedColors[i];

            DrawRay(_selectedVertices[i], _selectedNormals[i]);
            Handles.Label(_selectedVertices[i] + _selectedNormals[i] * 1.1f, i.ToString());
        }
    }

    private void visualizeTriangles()
    {
        for (int i = _startTriIndex * 3; i < _endTriIndex * 3; i += 3)
        {
            Handles.color = _randomColors[i % _randomColors.Length];
            Vector3 p1 = _selectedVertices[_selectedTriangles[i + 0]],
                p2 = _selectedVertices[_selectedTriangles[i + 1]],
                p3 = _selectedVertices[_selectedTriangles[i + 2]];

            DrawArrow(p1, p2 - p1);
            DrawArrow(p2, p3 - p2);
            DrawArrow(p3, p1 - p3);

            Handles.Label((p1 + p2 + p3) / 3f, (i / 3).ToString());
        }

    }

    //Draws an arrow in the handles. `arrowatpercent` determines where the arrow cap is, 1 means at the end 0 at the beginning 

    static void DrawArrow(Vector3 origin, Vector3 direction, float arrowAtPercent = .8f)
    {
        DrawRay(origin, direction);
        DrawRay(origin + direction * arrowAtPercent, Quaternion.LookRotation(direction) * Quaternion.Euler(0, 200f, 0) * Vector3.forward * 0.25f);
        DrawRay(origin + direction * arrowAtPercent, Quaternion.LookRotation(direction) * Quaternion.Euler(0, 160f, 0) * Vector3.forward * 0.25f);
    }

    //Draws a line that starts at origin and ends at origin+direction 

    static void DrawRay(Vector3 origin, Vector3 direction)
    {
        Handles.DrawLine(origin, origin + direction);
    }

    #endregion
}