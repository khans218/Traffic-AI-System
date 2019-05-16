using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public static GameObject instPlayerIcon;

    public Panels panels;
    public MapUI mapUI;
    public VehicleUI vehicleUI;

    private Transform player;
    private Transform camera;

   
    private int gearst = 0;
    private float thisAngle = -150;
    private AIVehicle AIVehicleComponent;
    private Vector3 curPosBigMap;// current position of big map

    [System.Serializable]
    public class Panels
    {
        public GameObject tachometer;
        public GameObject miniMap;
        public GameObject bigMap;
        public GameObject vehicleControl;
        public GameObject playerControl;
    }

    [System.Serializable]
    public class MapUI
    {
        public GameObject playerIcon;
        public Camera miniMapView, bigMapView;
        public Transform mapPlane;
    }


    [System.Serializable]
    public class VehicleUI
    {
        public Image tachometerNeedle;
        public Image barShiftGUI;

        public Text speedText;
        public Text gearText;
    }



    ////////////////////////////////////////////////////////////

    public void ShowVehicleUI()
    {

        AIVehicleComponent = AIContoller.manager.vehicleCamera.target.GetComponent<AIVehicle>();
        if (!panels.tachometer.activeSelf) panels.tachometer.SetActive(true);


        gearst = AIVehicleComponent.currentGear;
        vehicleUI.speedText.text = ((int)AIVehicleComponent.vehicleSpeed).ToString();

        if (AIVehicleComponent.automaticGear)
        {

            if (gearst > 0 && AIVehicleComponent.vehicleSpeed > 1)
            {
                vehicleUI.gearText.color = Color.green;
                vehicleUI.gearText.text = gearst.ToString();
            }
            else if (AIVehicleComponent.vehicleSpeed > 1)
            {
                vehicleUI.gearText.color = Color.red;
                vehicleUI.gearText.text = "R";
            }
            else
            {
                vehicleUI.gearText.color = Color.white;
                vehicleUI.gearText.text = "N";
            }
        }
        else
        {
            if (AIVehicleComponent.neutralGear)
            {
                vehicleUI.gearText.color = Color.white;
                vehicleUI.gearText.text = "N";
            }
            else
            {
                if (AIVehicleComponent.currentGear != 0)
                {
                    vehicleUI.gearText.color = Color.green;
                    vehicleUI.gearText.text = gearst.ToString();
                }
                else
                {

                    vehicleUI.gearText.color = Color.red;
                    vehicleUI.gearText.text = "R";
                }
            }
        }

        thisAngle = (AIVehicleComponent.motorRPM / 20) - 175;
        thisAngle = Mathf.Clamp(thisAngle, -180, 90);

        vehicleUI.tachometerNeedle.rectTransform.rotation = Quaternion.Euler(0, 0, -thisAngle);
        vehicleUI.barShiftGUI.rectTransform.localScale = new Vector3(AIVehicleComponent.powerShift / 100.0f, 1, 1);
    }


    public void ShowMiniMapUI()
    {
        // player icon status
        instPlayerIcon.transform.rotation = Quaternion.Euler(90, player.eulerAngles.y, 0);
        instPlayerIcon.transform.position = new Vector3(player.transform.position.x, mapUI.mapPlane.position.y + 5, player.transform.position.z);
        // minimap view status
        mapUI.miniMapView.transform.rotation = Quaternion.Euler(90, camera.eulerAngles.y + 180.0f, 0);
        mapUI.miniMapView.transform.position = new Vector3(player.transform.position.x, mapUI.mapPlane.position.y + 10, player.transform.position.z);
    }

    public void ShowBigMap(bool active)
    {

        mapUI.bigMapView.transform.position=curPosBigMap;

        if (mapUI.mapPlane.GetComponent<MoveMap>())
        mapUI.mapPlane.GetComponent<MoveMap>().enabled = active;

        panels.bigMap.SetActive(active);

        panels.miniMap.SetActive(!active);
        panels.tachometer.SetActive(!active);

        if (panels.playerControl) panels.playerControl.SetActive(!active);  // only for touch mode
        if (panels.vehicleControl) panels.vehicleControl.SetActive(!active);  // only for touch mode

    }

    public void MapSize(float value)
    {
        mapUI.bigMapView.orthographicSize += value;
    }

    void Start()
    {
        curPosBigMap = mapUI.bigMapView.transform.position;

        player = AIContoller.manager.playerCamera.target;
        camera = AIContoller.manager.playerCamera.transform;

        instPlayerIcon = Instantiate(mapUI.playerIcon, Vector3.zero, Quaternion.identity) as GameObject;
    }


    void Update()
    {
        if (panels.bigMap.activeSelf == false)
        {
            ShowMiniMapUI();
            if (AIContoller.manager.vehicleCamera.enabled)
            {
                ShowVehicleUI();
                if (panels.vehicleControl) panels.vehicleControl.SetActive(true); // only for touch mode
                if (panels.playerControl) panels.playerControl.SetActive(false); // only for touch mode
            }
            else
            {
                panels.tachometer.SetActive(false);
                if (panels.vehicleControl) panels.vehicleControl.SetActive(false); // only for touch mode
                if (panels.playerControl) panels.playerControl.SetActive(true); // only for touch mode
            }
        }
        else
        {
            mapUI.bigMapView.orthographicSize = Mathf.Clamp(mapUI.bigMapView.orthographicSize, -1200, -200);
        }
    }
}





