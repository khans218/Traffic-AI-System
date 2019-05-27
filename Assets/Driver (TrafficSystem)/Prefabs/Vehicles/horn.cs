using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class horn : MonoBehaviour {
    AIVehicle target;

    // Update is called once per frame
    void Update () {
        if (Input.GetKey("h"))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 8.0f))
            {
                Transform other = hit.collider.transform;
                while (other.parent != null)
                {
                    other = other.parent;
                }
                if (other.name == "AIVehicle")
                {
                    target = other.GetComponent<AIVehicle>();
                    target.moveAside = true;
                }
            } else
            {
                if (target != null)
                {
                    target.moveAside = false;
                }
            }
        } else
        {
            if (target != null)
            {
                target.moveAside = false;
            }
        }
	}
}
