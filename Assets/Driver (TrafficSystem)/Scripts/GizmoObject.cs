using UnityEngine;
using System.Collections;

public enum GizmoShape {Cube=0,Sphere=1,}
public class GizmoObject : MonoBehaviour
{

    public GizmoShape gizmoShape;
    public Color gizmoColor = Color.white;
    public float gizmoSize = 1.0f;

    public bool wireMode = false;
    public bool drawRay = false;
    public float rayLength = 2.0f;

    void OnDrawGizmos()
    {

        Gizmos.color = gizmoColor;

        if (drawRay)
        {
            Vector3 direction = transform.TransformDirection(Vector3.fwd);
            Gizmos.DrawRay(transform.position, direction * rayLength);
        }

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.matrix = rotationMatrix;

        switch (gizmoShape)
        {
            case GizmoShape.Cube:

                if (wireMode)
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one * gizmoSize);
                else
                    Gizmos.DrawCube(Vector3.zero, Vector3.one * gizmoSize);

                break;
            case GizmoShape.Sphere:

                if (wireMode)
                    Gizmos.DrawWireSphere(Vector3.zero, gizmoSize);
                else
                    Gizmos.DrawSphere(Vector3.zero, gizmoSize);

                break;
        }
    }
}
