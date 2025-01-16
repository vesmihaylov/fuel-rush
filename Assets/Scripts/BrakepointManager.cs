using UnityEngine;

public class BrakepointManager : MonoBehaviour
{
    [SerializeField] private string aiVehicleTag = "AIVehicle";

    private void OnTriggerEnter(Collider other)
    {
        AIVehicleController vehicle = GetAIVehicleController(other);
        if (vehicle != null)
        {
            vehicle.isInsideBraking = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        AIVehicleController vehicle = GetAIVehicleController(other);
        if (vehicle != null)
        {
            vehicle.isInsideBraking = false;
        }
    }

    private AIVehicleController GetAIVehicleController(Collider collider)
    {
        if (!collider.gameObject.transform.parent.gameObject.CompareTag(aiVehicleTag)) return null;

        return collider.GetComponentInParent<AIVehicleController>();
    }
}