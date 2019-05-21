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
    [MenuItem("Window/TrafficSystemEditor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<TrafficSystemBuilder>("TrafficSystemEditor");
    }

    public int numNodes;
    Object inter1Obj;
    Object inter2Obj;
    float widthDistance;
    float speedLimit;

    void OnGUI ()
    {
        EditorGUILayout.LabelField("Node width");
        widthDistance = EditorGUILayout.Slider(widthDistance, 0, 9.0f);
        if (GUILayout.Button("Set selected Nodes Width"))
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                if (obj.HasComponent<Node>())
                {
                    Node node = obj.GetComponent<Node>();
                    node.widthDistance = widthDistance;
                }
            }
        }

        EditorGUILayout.LabelField("Node speed limit");
        speedLimit = EditorGUILayout.Slider(speedLimit, 0, 100.0f);
        if (GUILayout.Button("Set selected Nodes speed limit"))
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                if (obj.HasComponent<Node>())
                {
                    Node node = obj.GetComponent<Node>();
                    node.SpeedLimit = speedLimit;
                }
            }
        }

        EditorGUILayout.LabelField("Number of nodes");
        numNodes = EditorGUILayout.IntSlider(numNodes, 1, 10);

        EditorGUILayout.LabelField("start wayControl/Node 1");
        inter1Obj = EditorGUILayout.ObjectField(inter1Obj, typeof(Object), true);
        EditorGUILayout.LabelField("end wayControl/Node 2");
        inter2Obj = EditorGUILayout.ObjectField(inter2Obj, typeof(Object), true);

        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Node 1 and Node 2 need to be adjacent");
        EditorGUILayout.LabelField("");
        if (GUILayout.Button("Link Intersections unidirectional"))
        {
            LinkIntersections(0);
        }

        if (GUILayout.Button("Link Intersections bidirectional"))
        {
            LinkIntersections(1);
        }

        if (GUILayout.Button("Add Nodes to Intersection as way"))
        {
            if (Selection.gameObjects.Length < 2)
            {
                Debug.Log("Please select atleast 2 object");
                return;
            }

            GameObject intersection = null;
            List<GameObject> nodes = new List<GameObject>();

            foreach (GameObject obj in Selection.gameObjects)
            {
                if (obj.HasComponent<Node>())
                {
                    nodes.Add(obj);
                }
                else if (obj.HasComponent<WaysControl>())
                {
                    if (intersection == null) {
                        intersection = obj;
                    } else
                    {
                        Debug.Log("Please select only one intersection");
                    }
                }
            }
            if (intersection == null)
            {
                Debug.Log("Please select an intersection");
                return;
            }
            if (nodes.Count == 0)
            {
                Debug.Log("Please select node(s)");
                return;
            }

            WaysControl wayController = intersection.GetComponent<WaysControl>();

            foreach (GameObject node in nodes)
            {
                AddWay(wayController, node, 0);
            }
        }

        if (GUILayout.Button("Equally space nodes group straight"))
        {
            spaceNodes();
        }

        if (GUILayout.Button("Add nodes between selected nodes"))
        {
            AddNodesBetweenNodes();
        }
        if (GUILayout.Button("Organize selected intersection ways"))
        {
            foreach(GameObject obj in Selection.gameObjects)
            {
                if (obj.HasComponent<WaysControl>())
                {
                    OrganizeWays(obj.GetComponent<WaysControl>());
                }
            }
        }
        if (GUILayout.Button("Delete Path"))
        {
            RemovePath();
        }

    }

    void OrganizeWays(WaysControl waycontroller)
    {
        List<Transform> ways = new List<Transform>();
        for (int i = 0; i < waycontroller.ways; i++)
        {
            if (i == 0 && waycontroller.way1 != null)
            {
                if (!ways.Contains(waycontroller.way1))
                {
                    ways.Add(waycontroller.way1);
                }
            } else if (i == 1 && waycontroller.way2 != null)
            {
                if (!ways.Contains(waycontroller.way2))
                {
                    ways.Add(waycontroller.way2);
                }
            } else if (i == 2 && waycontroller.way3 != null)
            {
                if (!ways.Contains(waycontroller.way3))
                {
                    ways.Add(waycontroller.way3);
                }
            } else if (i == 3 && waycontroller.way4 != null)
            {
                if (!ways.Contains(waycontroller.way4))
                {
                    ways.Add(waycontroller.way4);
                }
            }
        }
        if (ways.Count == 0)
        {
            waycontroller.ways = 1;
            return;
        }
        waycontroller.ways = ways.Count;
        for (int i = 0; i < waycontroller.ways; i++)
        {
            switch(i)
            {
                case 0:
                    {
                        waycontroller.way1 = ways[i];
                        break;
                    }
                case 1:
                    {
                        waycontroller.way2 = ways[i];
                        break;
                    }
                case 2:
                    {
                        waycontroller.way3 = ways[i];
                        break;
                    }
                case 3:
                    {
                        waycontroller.way4 = ways[i];
                        break;
                    }
            }
        }
    }

    void RemovePath()
    {

        if (Selection.gameObjects.Length != 1)
        {
            Debug.Log("Please select only one object");
            return;
        }
        if (!Selection.gameObjects[0].HasComponent<Node>())
        {
            Debug.Log("Please select a node");
        }
        GameObject parent = Selection.gameObjects[0].transform.parent.gameObject;
        Node firstNode = parent.transform.GetChild(0).gameObject.GetComponent<Node>();
        Node lastNode = parent.transform.GetChild(parent.transform.childCount - 1).gameObject.GetComponent<Node>();
        WaysControl waycontroller1 = firstNode.previousNode.gameObject.GetComponent<WaysControl>();
        WaysControl waycontroller2 = lastNode.nextNode.gameObject.GetComponent<WaysControl>();
        DestroyImmediate(parent);
        OrganizeWays(waycontroller1);
        OrganizeWays(waycontroller2);
    }

    void AddNodesBetweenNodes()
    {
        Node start = (Node)inter1Obj;
        Node end = (Node)inter2Obj;
        Transform node1 = start.transform;
        Transform node2 = end.transform;
        GameObject par1 = node1.parent.gameObject;
        GameObject par2 = node2.parent.gameObject;
        if (!node1.gameObject.HasComponent<Node>() || !node2.gameObject.HasComponent<Node>())
        {
            Debug.Log("Please select nodes only");
        }
        if (par1 != par2)
        {
            Debug.Log("Please select nodes of same group");
        } 
        if (Mathf.Abs(node1.GetSiblingIndex() - node2.GetSiblingIndex()) != 1)
        {
            Debug.Log("Please select adjacent nodes");
        }
        if (node1.GetSiblingIndex() > node2.GetSiblingIndex())
        {
            Node temp = start;
            start = end;
            end = temp;
            node1 = start.transform;
            node2 = end.transform;
        }
        Transform par = par1.transform;
        Vector3 dir = (node2.position - node1.position) / (numNodes + 1);

        List<GameObject> nodes = new List<GameObject>();
        for (int i = 0; i < numNodes; i++)
        {
            GameObject node = new GameObject();
            node.layer = 8;
            node.transform.SetParent(par);
            node.name = (i + node1.GetSiblingIndex() + 1).ToString();
            node.transform.SetSiblingIndex(i + node1.GetSiblingIndex() + 1);
            node.transform.position = node1.transform.position + dir * (i + 1);
            node.AddComponent<Node>();
            Node thisNode = node.GetComponent<Node>();
            thisNode.widthDistance = widthDistance;
            thisNode.SpeedLimit = speedLimit;
            node.AddComponent<BoxCollider>();
            Collider nodeCollider = node.GetComponent<BoxCollider>();
            nodeCollider.isTrigger = true;
            nodes.Add(node);
        }
        start.nextNode = nodes[0].transform;
        end.previousNode = nodes[numNodes - 1].transform;

        for (int i = 0; i < numNodes; i++)
        {
            Node thisNode = nodes[i].GetComponent<Node>();
            if (i == 0)
            {
                thisNode.previousNode = node1;
                thisNode.nextNode = nodes[i + 1].transform;
            } else if (i == numNodes - 1)
            {
                thisNode.previousNode = nodes[i - 1].transform;
                thisNode.nextNode = node2;
            } else
            {
                thisNode.previousNode = nodes[i - 1].transform;
                thisNode.nextNode = nodes[i + 1].transform;
            }
        }

    }

    void LinkIntersections(int wayMode)
    {
        WaysControl wayController1 = (WaysControl)inter1Obj;
        WaysControl wayController2 = (WaysControl)inter2Obj;
        GameObject inter1 = wayController1.gameObject;
        GameObject inter2 = wayController2.gameObject;

        if (!inter1.HasComponent<WaysControl>() || !inter2.HasComponent<WaysControl>())
        {
            Debug.Log("Please select intersections only");
        }

        if ((wayController1.ways == 4 && wayController1.way4 != null) || (wayController2.ways == 4 && wayController2.way4 != null))
        {
            Debug.Log("Intersection cannot have more than 4 links");
            return;
        }

        GameObject path = new GameObject();
        path.transform.position = (inter1.transform.position + inter2.transform.position) / 2;

        List<GameObject> Nodes = new List<GameObject>();

        Vector3 dir = (inter2.transform.position - inter1.transform.position) / (numNodes + 1);

        for (int i = 0; i < numNodes; i++)
        {
            GameObject node = new GameObject();
            node.layer = 8;
            node.transform.SetParent(path.transform);
            node.name = i.ToString();
            node.transform.position = inter1.transform.position + dir * (i + 1);
            node.AddComponent<Node>();
            Node thisNode = node.GetComponent<Node>();
            thisNode.widthDistance = widthDistance;
            thisNode.SpeedLimit = speedLimit;
            node.AddComponent<BoxCollider>();
            Collider nodeCollider = node.GetComponent<BoxCollider>();
            nodeCollider.isTrigger = true;
            Nodes.Add(node);
        }

        Node nodeProperties = Nodes[0].GetComponent<Node>();
        nodeProperties.previousNode = inter1.transform;

        if (numNodes > 1)
        {
            nodeProperties.nextNode = Nodes[1].transform;
        }

        for (int i = 1; i < numNodes - 1; i++)
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

        if (wayMode == 0)
        {
            AddWay(wayController1, Nodes[0], 0);
        } else if (wayMode == 1)
        {
            AddWay(wayController1, Nodes[0], 1);
            AddWay(wayController2, Nodes[numNodes - 1], 1);
        }

        Vector3 parentPos = Vector3.zero;
        foreach(GameObject node in Nodes)
        {
            parentPos += node.transform.position;
        }
        parentPos /= Nodes.Count;
        path.name = "0";
        path.AddComponent<VehiclePath>();
    }

    void AddWay(WaysControl wayController, GameObject node, int wayMode)
    {
        switch (wayController.ways)
        {
            case 1:
                {
                    if (wayController.way1 == null)
                    {
                        wayController.way1 = node.transform;
                        wayController.way1Mode = wayMode;
                    }
                    else
                    {
                        wayController.ways++;
                        wayController.way2 = node.transform;
                        wayController.way2Mode = wayMode;
                    }
                    break;
                }
            case 2:
                {
                    if (wayController.way2 == null)
                    {
                        wayController.way2 = node.transform;
                        wayController.way2Mode = wayMode;
                    }
                    else
                    {
                        wayController.ways++;
                        wayController.way3 = node.transform;
                        wayController.way3Mode = wayMode;
                    }
                    break;
                }
            case 3:
                {
                    if (wayController.way3 == null)
                    {
                        wayController.way3 = node.transform;
                        wayController.way3Mode = wayMode;
                    }
                    else
                    {
                        wayController.ways++;
                        wayController.way4 = node.transform;
                        wayController.way4Mode = wayMode;
                    }
                    break;
                }
        }
    }

    void spaceNodes()
    {
        List<Transform> nodes = new List<Transform>();
        if (Selection.gameObjects.Length != 1)
        {
            Debug.Log("Please select only 1 game object");
        }

        Transform parentNode = Selection.gameObjects[0].transform;
        if (parentNode.gameObject.HasComponent<Node>())
        {
            parentNode = parentNode.parent;
        } else if (!parentNode.gameObject.HasComponent<VehiclePath>())
        {
            Debug.Log("Please select a node or a parent node");
            return;
        }

        if (parentNode.childCount < 3) return;
        for (int i = 0; i < parentNode.childCount; i++)
        {
            nodes.Add(parentNode.GetChild(i));
        }
        Vector3 dir = nodes[nodes.Count - 1].position - nodes[0].position;
        dir = dir / (nodes.Count - 1);
        for (int i = 1; i < nodes.Count - 1; i++)
        {
            nodes[i].position = nodes[0].position + i * dir;
        }
    }

    void OnSceneGUI()
    {
        Event e = Event.current;
        Debug.Log("hello");
        if (e != null)
        {
            if (e.isMouse && e.shift && e.type == EventType.MouseDown)
            {
                Debug.Log("clicked");
            }
        }
    }
}
