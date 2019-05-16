using UnityEngine;
using System.Collections;

public class TrafficLight : MonoBehaviour
{
    public Transform currentNode;
    public GameObject redLight, yellowLight, greenLight;

    private Node nodeComponent;
    public void Start()
    {
        nodeComponent = currentNode.GetComponent<Node>();
    }
    void Update()
    {
        if (Vector3.Distance(AIContoller.manager.player.position, currentNode.position) > 200)
        {
            greenLight.SetActive(false);
            yellowLight.SetActive(false);
            redLight.SetActive(false);
            return;
        }

        switch (nodeComponent.trafficMode)
        {
            case TrafficMode.Go:

                greenLight.SetActive(true);
                yellowLight.SetActive(false);
                redLight.SetActive(false);

                break;
            case TrafficMode.Wait:

                greenLight.SetActive(false);
                yellowLight.SetActive(true);
                redLight.SetActive(false);

                break;
            case TrafficMode.Stop:

                greenLight.SetActive(false);
                yellowLight.SetActive(false);
                redLight.SetActive(true);

                break;
        }
    }
}
