using UnityEngine;

using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode()]
public class CurveImplementationDuplicate : MonoBehaviour
{

    public Vector3 VertLeftTopFront = new Vector3(-2, 8, 4);
    public Vector3 VertRightTopFront = new Vector3(2, 8, 4);
    public Vector3 VertRightTopBack = new Vector3(2, 3, 0);
    public Vector3 VertLeftTopBack = new Vector3(-2, 3, 0);

    //public GameObject[] _points = new GameObject[4];

    public List<Waypoint> _points = new List<Waypoint>();

    public int CurveResolution = 10;

    public Vector3[] CurveCoordinates;
    public Vector3[] Tangents;
    public Vector3[] TrackUp;

    public bool ClosedLoop = false;

    public float width = 1;

    //public Material RoadMaterial;

    void Start()
    {
        Waypoint[] way_points = GetComponentsInChildren<Waypoint>();

        Debug.Log("Size_initial:::");
        Debug.Log(way_points.Length);

        _points.Clear();
        foreach (Waypoint waypoint in way_points)
        {
            _points.Add(waypoint);
            //Debug.Log("WE HAVE A WAYPOINT!!!");
        }

        Debug.Log("Sizebefore:::");
        Debug.Log(_points.Count);

        Update();

        Debug.Log("SizeAfter:::");
        Debug.Log(_points.Count);
    }

    void Update()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = mf.mesh;

        Vector3 p0;
        Vector3 p1;
        Vector3 m0;
        Vector3 m1;

        int pointsToMake;

        int closedAdjustment = ClosedLoop ? 0 : 1;

        pointsToMake = (CurveResolution) * (_points.Count - closedAdjustment);

        CurveCoordinates = new Vector3[pointsToMake];

        Tangents = new Vector3[pointsToMake];

        TrackUp = new Vector3[pointsToMake];

        Vector3[] vertices = new Vector3[pointsToMake * 2 + 2];

        int[] triangles = new int[pointsToMake * 6];

        // First for loop goes through each individual control point and connects it to the next, so 0-1, 1-2, 2-3 and so on

        //int index = 0;
        Debug.Log("Size of Points inside: " + _points.Count + " closed ajust: " + closedAdjustment);

        for (int i = 0; i < _points.Count - closedAdjustment; i++)
        {
            Debug.Log("MainLoop" + i);
            //int index = 0;
            //if (_points[i] == null || _points[i + 1] == null || (i > 0 && _points[i - 1] == null) || (i < _points.Count - 2 && _points[i + 2] == null))
            //{
            //    return;
            //}

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

            Vector3 position;

            float t;
            float pointStep = 1.0f / CurveResolution;


            if ((i == _points.Count - 2 && ClosedLoop == false) || (i == _points.Count - 1 && ClosedLoop))
            {
                pointStep = 1.0f / (CurveResolution - 1);

                // last point of last segment should reach p1
            }

            // Second for loop actually creates the spline for this particular segment

            Debug.Log("Curve resolution: " + CurveResolution);
            for (int j = 0; j < CurveResolution; j++)
            {
                t = j * pointStep;

                Vector3 tangent;

                position = CatmullRom.Interpolate(p0, p1, m0, m1, t, out tangent);

                CurveCoordinates[i * CurveResolution + j] = position;

                Tangents[i * CurveResolution + j] = tangent;

                float percentThrough = (float)j / (float)CurveResolution;

                TrackUp[i * CurveResolution + j] = Vector3.Lerp(_points[i].Up, _points[(i + 1) % _points.Capacity].Up, percentThrough);

                //Debug.DrawRay(position, tangent.normalized * 2, Color.red);

                //Debug.DrawLine(position + Vector3.Cross(tangent, Vector3.up).normalized, position - Vector3.Cross(tangent, Vector3.up).normalized, Color.red);

                //vertices[index * 2] = CurveCoordinates[index] + Vector3.Cross(Tangents[index], TrackUp[index]).normalized * width;
                //vertices[index * 2 + 1] = CurveCoordinates[index] - Vector3.Cross(Tangents[index], TrackUp[index]).normalized * width;

                ////Debug.Log(index + " == " +  (i * CurveResolution + j));
                //Debug.Log(index + " Inside..");

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(CurveCoordinates[i * CurveResolution + j] + Vector3.Cross(Tangents[i * CurveResolution + j], TrackUp[i * CurveResolution + j]).normalized * width, 0.25f);
                Gizmos.DrawSphere(CurveCoordinates[i * CurveResolution + j] - Vector3.Cross(Tangents[i * CurveResolution + j], TrackUp[i * CurveResolution + j]).normalized * width, 0.25f);

                //triangles[index * 6] = index * 2 + 1;
                //triangles[index * 6 + 1] = index * 2;
                //triangles[index * 6 + 2] = index * 2 + 2;
                //triangles[index * 6 + 3] = index * 2 + 1;
                //triangles[index * 6 + 4] = index * 2 + 2;
                //triangles[index * 6 + 5] = index * 2 + 3;

                //index += 1;
                Debug.Log("inside...");
            }
            Debug.Log("at the end, phewww");
        }

        vertices[CurveCoordinates.Length * 2] = CurveCoordinates[0] + Vector3.Cross(Tangents[0], Vector3.up).normalized * width;
        vertices[CurveCoordinates.Length * 2 + 1] = CurveCoordinates[1] - Vector3.Cross(Tangents[1], Vector3.up).normalized * width;

        for (int i = 0; i < CurveCoordinates.Length - 1; ++i)
        {
            Debug.DrawLine(CurveCoordinates[i], CurveCoordinates[i + 1]);
        }

        //// TODO:: do this in the same loop as above...
        for (int index = 0; index < CurveCoordinates.Length; ++index)
        {
            //float percentThrough = (float)index / (float)CurveResolution;

            //float offset = Vector3.Lerp(_points[i].Up, _points[(i + 1) % _points.Capacity].Up, percentThrough);

            vertices[index * 2] = CurveCoordinates[index] + Vector3.Cross(Tangents[index], TrackUp[index]).normalized * width;
            vertices[index * 2 + 1] = CurveCoordinates[index] - Vector3.Cross(Tangents[index], TrackUp[index]).normalized * width;

            triangles[index * 6] = index * 2 + 1;
            triangles[index * 6 + 1] = index * 2;
            triangles[index * 6 + 2] = index * 2 + 2;
            triangles[index * 6 + 3] = index * 2 + 1;
            triangles[index * 6 + 4] = index * 2 + 2;
            triangles[index * 6 + 5] = index * 2 + 3;
        }

        vertices[CurveCoordinates.Length * 2] = CurveCoordinates[0] + Vector3.Cross(Tangents[0], Vector3.up).normalized * width;
        vertices[CurveCoordinates.Length * 2 + 1] = CurveCoordinates[1] - Vector3.Cross(Tangents[1], Vector3.up).normalized * width;

        mesh.Clear();
        mesh.vertices = vertices;

        mesh.triangles = triangles;

        mesh.Optimize();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshCollider MCollider = GetComponent<MeshCollider>();
        MCollider.sharedMesh = mesh;
    }

    //public void Extrude(Mesh mesh, ExtrudeShape shape, OrientedPoint[] path)
    //{
    //    mesh.Clear();
    //    mesh.vertices = vertices;
    //    mesh.normals = normals;
    //    mesh.uv = uvs;
    //    mesh.triangles = triangleIndices;
    //}

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        for (int i = 0; i < CurveCoordinates.Length; i++)
        {
            Gizmos.DrawWireCube(CurveCoordinates[i], new Vector3(.9f, .1f, .1f));
        }
    }
}