using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
public class VehicleCamera : MonoBehaviour
{

    public Transform target;

    public float smooth = 0.3f;
    public float distance = 5.0f;
    public float height = 1.0f;
    public float Angle = 20;

    public LayerMask lineOfSightMask = 0;

    public List<Transform> cameraSwitchView;

    [HideInInspector]
    public int Switch;

    private float yVelocity = 0.0f;
    private float xVelocity = 0.0f;


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

 
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.C))
        {
            Switch++;
            if (Switch > cameraSwitchView.Count) { Switch = 0; }
        }

        if (Switch == 0)
        {
            // Damp angle from current y-angle towards target y-angle

            float xAngle = Mathf.SmoothDampAngle(transform.eulerAngles.x,
           target.eulerAngles.x + Angle, ref xVelocity, smooth);

            float yAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y,
            target.eulerAngles.y, ref yVelocity, smooth);

            // Look at the target
            transform.eulerAngles = new Vector3(xAngle, yAngle,0.0f);

            var direction = transform.rotation * -Vector3.forward;
            var targetDistance = AdjustLineOfSight(target.position + new Vector3(0, height, 0), direction);


            transform.position = target.position + new Vector3(0, height, 0) + direction * targetDistance;
        }
        else
        {

            transform.position = cameraSwitchView[Switch - 1].position;
            transform.rotation = Quaternion.Lerp(transform.rotation, cameraSwitchView[Switch - 1].rotation, Time.deltaTime * 5.0f);

        }

    }

    float AdjustLineOfSight(Vector3 target, Vector3 direction)
    {
        RaycastHit hit;

        if (Physics.Raycast(target, direction, out hit, distance, lineOfSightMask.value))
            return hit.distance;
        else
            return distance;
    }

}
