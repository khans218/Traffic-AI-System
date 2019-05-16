using UnityEngine;
using System.Collections;

public class CreateAI : MonoBehaviour
{


    public LayerMask nodeMask = -1;
    public float InstantiateTime = 2.0f;



    private float vehicleTimer, humanTimer;

    public bool createVehicles = true;
    public bool createHumans = true;

    private AIContoller AICScript;
    private GameObject AiVehicleCreated;
    private GameObject AIVehicle;
    private float offsetDistance = 25;
    private int randomWay;



    public void InstantiateVehicle(Node CurrentNode)
    {

        Collider[] vehicles = Physics.OverlapSphere(CurrentNode.transform.position, offsetDistance);

        bool CanCreateVehicle = true;

        foreach (Collider vehicle in vehicles)
        {
            if (vehicle.CompareTag("Vehicle"))
                CanCreateVehicle = false;
        }


        AIVehicle = AIContoller.manager.vehiclesPrefabs[Random.Range(0, AIContoller.manager.vehiclesPrefabs.Length)];

        if (AIVehicle)
        {
            if (CanCreateVehicle && AIContoller.manager.currentVehicles < AIContoller.manager.maxVehicles)
            {
                RaycastHit hit;
                if (Physics.Raycast(CurrentNode.transform.position, -Vector3.up, out hit))
                {
                    AIContoller.manager.currentVehicles++;
                    AiVehicleCreated = Instantiate(AIVehicle, hit.point + (Vector3.up / 2.0f), Quaternion.identity) as GameObject;
                }
                AiVehicleCreated.name = "AIVehicle";

                if (AiVehicleCreated.GetComponent<AIVehicle>())
                {

                    AIVehicle AIVehicleScript = AiVehicleCreated.GetComponent<AIVehicle>();

                    if (CurrentNode.mode == "TwoWay")
                    {
                        randomWay = Random.Range(1, 3);

                        if (randomWay == 1)
                        {
                            AIVehicleScript.wayMove = WayMove.Left;
                            AIVehicleScript.myStatue = "NextPoint";
                            AiVehicleCreated.transform.LookAt(CurrentNode.previousNode);
                            AIVehicleScript.currentNode = CurrentNode.transform;
                            AIVehicleScript.nextNode = CurrentNode.nextNode;

                            AiVehicleCreated.transform.position = AiVehicleCreated.transform.TransformPoint(CurrentNode.widthDistance, 0, 0);


                        }
                        else
                        {
                            AIVehicleScript.wayMove = WayMove.Right;
                            AIVehicleScript.myStatue = "PreviousPoint";
                            AiVehicleCreated.transform.LookAt(CurrentNode.nextNode);
                            AIVehicleScript.currentNode = CurrentNode.transform;
                            AIVehicleScript.nextNode = CurrentNode.previousNode;

                            AiVehicleCreated.transform.position = AiVehicleCreated.transform.TransformPoint(CurrentNode.widthDistance, 0, 0);

                        }
                    }
                    else
                    {

                        AIVehicleScript.wayMove = WayMove.Right;
                        AIVehicleScript.myStatue = "PreviousPoint";
                        AiVehicleCreated.transform.LookAt(CurrentNode.nextNode);
                        AIVehicleScript.currentNode = CurrentNode.transform;
                        AIVehicleScript.nextNode = CurrentNode.nextNode;

                        AiVehicleCreated.transform.position = AiVehicleCreated.transform.TransformPoint(Random.Range(-CurrentNode.widthDistance, CurrentNode.widthDistance) / 2.0f, 0, 0);

                    }
                }

            }


        }
    }


    void CeateAIHuman(GameObject AIHuman)
    {
        Vector3 randomDirection = Random.insideUnitSphere * 200;
        randomDirection += transform.position;
        UnityEngine.AI.NavMeshHit closestHit;

        if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out closestHit, 200f, UnityEngine.AI.NavMesh.AllAreas))
        {
            Collider[] Colliders = Physics.OverlapSphere(closestHit.position, 25.0f);
            bool CreateHuman = true;

            foreach (Collider collider in Colliders)
                if (collider.tag == "Human" || collider.tag == "Vehicle") CreateHuman = false;

            if (CreateHuman && AIContoller.manager.currentHumans < AIContoller.manager.maxHumans)
            {
                AIContoller.manager.currentHumans++;
                Instantiate(AIHuman, closestHit.position, Quaternion.identity);
            }
        }
    }



    void Awake()
    {
        AICScript = AIContoller.manager;
    }



    void Update()
    {


        if (createHumans)
        {
            if (AIContoller.manager.humansPrefabs.Length > 0)
            {
                if (humanTimer == 0)
                {
                    CeateAIHuman(AIContoller.manager.humansPrefabs[Random.Range(0, AIContoller.manager.humansPrefabs.Length)]);
                    humanTimer = InstantiateTime;
                }
                else
                {
                    humanTimer = Mathf.MoveTowards(humanTimer, 0.0f, Time.deltaTime);
                }
            }
        }



        if (createVehicles)
        {
            if (vehicleTimer == 0)
            {
                Collider[] nodes = Physics.OverlapSphere(transform.position, 300, nodeMask);

                    foreach (Collider node in nodes)
                    {
                        float Dist = Vector3.Distance(transform.position, node.transform.position);

                        if (Dist < 250 && Dist > 200)
                        {
                            if (node.GetComponent<Node>() && AIContoller.manager.vehiclesPrefabs.Length > 0)
                            {
                                if (!GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), node.bounds))
                                {
                                    InstantiateVehicle(node.GetComponent<Node>());
                                    vehicleTimer = InstantiateTime;

                                }


                            }

                        }
                    }
                
            }
            else
            {
                vehicleTimer = Mathf.MoveTowards(vehicleTimer, 0.0f, Time.deltaTime);
            }
        }


    }



}
