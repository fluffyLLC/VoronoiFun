using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
//[RequireComponent(typeof(Voronoi))]
public class FractalCoral : MonoBehaviour
{
    public GameObject[] v100Prefabs;
    public GameObject[] v75Prefabs;
    public GameObject[] v50Prefabs;
    public GameObject[] v20Prefabs;

    Voronoi VoronoiMeshes;// = new Voronoi();

    int numItterations = 0;
    public float scaleMod = 0.9f;
    public float startScale = 1;
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

    public void GenerateCoral()
    {
        VoronoiMeshes = GetComponent<Voronoi>();
        scale = startScale;
        numItterations = 0;
        // print(scale);
        List <CombineInstance> meshList = new List<CombineInstance>();
        SpawnCoral(Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale), meshList, 3);
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(meshList.ToArray());

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }



    // Update is called once per frame
    void Update()
    {


    }




    void SpawnCoral(Matrix4x4 spawnPoint, List<CombineInstance> meshList, int stageItterations)
    {
        // print("spawning Coral");

        //Quaternion newRot = Quaternion.Euler(spawnPoint.eulerAngles.x, Random.Range(0, 360), spawnPoint.eulerAngles.z);
        //GameObject g = Instantiate(PickCoral(), spawnPoint.position, spawnPoint.rotation, spawnPoint);
        //g.transform.localScale = new Vector3(scale, scale, scale);

        CombineInstance m = new CombineInstance();

        m.mesh = PickCoral();
        m.transform = spawnPoint;// Matrix4x4.TRS(spawnPoint.position, spawnPoint.rotation, spawnPoint.lossyScale);


        scale *= scaleMod;
        numItterations++;

        Vector3 pos = m.transform.MultiplyPoint(new Vector3(0, 1, 0));
        Quaternion rotation = spawnPoint.rotation * Quaternion.Euler(Random.Range(-30, 30), Random.Range(-30, 30), Random.Range(-30, 30));


        scale *= scaleMod;

        Matrix4x4 t = Matrix4x4.TRS(pos,rotation,spawnPoint.lossyScale*scale);


        float stageScale = scale;

        if (scale > scaleMin)
        {

            //Transform[] snaps = GetSnaps(g);

            for (int i = 0; i < 1; i++)
            {
                if (Random.value > .25f)
                {
                    numItterations = stageItterations;
                    scale = stageScale;
                    // print(scale);
                    SpawnCoral(t,meshList,3);
                }
            }
        }



        //return g;

    }


    Mesh PickCoral()
    {
        //print("picking coral");
        float a = Random.value;
        //GameObject coral;

        if (numItterations == 0)
        {
            return VoronoiMeshes.GetVoronoiMesh(100); //RandomFromArray(v100Prefabs);
        }
        else if (numItterations <= 2)
        {
            if (a >= 0.9)
            {
                return VoronoiMeshes.GetVoronoiMesh(100); RandomFromArray(v75Prefabs);

            }
            else
            {
                return VoronoiMeshes.GetVoronoiMesh(75); RandomFromArray(v100Prefabs);
            }
        }
        else if (numItterations <= 4)
        {

            if (a >= 0.9)
            {

                return VoronoiMeshes.GetVoronoiMesh(75);
            }
            else
            {
                return VoronoiMeshes.GetVoronoiMesh(50);

            }
        }
        else
        {
            return VoronoiMeshes.GetVoronoiMesh(20);

        }

        //print(coral);
        //return coral;
    }

    Transform[] GetSnaps(GameObject prefab)
    {
        TransformRefrence tRef = prefab.GetComponent<TransformRefrence>();
        return tRef.snapPoints;
    }


    GameObject RandomFromArray(GameObject[] array)
    {
        return array[Mathf.RoundToInt(Random.Range(-0.4f, (array.Length - 1) + 0.4f))];
    }
}
