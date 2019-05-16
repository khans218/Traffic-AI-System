using UnityEngine;
using System.Collections;

public class DestroyGameObject : MonoBehaviour
{

    public float clearDistance = 150.0f;
    public GameObject myRoot;
    public Renderer myBody;

    public bool human, vehicle;
    void Update()
    {

        if (!AIContoller.manager.player) return;

        if (Vector3.Distance(transform.position, AIContoller.manager.player.transform.position) > clearDistance && !myBody.isVisible)
        {
            Destroy(myRoot);

            if (human) AIContoller.manager.currentHumans--;
            if (vehicle) AIContoller.manager.currentVehicles--;
            

        }

    }
}
