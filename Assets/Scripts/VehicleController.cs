using UnityEngine;

public class VehicleController : MonoBehaviour
{
    public Rigidbody rb;
    public WheelCollider flw, frw, rlw, rrw;
    public float driveSpeed, steerSpeed, brakeTorque;
    public float driftFactor = 0.5f; // Lower values = more sliding
    public float forwardBoost = 400f; // Force applied to maintain forward motion during drift
    float horizontalInput, verticalInput;
    private bool isHandbrakeEngaged;
    private WheelFrictionCurve originalFrictionRL, originalFrictionRR;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("The object does not have a Rigidbody component!");
        }

        originalFrictionRL = rlw.sidewaysFriction;
        originalFrictionRR = rrw.sidewaysFriction;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isHandbrakeEngaged = Input.GetKey(KeyCode.Space);
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

    public void ToggleEngine(bool isEnabled)
    {
        float motor = isEnabled ? verticalInput * driveSpeed : 0;
        float brake = isEnabled ? 0 : 1;
        ApplyTorqueAndBrakeToWheels(motor, brake);
        enabled = isEnabled;
    }

    private void ApplyTorqueAndBrakeToWheels(float motor, float brake)
    {
        WheelCollider[] wheels = { flw, frw, rlw, rrw };
        foreach (var wheel in wheels)
        {
            wheel.motorTorque = motor;
            wheel.brakeTorque = brake;
        }
    }
}