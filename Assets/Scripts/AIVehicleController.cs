using System.Collections.Generic;
using UnityEngine;

public class AIVehicleController : MonoBehaviour, IVehicle
{
    public WaypointManager waypointManager;
    public List<Transform> waypoints;
    public int currentWaypoint;
    public float waypointRange;
    public bool isInsideBraking;
    public float driveSpeed, steerSpeed, brakeTorque;

    public Rigidbody rb;
    public WheelCollider flw, frw, rlw, rrw;
    private float steerInput, throttleInput;
    private bool isHandbrakeEngaged;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        waypoints = waypointManager.waypoints;
        currentWaypoint = 0;
    }

    private void FixedUpdate()
    {
        NavigateTowardsWaypoint();
        ApplyThrottle(throttleInput);
        ApplySteering(steerInput);
        ApplyBrakes(throttleInput);
        ApplyHandbrake(isHandbrakeEngaged);
        Debug.DrawRay(transform.position, waypoints[currentWaypoint].position - transform.position, Color.yellow);
    }

    private void NavigateTowardsWaypoint()
    {
        if (waypoints.Count == 0) return;

        Vector3 targetDirection = waypoints[currentWaypoint].position - transform.position;
        float distanceToWaypoint = targetDirection.magnitude;
        float steeringAngle = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);

        steerInput = Mathf.Clamp(steeringAngle / steerSpeed, -1f, 1f);
        throttleInput = distanceToWaypoint > waypointRange ? 1f : 0f;

        if (isInsideBraking)
        {
            throttleInput = -0.5f;
        }

        if (distanceToWaypoint < waypointRange)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Count;
        }
    }

    public void ApplyThrottle(float throttleInput)
    {
        float motorTorque = throttleInput * driveSpeed;
        flw.motorTorque = frw.motorTorque = rlw.motorTorque = rrw.motorTorque = motorTorque;
    }

    public void ApplySteering(float steeringInput)
    {
        flw.steerAngle = frw.steerAngle = steerSpeed * steeringInput;
    }

    public void ApplyBrakes(float throttleInput)
    {
        float forwardVelocity = Vector3.Dot(rb.linearVelocity, transform.forward);
        float brakingForce = (throttleInput == 0 || Mathf.Sign(throttleInput) != Mathf.Sign(forwardVelocity))
            ? brakeTorque
            : 0;
        flw.brakeTorque = frw.brakeTorque = rlw.brakeTorque = rrw.brakeTorque = brakingForce;
    }

    public void ApplyHandbrake(bool isEngaged)
    {
        isHandbrakeEngaged = isEngaged;
        if (isEngaged)
        {
            rlw.brakeTorque = rrw.brakeTorque = brakeTorque * 0.5f;
        }
    }

    public void ToggleEngine(bool isEnabled)
    {
        float motor = isEnabled ? driveSpeed : 0;
        float brake = isEnabled ? 0 : 1;
        ApplyThrottle(motor);
        ApplyBrakes(brake);
        enabled = isEnabled;
    }

    public void SetInputs(float throttle, float steering, bool handbrake)
    {
        throttleInput = throttle;
        steerInput = steering;
        isHandbrakeEngaged = handbrake;
    }
}