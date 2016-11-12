// Class to manage the movement of a racing agent.

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody))]
public class HoverMotor : MonoBehaviour
{
    //TODO:: use a layer mask to increase efficiency, and prevent accidental hits.
    //TODO:: experiment with using Vector3.down vs (thruster.up * 1)
    public float HoverForce = 100.0f;
    public float HoverHeight = 4.0f;
    public float FloatHeight = 8.0f;
    public float Speed = 200.0f;
    public float BackwardsSpeed = 100.0f;
    public float TurnSpeed = 80f;
    public float MaxDrag = 4f;
    public float TrackSuction = 4f;
    public float JumpThrust = 5000;

    public Transform[] Thrusters;

    // TODO:: is this used at all anymore??
    public float MaxSpeed = 36.80006f;

    public float CurrentSpeed { get { return CarRigidBody.velocity.magnitude * 2.23693629f; } }

    public Transform Centre;

    Rigidbody CarRigidBody;

    private int TrackMask;

    float maxSpeed;

    private FollowWayPoint _waypointFollow;

    private UnityAction spawnListener;

    public bool _setToSpawn;

    //TODO:: set this in a better way, auto assign an id?
    public string id;

    public bool isPlayer;

    private float powerInput = 0.0f;
    private float turnInput = 0.0f;

    private float _distanceRatio;
    private float _amountOverThrustors;
    private float _aboveThrusterDistance;

    void Start()
    {
        spawnListener = new UnityAction(Spawn);

        CarRigidBody = GetComponent<Rigidbody>();

        _waypointFollow = GetComponent<FollowWayPoint>();

        _aboveThrusterDistance = FloatHeight - HoverHeight;

        //TrackMask = 1 << LayerMask.NameToLayer("CarBody");
        TrackMask = 1 << LayerMask.NameToLayer("Track");

        _setToSpawn = false;
        //TrackMask = ~TrackMask;
        EventManager.StartListening(id, spawnListener);
    }

    void OnDisable()
    {
        EventManager.StopListening(id, spawnListener);
    }

    internal void Move(float powerInputIn, float turnInputIn)
    {
        powerInput = powerInputIn;
        turnInput = turnInputIn;
    }

    internal void Jump()
    {
        RaycastHit hit;

        if (Physics.Raycast(Centre.position, Vector3.down, out hit, HoverHeight, TrackMask))
        {
            CarRigidBody.AddForce(Vector3.up * JumpThrust);
        }
    }

    void FixedUpdate()
    {
        RaycastHit hit;

      //  float ratioSqrd;

       // move vehicle with force relative to track.
       if (Physics.Raycast(Centre.position, -Centre.up, out hit, FloatHeight, TrackMask))
       {
           //float floatRatio = Mathf.Pow((1.0f - (hit.distance / FloatHeight)), 2);
           _distanceRatio = (1.0f - (hit.distance / FloatHeight));
           if (hit.distance > HoverHeight)
           {
               _amountOverThrustors = (hit.distance - HoverHeight) / _aboveThrusterDistance;
           }

          //  ratioSqrd = _distanceRatio * _distanceRatio;

           CarRigidBody.AddRelativeForce(_distanceRatio * Vector3.forward * ((powerInput > 0) ? powerInput * Speed * 4 : powerInput * BackwardsSpeed * 4));

           CarRigidBody.AddRelativeTorque(0f, _distanceRatio * turnInput * TurnSpeed, 0f);

           CarRigidBody.drag = _distanceRatio * MaxDrag;
       }
       else
       {
          CarRigidBody.drag = 0.01f;
           _distanceRatio = 1f;
       }

       foreach (Transform thruster in Thrusters)
       {
           if (Physics.Raycast(thruster.position, -thruster.up, out hit, HoverHeight, TrackMask))
           {
               CarRigidBody.AddForceAtPosition(Vector3.up * HoverForce * Mathf.Pow((1.0f - (hit.distance / HoverHeight)), 2), thruster.position);
           } else
           {
               CarRigidBody.AddForceAtPosition(Vector3.down * HoverForce * _amountOverThrustors * TrackSuction, thruster.position);
           }
       }

       if (!Physics.Raycast(Centre.position, -Centre.up, out hit, 2000, TrackMask))
       {
           if (!_setToSpawn)
           {
               _setToSpawn = true;
               // TODO:: implement a static metronome
               Metronome.addSpawner(id);
           }
       }
    }

    private void Spawn()
    {
        //Debug.Log("Spawn called for: " + id);
        RaycastHit hit;
        if (!Physics.Raycast(Centre.position, -Centre.up, out hit, 20, TrackMask))
        {
            CarRigidBody.velocity = Vector3.zero;
            _waypointFollow.getSpawnPoint(CarRigidBody.transform);
        }
        _setToSpawn = false;
    }

    // Gizmo's
    void OnDrawGizmos()
    {
        RaycastHit hit;

        foreach (Transform thruster in Thrusters)
        {
            if (Physics.Raycast(thruster.position, thruster.up * -1, out hit, HoverHeight, TrackMask))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(thruster.position, hit.point);
                Gizmos.DrawSphere(hit.point, 0.5f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(thruster.position, thruster.position - Vector3.up * HoverHeight);
            }

            if (Physics.Raycast(thruster.position, Centre.up, out hit, HoverHeight, TrackMask))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(thruster.position, hit.point);
                Gizmos.DrawSphere(hit.point, 0.25f);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(thruster.position, thruster.position - Centre.up * HoverHeight);
            }
        }
    }
}
