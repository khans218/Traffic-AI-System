using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficSignalController : MonoBehaviour {

    public float delayTime;
    public Node[] way1Nodes;
    public Node[] way2Nodes;
    float currentTime;
    bool right;

    private void Start()
    {
        foreach(Node node in way1Nodes)
        {
            node.trafficNode = true;
        }
        foreach(Node node in way2Nodes)
        {
            node.trafficNode = true;
        }
        currentTime = 0;
        int i = Random.Range(0, 2);
        if (i == 0)
        {
            callSignal(1, TrafficMode.Stop);
            callSignal(2, TrafficMode.Go);
            right = false;
        } else if (i == 1)
        {
            callSignal(1, TrafficMode.Go);
            callSignal(2, TrafficMode.Stop);
            right = true;
        }
    }

    // Update is called once per frame
    void Update () {
        currentTime += Time.deltaTime;
        if (currentTime > delayTime)
        {
            right = !right;
            currentTime = 0;
            if (right)
            {
                callSignal(1, TrafficMode.Go);
                callSignal(2, TrafficMode.Stop);
            } else
            {
                callSignal(1, TrafficMode.Stop);
                callSignal(2, TrafficMode.Go);
            }
        }
	}

    void callSignal(int NodeNum, TrafficMode trafficMode)
    {
        if (NodeNum == 1)
        {
            foreach(Node node in way1Nodes)
            {
                node.trafficMode = trafficMode;
            }
        } else if (NodeNum == 2)
        {
            foreach(Node node in way2Nodes)
            {
                node.trafficMode = trafficMode;
            }
        }
    }
}
