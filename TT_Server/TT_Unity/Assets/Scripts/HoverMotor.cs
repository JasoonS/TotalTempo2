using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody))]
public class HoverMotor : MonoBehaviour
{
    //TODO:: use a layer mask to increase efficiency, and prevent accidental hits.
    //TODO:: experiment with using Vector3.down vs (thruster.up * 1)

    public float HoverForce = 10000.0f;
    public float HoverHeight = 3.0f;
    public float AirTimeGravity = 100.0f;
    public float Speed = 20000.0f;
    public float BackwardsSpeed = 10000.0f;
    public float TurnSpeed = 8000f;
    public float JumpThrust = 500000;

    public Transform[] Thrusters;

    public float MaxSpeed = 36.80006f;

    //public float CurrentSteerAngle { get { return m_SteerAngle; } }

    public float CurrentSpeed { get { return CarRigidBody.velocity.magnitude * 2.23693629f; } }

    public Transform Centre;
    public Transform Body;

    Rigidbody CarRigidBody;

    private int BodyLayerMask;

    float maxSpeed;

    private FollowWayPoint _waypointFollow;

    private UnityAction spawnListener;

    public bool _setToSpawn;

    //TODO:: set this in a better way, auto assign an id?
    public string id;

    public bool isPlayer;

    void Start()
    {
        spawnListener = new UnityAction(Spawn);

        CarRigidBody = GetComponent<Rigidbody>();

        _waypointFollow = GetComponent<FollowWayPoint>();

        //BodyLayerMask = 1 << LayerMask.NameToLayer("CarBody");
        BodyLayerMask = 1 << LayerMask.NameToLayer("Track");

        _setToSpawn = false;
        //BodyLayerMask = ~BodyLayerMask;
        EventManager.StartListening(id, spawnListener);
    }

    void OnDisable()
    {
        EventManager.StopListening(id, spawnListener);
    }

    internal void Move(float powerInput, float turnInput)
    {
        RaycastHit hit;

        // TODO:: REMOVE:: *4: the *4 is just a temporary tweak.

        if (Physics.Raycast(Centre.position, Vector3.down, out hit, HoverHeight, BodyLayerMask))
        {
            CarRigidBody.AddRelativeForce(Vector3.forward * ((powerInput > 0) ? powerInput * Speed * 4 : powerInput * BackwardsSpeed * 4));

            CarRigidBody.AddRelativeTorque(0f, turnInput * TurnSpeed, 0f);
        }
    }

    internal void Jump()
    {
        RaycastHit hit;

        if (Physics.Raycast(Centre.position, Vector3.down, out hit, HoverHeight, BodyLayerMask))
        {
            CarRigidBody.AddForce(Vector3.up * JumpThrust);
        }
    }

    void OnDrawGizmos()
    {
        RaycastHit hit;

        foreach (Transform thruster in Thrusters)
        {
            // draw green if at/below hover height, otherwise red.

            if (Physics.Raycast(thruster.position, thruster.up * -1, out hit, HoverHeight, BodyLayerMask))
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

            // draw green if at/below hover height, otherwise red.

            if (Physics.Raycast(thruster.position, Body.up, out hit, HoverHeight, BodyLayerMask))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(thruster.position, hit.point);
                Gizmos.DrawSphere(hit.point, 0.25f);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(thruster.position, thruster.position - Body.up * HoverHeight);
            }

            //eulerAngles
        }
    }

    void FixedUpdate()
    {
        RaycastHit hit;

        //Vertical thrusters

        foreach (Transform thruster in Thrusters)
        {
            // TODO :: add some sort of control for flipping upside down, or change the
            // angle at which the hover opperates, or use multiple types of raycasts etc.
            // basically any way to make the care not freak out when going around curves.

            if (Physics.Raycast(thruster.position, -thruster.up, out hit, HoverHeight, BodyLayerMask))
            {
                //NOTE:: I added the square to the scalar because it was sometimes tipping over.

                CarRigidBody.AddForceAtPosition(thruster.up * HoverForce * Mathf.Pow((1.0f - (hit.distance / HoverHeight)), 2), thruster.position);

                //TODO:: make this more efficient, no need to recalculate on each thrustor....

                CarRigidBody.drag = 4;

                //carRigidBody.mass = 500;
            }
            else
            {
                //NOTE:: this is a hack, should rather examin the gravity carefully.

                CarRigidBody.AddForceAtPosition(Vector3.down * AirTimeGravity, thruster.position);
                CarRigidBody.drag = 0;

                //carRigidBody.mass = 8000;
            }
        }

        if (!Physics.Raycast(Centre.position, -Centre.up, out hit, 20, BodyLayerMask))
        {
            if (!_setToSpawn)
            {
                _setToSpawn = true;
                // TODO:: implement a static metronome
                Metronome.addSpawner(id);
            }
        }

        //********
        //PLEASE DONT DELETE THE BELLOW SEGMENT!!!
        //********
        //IT IS USEFULL FOR DEBUGGING
        //********
        //if (maxSpeed < carRigidBody.velocity.magnitude)
        //{
        //    Debug.Log("The speed is:");
        //    maxSpeed = carRigidBody.velocity.magnitude;
        //    Debug.Log(maxSpeed);
        //}
    }

    private void Spawn()
    {
        Debug.Log("Spawn called for: " + id);
        RaycastHit hit;
        if (!Physics.Raycast(Centre.position, -Centre.up, out hit, 20, BodyLayerMask))
        {
            CarRigidBody.velocity = Vector3.zero;
            _waypointFollow.getSpawnPoint(CarRigidBody.transform);
        }
        _setToSpawn = false;
    }
}