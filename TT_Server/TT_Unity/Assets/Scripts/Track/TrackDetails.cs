using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrackDetails : MonoBehaviour {
    public List<Waypoint> Points = new List<Waypoint>();

    void Start()
    {
        Waypoint[] wayPoints = GetComponentsInChildren<Waypoint>();

        Points.Clear();
        foreach (Waypoint waypoint in wayPoints)
        {
            Points.Add(waypoint);
        }
    }
}
