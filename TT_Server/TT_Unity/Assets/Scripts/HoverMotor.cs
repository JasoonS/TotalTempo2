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
    public float FloatHeight = 12.0f;
    public float Speed = 200.0f;
    public float BackwardsSpeed = 100.0f;
    public float TurnSpeed = 80.0f;
    public float MaxDrag = 4.0f;
    public float TrackSuction = 4.0f;
    public float JumpThrust = 5000.0f;

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

    //TODO:: set this in a better way, auto assign an _id?
    public string _id;

    public bool isPlayer;
    public bool isAI;

    private float powerInput = 0.0f;
    private float turnInput = 0.0f;

    private float _distanceRatio;
    private float _amountOverThrustors;
    private float _aboveThrusterDistance;

    // the 4 states possible for the vehicle.
    public enum VehicleStates { NONE, RIGHT, LEFT, SAME, DIFF };
    /*
     * NONE is neutral.
     * RIGHT is for speed.
     * LEFT is for TURN.
     * SAME is Both Speed and Turn boost.
     * DIFF is decrease in both.
     */

    public VehicleStates VehicleState = VehicleStates.NONE;
    public PlayerState[] State;

    void Start()
    {
        spawnListener = new UnityAction(Spawn);

        CarRigidBody = GetComponent<Rigidbody>();

        _waypointFollow = GetComponent<FollowWayPoint>();
        _waypointFollow.setID(_id);

        TrackMask = 1 << LayerMask.NameToLayer("Track");

        _setToSpawn = false;
        //TrackMask = ~TrackMask;
        EventManager.StartListening(_id, spawnListener);

        State = new PlayerState[5];
        State[(int)VehicleStates.NONE] = new PlayerState{};
        State[(int)VehicleStates.LEFT] = new PlayerState{ Speed = 300.0f };
        State[(int)VehicleStates.RIGHT] = new PlayerState{ TurnSpeed = 100.0f };
        State[(int)VehicleStates.SAME] = new PlayerState{ Speed = 400.0f, TurnSpeed = 200.0f};
        State[(int)VehicleStates.DIFF] = new PlayerState{ Speed = 100.0f, HoverForce = 50.0f };

        _aboveThrusterDistance = FloatHeight - State[(int)VehicleState].HoverHeight;
    }

    internal void SetState(VehicleStates state)
    {
        if (!isAI)
            VehicleState = state;
    }

    void OnDisable()
    {
        EventManager.StopListening(_id, spawnListener);
    }

    internal void Move(float powerInputIn, float turnInputIn)
    {
        powerInput = powerInputIn;
        turnInput = turnInputIn;
    }

    internal void Jump()
    {
        RaycastHit hit;

        if (Physics.Raycast(Centre.position, Vector3.down, out hit, State[(int)VehicleState].HoverHeight, TrackMask))
        {
            CarRigidBody.AddForce(Vector3.up * JumpThrust);
        }
    }

    void FixedUpdate()
    {
        RaycastHit hit;

      //  float ratioSqrd;

      //Debug.Log("state::" + (int)VehicleState);
      //Debug.Log("HoverHeight::" + State[(int)VehicleState].HoverHeight);


       // move vehicle with force relative to track.
       if (Physics.Raycast(Centre.position, -Centre.up, out hit, FloatHeight, TrackMask))
       {
           //float floatRatio = Mathf.Pow((1.0f - (hit.distance / FloatHeight)), 2);
           _distanceRatio = (1.0f - (hit.distance / FloatHeight));
           if (hit.distance > State[(int)VehicleState].HoverHeight)
           {
               _amountOverThrustors = (hit.distance - State[(int)VehicleState].HoverHeight) / _aboveThrusterDistance;
           }

          //  ratioSqrd = _distanceRatio * _distanceRatio;

           CarRigidBody.AddRelativeForce(_distanceRatio * Vector3.forward * ((powerInput > 0) ? powerInput * State[(int)VehicleState].Speed * 4 : powerInput * BackwardsSpeed * 4));

           CarRigidBody.AddRelativeTorque(0f, _distanceRatio * turnInput * State[(int)VehicleState].TurnSpeed, 0f);

           CarRigidBody.drag = _distanceRatio * MaxDrag;
       }
       else
       {
          CarRigidBody.drag = 0.01f;
           _distanceRatio = 1f;
       }

       foreach (Transform thruster in Thrusters)
       {
           if (Physics.Raycast(thruster.position, -thruster.up, out hit, State[(int)VehicleState].HoverHeight, TrackMask))
           {
               CarRigidBody.AddForceAtPosition(Vector3.up * State[(int)VehicleState].HoverForce * Mathf.Pow((1.0f - (hit.distance / State[(int)VehicleState].HoverHeight)), 2), thruster.position);
           } else
           {
               CarRigidBody.AddForceAtPosition(Vector3.down * State[(int)VehicleState].HoverForce * _amountOverThrustors * TrackSuction, thruster.position);
           }
       }

       if (!Physics.Raycast(Centre.position, -Centre.up, out hit, 2000, TrackMask))
       {
           if (!_setToSpawn)
           {
               _setToSpawn = true;
               // TODO:: implement a static metronome
               Metronome.addSpawner(_id);
           }
       }
    }

    private void Spawn()
    {
        //Debug.Log("Spawn called for: " + _id);
        RaycastHit hit;
        if (!Physics.Raycast(Centre.position, -Centre.up, out hit, 20, TrackMask))
        {
            CarRigidBody.velocity = Vector3.zero;
            _waypointFollow.getSpawnPoint(CarRigidBody.transform);
        }
        _setToSpawn = false;
    }

    // TODO:: move to a separate file...
    public class PlayerState
    {
      float _HoverForce = 100.0f;
      float _HoverHeight = 4.0f;
      float _FloatHeight = 12.0f;
      float _Speed = 200.0f;
      float _BackwardsSpeed = 100.0f;
      float _TurnSpeed = 80.0f;
      float _MaxDrag = 4.0f;
      float _TrackSuction = 4.0f;
      float _JumpThrust = 5000.0f;

        public float HoverForce { get {return _HoverForce;} set { _HoverForce = value;} }
        public float HoverHeight { get {return _HoverHeight;} set { _HoverHeight = value;} }
        public float FloatHeight { get {return _FloatHeight;} set { _FloatHeight = value;} }
        public float Speed { get {return _Speed;} set { _Speed = value;} }
        public float BackwardsSpeed { get {return _BackwardsSpeed;} set { _BackwardsSpeed = value;} }
        public float TurnSpeed { get {return _TurnSpeed;} set { _TurnSpeed = value;} }
        public float MaxDrag { get {return _MaxDrag;} set { _MaxDrag = value;} }
        public float TrackSuction { get {return _TrackSuction;} set { _TrackSuction = value;} }
        public float JumpThrust { get {return _JumpThrust;} set { _JumpThrust = value;} }
        // PlayerState(float HoverForce,
        //             float HoverHeight,
        //             float FloatHeight,
        //             float Speed,
        //             float BackwardsSpeed,
        //             float TurnSpe,
        //             float MaxDr,
        //             float TrackSucti,
        //             float JumpThrust)
        // {
        // }
    }

    // Gizmo's
    void OnDrawGizmos()
    {
        RaycastHit hit;

        foreach (Transform thruster in Thrusters)
        {
            if (Physics.Raycast(thruster.position, thruster.up * -1, out hit, State[(int)VehicleState].HoverHeight, TrackMask))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(thruster.position, hit.point);
                Gizmos.DrawSphere(hit.point, 0.5f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(thruster.position, thruster.position - Vector3.up * State[(int)VehicleState].HoverHeight);
            }

            if (Physics.Raycast(thruster.position, Centre.up, out hit, State[(int)VehicleState].HoverHeight, TrackMask))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(thruster.position, hit.point);
                Gizmos.DrawSphere(hit.point, 0.25f);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(thruster.position, thruster.position - Centre.up * State[(int)VehicleState].HoverHeight);
            }
        }
    }
}
