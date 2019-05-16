using UnityEngine;
using System.Collections;

public enum VehicleStatus { EmptyOff, EmptyOn, AI, Player }
public enum WayMove { Center = 1, Right = 2, Left = 3 }

public class AIVehicle : MonoBehaviour
{
    public VehicleStatus vehicleStatus = VehicleStatus.Player;


    public float forwardSpeed = 1.0f;
    public float steerSpeed = 1.0f;
    public float nextNodeDistance = 10.0f;

    public float rayCastLentgh = 1.0f;
    public float rayCastBackLentgh = 1.0f;

    public float rayCastAngle = 1.0f;

    public bool drawGozmos = true;


    public Transform raycastPoint;

    public LayerMask layerMask;


    [HideInInspector]
    public bool trafficStop = false;
    [HideInInspector]
    public bool AIActive = true;
    [HideInInspector]
    public Transform currentNode, lastNode, nextNode;
    [HideInInspector]
    public WayMove wayMove = WayMove.Center;
    [HideInInspector]
    public string myStatue;
    [HideInInspector]
    public float AIAccel, AISteer = 0.0f;
    [HideInInspector]
    public bool AIBrake = false;
    [HideInInspector]
    public bool oneWay = false;
    [HideInInspector]
    public float widthDistance, minWidthDistance, maxWidthDistance;
    [HideInInspector]
    public float vehicleSpeed = 0.0f;
    [HideInInspector]
    public AudioSource horn;



    [HideInInspector]
    public bool automaticGear, neutralGear;
    [HideInInspector]
    public int currentGear;
     [HideInInspector]
    public float motorRPM;
     [HideInInspector]
     public float powerShift;



    private float waitingTime;
    private float raycastSteer;
    private float currentFwdSpeed;
    private float stopTimer = 0.0f;
    private float targetAngle;
    private int randomWays = 0;
    private bool wayActive, waysActive;
    private bool RearGear = false;
    private Transform player;
    private Node nodeComponenet;

    //-----------------------------------------------------------------------------------------------

    void Start()
    {
        player = AIContoller.manager.player;
        currentFwdSpeed = forwardSpeed;

        if (vehicleStatus == VehicleStatus.AI)
        {
            AIActive = true;
            if (currentNode && currentNode.GetComponent<Node>().mode == "OnWay") oneWay = true;
        }
        else
        {
            AIActive = false;
        }
    }


    private float hittingTimer = 0.0f;
    void Update()
    {


        if (!AIActive) return;


        AIBrake = false;
        trafficStop = false;
        currentFwdSpeed = forwardSpeed;


        if (raycastSteer != 0.0f)
            raycastSteer = Mathf.SmoothStep(raycastSteer, 0, Time.deltaTime * 10.0f);


        float lentgh = 0;
        float rotate = 0;

        bool hitting = false;

        float RightHitDistance = 0;
        float LeftHitDistance = 0;



        RaycastHit raycastHit;

        for (int i = 0; i < 6; i++)
        {

            switch (i)
            {
                case 0: rotate = -1.0f; lentgh = 7; break;
                case 1: rotate = 0; lentgh = 10; break;
                case 2: rotate = 1.0f; lentgh = 7; break;
                case 3: rotate = -3.0f; lentgh = 6; break;
                case 4: rotate = 0; lentgh = -rayCastBackLentgh; break;
                case 5: rotate = 3.0f; lentgh = 6; break;
            }

            if (Physics.Raycast(raycastPoint.TransformPoint((Mathf.Repeat((float)i, 3) - 1), 0, 0), raycastPoint.TransformDirection((rotate * rayCastAngle), 0, (lentgh * rayCastLentgh)), out raycastHit, (lentgh * rayCastLentgh), layerMask.value))
            {
                hitting = true;


                //left
                if (i == 3 || i == 0)
                {
                    LeftHitDistance = 10 - raycastHit.distance;
                    if (vehicleSpeed > 10) currentFwdSpeed -= 1.0f;

                    if (raycastHit.distance < 0.5f) { if (waitingTime == 0 && !RearGear) { waitingTime = Random.Range(2.0f, 5.0f); if (horn)horn.Play(); } RearGear = true; }
                }
                //center front
                else if (i == 1)
                {
                    LeftHitDistance = 5.0f;

                    if (raycastHit.distance < lentgh / 2.0f) { if (waitingTime == 0 && !RearGear) { waitingTime = Random.Range(5.0f, 10.0f); if (horn)horn.Play(); } RearGear = true; }
                }//center back
                else if (i == 4)
                {
                    if (vehicleSpeed > 0) { AIBrake = true; }
                }
                //right
                else if (i == 5 || i == 2)
                {

                    RightHitDistance = -10 + raycastHit.distance;
                    if (vehicleSpeed > 10) currentFwdSpeed -= 1.0f;
                    if (raycastHit.distance < 0.5f) { if (waitingTime == 0 && !RearGear) { waitingTime = Random.Range(2.5f, 5.0f); if (horn)horn.Play(); } RearGear = true; }
                }


                if (raycastHit.transform.root.GetComponent<AIVehicle>() && raycastHit.transform.root.GetComponent<AIVehicle>().trafficStop == true &&
                   Mathf.Abs(Quaternion.Dot(transform.rotation, raycastHit.transform.root.rotation)) > 0.5f)
                    AIBrake = raycastHit.transform.root.GetComponent<AIVehicle>().AIBrake;


            }
        }



        if (Vector3.Distance(raycastPoint.position, player.position) < 5)
            AIBrake = true;


        if (RearGear)
        {


            if (waitingTime==0.0f)
            {

                if (hitting)
                {
                    if (stopTimer != 3.0f)
                    {
                        stopTimer = Mathf.MoveTowards(stopTimer, 3.0f, Time.deltaTime);
                        AIBrake = true;
                    }
                    hittingTimer = 0.0f;
                }
                else
                {
                    hittingTimer = Mathf.MoveTowards(hittingTimer, 1.0f, Time.deltaTime);

                    if (hittingTimer == 1.0f)
                    {
                        stopTimer = 0.0f;
                        RearGear = false;
                    }
                }

            }
            else
            {
                waitingTime = Mathf.MoveTowards(waitingTime,0.0f,Time.deltaTime);
            }
        }



        if (!AIBrake && !RearGear)
            AIAccel = currentFwdSpeed / 50.0f;
        else
            if (stopTimer == 3.0f) AIAccel = -currentFwdSpeed / 100.0f; else AIAccel = 0.0f;


        raycastSteer = Mathf.SmoothStep(raycastSteer, (RightHitDistance + LeftHitDistance) * 50.0f, Time.deltaTime * 5.0f);



        if (stopTimer != 0)
            AISteer = Mathf.SmoothStep(AISteer, -targetAngle / 40, steerSpeed / 3.0f);
        else
            AISteer = Mathf.SmoothStep(AISteer, targetAngle / 60, steerSpeed / 3.0f);


        AIControl();


        var relativeTarget = transform.InverseTransformPoint(nextNode.position);

        targetAngle = Mathf.Atan2(relativeTarget.x + widthDistance, relativeTarget.z);
        targetAngle *= Mathf.Rad2Deg;
        targetAngle = Mathf.Clamp(targetAngle + raycastSteer, -65, 65);

    }

    //AIWayPoints//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public void widthDistanceRefrash(Node node)
    {

        if (node != null)
        {
            if (oneWay)
            {
                if (maxWidthDistance != node.widthDistance)
                {
                    maxWidthDistance = node.widthDistance;
                    widthDistance = Random.Range(-node.widthDistance, node.widthDistance);
                }
            }
            else
            {
                minWidthDistance = 3.0f;
                if (maxWidthDistance != node.widthDistance)
                {
                    maxWidthDistance = Mathf.Clamp(node.widthDistance, 3.0f, 20.0f);
                    widthDistance = Random.Range(minWidthDistance, maxWidthDistance);
                }
            }

        }
    }


    void AIControl()
    {

        //////////////////////////////////////////////////////////////////////////////////////////////////	

        if (nextNode)
        {
            if (nextNode.GetComponent<WaysControl>())
            {
                if (Vector3.Distance(raycastPoint.position, nextNode.position) < nextNodeDistance && currentNode != nextNode)
                    currentNode = nextNode;
            }
            else
            {
                if (Vector3.Distance(raycastPoint.position, nextNode.position) < nextNodeDistance && nextNode != currentNode)
                    currentNode = nextNode;

            }
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////	


        if (currentNode != null)
        {

            if (currentNode.GetComponent<WaysControl>())
            {
                if (!waysActive)
                {
                    var waysScript = currentNode.GetComponent<WaysControl>();
                    nextNode = RandomWay(nextNode, waysScript.ways);
                    waysActive = true;
                }

            }
            else
            {
                nodeComponenet = currentNode.GetComponent<Node>();
                widthDistanceRefrash(currentNode.GetComponent<Node>());


                if (nodeComponenet)
                {
                    if (((nodeComponenet.nodeState == "NextPoint" && myStatue == "PreviousPoint")
                    || (nodeComponenet.nodeState == "PreviousPoint" && myStatue == "NextPoint")) && nodeComponenet.trafficMode != TrafficMode.Go)
                    {

                        trafficStop = true;
                        AIBrake = true;
                        currentFwdSpeed = -1.0f;
                    }
                }

                if (wayMove == WayMove.Right) { lastNode = currentNode; nextNode = nodeComponenet.nextNode; }
                else if (wayMove == WayMove.Left) { lastNode = currentNode; nextNode = nodeComponenet.previousNode; }

                waysActive = false;
            }
        }
    }



    //RandomWay//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    Transform RandomWay(Transform node, int maxWays)
    {

        WaysControl waysControl = currentNode.GetComponent<WaysControl>();


        while (wayActive == false)
        {
            if (maxWays == 1)
            {
                randomWays = 1;
                wayActive = true;
            }
            else
            {
                randomWays = Random.Range(1, maxWays + 1);

                switch (randomWays)
                {
                    case 1:

                        if (waysControl.way1 != lastNode)
                        {
                            oneWay = waysControl.way1Mode == 0 ? true : false;

                            if (int.Parse(waysControl.way1.name) > 1) wayMove = WayMove.Left; else wayMove = WayMove.Right;

                            node = waysControl.way1;
                            myStatue = waysControl.way1.GetComponent<Node>().nodeState;

                            wayActive = true;
                        }
                        break;

                    case 2:

                        if (waysControl.way2 != lastNode)
                        {
                            oneWay = waysControl.way2Mode == 0 ? true : false;

                            if (int.Parse(waysControl.way2.name) > 1) wayMove = WayMove.Left; else wayMove = WayMove.Right;

                            node = waysControl.way2;
                            myStatue = waysControl.way2.GetComponent<Node>().nodeState;

                            wayActive = true;

                        }
                        break;

                    case 3:
                        if (waysControl.way3 != lastNode)
                        {
                            oneWay = waysControl.way3Mode == 0 ? true : false;

                            if (int.Parse(waysControl.way3.name) > 1) wayMove = WayMove.Left; else wayMove = WayMove.Right;

                            node = waysControl.way3;
                            myStatue = waysControl.way3.GetComponent<Node>().nodeState;

                            wayActive = true;

                        }
                        break;

                    case 4:
                        if (waysControl.way4 != lastNode)
                        {
                            oneWay = waysControl.way4Mode == 0 ? true : false;

                            if (int.Parse(waysControl.way4.name) > 1) wayMove = WayMove.Left; else wayMove = WayMove.Right;

                            node = waysControl.way4;
                            myStatue = waysControl.way4.GetComponent<Node>().nodeState;

                            wayActive = true;
                        }
                        break;
                }
            }
        }


        wayActive = false;

        maxWidthDistance = maxWidthDistance - 0.1f;

        if (lastNode != null)
            widthDistanceRefrash(lastNode.GetComponent<Node>());

        return node;
    }




    void OnDrawGizmos()
    {
        if (!drawGozmos || raycastPoint == null) return;

        float lentgh = 0;
        float rotate = 0;

        if (raycastPoint)
            Gizmos.color = raycastPoint.GetComponent<GizmoObject>().gizmoColor;
        else
            Gizmos.color = Color.red;

        for (int i = 0; i < 6; i++)
        {
            switch (i)
            {
                case 0: rotate = -1.0f; lentgh = 7; break;
                case 1: rotate = 0; lentgh = 10; break;
                case 2: rotate = 1.0f; lentgh = 7; break;
                case 3: rotate = -3.0f; lentgh = 6; break;
                case 4: rotate = 0; lentgh = -rayCastBackLentgh; break;
                case 5: rotate = 3.0f; lentgh = 6; break;
            }

            Gizmos.DrawRay(raycastPoint.TransformPoint((Mathf.Repeat((float)i, 3) - 1), 0, 0), raycastPoint.TransformDirection((rotate * rayCastAngle), 0, (lentgh * rayCastLentgh)));
        }

    }

}