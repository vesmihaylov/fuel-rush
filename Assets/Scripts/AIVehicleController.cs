using System.Collections;
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

    private float stuckTimer = 0f;
    private float stuckTimeThreshold = 2f;
    private float minVelocityThreshold = 1f;
    private bool isRecovering = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        waypoints = waypointManager.waypoints;
        currentWaypoint = 0;
    }

    private void FixedUpdate()
    {
        if (!isRecovering)
        {
            FollowWaypoints();
            ApplyThrottle(throttleInput);
            ApplySteering(steerInput);
            ApplyBrakes(throttleInput);
            ApplyHandbrake(isHandbrakeEngaged);
        }

        CheckIfStuck();
    }

    public void ApplyThrottle(float throttleInput)
    {
        // If the AI is recovering, ignore braking zones
        if (isInsideBraking && !isRecovering)
        {
            throttleInput = Mathf.Max(throttleInput, 0.5f);
        }

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
        float dynamicBrakeTorque = (rb.linearVelocity.magnitude > 10f) ? brakeTorque * 2f : brakeTorque;
        float brakingForce = (throttleInput == 0 || Mathf.Sign(throttleInput) != Mathf.Sign(forwardVelocity))
            ? dynamicBrakeTorque
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

    private void FollowWaypoints()
    {
        if (waypoints.Count == 0) return;

        Vector3 targetDirection = waypoints[currentWaypoint].position - transform.position;
        float distanceToWaypoint = targetDirection.magnitude;
        float steeringAngle = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);

        steerInput = Mathf.Clamp(steeringAngle / steerSpeed, -1f, 1f);
        throttleInput = distanceToWaypoint > waypointRange ? 1f : 0f;

        if (isInsideBraking)
        {
            // Apply brakes based on the AI drive speed
            float speed = rb.linearVelocity.magnitude;
            if (speed > 10f)
            {
                throttleInput = -1f;
            }
            else if (speed > 5f)
            {
                throttleInput = -0.5f;
            }
            else
            {
                throttleInput = 0f;
            }

            // If stopped inside braking zone, force movement
            if (speed < 0.5f)
            {
                throttleInput = 1f;
            }
        }

        if (distanceToWaypoint < waypointRange)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Count;
        }
    }

    private void NavigateTowardClosestCheckpoint()
    {
        float closestDistance = float.MaxValue;
        int closestIndex = currentWaypoint;

        for (int i = 0; i < waypoints.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, waypoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        currentWaypoint = closestIndex;
        Vector3 targetDirection = waypoints[currentWaypoint].position - transform.position;
        float targetAngle = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);

        ApplyThrottle(driveSpeed);
        steerInput = Mathf.Lerp(steerInput, Mathf.Clamp(targetAngle / 45f, -1f, 1f), 0.1f);
    }

    private void CheckIfStuck()
    {
        if (throttleInput > 0 && rb.linearVelocity.magnitude < minVelocityThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckTimeThreshold && !isRecovering)
            {
                StartCoroutine(RecoverFromStuck());
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    private IEnumerator RecoverFromStuck()
    {
        isRecovering = true;
        Vector3 targetDirection = waypoints[currentWaypoint].position - transform.position;
        float targetAngle = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);
        float reverseSteerPosition = -Mathf.Clamp(targetAngle / 45f, -1f, 1f);
        float reverseDuration = 1.0f;
        float reverseStartTime = Time.time;
        while (Time.time - reverseStartTime < reverseDuration)
        {
            ApplyThrottle(-1.0f);
            ApplySteering(reverseSteerPosition);
            yield return null;
        }

        ApplyThrottle(0);
        ApplyBrakes(0);
        yield return new WaitForSeconds(0.5f);

        NavigateTowardClosestCheckpoint();
        yield return new WaitForSeconds(0.5f);

        isRecovering = false;
        stuckTimer = 0f;
    }
}