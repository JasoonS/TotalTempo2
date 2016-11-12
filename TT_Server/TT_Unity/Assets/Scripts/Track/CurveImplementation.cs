using UnityEngine;

using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode()]
public class CurveImplementation : MonoBehaviour
{
    public List<Waypoint> _points = new List<Waypoint>();

    public int CurveResolution = 500;

    public bool ClosedLoop = false;

    private float[] _distances;
    private float[] _secDistances;

    public float TotalLength;

    private Renderer renderer;
    public Material[] materials;
    private int lastColourIndex = 0;

    Vector3 p0;
    Vector3 p1;
    Vector3 m0;
    Vector3 m1;

    private static CurveImplementation _curve;

    // Use the static object pattern to guarantee that this object is correctly assigned and pressent in the scene.
    public static CurveImplementation Instance
    {
        get
        {
            if (!_curve)
            {
                _curve = FindObjectOfType(typeof(CurveImplementation)) as CurveImplementation;

                if (!_curve)
                {
                    Debug.LogError("You need to have at least one active Event Manager script in your scene.");
                }
            }
            return _curve;
        }
    }

    void Start()
    {
        Waypoint[] way_points = GetComponentsInChildren<Waypoint>();

        _points.Clear();
        foreach (Waypoint waypoint in way_points)
        {
            _points.Add(waypoint);
        }

        FillAccumalativeDistances();
        TotalLength = _distances[_distances.Length - 1];

        renderer = GetComponent<Renderer>();

        UpdateTrack();

        EventManager.StartListening("k_changeColour", ChangeColour);
    }

    void OnDisable()
    {
        EventManager.StopListening("k_changeColour", ChangeColour);
    }

    public void ChangeColour()
    {
        //Debug.Log("MY OWN COLOUR CHANGE");
        int index = Random.Range(0, materials.Length);
        if (index == lastColourIndex)
        {
            index = (1 + index) % materials.Length;
        }
        renderer.material = materials[index];

        lastColourIndex = index;
    }

    private void FillAccumalativeDistances()
    {
        _distances = new float[_points.Count + 1];
        _secDistances = new float[_points.Count + 1];

        float accumulateDistance = 0;

        for (int i = 0; i < _points.Count + 1; ++i)
        {
            var t1 = _points[(i) % _points.Count].Position;
            var t2 = _points[(i + 1) % _points.Count].Position;

            if (t1 != null && t2 != null)
            {
                _distances[i] = accumulateDistance;
                _secDistances[i] = (t1 - t2).magnitude;

                accumulateDistance += _secDistances[i];
            }
        }
    }

    void UpdateTrack()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = mf.sharedMesh;

        int closedAdjustment = ClosedLoop ? 0 : 1;

        int pointsToMake = CurveResolution + 1;

        Vector3[] vertices = new Vector3[pointsToMake * 2 + 2];

        int[] triangles = new int[pointsToMake * 6];

        float currentDistance = 0.0f;
        float step = TotalLength / CurveResolution;

        int ind = 0;

        // First for loop goes through each individual control point and connects it to the next, so 0-1, 1-2, 2-3 and so on
        for (int i = 0; i < _points.Count - closedAdjustment; i++)
        {
            calculatePandM(i);

            Vector3 position;

            float t;

            // Second for loop actually creates the spline for this particular segment
            while (currentDistance <= _distances[i + 1])
            {
                t = Mathf.InverseLerp(_distances[i], _distances[i + 1], currentDistance);

                Vector3 tangent;

                float percentThrough = t;

                position = CatmullRom.Interpolate(p0, p1, m0, m1, t, out tangent);

                Vector3 normal = Vector3.Lerp(_points[i].Up, _points[(i + 1) % _points.Capacity].Up, percentThrough);
                float center = Mathf.Lerp(_points[i].Centre, _points[(i + 1) % _points.Capacity].Centre, percentThrough);
                float width = Mathf.Lerp(_points[i].Width, _points[(i + 1) % _points.Capacity].Width, percentThrough);

                vertices[ind * 2] = position + Vector3.Cross(tangent, normal).normalized * width * (1 + center);
                vertices[ind * 2 + 1] = position - Vector3.Cross(tangent, normal).normalized * width * (1 - center);

                triangles[ind * 6] = ind * 2 + 1;
                triangles[ind * 6 + 1] = ind * 2;
                triangles[ind * 6 + 2] = ind * 2 + 2;
                triangles[ind * 6 + 3] = ind * 2 + 1;
                triangles[ind * 6 + 4] = ind * 2 + 2;
                triangles[ind * 6 + 5] = ind * 2 + 3;

                ++ind;
                currentDistance += step;
            }
        }

        vertices[pointsToMake * 2] = vertices[0];
        vertices[pointsToMake * 2 + 1] = vertices[1];

        mesh.Clear();
        mesh.vertices = vertices;

        mesh.triangles = triangles;

        mesh.Optimize();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshCollider MCollider = GetComponent<MeshCollider>();
        MCollider.sharedMesh = mesh;
    }

    private void calculatePandM(int i)
    {
        p0 = _points[i].Position;
        p1 = (ClosedLoop == true && i == _points.Count - 1) ? _points[0].Position : _points[i + 1].Position;

        // Tangent calculation for each control point
        // Tangent M[k] = (P[k+1] - P[k-1]) / 2
        // With [] indicating subscript
        // m0
        if (i == 0)
        {
            m0 = ClosedLoop ? 0.5f * (p1 - _points[_points.Count - 1].Position) : p1 - p0;
        }
        else
        {
            m0 = 0.5f * (p1 - _points[i - 1].Position);
        }
        // m1
        if (ClosedLoop)
        {
            if (i == _points.Count - 1)
            {
                m1 = 0.5f * (_points[(i + 2) % _points.Count].Position - p0);
            }
            else if (i == 0)
            {
                m1 = 0.5f * (_points[i + 2].Position - p0);
            }
            else
            {
                m1 = 0.5f * (_points[(i + 2) % _points.Count].Position - p0);
            }
        }
        else
        {
            if (i < _points.Count - 2)
            {
                m1 = 0.5f * (_points[(i + 2) % _points.Count].Position - p0);
            }
            else
            {
                m1 = p1 - p0;
            }
        }
    }

    public static void SetSpawnTransform(Transform car, float dist, int height)
    {
        int point = 0;

        dist = Mathf.Repeat(dist, Instance.TotalLength);

        while (Instance._distances[point] < dist)
        {
            ++point;
        }

        if (dist == 0)
            ++point;

        Instance.calculatePandM(--point);

        float t = Mathf.InverseLerp(Instance._distances[point], Instance._distances[point + 1], dist);

        Vector3 tangent;

        float percentThrough = t;

        Vector3 position = CatmullRom.Interpolate(Instance.p0, Instance.p1, Instance.m0, Instance.m1, t, out tangent);

        Vector3 normal = Vector3.Lerp(Instance._points[point].Up, Instance._points[(point + 1) % Instance._points.Capacity].Up, percentThrough);
        float center = Mathf.Lerp(Instance._points[point].Centre, Instance._points[(point + 1) % Instance._points.Capacity].Centre, percentThrough);
        float width = Mathf.Lerp(Instance._points[point].Width, Instance._points[(point + 1) % Instance._points.Capacity].Width, percentThrough);

        car.position = position + Vector3.Cross(tangent, normal).normalized * width * center + height * Vector3.up;
        car.up = normal;
        car.forward = tangent;
    }

    public static void SetTokenPositions(int numTokens)
    {
        int closedAdjustment = Instance.ClosedLoop ? 0 : 1;

        float currentDistance = 0.0f;
        float step = Instance.TotalLength / numTokens;

        int ind = 0;

        //// First for loop goes through each individual control point and connects it to the next, so 0-1, 1-2, 2-3 and so on
        for (int i = 0; i < Instance._points.Count - closedAdjustment; i++)
        {
            Instance.calculatePandM(i);

            Vector3 position;

            float t;

            while (currentDistance < Instance._distances[i + 1])
            {
                t = Mathf.InverseLerp(Instance._distances[i], Instance._distances[i + 1], currentDistance);

                Vector3 tangent;

                float percentThrough = t;

                position = CatmullRom.Interpolate(Instance.p0, Instance.p1, Instance.m0, Instance.m1, t, out tangent);

                Vector3 normal = Vector3.Lerp(Instance._points[i].Up, Instance._points[(i + 1) % Instance._points.Capacity].Up, percentThrough);
                float center = Mathf.Lerp(Instance._points[i].Centre, Instance._points[(i + 1) % Instance._points.Capacity].Centre, percentThrough);
                float width = Mathf.Lerp(Instance._points[i].Width, Instance._points[(i + 1) % Instance._points.Capacity].Width, percentThrough);

                float centerOffset = Random.Range(-1.0f, 1.0f);

                TokenSpawner.SpawnTokensAtPoint(2 * normal + position + Vector3.Cross(tangent, normal).normalized * width * (center + centerOffset));
                
                ++ind;
                currentDistance += step;
            }
        }
    }

    void OnDrawGizmos()
    {
        int closedAdjustment = ClosedLoop ? 0 : 1;

        float currentDistance = 0.0f;
        float step = TotalLength / CurveResolution;

        int ind = 0;

        //// First for loop goes through each individual control point and connects it to the next, so 0-1, 1-2, 2-3 and so on
        for (int i = 0; i < _points.Count - closedAdjustment; i++)
        {
            calculatePandM(i);

            Vector3 position;

            float t;

            // Second for loop actually creates the spline for this particular segment
            while (currentDistance < _distances[i + 1])
            {
                t = Mathf.InverseLerp(_distances[i], _distances[i + 1], currentDistance);

                Vector3 tangent;

                float percentThrough = t;

                position = CatmullRom.Interpolate(p0, p1, m0, m1, t, out tangent);

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(position, 0.25f);

                Vector3 normal = Vector3.Lerp(_points[i].Up, _points[(i + 1) % _points.Capacity].Up, percentThrough);
                float center = Mathf.Lerp(_points[i].Centre, _points[(i + 1) % _points.Capacity].Centre, percentThrough);
                float width = Mathf.Lerp(_points[i].Width, _points[(i + 1) % _points.Capacity].Width, percentThrough);

                Gizmos.color = Color.green;
                Gizmos.DrawSphere(position + Vector3.Cross(tangent, normal).normalized * width * (1 + center), 0.5f);
                Gizmos.DrawSphere(position - Vector3.Cross(tangent, normal).normalized * width * (1 - center), 0.5f);

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(position + Vector3.Cross(tangent, normal).normalized * width * center, 0.8f);

                ++ind;
                currentDistance += step;
            }
        }
    }
}