using UnityEngine;
using System.Collections;

public enum PlayerCarStatus { Idle = 0, OpenDoor = 1, inCar = 2, RollDoor = 3, Sit = 4, OutCar = 5, CloseDoor = 6 };
public enum PlayerBikeStatus { Idle = 0, GettingOn = 1, Sit = 2, GettingOff = 3};

public class DriveVehicle : MonoBehaviour
{

    public CharacterComponents characterComponents;


    private PlayerCarStatus playerCarStatus = PlayerCarStatus.Idle;
    private PlayerBikeStatus playerBikeStatus = PlayerBikeStatus.Idle;

    private bool gettingOnCar = false;
    private bool gettingOnBike = false;

    private Animator m_Animator;

    private CarComponents carComponents;
    private BikeComponents bikeComponents;


    private AIVehicle m_AIVehicle;


    private VehicleControl m_VehicleControl;
    private BikeControl m_BikeControl;

    private Transform handleTrigger;
    private Transform door;
    private Transform sitPoint;

    
    [System.Serializable]
    public class CharacterComponents
    {
        public Rigidbody myRigidbody;
        public Collider myCollider;
        public UnityEngine.AI.NavMeshObstacle myNavMeshObstacle;

        public ThirdPersonCharacter myThirdPersonCharacter;
        public ThirdPersonUserControl myThirdPersonUserControl;
    }


    void OnTriggerStay(Collider other)
    {

        if (other.CompareTag("HandleTrigger"))
        {
            GameControl.manager.getInVehicle.SetActive(true);

            if ((GameControl.manager.controlMode == ControlMode.simple&&Input.GetKey(KeyCode.F))
                || (GameControl.manager.controlMode == ControlMode.touch && GameControl.driving == true))
            {


                if (other.transform.root.GetComponent<CarComponents>())
                {
                    carComponents = other.transform.root.GetComponent<CarComponents>();
                    m_AIVehicle = other.transform.root.GetComponent<AIVehicle>();
                    m_VehicleControl = other.transform.root.GetComponent<VehicleControl>();

                    door = carComponents.door;
                    handleTrigger = carComponents.handleTrigger;
                    sitPoint = carComponents.sitPoint;
                    gettingOnCar = true;
                    gettingOnBike = false;

                    GameControl.manager.getInVehicle.SetActive(false);

                }


                if (other.transform.root.GetComponent<BikeComponents>())
                {
                    bikeComponents = other.transform.root.GetComponent<BikeComponents>();
                    m_AIVehicle = other.transform.root.GetComponent<AIVehicle>();
                    m_BikeControl = other.transform.root.GetComponent<BikeControl>();

                    handleTrigger = bikeComponents.handleTrigger;
                    sitPoint = bikeComponents.sitPoint;

                    gettingOnBike = true;
                    gettingOnCar = false;

                    GameControl.manager.getInVehicle.SetActive(false);

                }


            }

        }

    }


    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HandleTrigger"))
            GameControl.manager.getInVehicle.SetActive(false);
        
    }
    public void ComponentsStatus(bool active)
    {

        characterComponents.myThirdPersonUserControl.enabled = active;
        characterComponents.myThirdPersonCharacter.enabled = active;
        characterComponents.myNavMeshObstacle.enabled = active;
        characterComponents.myRigidbody.isKinematic = !active;
        characterComponents.myCollider.enabled = active;
    }


    void Start()
    {
        m_Animator = GetComponent<Animator>();
    }

    public void GetinCar()
    {


        switch (playerCarStatus)
        {

            case PlayerCarStatus.Idle:

                transform.parent = carComponents.sitPoint.transform;
                ComponentsStatus(false);

                transform.position = Vector3.MoveTowards(transform.position, handleTrigger.position, Time.deltaTime * 3.0f);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, handleTrigger.rotation, Time.deltaTime * 250.0f);

                if (transform.position == handleTrigger.position && transform.rotation == handleTrigger.rotation)
                {
                    m_Animator.ForceStateNormalizedTime(0.0f);

                    m_Animator.SetFloat("CarStatus", 1);
                    m_Animator.SetBool("DriveCar", true);
                    m_Animator.Play("Drive Car", 0);
                    m_VehicleControl.carSounds.openDoor.Play();
                    playerCarStatus++;
                }

                break;
            case PlayerCarStatus.OpenDoor:

                if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                {
                    m_Animator.SetFloat("CarStatus", 2);
                    m_Animator.ForceStateNormalizedTime(0.0f);
                    playerCarStatus++;
                }
                else if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.6f)
                {
                    door.localRotation = Quaternion.RotateTowards(door.localRotation, Quaternion.Euler(0, 45, 0), Time.deltaTime * 300.0f);
                }


                break;
            case PlayerCarStatus.inCar:


                carComponents.driving = false;

                if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                {
                    m_Animator.SetFloat("CarStatus", 3);
                    m_Animator.ForceStateNormalizedTime(0.0f);
                    playerCarStatus++;
                }
                else
                {

                    if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.3f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, sitPoint.position, Time.deltaTime * 3.0f);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, sitPoint.rotation, Time.deltaTime * 250.0f);
                    }
                    else
                    {
                        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 500.0f);
                    }
                }


                break;
            case PlayerCarStatus.RollDoor:



                if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                {
                    m_Animator.SetFloat("CarStatus", 4);
                    m_Animator.ForceStateNormalizedTime(0.0f);
                    m_VehicleControl.carSounds.closeDoor.Play();
                    playerCarStatus++;
                }
                else if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f)
                {
                    door.localRotation = Quaternion.RotateTowards(door.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 250.0f);
                }

                break;
            case PlayerCarStatus.Sit:


                AIContoller.manager.playerCamera.enabled = false;

                AIContoller.manager.vehicleCamera.cameraSwitchView = carComponents.cameraViewSetting.cameraViews;
                AIContoller.manager.vehicleCamera.distance = carComponents.cameraViewSetting.distance;
                AIContoller.manager.vehicleCamera.height = carComponents.cameraViewSetting.height;
                AIContoller.manager.vehicleCamera.Angle = carComponents.cameraViewSetting.Angle;


                AIContoller.manager.vehicleCamera.target = handleTrigger.root.transform;
                AIContoller.manager.vehicleCamera.enabled = true;


                m_AIVehicle.vehicleStatus = VehicleStatus.Player;

                if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                {
                    if ((GameControl.manager.controlMode == ControlMode.simple && Input.GetKey(KeyCode.F))
                        || (GameControl.manager.controlMode == ControlMode.touch && GameControl.driving == false))
                    {
                        m_Animator.SetFloat("CarStatus", 5);
                        m_Animator.ForceStateNormalizedTime(0.0f);
                        m_AIVehicle.vehicleStatus = VehicleStatus.EmptyOn;
                        m_VehicleControl.carSounds.openDoor.Play();
                        playerCarStatus++;
                    }

                }


                break;
            case PlayerCarStatus.OutCar:

                if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                {
                    m_Animator.SetFloat("CarStatus", 6);
                    m_Animator.ForceStateNormalizedTime(0.0f);
                    playerCarStatus++;
                }
                else if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.3f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, handleTrigger.position, Time.deltaTime * 3.0f);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, handleTrigger.rotation, Time.deltaTime * 250.0f);
                }

                else if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.1f)
                {

                    door.localRotation = Quaternion.RotateTowards(door.localRotation, Quaternion.Euler(0, 45, 0), Time.deltaTime * 250.0f);
                }




                break;
            case PlayerCarStatus.CloseDoor:

                AIContoller.manager.playerCamera.enabled = true;
                AIContoller.manager.vehicleCamera.enabled = false;


                if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f)
                {
                    gettingOnCar = false;
                    transform.parent = null;
                    ComponentsStatus(true);
                    m_VehicleControl.carSounds.closeDoor.Play();

                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

                    playerCarStatus = PlayerCarStatus.Idle;

                }
                else if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.2f)
                {
                    handleTrigger.root.GetComponent<CarComponents>().door.localRotation = Quaternion.RotateTowards(
                        handleTrigger.root.GetComponent<CarComponents>().door.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 250.0f);
                    m_Animator.SetBool("DriveCar", false);
                }


                break;
        }

    }



    public void GetinBike()
    {

        switch (playerBikeStatus)
        {

            case PlayerBikeStatus.Idle:

                transform.parent = bikeComponents.sitPoint.transform;
                ComponentsStatus(false);

                transform.position = Vector3.MoveTowards(transform.position, handleTrigger.position, Time.deltaTime * 3.0f);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, handleTrigger.rotation, Time.deltaTime * 250.0f);

                if (transform.position == handleTrigger.position && transform.rotation == handleTrigger.rotation)
                {
                    m_Animator.ForceStateNormalizedTime(0.0f);

                    m_Animator.SetFloat("BikeStatus", 1);
                    m_Animator.SetBool("DriveBike", true);
                    m_Animator.Play("Drive Bike", 0);
                    playerBikeStatus++;
                }

                break;
            case PlayerBikeStatus.GettingOn:


                bikeComponents.driving = false;


                if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                {
                    m_Animator.SetFloat("BikeStatus", 2);
                    m_Animator.ForceStateNormalizedTime(0.0f);
                    playerBikeStatus++;
                }
                else if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.4f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, sitPoint.position, Time.deltaTime * 3.0f);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, sitPoint.rotation, Time.deltaTime * 250.0f);
                }


                break;
            case PlayerBikeStatus.Sit:

                AIContoller.manager.playerCamera.enabled = false;


                AIContoller.manager.vehicleCamera.cameraSwitchView = bikeComponents.cameraViewSetting.cameraViews;
                AIContoller.manager.vehicleCamera.distance = bikeComponents.cameraViewSetting.distance;
                AIContoller.manager.vehicleCamera.height = bikeComponents.cameraViewSetting.height;
                AIContoller.manager.vehicleCamera.Angle = bikeComponents.cameraViewSetting.Angle;


                AIContoller.manager.vehicleCamera.target = handleTrigger.root.transform;
                AIContoller.manager.vehicleCamera.enabled = true;


                m_AIVehicle.vehicleStatus = VehicleStatus.Player;

                if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                {
                    if ((GameControl.manager.controlMode == ControlMode.simple && Input.GetKey(KeyCode.F)) 
                        || (GameControl.manager.controlMode == ControlMode.touch && GameControl.driving == false))
                    {
                        m_Animator.SetFloat("BikeStatus", 3);
                        m_Animator.ForceStateNormalizedTime(0.0f);

                        m_AIVehicle.vehicleStatus = VehicleStatus.EmptyOn;
                        playerBikeStatus++;
                    }

                }


                break;

            case PlayerBikeStatus.GettingOff:

                AIContoller.manager.playerCamera.enabled = true;
                AIContoller.manager.vehicleCamera.enabled = false;

                if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.8f)
                {
                    gettingOnBike = false;
                    transform.parent = null;
                    ComponentsStatus(true);
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                    playerBikeStatus = PlayerBikeStatus.Idle;
                }
                else if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.4f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, handleTrigger.position, Time.deltaTime * 3.0f);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, handleTrigger.rotation, Time.deltaTime * 250.0f);

                    m_Animator.SetBool("DriveBike", false);
                }


                break;
        }



    }


    void Update()
    {
        if (gettingOnCar) GetinCar(); else if (gettingOnBike) GetinBike();
    }


}


















