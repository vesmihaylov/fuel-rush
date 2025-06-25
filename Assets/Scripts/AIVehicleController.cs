using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIVehicleController : MonoBehaviour, IVehicle
{
    public WaypointManager waypointManager;
    private List<Transform> waypoints = new();
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
    private int recoveryAttempts = 0;
    private int maxRecoveryAttempts = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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

    private (float steering, float throttle) CalculateInputsToWaypoint(Vector3 waypointPosition)
    {
        Vector3 targetDirection = waypointPosition - transform.position;
        float distanceToWaypoint = targetDirection.magnitude;
        float steeringAngle = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);

        float steering = Mathf.Clamp(steeringAngle / steerSpeed, -1f, 1f);
        float throttle = distanceToWaypoint > waypointRange ? 1f : 0f;

        return (steering, throttle);
    }

    private void FollowWaypoints()
    {
        if (waypoints.Count == 0) return;

        var targetWaypoint = waypoints[currentWaypoint];
        var (steering, throttle) = CalculateInputsToWaypoint(targetWaypoint.position);

        steerInput = steering;
        throttleInput = throttle;

        ApplyInsideBraking();

        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);
        if (distanceToWaypoint < waypointRange)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Count;
        }
    }

    private int FindBestWaypointIndex()
    {
        float closestScore = float.MaxValue;
        int bestIndex = currentWaypoint;

        for (int i = 0; i < Mathf.Min(waypoints.Count, 3); i++)
        {
            int checkIndex = (currentWaypoint + i) % waypoints.Count;
            Vector3 toWaypoint = waypoints[checkIndex].position - transform.position;
            float distance = toWaypoint.magnitude;
            float forwardness = Vector3.Dot(transform.forward, toWaypoint.normalized);
            float score = distance / (forwardness > 0 ? forwardness + 0.5f : 0.2f);

            if (score < closestScore)
            {
                closestScore = score;
                bestIndex = checkIndex;
            }
        }

        return bestIndex;
    }

    private void NavigateTowardClosestWaypointAfterUnstuck()
    {
        currentWaypoint = FindBestWaypointIndex();
        var (steering, throttle) = CalculateInputsToWaypoint(waypoints[currentWaypoint].position);
        ApplyThrottle(throttle * driveSpeed);
        steerInput = Mathf.Lerp(steerInput, steering, 0.1f);
    }

    private void CheckIfStuck()
    {
        bool isMovingSlowly = rb.linearVelocity.magnitude < minVelocityThreshold;
        bool isAttemptingToMove = throttleInput > 0.1f;

        if (isAttemptingToMove && isMovingSlowly)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckTimeThreshold && !isRecovering)
            {
                if (recoveryAttempts < maxRecoveryAttempts)
                {
                    StartCoroutine(ApplyUnstuckMovement());
                }
                else
                {
                    Debug.Log("Repositioning to next waypoint.");
                    RepositionToNextWaypoint();
                }
            }
        }
        else
        {
            stuckTimer = 0f;

            if (rb.linearVelocity.magnitude > 5f)
            {
                recoveryAttempts = Mathf.Max(0, recoveryAttempts - 1);
            }
        }
    }

    private IEnumerator ApplyUnstuckMovement()
    {
        isRecovering = true;
        recoveryAttempts++;
        Debug.Log($"{gameObject.name} is attempting recovery #{recoveryAttempts}");

        float reverseSteer = -Mathf.Clamp(steerInput, -0.7f, 0.7f);
        float reverseDuration = 0f;
        float maxReverseDuration = 1.2f;

        while (reverseDuration < maxReverseDuration)
        {
            ApplyThrottle(-0.7f);
            ApplySteering(reverseSteer);
            reverseDuration += Time.deltaTime;

            if (rb.linearVelocity.magnitude > 2f && Vector3.Dot(rb.linearVelocity, -transform.forward) > 1f)
            {
                if (reverseDuration > 0.5f)
                    break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        NavigateTowardClosestWaypointAfterUnstuck();

        float recoveryTimer = 0f;
        float maxRecoveryTime = 1.5f;

        while (recoveryTimer < maxRecoveryTime)
        {
            recoveryTimer += Time.deltaTime;

            if (rb.linearVelocity.magnitude > 3f)
            {
                break;
            }

            yield return null;
        }

        if (rb.linearVelocity.magnitude < 1f)
        {
            Debug.Log($"{gameObject.name} recovery was unsuccessful");
        }
        else
        {
            Debug.Log($"{gameObject.name} recovery was successful");
            recoveryAttempts = Mathf.Max(0, recoveryAttempts - 1);
        }

        isRecovering = false;
        stuckTimer = 0f;
    }

    private void RepositionToNextWaypoint()
    {
        Debug.LogWarning($"{gameObject.name} repositioning after {recoveryAttempts} failed attempts");
        int nextWaypointIndex = (currentWaypoint + 1) % waypoints.Count;
        Vector3 waypointPosition = waypoints[nextWaypointIndex].position;

        int followingWaypointIndex = (nextWaypointIndex + 1) % waypoints.Count;
        Vector3 followingWaypointPosition = waypoints[followingWaypointIndex].position;
        Vector3 trackDirection = (followingWaypointPosition - waypointPosition).normalized;

        if (trackDirection.magnitude < 0.1f)
        {
            trackDirection = transform.forward;
        }

        Vector3 newPosition = waypointPosition + Vector3.up * 0.5f;

        transform.position = newPosition;
        transform.forward = trackDirection;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isRecovering = false;
        stuckTimer = 0f;
        recoveryAttempts = 0;

        currentWaypoint = nextWaypointIndex;
    }

    private void ApplyInsideBraking()
    {
        if (isInsideBraking)
        {
            float speed = rb.linearVelocity.magnitude;

            if (speed > 15f)
            {
                throttleInput = -0.8f;
            }
            else if (speed > 10f)
            {
                throttleInput = -0.5f;
            }
            else if (speed > 5f)
            {
                throttleInput = -0.2f;
            }
            else if (speed < 0.5f)
            {
                throttleInput = 1f;
            }
            else
            {
                throttleInput = 0f;
            }
        }
    }

    public void SetWaypoints(List<List<Transform>> allWaypointChoices)
    {
        waypoints.Clear();
        foreach (var group in allWaypointChoices)
        {
            int randomIndex = Random.Range(0, group.Count);
            waypoints.Add(group[randomIndex]);
        }

        currentWaypoint = 0;
    }
}