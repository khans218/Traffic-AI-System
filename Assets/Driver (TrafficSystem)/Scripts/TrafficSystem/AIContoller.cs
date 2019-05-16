using UnityEngine;
using System.Collections;
public class AIContoller : MonoBehaviour
{

    public static AIContoller manager;

    public bool showStatus = true;

    public int maxVehicles = 8;
    public int maxHumans = 8;

    public VehicleCamera vehicleCamera;
    public PlayerCamera playerCamera;

    public GameObject[] vehiclesPrefabs;
    public GameObject[] humansPrefabs;

    [HideInInspector]
    public int currentVehicles = 0;
    [HideInInspector]
    public int currentHumans = 0;
    [HideInInspector]
    public Transform player;

    private int frameCount = 0;
    private float dt = 0.0f;
    private float fps = 0.0f;
    private float updateRate = 10.0f; // 10 updates per sec.


    void Awake()
    {
        player = playerCamera.transform;
        manager = this;
    }


    void OnGUI()
    {
        if (showStatus)
        {
            GUI.color = Color.black;
            GUI.Label(new Rect(10, 30, 200, 20), "Max Vehicles: " + currentVehicles + "/" + maxVehicles);
            GUI.Label(new Rect(10, 60, 200, 20), "Max Humans: " + currentHumans + "/" + maxHumans);

            GUI.Label(new Rect(10, 100, 200, 20), "FPS: " + fps.ToString("F1"));
        }
    }


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
            Application.LoadLevel(Application.loadedLevel);
        

        if (!showStatus) return;

        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1.0 / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1.0f / updateRate;
        }

    }


}