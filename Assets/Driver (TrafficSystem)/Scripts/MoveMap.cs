using UnityEngine;
using System.Collections;

public class MoveMap : MonoBehaviour {

    public Camera bigMapView;

    private Vector3 offset;
    private Vector3 currentStartPosition;
    private Vector3 curScreenPoint;

    void Start()
    {
        currentStartPosition = bigMapView.transform.position;
    }

    void OnEnable()
    {
        bigMapView.transform.position = new Vector3(GameUI.instPlayerIcon.transform.position.x, currentStartPosition.y, GameUI.instPlayerIcon.transform.position.z);
    }

    void OnMouseDown()
    {
        offset = bigMapView.transform.position;
        curScreenPoint= Vector3.zero;
    }
    
    void OnMouseDrag()
    {
         curScreenPoint += new Vector3(-Input.GetAxis("Mouse X"), 0, -Input.GetAxis("Mouse Y"))*25.0f;

        Vector3 curPosition = offset + curScreenPoint;
        bigMapView.transform.position = curPosition;
    }
}
