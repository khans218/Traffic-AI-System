using UnityEngine;
using System.Collections;
using CnControls;
public class PlayerCamera : MonoBehaviour
{

    public Transform target;
    public LayerMask lineOfSightMask = 0;


    public float smoothTime = 0.15f;
    public float smoothRotate = 0.1f;


    public float xSpeed = 150.0f;
    public float ySpeed = 150.0f;

    public float yMinLimit = -40.0f;
    public float yMaxLimit = 60.0f;

    public float cameraDistance = 2.5f;
    public Vector3 targetOffset = Vector3.zero;

    public bool visibleMouseCursor = true;

   

    [HideInInspector]
    public float x, y, z = 0.0f;

    [HideInInspector]
    public float xSmooth, ySmooth, zSmooth = 0.0f;


    private float xSmooth2 = 0.0f;
    private float ySmooth2 = 0.0f;

    private float distance = 10.0f;

    private float xVelocity = 0.0f;
    private float yVelocity = 0.0f;
    private float zVelocity = 0.0f;

    private float xSmooth2Velocity = 0.0f;
    private float ySmooth2Velocity = 0.0f;

    private Vector3 posVelocity = Vector3.zero;
    private float distanceVelocity = 0.0f;

    private Vector3 targetPos;
    private Quaternion rotation;



    void Start()
    {
        if (visibleMouseCursor) { Cursor.visible = true; } else { Cursor.visible = false; }
        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }



    void LateUpdate()
    {

        if (!target) return;


        Rigidbody controller = target.GetComponent<Rigidbody>();


        if (GameControl.manager.controlMode == ControlMode.simple)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            xSmooth2 = Mathf.SmoothDamp(xSmooth2, Input.GetAxis("Mouse X") / 5, ref xSmooth2Velocity, 0.1f);
            ySmooth2 = Mathf.SmoothDamp(ySmooth2, Input.GetAxis("Mouse Y") / 5, ref ySmooth2Velocity, 0.1f);
        }
        else if (GameControl.manager.controlMode == ControlMode.touch)
        {
            x += CnInputManager.GetAxis("Camera X") * xSpeed * 0.02f;
            y -= CnInputManager.GetAxis("Camera Y") * ySpeed * 0.02f;

            xSmooth2 = Mathf.SmoothDamp(xSmooth2, CnInputManager.GetAxis("Camera X") / 5, ref xSmooth2Velocity, 0.1f);
            ySmooth2 = Mathf.SmoothDamp(ySmooth2, CnInputManager.GetAxis("Camera Y") / 5, ref ySmooth2Velocity, 0.1f);
        }






        y = ClampAngle(y, yMinLimit, yMaxLimit);
        distance = Mathf.SmoothDamp(distance, Mathf.Clamp(y / 30, -100, 0) + cameraDistance, ref distanceVelocity, 0.2f);


        xSmooth = Mathf.SmoothDamp(xSmooth, x + (CameraMotion(2, 1.0f) * controller.velocity.magnitude), ref xVelocity, smoothTime);
        ySmooth = Mathf.SmoothDamp(ySmooth, y + (CameraMotion(2, 0.5f) * controller.velocity.magnitude), ref yVelocity, smoothTime);
        zSmooth = Mathf.SmoothDamp(zSmooth, (CameraMotion(1, 0.5f) * controller.velocity.magnitude), ref zVelocity, smoothTime);



        rotation = Quaternion.Euler(ySmooth, xSmooth, zSmooth);

        targetPos = Vector3.SmoothDamp(targetPos,
        transform.TransformDirection(Mathf.Clamp(xSmooth2, -0.4f, 0.4f), 0, 0)
        + new Vector3(0, targetOffset.y - Mathf.Clamp(ySmooth2, -0.2f, 0.2f)), ref posVelocity, smoothRotate);


        var direction = rotation * -Vector3.forward;


        var targetDistance = AdjustLineOfSight(targetPos + target.position, direction);


        transform.rotation = rotation;
        transform.position = targetPos + target.position + transform.TransformDirection(targetOffset.x, 0, 0.1f) + direction * targetDistance;

    }

    float CameraMotion(float speed, float angle)
    {
        return Mathf.PingPong(Time.time * speed, angle) - angle / 2.0f;
    }

    float AdjustLineOfSight(Vector3 target, Vector3 direction)
    {
        RaycastHit hit;

        if (Physics.Raycast(target, direction, out hit, distance, lineOfSightMask.value))
            return hit.distance;
        else
            return distance;
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
