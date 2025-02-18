using UnityEngine;
using System.Collections.Generic;

public class AIVehicleProperties : MonoBehaviour
{
    public string aiName;
    public Material[] materials;
    private static List<Material> availableMaterials = new List<Material>();
    private static HashSet<string> usedNames = new HashSet<string>();

    private static readonly List<string> names = new List<string>()
    {
        "Alex", "Jordan", "Riley", "Morgan", "Taylor", "Casey", "Skyler", "Drew", "Jamie", "Cameron",
        "Speedy McZoom", "Nitro Noodle", "Turbo Tornado", "Crash Test Dummy", "Slipstream Sam", "Vroom Vroom Von",
        "Sir Skidsalot", "Screech McSwerve", "Drift Kingpin", "Burnout Barry", "Fast & the Fluffy",
        "Pedal to the Medal", "The Revenger", "Skidmark Steve", "Blaze McRaceface", "Spanner Man", "Wheels McGrill",
        "Fender Bender", "Lap King Larry", "Turbo Tuna", "Dom T.", "Ryan OC", "Transporter", "Razor from MW",
        "BMW lover", "Japan enjoyer", "GodZILLA", "DK"
    };

    private void Awake()
    {
        AssignUniqueMaterial();
        AssignUniqueName();
    }

    private void AssignUniqueMaterial()
    {
        if (availableMaterials.Count == 0)
        {
            availableMaterials = new List<Material>(materials);
        }

        if (availableMaterials.Count > 0)
        {
            int randomIndex = Random.Range(0, availableMaterials.Count);
            Material selectedMaterial = availableMaterials[randomIndex];
            availableMaterials.RemoveAt(randomIndex);

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (!renderer.gameObject.CompareTag("VehicleBody")) continue;
                renderer.material = selectedMaterial;
            }
        }
    }

    private void AssignUniqueName()
    {
        List<string> availableNames = new List<string>(names);
        availableNames.RemoveAll(name => usedNames.Contains(name));
        int randomIndex = Random.Range(0, availableNames.Count);
        aiName = availableNames[randomIndex];
        usedNames.Add(aiName);
    }
}