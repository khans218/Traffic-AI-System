using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public enum ControlMode { simple = 1, touch = 2 }
public class GameControl : MonoBehaviour {

    public static GameControl manager;

    public static float accelFwd,accelBack;
    public static float steerAmount;

    public static bool shift;
    public static bool brake;
    public static bool driving;
    public static bool jump;



    public ControlMode controlMode = ControlMode.simple;

    public GameObject getInVehicle;


    private VehicleCamera vehicleCamera;
    private float drivingTimer=0.0f;
    public void VehicleAccelForward(float amount) { accelFwd = amount; }
    public void VehicleAccelBack(float amount) { accelBack = amount; }
    public void VehicleSteer(float amount) { steerAmount = amount; }
    public void VehicleHandBrake(bool HBrakeing) { brake = HBrakeing; }
    public void VehicleShift(bool Shifting) { shift = Shifting; }
    public void GetInVehicle() { if (drivingTimer == 0) { driving = true; drivingTimer = 3.0f; } }
    public void GetOutVehicle() { if (drivingTimer == 0) { driving = false; drivingTimer = 3.0f; } }
    public void Jumping() { jump = true; }

    void Awake()
    {
        manager = this;
    }
    void Start()
    {
        vehicleCamera = AIContoller.manager.vehicleCamera;
    }
    void Update()
    {
        drivingTimer = Mathf.MoveTowards(drivingTimer,0.0f,Time.deltaTime);
    }
    public void CameraSwitch()
    {
        vehicleCamera.Switch++;
        if (vehicleCamera.Switch > vehicleCamera.cameraSwitchView.Count) { vehicleCamera.Switch = 0; }
    }

}
