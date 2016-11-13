﻿using System;
using UnityEngine;

public class FollowWayPoint : MonoBehaviour
{
    // This script can be used with any object that is supposed to follow a
    // route marked out by waypoints.

    // This script manages the amount to look ahead along the route,
    // and keeps track of progress and laps.

    [SerializeField]
    private float _lookAheadForTargetOffset = 5;
    // The offset ahead along the route that the we will aim for

    [SerializeField]
    private float _lookAheadForTargetFactor = .1f;
    // A multiplier adding distance ahead along the route to aim for, based on current speed

    [SerializeField]
    private float _lookAheadForSpeedOffset = 10;
    // The offset ahead only the route for speed adjustments (applied as the rotation of the waypoint target transform)

    [SerializeField]
    private float _lookAheadForSpeedFactor = .2f;
    // A multiplier adding distance ahead along the route for speed adjustments

    [SerializeField]
    private float _pointToPointThreshold = 4;
    // proximity to waypoint which must be reached to switch target to next waypoint : only used in PointToPoint mode.

    private string _vehicleID;

    // these are public, readable by other objects - i.e. for an AI to know where to head!
    public PathFinder.RoutePoint TargetPoint { get; private set; }
    public PathFinder.RoutePoint SpeedPoint { get; private set; }
    public PathFinder.RoutePoint ProgressPoint { get; private set; }

    public Transform Target;

    private float _progressDistance; // The progress round the route, used in smooth mode.
    private int _progressNum; // the current waypoint number, used in point-to-point mode.

    internal void setID(string id)
    {
        _vehicleID = id;
    }

    private Vector3 _lastPosition; // Used to calculate current speed (since we may not have a rigidbody component)
    private float _speed; // current speed of this object (calculated from delta since last frame)

    // setup script properties
    private void Start()
    {
        // we use a transform to represent the point to aim for, and the point which
        // is considered for upcoming changes-of-speed. This allows this component
        // to communicate this information to the AI without requiring further dependencies.

        // You can manually create a transform and assign it to this component *and* the AI,
        // then this component will update it, and the AI can read it.

        if (Target == null)
        {
            Target = new GameObject(name + " Waypoint Target").transform;
        }

        Reset();
    }

    // TODO:: May not be necessary when this functionality becomes integrated with Curve Implementation.
    // reset the object to sensible values.
    public void Reset()
    {
        _progressDistance = 0;
        _progressNum = 0;

        Target.position = TrackManager.Track.Points[_progressNum].Position;
        Target.rotation = TrackManager.Track.Points[_progressNum].Rotation;
    }


    private void Update()
    {
        Vector3 targetDelta = Target.position - transform.position;

        if (targetDelta.magnitude < _pointToPointThreshold)
        {
            _progressNum = (_progressNum + 1) % TrackManager.Track.Points.Count;
            Debug.Log(_vehicleID + " is at position: " + _progressNum);
        }


        Target.position = TrackManager.Track.Points[_progressNum].Position;
        Target.rotation = TrackManager.Track.Points[_progressNum].Rotation;

        // get our current progress along the route

        ProgressPoint = PathFinder.Instance.GetRoutePoint(_progressDistance);

        Vector3 progressDelta = ProgressPoint.position - transform.position;

        if (Vector3.Dot(progressDelta, ProgressPoint.direction) < 0)
        {
            _progressDistance += progressDelta.magnitude;
        }

        _lastPosition = transform.position;
    }

    public void getSpawnPoint(Transform carBody)
    {
        //TODO:: work out why the respawn doesn't work when it is so high, changing the value to 50, is too high...
        CurveImplementation.SetSpawnTransform(carBody, _progressDistance, 20);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawLine(transform.position, Target.position);
            Gizmos.DrawWireSphere(PathFinder.Instance.GetRoutePosition(_progressDistance), 1);

            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(Target.position, Target.position + Target.forward);
        }
    }
}
