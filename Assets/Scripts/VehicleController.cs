using UnityEngine;

public class VehicleController : MonoBehaviour, IVehicle
{
    public Rigidbody rb;
    public WheelCollider flw, frw, rlw, rrw;
    public float driveSpeed, steerSpeed, brakeTorque;
    public float driftFactor = 0.5f; // Lower values = more sliding
    public float forwardBoost = 400f; // Force applied to maintain forward motion during drift

    private float horizontalInput, verticalInput;
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
        ApplyThrottle(verticalInput);
        ApplySteering(horizontalInput);
        ApplyBrakes(verticalInput);
        ApplyHandbrake(isHandbrakeEngaged);
    }

    public void ApplyThrottle(float throttleInput)
    {
        float motorTorque = throttleInput * driveSpeed;

        flw.motorTorque = motorTorque;
        frw.motorTorque = motorTorque;
        rlw.motorTorque = motorTorque;
        rrw.motorTorque = motorTorque;
    }

    public void ApplySteering(float steeringInput)
    {
        flw.steerAngle = steerSpeed * steeringInput;
        frw.steerAngle = steerSpeed * steeringInput;
    }

    public void ApplyBrakes(float throttleInput)
    {
        float forwardVelocity = Vector3.Dot(rb.linearVelocity, transform.forward);

        // Apply strong brakes when switching directions
        if ((throttleInput < 0 && forwardVelocity > 0.5f) || (throttleInput > 0 && forwardVelocity < -0.5f))
        {
            // Full braking when switching direction
            ApplyBrakeTorque(brakeTorque * 1.5f);
        }
        else if (throttleInput == 0)
        {
            // Apply standard braking when no input
            ApplyBrakeTorque(brakeTorque * 0.5f);
        }
        else
        {
            // No braking when moving as intended
            ApplyBrakeTorque(0);
        }
    }

    private void ApplyBrakeTorque(float brakingForce)
    {
        flw.brakeTorque = brakingForce;
        frw.brakeTorque = brakingForce;
        rlw.brakeTorque = brakingForce;
        rrw.brakeTorque = brakingForce;
    }

    public void ApplyHandbrake(bool isEngaged)
    {
        if (isEngaged)
        {
            rlw.brakeTorque = brakeTorque * 0.5f;
            rrw.brakeTorque = brakeTorque * 0.5f;

            AdjustFriction(rlw, driftFactor);
            AdjustFriction(rrw, driftFactor);

            rlw.motorTorque = 0;
            rrw.motorTorque = 0;

            Vector3 forwardForce = transform.forward * forwardBoost * verticalInput;
            rb.AddForce(forwardForce, ForceMode.Force);
        }
        else
        {
            ResetFriction(rlw, originalFrictionRL);
            ResetFriction(rrw, originalFrictionRR);
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

    public void SetInputs(float vertical, float horizontal, bool handbrake)
    {
        verticalInput = vertical;
        horizontalInput = horizontal;
        isHandbrakeEngaged = handbrake;
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