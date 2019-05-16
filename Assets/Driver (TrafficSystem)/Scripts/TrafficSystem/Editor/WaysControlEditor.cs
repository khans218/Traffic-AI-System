using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(WaysControl))]
public class WaysControlEditor : Editor
{

    string[] mode = new string[] { "One Way", "Two Way" };
    public override void OnInspectorGUI()
    {

        WaysControl myPlayer = (WaysControl)target;

        EditorGUILayout.Space();


        GUI.color = new Color(0.5f, 1, 0.5f);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Stop Distance:");
        myPlayer.stopDistance = EditorGUILayout.FloatField(myPlayer.stopDistance);
        EditorGUILayout.EndHorizontal();

        GUI.color = Color.white;

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Ways:");
        myPlayer.ways = EditorGUILayout.IntSlider(myPlayer.ways, 1, 4);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        for (int ways = 1; ways < (myPlayer.ways + 1); ways++)
        {

            if (myPlayer.ways > 0)
            {

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Way " + ways + ":");

                switch (ways)
                {
                    case 1:
                        myPlayer.way1 = EditorGUILayout.ObjectField(myPlayer.way1, typeof(Transform)) as Transform;
                        myPlayer.way1Mode = EditorGUILayout.Popup(myPlayer.way1Mode, mode);
                        if (myPlayer.way1)
                        {
                            if (myPlayer.way1.GetComponent<Node>().firistNode)
                            {
                                myPlayer.way1.GetComponent<Node>().previousNode = myPlayer.transform;
                            }
                            else if (myPlayer.way1.GetComponent<Node>().lastNode)
                            {
                                myPlayer.way1.GetComponent<Node>().nextNode = myPlayer.transform;
                            }
                        }

                        break;

                    case 2:
                        myPlayer.way2 = EditorGUILayout.ObjectField(myPlayer.way2, typeof(Transform)) as Transform;
                        myPlayer.way2Mode = EditorGUILayout.Popup(myPlayer.way2Mode, mode);

                        if (myPlayer.way2)
                        {
                            if (myPlayer.way2.GetComponent<Node>().firistNode)
                            {
                                myPlayer.way2.GetComponent<Node>().previousNode = myPlayer.transform;
                            }
                            else if (myPlayer.way2.GetComponent<Node>().lastNode)
                            {
                                myPlayer.way2.GetComponent<Node>().nextNode = myPlayer.transform;
                            }

                        }
                        break;

                    case 3:
                        myPlayer.way3 = EditorGUILayout.ObjectField(myPlayer.way3, typeof(Transform)) as Transform;
                        myPlayer.way3Mode = EditorGUILayout.Popup(myPlayer.way3Mode, mode);

                        if (myPlayer.way3)
                        {
                            if (myPlayer.way3.GetComponent<Node>().firistNode)
                            {
                                myPlayer.way3.GetComponent<Node>().previousNode = myPlayer.transform;
                            }
                            else if (myPlayer.way3.GetComponent<Node>().lastNode)
                            {
                                myPlayer.way3.GetComponent<Node>().nextNode = myPlayer.transform;
                            }
                        }

                        break;

                    case 4:
                        myPlayer.way4 = EditorGUILayout.ObjectField(myPlayer.way4, typeof(Transform)) as Transform;
                        myPlayer.way4Mode = EditorGUILayout.Popup(myPlayer.way4Mode, mode);

                        if (myPlayer.way4)
                        {
                            if (myPlayer.way4.GetComponent<Node>().firistNode)
                            {
                                myPlayer.way4.GetComponent<Node>().previousNode = myPlayer.transform;
                            }
                            else if (myPlayer.way4.GetComponent<Node>().lastNode)
                            {
                                myPlayer.way4.GetComponent<Node>().nextNode = myPlayer.transform;
                            }

                        }
                        break;

                }

                EditorGUILayout.EndHorizontal();
            }

        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Traffic Control:");
        myPlayer.TCActive = EditorGUILayout.Toggle(myPlayer.TCActive);
        EditorGUILayout.EndHorizontal();


        if (myPlayer.TCActive)
        {
            myPlayer.TrafficTime = EditorGUILayout.Slider("Traffic Time:", myPlayer.TrafficTime, 1, 60);
            EditorGUILayout.Space();
            myPlayer.TrafficWays = EditorGUILayout.IntSlider("Traffic Ways:", myPlayer.TrafficWays, 1, 3);

            EditorGUILayout.Space();
            GUI.color = (myPlayer.TrafficNumber == 1) ? new Color(0.5f, 1, 0.5f) : new Color(1, 0.5f, 0.5f);

            for (int TrafficWay = 1; TrafficWay < (myPlayer.TrafficWays + 1); TrafficWay++)
            {

                if (myPlayer.TrafficWays > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("TrafficWay " + TrafficWay + ":");

                    switch (TrafficWay)
                    {
                        case 1:
                            GUI.color = (myPlayer.TrafficNumber == 1) ? new Color(0.5f, 1, 0.5f) : new Color(1, 0.5f, 0.5f);
                            if (myPlayer.TrafficWaitTimer != 1) GUI.color = Color.yellow;
                            myPlayer.trafficNumbers[0] = EditorGUILayout.MaskField(myPlayer.trafficNumbers[0], myPlayer.trafficMode);
                            break;
                        case 2:
                            GUI.color = (myPlayer.TrafficNumber == 2) ? new Color(0.5f, 1, 0.5f) : new Color(1, 0.5f, 0.5f);
                            if (myPlayer.TrafficWaitTimer != 1) GUI.color = Color.yellow;
                            myPlayer.trafficNumbers[1] = EditorGUILayout.MaskField(myPlayer.trafficNumbers[1], myPlayer.trafficMode);
                            break;
                        case 3:
                            GUI.color = (myPlayer.TrafficNumber == 3) ? new Color(0.5f, 1, 0.5f) : new Color(1, 0.5f, 0.5f);
                            if (myPlayer.TrafficWaitTimer != 1) GUI.color = Color.yellow;
                            myPlayer.trafficNumbers[2] = EditorGUILayout.MaskField(myPlayer.trafficNumbers[2], myPlayer.trafficMode);
                            break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }


        }

        EditorUtility.SetDirty(target);
    }

}
	
	
	
	
	
	
	
	
