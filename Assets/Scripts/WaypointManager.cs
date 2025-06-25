using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public List<List<Transform>> waypoints = new();

    void Awake()
    {
        var all = gameObject.GetComponentsInChildren<Transform>().Where(t => t != transform).ToList();
        all = all.OrderBy(t => ExtractBaseWaypointNumber(t.name)).ToList();

        int currentIndex = -1;
        List<Transform> group = new();

        foreach (var wp in all)
        {
            int index = ExtractBaseWaypointNumber(wp.name);
            if (index != currentIndex)
            {
                if (group.Count > 0)
                    waypoints.Add(group);

                group = new List<Transform>();
                currentIndex = index;
            }

            group.Add(wp);
        }

        if (group.Count > 0)
            waypoints.Add(group);
    }

    int ExtractBaseWaypointNumber(string name)
    {
        var digits = System.Text.RegularExpressions.Regex.Match(name, @"\d+");
        return digits.Success ? int.Parse(digits.Value) : 0;
    }
}