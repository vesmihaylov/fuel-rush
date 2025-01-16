public interface IVehicle
{
    void ApplyThrottle(float throttleInput);
    void ApplySteering(float steeringInput);
    void ApplyBrakes(float brakeInput);
    void ApplyHandbrake(bool isEngaged);
    void ToggleEngine(bool isEnabled);
    void SetInputs(float vertical, float horizontal, bool handbrake);
}