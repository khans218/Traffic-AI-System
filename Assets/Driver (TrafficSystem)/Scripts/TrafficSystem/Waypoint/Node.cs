using UnityEngine;
using System.Collections;

public enum TrafficMode {Go=1,Wait=2,Stop=3}; //Traffic light (Green,Yellow,Red)
public class Node : MonoBehaviour
{

    public Transform previousNode;
    public Transform nextNode;

    public float widthDistance = 5.0f; // width distance (Street)

    public Color nodeColor = Color.green;

    [HideInInspector]
    public TrafficMode trafficMode = TrafficMode.Go; // Traffic Control (tarffic light)
    [HideInInspector]
    public string nodeState; // The status of each node (previous or next node)
    [HideInInspector]
    public string mode = "OneWay"; // The mode of each node (OneWay or TwoWay)
    [HideInInspector]
    public string parentPath; // Parent path of nodes
    [HideInInspector]
    public bool firistNode, lastNode = false;
    [HideInInspector]
    public bool trafficNode = false; // When traffic control active (tarffic light)

    void OnDrawGizmos()
    {

        if (trafficNode)
        {
            switch (trafficMode)
            {
                case TrafficMode.Go:
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(transform.position, 2);
                    break;
                case TrafficMode.Wait:
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(transform.position, 2);
                    break;
                case TrafficMode.Stop:
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(transform.position, 2);
                    break;
            }
        }

        Gizmos.color = nodeColor;

        Vector3 direction = transform.TransformDirection(Vector3.left);

        Gizmos.DrawRay(transform.position, direction * widthDistance);
        Gizmos.DrawRay(transform.position, direction * -widthDistance);
        Gizmos.DrawSphere(transform.position, 1);

        if (nextNode)
        {
            Vector3 directionLookAt = transform.position - nextNode.position;
            directionLookAt.y = 0;
            transform.rotation = Quaternion.LookRotation(directionLookAt);
        }
    }

    void Awake()
    {
        if (!previousNode)
            Debug.LogError("previousNode is missing on : " + parentPath + " Node " + this.name);

        if (nextNode)
            if (nextNode.GetComponent<WaysControl>()) nodeState = "NextPoint"; else nodeState = "PreviousPoint";
        else
            Debug.LogError("NextNode is missing on : " + parentPath + " Node " + this.name);

    }

}
