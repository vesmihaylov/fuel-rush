using UnityEngine;

public class WheelRotation : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public Transform wheelMesh;
    public bool wheelTurn;

    void Update()
    {
        if (wheelTurn)
        {
            wheelMesh.localEulerAngles = new Vector3(wheelMesh.localEulerAngles.x, wheelCollider.steerAngle - wheelMesh.localEulerAngles.z, wheelMesh.localEulerAngles.z);
        }
        wheelMesh.Rotate(wheelCollider.rpm / 60 * 360 * Time.deltaTime, 0, 0);
    }
}
