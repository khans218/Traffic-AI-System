using UnityEngine;
using System.Collections;

public class SortPaths : MonoBehaviour
{
    void OnDrawGizmos()
    {
        int pathId = 1;
        foreach (Transform path in transform)
        {
            if (path.name != "Path-" + pathId.ToString())
                path.name = "Path-" + pathId.ToString();

            pathId++;
        }
    }
}
