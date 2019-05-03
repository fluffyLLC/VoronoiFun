using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawTransformSphear : MonoBehaviour
{
    [Range(0.05f,0.25f)]
    public float radius = 0.05f;
    public Color color = new Color(0,0,1,0.25f);
    public Transform target;
    public static bool draw = true;


    void OnDrawGizmos() {
        if (draw)
        {
            Gizmos.color = color;
            if (target != null)
            {
                Gizmos.DrawSphere(target.position, radius);
            }
            else
            {
                Gizmos.DrawSphere(transform.position, radius);
            }
        }
    }

        /*
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
