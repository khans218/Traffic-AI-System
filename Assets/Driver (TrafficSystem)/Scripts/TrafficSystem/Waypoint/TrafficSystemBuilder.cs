using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class hasComponent
{
    public static bool HasComponent<T>(this GameObject flag) where T : Component
    {
        return flag.GetComponent<T>() != null;
    }
}

public class TrafficSystemBuilder : EditorWindow {
    [MenuItem("Window/Example")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<TrafficSystemBuilder>("TrafficSystemEditor");
    }

    public int numNodes;

    void OnGUI ()
    {
        numNodes = EditorGUILayout.IntSlider(numNodes, 1, 10);
        if (GUILayout.Button("Link Intersections"))
        {
            if (Selection.gameObjects.Length != 2)
            {
                Debug.Log("Please select only two objects");
                return;
            }

            foreach (GameObject obj in Selection.gameObjects)
            {
                if (!(obj.HasComponent<WaysControl>()))
                {
                    Debug.Log("Please Select an Intersection");
                    return;
                }
            }

            GameObject inter1 = Selection.gameObjects[0];
            GameObject inter2 = Selection.gameObjects[1];

            WaysControl wayController1 = inter1.GetComponent<WaysControl>();
            WaysControl wayController2 = inter2.GetComponent<WaysControl>();

            if ((wayController1.ways == 4 && wayController1.way4 != null) || (wayController2.ways == 4 && wayController2.way4 != null))
            {
                Debug.Log("Intersection cannot have more than 4 links");
                return;
            }

            GameObject path = new GameObject();
            path.transform.position = (inter1.transform.position + inter2.transform.position) / 2;
            path.name = "way0";

            List<GameObject> Nodes = new List<GameObject>();

            Vector3 dir = (inter2.transform.position - inter1.transform.position) / (numNodes + 1);

            for (int i = 0; i < numNodes; i++)
            {
                GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cube);
                node.transform.SetParent(path.transform);
                node.name = i.ToString();
                node.transform.position = inter1.transform.position + dir * (i + 1);
                node.AddComponent<Node>();
                Nodes.Add(node);
            }

            Node nodeProperties = Nodes[0].GetComponent<Node>();
            nodeProperties.previousNode = inter1.transform;

            if (numNodes > 1)
            {
                nodeProperties.nextNode = Nodes[1].transform;
            }

            for (int i = 1; i < numNodes-1; i++)
            {
                nodeProperties = Nodes[i].GetComponent<Node>();
                nodeProperties.previousNode = Nodes[i - 1].transform;
                nodeProperties.nextNode = Nodes[i + 1].transform;
            }

            nodeProperties = Nodes[numNodes - 1].GetComponent<Node>();

            if (numNodes > 1)
            {
                nodeProperties.previousNode = Nodes[numNodes - 2].transform;
            }

            nodeProperties.nextNode = inter2.transform;

            switch(wayController1.ways)
            {
                case 1:
                    {
                        wayController1.way1 = Nodes[0].transform;
                        break;
                    }
                case 2:
                    {
                        wayController1.way2 = Nodes[0].transform;
                        break;
                    }
                case 3:
                    {
                        wayController1.way3 = Nodes[0].transform;
                        break;
                    }
                case 4:
                    {
                        wayController1.way4 = Nodes[0].transform;
                        break;
                    }
            }
            wayController1.ways++;

            switch (wayController2.ways)
            {
                case 1:
                    {
                        wayController2.way1 = Nodes[numNodes - 1].transform;
                        break;
                    }
                case 2:
                    {
                        wayController2.way2 = Nodes[numNodes - 1].transform;
                        break;
                    }
                case 3:
                    {
                        wayController2.way3 = Nodes[numNodes - 1].transform;
                        break;
                    }
                case 4:
                    {
                        wayController2.way4 = Nodes[numNodes - 1].transform;
                        break;
                    }
            }
            wayController2.ways++;
            path.name = "0";
        }
    }
}
