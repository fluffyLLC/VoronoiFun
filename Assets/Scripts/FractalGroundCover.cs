﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FractalGroundCover : MonoBehaviour
{

    /*
    struct PrefabContainer
    {
        public Transform[] snapPoints { get; }
        public Transform meshPivot { get; }
        public Transform prefabPivot { get; }

        public PrefabContainer(Transform[] snapPoints, Transform meshPivot, Transform prefabPivot)
        {
            this.snapPoints = snapPoints;
            this.meshPivot = meshPivot;
            this.prefabPivot = prefabPivot;
        }


    }
    */

    public GameObject[] v100Prefabs;
    public GameObject[] v75Prefabs;
    public GameObject[] v50Prefabs;
    public GameObject[] v20Prefabs;

    int numItterations = 0;
    public float scaleMod = 0.9f;
    public float startScale = 2;
    float scale;
    public float scaleMin = 0.25f;

    /*
    PrefabContainer[] v100_transforms;
    PrefabContainer[] v75_transforms;
    PrefabContainer[] v50_transforms;
    PrefabContainer[] v20_transforms;
    */

    // Start is called before the first frame update
    void Start()
    {
        //scale = startScale;
        //SpawnCoral(transform);
    }

    public void GenerateCoral() {
        scale = startScale;
        numItterations = 0;
       // print(scale);
        SpawnCoral(transform);
    }



    // Update is called once per frame
    void Update()
    {


    }




    void SpawnCoral(Transform spawnPoint)
    {
       // print("spawning Coral");

        //Quaternion newRot = Quaternion.Euler(spawnPoint.eulerAngles.x, Random.Range(0, 360), spawnPoint.eulerAngles.z);
        GameObject g = Instantiate(PickCoral(), spawnPoint.position, spawnPoint.rotation, spawnPoint);
        g.transform.localScale = new Vector3(scale,scale,scale);

        scale *= scaleMod;
        numItterations++;

        float stageScale = scale;
        int stageItterations = numItterations;

        if (scale > scaleMin) {
           
            Transform[] snaps = GetSnaps(g);

            for (int i = 0; i < snaps.Length; i++) {
                if (Random.value > .25f)
                {
                    numItterations = stageItterations;
                    scale = stageScale;
                   // print(scale);
                    SpawnCoral(snaps[i]);
                }
            }
        }
       


        //return g;

    }


    GameObject PickCoral()
    {
        //print("picking coral");
        float a = Random.value;
        //GameObject coral;

        if (numItterations == 0)
        {
            return RandomFromArray(v100Prefabs);
        }
        else if (numItterations <= 2)
        {
            if (a >= 0.75)
            {
                return RandomFromArray(v75Prefabs);

            }
            else {
                return RandomFromArray(v100Prefabs);
            }
        }
        else if (numItterations <= 4)
        {

            if (a >= 0.75)
            {

                return RandomFromArray(v50Prefabs);
            }
            else {
                return RandomFromArray(v75Prefabs);

            }
        }
        else
        {
            return RandomFromArray(v20Prefabs);

        }

        //print(coral);
        //return coral;
    }

    Transform[] GetSnaps(GameObject prefab) {
        TransformRefrence tRef = prefab.GetComponent<TransformRefrence>();
        return tRef.snapPoints;
    }


    GameObject RandomFromArray(GameObject[] array) {
        return array[Mathf.RoundToInt(Random.Range(-0.4f,(array.Length-1)+0.4f))]; 
    }


    /*
    PrefabContainer[] SetPrefabContainer(GameObject[] prefabs) {
        PrefabContainer[] transforms = new PrefabContainer[prefabs.Length];



    }
    */

}
