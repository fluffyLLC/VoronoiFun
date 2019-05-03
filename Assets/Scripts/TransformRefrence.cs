using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformRefrence : MonoBehaviour
{

    public Transform[] snapPoints;
    public Transform meshTransform;
    Transform baseTransform;

    // Start is called before the first frame update
    void Start()
    {
        baseTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
