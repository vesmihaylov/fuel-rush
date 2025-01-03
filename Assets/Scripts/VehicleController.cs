using UnityEngine;

public class VehicleController : MonoBehaviour
{
    public Rigidbody rb;
    public WheelCollider flw, frw, rlw, rrw;
    public float driveSpeed, steerSpeed, brakeTorque;
    public float driftFactor = 0.5f; // Lower values = more sliding
    public float forwardBoost = 400f; // Force applied to maintain forward motion during drift
    public float unstuckForce = 5000f; // Force applied to "unstick" the car
    public float unstuckUpwardForce = 5000f; // Additional upward force to lift the car if stuck
    public float stuckThreshold = 0.5f; // Speed threshold to determine if the car is stuck
    public float unstuckHoldTime = 1f; // Time in seconds to hold T before triggering unstuck action
    float horizontalInput, verticalInput;
    private bool isHandbrakeEngaged;
    private WheelFrictionCurve originalFrictionRL, originalFrictionRR;
    private float unstuckTimer = 0f; // Timer for holding 'T'
    private bool isUnstuckButtonHeld = false;
    private Vector3 previousPosition;
    private float stuckTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("The object does not have a Rigidbody component!");
        }

        originalFrictionRL = rlw.sidewaysFriction;
        originalFrictionRR = rrw.sidewaysFriction;
        previousPosition = transform.position;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isHandbrakeEngaged = Input.GetKey(KeyCode.Space);

        // Detect "T" key being held down for unstuck
        if (Input.GetKey(KeyCode.T))
        {
            unstuckTimer += Time.deltaTime;
            isUnstuckButtonHeld = true;
        }
        else
        {
            unstuckTimer = 0f;
            isUnstuckButtonHeld = false;
        }

        // Check if car is stuck (not moving for a while)
        if (Vector3.Distance(previousPosition, transform.position) < stuckThreshold)
        {
            stuckTime += Time.deltaTime;
        }
        else
        {
            stuckTime = 0f;
        }

        previousPosition = transform.position;
    }

    void FixedUpdate()
    {
        // Drive motor
        float motor = verticalInput * driveSpeed;
        flw.motorTorque = motor;
        frw.motorTorque = motor;

        if (!isHandbrakeEngaged)
        {
            // Normal driving
            rlw.motorTorque = motor;
            rrw.motorTorque = motor;
            rlw.brakeTorque = 0;
            rrw.brakeTorque = 0;

            // Restore original friction
            ResetFriction(rlw, originalFrictionRL);
            ResetFriction(rrw, originalFrictionRR);
        }
        else
        {
            // Handbrake logic
            rlw.brakeTorque = brakeTorque * 0.1f; // Apply minimal brake torque
            rrw.brakeTorque = brakeTorque * 0.1f;

            // Reduce lateral friction for drifting
            AdjustFriction(rlw, driftFactor);
            AdjustFriction(rrw, driftFactor);

            // Apply forward force to maintain speed during drift
            Vector3 forwardForce = transform.forward * forwardBoost * verticalInput;
            rb.AddForce(forwardForce, ForceMode.Force);

            // Disable rear motor torque for realistic drift
            rlw.motorTorque = 0;
            rrw.motorTorque = 0;
        }

        // Steering
        flw.steerAngle = steerSpeed * horizontalInput;
        frw.steerAngle = steerSpeed * horizontalInput;

        // Check if car is stuck and T is held for 2 seconds
        if (stuckTime >= 1f && unstuckTimer >= unstuckHoldTime && isUnstuckButtonHeld)
        {
            UnstuckVehicle();
        }
    }

    private void AdjustFriction(WheelCollider wheel, float factor)
    {
        WheelFrictionCurve friction = wheel.sidewaysFriction;
        friction.stiffness *= factor;
        wheel.sidewaysFriction = friction;
    }

    private void ResetFriction(WheelCollider wheel, WheelFrictionCurve originalFriction)
    {
        wheel.sidewaysFriction = originalFriction;
    }

    private void UnstuckVehicle()
    {
        // Apply a force to push the car back onto the track
        Vector3 unstuckDirection = transform.forward * unstuckForce + transform.up * unstuckUpwardForce;

        rb.AddForce(unstuckDirection, ForceMode.Impulse);

        // Optionally, reset other things like stuck timer after unstucking
        stuckTime = 0f;
        unstuckTimer = 0f;
        Debug.Log("Car is unstuck!");
    }

    public void StopVehicle()
    {
        flw.motorTorque = 0;
        frw.motorTorque = 0;
        rlw.motorTorque = 0;
        rrw.motorTorque = 0;
        flw.brakeTorque = 1;
        frw.brakeTorque = 1;
        rlw.brakeTorque = 1;
        rrw.brakeTorque = 1;
        this.enabled = false;
    }
}
