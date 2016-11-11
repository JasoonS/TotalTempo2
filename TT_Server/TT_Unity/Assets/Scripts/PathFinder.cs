using UnityEngine;

using System.Collections.Generic;

public class PathFinder : MonoBehaviour
{
    public Color RayColor = Color.white;

    public List<Transform> Waypoints = new List<Transform>();

    private float[] _distances;

    private Vector3[] _points;

    private Transform[] _childWaypoints;

    public bool SmoothCorners = true;
    public int NumGizmoLines;

    public float TotalLength;
    private int _numPoints = 1;

    private bool _runOnce = false;

    // the following 3 lines contain variables used in the smoothing/Catmull-Rom process

    private int _p0n;
    private int _p1n;
    private int _p2n;
    private int _p3n;

    private float _i;

    private Vector3 _P0;
    private Vector3 _P1;
    private Vector3 _P2;
    private Vector3 _P3;

    private static PathFinder _curve;

    // Use the static object pattern to guarantee that this object is correctly assigned and pressent in the scene.
    public static PathFinder Instance
    {
        get
        {
            if (!_curve)
            {
                _curve = FindObjectOfType(typeof(PathFinder)) as PathFinder;

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
        _childWaypoints = GetComponentsInChildren<Transform>();

        Waypoints.Clear();

        foreach (Transform waypoint in _childWaypoints)
        {
            if (waypoint != transform)
            {
                Waypoints.Add(waypoint);
            }
        }

        _numPoints = Waypoints.Count;

        FillAccumalativeDistances();
        TotalLength = _distances[_distances.Length - 1];
    }

    // function to create lookup of distances and

    private void FillAccumalativeDistances()
    {
        _distances = new float[Waypoints.Count + 1];

        float accumulateDistance = 0;

        for (int i = 0; i < Waypoints.Count + 1; ++i)
        {
            var t1 = Waypoints[(i) % Waypoints.Count];
            var t2 = Waypoints[(i + 1) % Waypoints.Count];

            if (t1 != null && t2 != null)
            {
                _distances[i] = accumulateDistance;

                accumulateDistance += (t1.position - t2.position).magnitude;
            }
        }
    }

    public RoutePoint GetRoutePoint(float dist)
    {
        // position and direction

        Vector3 p1 = GetRoutePosition(dist);
        Vector3 p2 = GetRoutePosition(dist + 0.1f);

        Vector3 delta = p2 - p1;

        return new RoutePoint(p1, delta.normalized);
    }

    public Vector3 GetRoutePosition(float dist)
    {
        int point = 0;

        dist = Mathf.Repeat(dist, TotalLength);

        while (_distances[point] < dist)
        {
            ++point;
        }

        // get nearest two points, ensuring points wrap-around start & end of circuit

        _p1n = ((point - 1) + _numPoints) % _numPoints;
        _p2n = point;

        // found point numbers, now find interpolation value between the two middle points

        _i = Mathf.InverseLerp(_distances[_p1n], _distances[_p2n], dist);

        if (SmoothCorners)
        {
            // Get the surrounding points required for the Catmull-Rom algorithm.

            _p0n = ((point - 2) + _numPoints) % _numPoints;
            _p2n = _p2n % _numPoints;
            _p3n = (point + 1) % _numPoints;

            _P0 = Waypoints[_p0n].position;
            _P1 = Waypoints[_p1n].position;
            _P2 = Waypoints[_p2n].position;
            _P3 = Waypoints[_p3n].position;

            return CatmullRom(_P0, _P1, _P2, _P3, _i);
        }

        else
        {
            // linear lerp between these two points:

            _p1n = ((point - 1) + _numPoints) % _numPoints;
            _p2n = point;

            return Vector3.Lerp(Waypoints[_p1n].position, Waypoints[_p2n].position, _i);
        }
    }

    // Wikipedia knows best ;) This is a standard algorithm.

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i)
    {
        return 0.5f *
               ((2 * p1) + (-p0 + p2) * i + (2 * p0 - 5 * p1 + 4 * p2 - p3) * i * i +
                (-p0 + 3 * p1 - 3 * p2 + p3) * i * i * i);
    }

    private void OnDrawGizmos()
    {
        // to avoid stupid errors (need more than one to make a looooop ;) )

        if (Waypoints.Count > 1)
        {
            Vector3 previous = Waypoints[0].position;

            if (SmoothCorners)
            {
                for (float dist = 0; dist < TotalLength; dist += TotalLength / NumGizmoLines)
                {
                    Vector3 next = GetRoutePosition(dist + 1);

                    //Gizmos.DrawLine(prev, next);

                    Gizmos.DrawLine(previous, next);
                    Gizmos.DrawWireSphere(next, 0.3f);

                    previous = next;
                }

                Gizmos.DrawLine(previous, Waypoints[0].position);
            }

            else
            {
                for (int i = 0; i < Waypoints.Count; i++)
                {
                    previous = Waypoints[i].position;

                    Vector3 position = Waypoints[(i + 1) % _numPoints].position;

                    Gizmos.DrawLine(previous, position);
                    Gizmos.DrawWireSphere(position, 2f);
                }
            }
        }
    }

    public struct RoutePoint
    {
        public Vector3 position;
        public Vector3 direction;

        public RoutePoint(Vector3 position, Vector3 direction)
        {
            this.position = position;
            this.direction = direction;
        }
    }
}