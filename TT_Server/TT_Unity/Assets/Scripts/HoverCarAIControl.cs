using UnityEngine;

public class HoverCarAIControl : MonoBehaviour
{
    [SerializeField] [Range(-1, 1)] private float _m_steerAmount = 0.5f;
    [SerializeField] [Range(0, 1)] private float _m_CautiousSpeedFactor = 0.05f;               // percentage of max speed to use when being maximally cautious
    [SerializeField] [Range(0, 180)] private float _m_CautiousMaxAngle = 50f;                  // angle of approaching corner to treat as warranting maximum caution

    [SerializeField] private float _m_CautiousMaxDistance = 100f;                              // distance at which distance-based cautiousness begins
    [SerializeField] private float _m_CautiousAngularVelocityFactor = 30f;                     // how cautious the AI should be when considering its own current angular velocity (i.e. easing off acceleration if spinning!)
    [SerializeField] private float _m_SteerSensitivity = 0.05f;                                // how sensitively the AI uses steering input to turn to the desired direction
    [SerializeField] private float _m_AccelSensitivity = 0.04f;                                // How sensitively the AI uses the accelerator to reach the current desired speed
    [SerializeField] private float _m_BrakeSensitivity = 1f;                                   // How sensitively the AI uses the brake to reach the current desired speed
    [SerializeField] private float _m_LateralWanderDistance = 3f;                              // how far the car will wander laterally towards its target
    [SerializeField] private float _m_LateralWanderSpeed = 0.1f;                               // how fast the lateral wandering will fluctuate

    [SerializeField] [Range(0, 1)] private float _m_AccelWanderAmount = 0.1f;                  // how much the cars acceleration will wander

    [SerializeField] private float _m_AccelWanderSpeed = 0.1f;                                 // how fast the cars acceleration wandering will fluctuate
    [SerializeField] private bool _m_Driving;                                                  // whether the AI is currently actively driving or stopped.

    [SerializeField] private Transform _m_Target;                                              // 'target' the target object to aim for.

    [SerializeField] private bool _m_StopWhenTargetReached;                                    // should we stop driving when we reach the target?
    [SerializeField] private float _m_ReachTargetThreshold = 2;                                // proximity to target to consider we 'reached' it, and stop driving.

    private float _m_RandomPerlin;             // A random value for the car to base its wander on (so that AI cars don't all wander in the same pattern)

    private HoverMotor _hoverMotor;    // Reference to actual car controller we are controlling

    private float _m_AvoidOtherCarTime;        // time until which to avoid the car we recently collided with
    private float _m_AvoidOtherCarSlowdown;    // how much to slow down due to colliding with another car, whilst avoiding
    private float _m_AvoidPathOffset;          // direction (-1 or 1) in which to offset path to avoid other car, whilst avoiding

    private Rigidbody _m_Rigidbody;

    // Use this for initialization

    void Start()
    {
        // get the car controller reference

        _hoverMotor = GetComponent<HoverMotor>();

        // give the random perlin a random value

        _m_RandomPerlin = Random.value * 100;

        _m_Rigidbody = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame

	void FixedUpdate()
    {
        if (_m_Target == null || !_m_Driving)
        {
            return;
        }

        Vector3 fwd = transform.forward;


        float desiredSpeed = _hoverMotor.MaxSpeed;


        // check out the angle of our target compared to the current direction of the car

        float approachingCornerAngle = Vector3.Angle(_m_Target.forward, fwd);

        // also consider the current amount we're turning, multiplied up and then compared in the same way as an upcoming corner angle

        float spinningAngle = _m_Rigidbody.angularVelocity.magnitude * _m_CautiousAngularVelocityFactor;

        // if it's different to our current angle, we need to be cautious (i.e. slow down) a certain amount

        float cautiousnessRequired = Mathf.InverseLerp(0, _m_CautiousMaxAngle,
                                                       Mathf.Max(spinningAngle,
                                                                 approachingCornerAngle));
        desiredSpeed = Mathf.Lerp(_hoverMotor.MaxSpeed, _hoverMotor.MaxSpeed * _m_CautiousSpeedFactor,
                                  cautiousnessRequired);

        // Evasive action due to collision with other cars:

        // our target position starts off as the 'real' target position

        Vector3 offsetTargetPos = _m_Target.position;

        // if are we currently taking evasive action to prevent being stuck against another car:

        if (Time.time < _m_AvoidOtherCarTime)
        {
            // slow down if necessary (if we were behind the other car when collision occured)

            desiredSpeed *= _m_AvoidOtherCarSlowdown;

            // and veer towards the side of our path-to-target that is away from the other car

            offsetTargetPos += _m_Target.right * _m_AvoidPathOffset;
        }

        else
        {
            // no need for evasive action, we can just wander across the path-to-target in a random way,
            // which can help prevent AI from seeming too uniform and robotic in their driving

            offsetTargetPos += _m_Target.right *
                               (Mathf.PerlinNoise(Time.time * _m_LateralWanderSpeed, _m_RandomPerlin) * 2 - 1) *
                               _m_LateralWanderDistance;
        }

        // use different sensitivity depending on whether accelerating or braking:

        float accelBrakeSensitivity = (desiredSpeed < _hoverMotor.CurrentSpeed)
                                          ? _m_BrakeSensitivity
                                          : _m_AccelSensitivity;

        // decide the actual amount of accel/brake input to achieve desired speed.

        float accel = Mathf.Clamp((desiredSpeed - _hoverMotor.CurrentSpeed) * accelBrakeSensitivity, -1, 1);

        // add acceleration 'wander', which also prevents AI from seeming too uniform and robotic in their driving
        // i.e. increasing the accel wander amount can introduce jostling and bumps between AI cars in a race

        accel *= (1 - _m_AccelWanderAmount) +
                 (Mathf.PerlinNoise(Time.time * _m_AccelWanderSpeed, _m_RandomPerlin) * _m_AccelWanderAmount);

        // calculate the local-relative position of the target, to steer towards

        Vector3 localTarget = transform.InverseTransformPoint(offsetTargetPos);

        // work out the local angle towards the target

        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        // get the amount of steering needed to aim the car towards the target

        float steer = Mathf.Clamp(targetAngle * _m_SteerSensitivity, -1, 1) * Mathf.Sign(_hoverMotor.CurrentSpeed);

        // feed input to the car controller.

        _hoverMotor.Move(-accel, steer);
    }

    private void OnCollisionStay(Collision col)
    {
        // TODO :: our AI is still too STUPPPIIDDD :(
    }

    public void SetTarget(Transform target)
    {
        _m_Target = target;
        _m_Driving = true;
    }
}