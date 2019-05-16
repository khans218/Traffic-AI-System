using UnityEngine;
using System.Collections;

public class SortWays : MonoBehaviour
{
    void OnDrawGizmos()
    {
        int wayId = 1;
        foreach (Transform way in transform)
        {
            if (way.name != "Way-" + wayId.ToString())
                way.name = "Way-" + wayId.ToString();

            wayId++;
        }
    }
}
