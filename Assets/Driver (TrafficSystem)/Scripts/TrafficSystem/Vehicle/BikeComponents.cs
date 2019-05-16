using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class BikeComponents : MonoBehaviour
{

    public Transform handleTrigger;
    public Transform sitPoint;
   
    public Transform driver;
    public AudioClip[] deathSoundClips;

    [HideInInspector]
    public bool driving = true;

    public CameraViewSetting cameraViewSetting;


    [System.Serializable]
    public class CameraViewSetting
    {
        public List<Transform> cameraViews;

        public float distance = 5.0f;
        public float height = 1.0f;
        public float Angle = 20;
    }

    void Update()
    {

        if (!driver) return;

        if (driving)
        {
            driver.position = sitPoint.position;
            driver.rotation = sitPoint.rotation;
        }
        else
        {

            Component[] Rigidbodys = driver.GetComponentsInChildren(typeof(Rigidbody));

            foreach (Rigidbody rigidbody in Rigidbodys)
            {
                rigidbody.isKinematic = false;
            }

            Component[] Colliders = driver.GetComponentsInChildren(typeof(Collider));

            foreach (Collider collider in Colliders)
            {
                collider.enabled = true;
            }


            driver.GetComponent<AudioSource>().clip = deathSoundClips[Random.Range(0, deathSoundClips.Length)];
            driver.GetComponent<AudioSource>().Play();

            Destroy(driver.gameObject, 10.0f);
            driver.parent = null;
            driver = null;
        }
    }

}
