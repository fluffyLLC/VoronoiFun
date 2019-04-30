using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;




   [ExecuteInEditMode]
public class Voronoi : MonoBehaviour {
    [Range(20, 100)] public int numberOfPoints = 40;
    List<DelaunyTriangle> triangles = new List<DelaunyTriangle>();
    List<Cell> cells;
    List<Vert> meshVerts;
    List<MeshTri> cornerTris;
    List<MeshQuad> quads;
    Vector3[] pts;

    Mesh voronoiMesh;

    //TODO: remove this after debug is complete
    //public int numVertList = 0;
    //public List<Vector3> badSite;
    //Vector3 center =  


    int radius = 10;
    public bool useCellRadius = true;
    float cellRadius = .25f;

    public bool drawDelauny = false;
    public bool drawCells = true;
    public bool drawCorners = true;
    public bool drawQuads = true;



    /////////////////////////////////////////////////////////////////////////////////////////////// Functions that are run by the engine

    void Start() {

    }

    void OnValidate() {
        

    }

    public void SaveMesh() {
        print("Mesh Saved");
        AssetDatabase.CreateAsset(voronoiMesh,"Assets");
        AssetDatabase.SaveAssets();

    }

    public void NewMesh() {
        //badSite = new List<Vector3>();
        pts = new Vector3[numberOfPoints];
        cells = new List<Cell>();
        cornerTris = new List<MeshTri>();
        quads = new List<MeshQuad>();

        //loop set the location of a ll the points
        for (int i = 0; i < pts.Length; i++)
        {
            //TODO:scale the mag of these unit vectors
            if (useCellRadius)
            {
                pts[i] = GetRandomPoint() + transform.position;
            }
            else
            {
                pts[i] = Random.onUnitSphere + transform.position;
            }

        }

        //print("pts: " + pts.Length);
        //triangulate the points
        Triangulate(pts);

        //print("DTris: " + triangles.Count);

        triangles = ClearDuplicateTris(triangles);

        //print("nonDTris: " + triangles.Count);

        //triangles = ClearDuplicateTris(triangles);

        //print("nonDTris 2: " + triangles.Count);

        for (int i = pts.Length - 1; i >= 0; i--)
        {

            List<DelaunyTriangle> cellTris = FindTris(pts[i]);
            //print(cellTris.Count);
            cells.Add(new Cell(pts[i], cellTris, OrderedTris(cellTris, pts[i])));
        }

        meshVerts = GenerateVerts();

        GenerateCornerTris();

        GenerateQuads();

        GenerateMesh();


    }

    /// <summary>
    /// Draws a line between each point within the triangles array
    /// </summary>
    void OnDrawGizmos() {

        Vector3 size = Vector3.one * .1f;
        //Gizmos.color = new Color(0,1,0,0.25f);
        if (drawDelauny) {
            foreach (DelaunyTriangle tri in triangles) {
                if (tri.TriContains(pts[0])) {
                    Gizmos.color = Color.green;
                } else {
                    Gizmos.color = Color.white;
                }
                Gizmos.DrawLine(tri.a, tri.b);
                Gizmos.DrawLine(tri.b, tri.c);
                Gizmos.DrawLine(tri.c, tri.a);
            }
        }

        if (drawCells) {
            Gizmos.color = Color.black; //new Color(1, 0, 0, .05f); //Color.red;
                                        //print(testCell.cellTris.Count);

            foreach (Cell c in cells) {
                for (int i = c.orderedVerts.Count - 1; i >= 0; i--) {
                    if (i > 0) {
                        Gizmos.DrawLine(c.orderedVerts[i], c.orderedVerts[i - 1]);
                    } else {
                        Gizmos.DrawLine(c.orderedVerts[i], c.orderedVerts[c.orderedVerts.Count - 1]);
                    }

                }

            }
        }




        //print("Corner Tris:" + cornerTris.Count);
        if (drawCorners) {
            for (int i = cornerTris.Count - 1; i >= 0; i--) {
                //print(verts[0][0].position);
                // print(cornerTris[i].vertisies.Length);
                for (int a = cornerTris[i].vertisies.Length - 1; a >= 0; a--) {

                    //print(cornerTris[i].vertisies.Length);
                    //print(a + ": " + verts[i][a].position);
                    /*
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(verts[i][a].cellVert, verts[i][a].position);
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(verts[i][a].position, verts[i][a].site);
                    */


                    Gizmos.color = Color.green;
                    if (a > 0) {
                        Gizmos.DrawLine(cornerTris[i].vertisies[a].position, cornerTris[i].vertisies[a - 1].position);
                    } else if (a == 0) {
                        Gizmos.DrawLine(cornerTris[i].vertisies[a].position, cornerTris[i].vertisies[cornerTris[i].vertisies.Length - 1].position);
                    }

                }
            }
        }


        if (drawQuads) {
            for (int i = 0; i < quads.Count; i++) {
                Gizmos.color = Color.cyan;
                for (int v = quads[i].a.vertisies.Length - 1; v >= 0; v--) {

                    if (v > 0) {
                        Gizmos.DrawLine(quads[i].a.vertisies[v].position, quads[i].a.vertisies[v - 1].position);
                    } else if (v == 0) {
                        Gizmos.DrawLine(quads[i].a.vertisies[v].position, quads[i].a.vertisies[2].position);
                    }
                }

                Gizmos.color = Color.yellow;
                for (int j = quads[i].b.vertisies.Length - 1; j >= 0; j--) {

                    if (j > 0) {
                        Gizmos.DrawLine(quads[i].b.vertisies[j].position, quads[i].b.vertisies[j - 1].position);
                    } else if (j == 0) {
                        Gizmos.DrawLine(quads[i].b.vertisies[j].position, quads[i].b.vertisies[2].position);
                    }
                }

            }
        }



        /*
            foreach (Cell c in cells) {
            for (int i = c.cellTris.Count - 1; i >= 0; i--) {
                //print("runing");
                for (int j = c.cellTris.Count - 1; j >= 0; j--) {

                    if (c.cellTris[i] == c.cellTris[j]) continue;
                    if (TriAjacent(c.cellTris[i], c.cellTris[j], c.site)) {
                        Gizmos.DrawLine(c.cellTris[i].cicumCenter, c.cellTris[j].cicumCenter);
                    }
                }
            }
        }
        */

        /*
        Gizmos.color = new Color(0, 1, 1, 0.05f);
        for (int i = 0; i < badSite.Count; i++) {
            Gizmos.DrawSphere(badSite[i], 0.1f);
        }
        */
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////Structs

    struct DelaunyTriangle {
        //public int i1;
        //public int i2;
        //public int i3;
        //public int index { get; }
        public Vector3 a { get; }
        public Vector3 b { get; }
        public Vector3 c { get; }
        public Vector3 cicumCenter { get; }
        public float radiusSquared { get; }

        public DelaunyTriangle(Vector3 a, Vector3 b, Vector3 c)//,int index)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            //this.index = index;
            cicumCenter = Circumscribe(a, b, c);
            radiusSquared = (cicumCenter - a).sqrMagnitude;
        }

        public bool CloserThanVerts(Vector3 other) {
            return (cicumCenter - other).sqrMagnitude < radiusSquared;
        }

        /// <summary>
        /// This function checks if a point is present within this delauny tri
        /// </summary>
        /// <param name="pt">The point we are checking against</param>
        /// <returns>true if the point is contained within this tri, false if it is not</returns>
        public bool TriContains(Vector3 pt) {
            if (a == pt) return true;
            if (b == pt) return true;
            if (c == pt) return true;
            return false;
        }

        public bool TriSame(DelaunyTriangle tri) {
            if (a == tri.a && b == tri.b && c == tri.c) return true;
            return false;
        }

        public bool TriEquivelent(DelaunyTriangle tri) {
            if (a == tri.a && b == tri.b && c == tri.c) return true;
            if (a == tri.b && b == tri.c && c == tri.a) return true;
            if (a == tri.c && b == tri.a && c == tri.b) return true;
            if (cicumCenter == tri.cicumCenter) return true;
            return false;
        }

        public static bool operator ==(DelaunyTriangle t1, DelaunyTriangle t2) {
            return t1.Equals(t2);
        }

        public static bool operator !=(DelaunyTriangle t1, DelaunyTriangle t2) {
            return !t1.Equals(t2);
        }

    }

    struct MeshTri {
        public Vert[] vertisies { get; set; }
        public Vector3 center { get; }
        public Vector3 normal { get; set; }
        public Vector2[] UVs { get; }
        public int[] tri { get; }

        public MeshTri(Vert[] vertisies, Vector3 center) {
            this.vertisies = vertisies;
            this.center = center;
            UVs = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
            tri = new int[3];
            normal = Vector3.zero;

            //normal = CalcNormal();
            ArrangeVerts();
        }

        private void ArrangeVerts() {
            Vector3 targetNormal = CalcTargetNormal();
            Vector3 currentNormal = CalcCurrentNormal();
            //print("currentNormal: " + currentNormal);
            //print("targetNormal: " + targetNormal);
            //print("Dot: " + Vector3.Dot(targetNormal, currentNormal));


            if (Vector3.Dot(targetNormal, currentNormal) < 0) {
                vertisies = new Vert[] { vertisies[2], vertisies[1], vertisies[0] };
                normal = CalcCurrentNormal();
            } else {
                normal = currentNormal;
            }
            //TODO: take the dot product of target normal vs current normal, if negative reverse vertisy order
            //TODO: store indexes of tris after the order is set

            tri[0] = vertisies[0].index;
            tri[1] = vertisies[1].index;
            tri[2] = vertisies[2].index;


        }

        private Vector3 CalcCurrentNormal() {
            //Refrence: https://www.khronos.org/opengl/wiki/Calculating_a_Surface_Normal


            Vector3 n;

            Vector3 U = vertisies[1].position - vertisies[0].position;
            Vector3 V = vertisies[2].position - vertisies[0].position;

            n = Vector3.Cross(U, V);

            n.Normalize();

            return n;
        }


        private Vector3 CalcTargetNormal() {
            Vector3 avgPosition = Vector3.zero;

            for (int i = vertisies.Length - 1; i >= 0; i--) {
                avgPosition += vertisies[i].position;
            }

            avgPosition /= vertisies.Length;

            Vector3 n = avgPosition - center;
            //n.Normalize();
            return n;

        }

        public static bool operator ==(MeshTri t1, MeshTri t2) {
            return t1.Equals(t2);
        }

        public static bool operator !=(MeshTri t1, MeshTri t2) {
            return !t1.Equals(t2);
        }

        /*
        public override bool Equals(object obj)
        {
            MeshTri tri = (MeshTri)obj;

            if (this.vertisies.Length != tri.vertisies.Length) return false;

            int numEquals = 0;

            for (int i = 0; i < this.vertisies.Length; i++)
            {
                for (int j = 0; j < tri.vertisies.Length; j++)
                {
                    if (i == j) continue;
                    if (this.vertisies[i] == tri.vertisies[j]) {
                        numEquals++;
                    }
                }
            }

            if (numEquals == this.vertisies.Length) return true;


            return false;
        }
        */


    }


    struct MeshQuad {

        public Vert[] vertisies;
        public Vector3 center;
        public MeshTri a;
        public MeshTri b;

        public MeshQuad(Vert[] vertisies, Vector3 center) {
            this.vertisies = vertisies;
            this.center = center;
            Vert[] aVerts;
            Vert[] bVerts;
            if (vertisies[0].site == vertisies[2].site) {

                aVerts = new Vert[3] { vertisies[0], vertisies[1], vertisies[2] };
                bVerts = new Vert[3] { vertisies[2], vertisies[3], vertisies[1] };
            } else {
                aVerts = new Vert[3] { vertisies[0], vertisies[1], vertisies[3] };
                bVerts = new Vert[3] { vertisies[3], vertisies[2], vertisies[1] };
            }
            a = new MeshTri(aVerts, center);
            b = new MeshTri(bVerts, center);
        }



    }

    /// <summary>
    /// This structure contains all of the information that defines a vert, these vertes are designed to be generated in association with a vornoi cell.
    /// </summary>
    struct Vert {

        /// <summary>
        /// The location of the "cell vert" used to gnerate this verticie. It is used to determin the position of this vert. 
        /// </summary>
        public Vector3 cellVert { get; }
        /// <summary>
        /// The site of the vornoi cell this vert is pointing twords. It is used with cell vert to determin the position of the vert
        /// </summary>
        public Vector3 site { get; }
        /// <summary>
        /// The world space position of this verticy
        /// </summary>
        public Vector3 position { get; set; }
        /// <summary>
        /// This number is set to 0 by default, but should be set manualy upon creation. It is intended to be used to refrence the index of this verticy when generating triangles  
        /// </summary>
        public int index { get; set; }
        /// <summary>
        /// What percent along the vector pointing from cellVert to site the position value position should be set 
        /// </summary>
        static float offset { get { return 0.025f; } }

        public int siteIndex { get; set; }



        public Vert(Vector3 cellVert, Vector3 site, int index, int siteIndex) {
            this.cellVert = cellVert;
            this.site = site;
            position = Vector3.zero;//if we don't set position here it won't let us set it at the end of the function
            this.index = index;
            this.siteIndex = siteIndex;
            position = SetPosition();
        }

        Vector3 SetPosition() {
            Vector3 vecToSite = cellVert - site;
            float p = offset / vecToSite.magnitude;
            return Vector3.Lerp(cellVert, site, p);

        }

        public void PrintAll() {

            //print("Index:" + index);
            //print("Position:" + position);
            //print("CellVert:" + cellVert);
            //print("Site:" + site);
            //print("SiteIndex: " + siteIndex);
            //print("Offset: " + offset);

        }

        /*
        public void SetIndex(int newIndex)
        {
            //print("newIndex: " + newIndex);
            this.index = newIndex;
            // print("Index: " + this.index);
        }*/

        public static bool operator ==(Vert t1, Vert t2) {
            return t1.Equals(t2);
        }

        public static bool operator !=(Vert t1, Vert t2) {
            return !t1.Equals(t2);
        }

        public override bool Equals(object obj) {
            Vert vert = (Vert)obj;

            if (this.position != vert.position) return false;
            if (this.cellVert != vert.cellVert) return false;
            if (this.site != vert.site) return false;

            return true;
        }

    }

    /// <summary>
    /// This struct continains teh data that defines a voranoi cell
    /// </summary>
    struct Cell {

        public List<DelaunyTriangle> cellTris { get; set; }
        public List<Vector3> orderedVerts { get; set; }
        public Vector3 site { get; }


        public Cell(Vector3 site, List<DelaunyTriangle> cellTris, List<Vector3> orderedVerts) : this() {
            this.site = site;
            this.cellTris = cellTris;
            this.orderedVerts = orderedVerts;
        }

    }


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////Logic


    /// <summary>
    /// Delauney triangulation for a set of points
    /// </summary>
    /// <param name="points"></param>
    void Triangulate(Vector3[] points) {
        triangles = new List<DelaunyTriangle>();
        //int calculations = 0;
        foreach (Vector3 p1 in points) {
            //if (TrianglesContains(p1)) continue;
            foreach (Vector3 p2 in points) {
                //if (TrianglesContains(p2)) continue;
                if (p1 == p2) continue;
                foreach (Vector3 p3 in points) {
                    //if (TrianglesContains(p3)) continue;
                    if (p1 == p3) continue;
                    if (p2 == p3) continue;

                    DelaunyTriangle tri = new DelaunyTriangle(p1, p2, p3);
                    bool success = true;
                    foreach (Vector3 p4 in points) {
                        if (p1 == p4) continue;
                        if (p2 == p4) continue;
                        if (p3 == p4) continue;


                        //calculations++;
                        if (tri.CloserThanVerts(p4)) {
                            success = false;
                            break;
                        }
                    }
                    if (success) triangles.Add(tri);
                }
            }
        }
        //print($"triangulization complete after {calculations} calculations");
    }


    List<DelaunyTriangle> ClearDuplicateTris(List<DelaunyTriangle> tris) {
        //print("tris in: " + tris.Count);
        List<DelaunyTriangle> nonDupeTris = new List<DelaunyTriangle>();// = tris;
                                                                        // new List<DelaunyTriangle>();//verts;
                                                                        //int showMeTheDupes = 0;


        for (int i = 0; i < tris.Count; i++) {
            nonDupeTris.Add(tris[i]);
            for (int j = nonDupeTris.Count - 1; j >= 0; j--) {
                if (nonDupeTris[j].TriSame(tris[i])) continue;

                if (nonDupeTris[j].TriEquivelent(tris[i])) {
                    nonDupeTris.RemoveAt(j);
                }
            }
        }

        //print("tris out: " + nonDupeTris.Count);
        return nonDupeTris;

    }

    /*
    bool TrianglesContains(Vector3 pt) {
        for (int i = 0; i < triangles.Count; i++)
        {
            if (triangles[i].TriContains(pt))
            {
                return true;
            }
        }
        return false;
    }*/

    /// <summary>
    /// Returns the circumcenter of 3 vectors. The circumcenter is equidistant from all 3 vectors.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <returns>The position of the circumcenter</returns>
    public static Vector3 Circumscribe(Vector3 a, Vector3 b, Vector3 c) {
        Vector3 result = (c - a).sqrMagnitude * (Vector3.Cross(Vector3.Cross(b - a, c - a), b - a));
        result += (b - a).sqrMagnitude * Vector3.Cross(c - a, Vector3.Cross(b - a, c - a));
        result /= 2 * Vector3.Cross(b - a, c - a).sqrMagnitude;
        result += a;

        return result;
    }

    /// <summary>
    /// This functioin takes a list of Delauny Tris associated with a single site, and returns them so that they are in ajacent order 
    /// </summary>
    /// <param name="tris">the tris to be ordered</param>
    /// <param name="site">the site they all share</param>
    /// <returns> a list where the tris are in ajacent order </returns>
    List<Vector3> OrderedTris(List<DelaunyTriangle> tris, Vector3 site) {
        if (tris.Count == 0) return new List<Vector3>();// { Vector3.zero };
        List<Vector3> orderedVerts = new List<Vector3>();
        List<DelaunyTriangle> orderedTris = new List<DelaunyTriangle>();
        //print("Tris in:" + tris.Count);
        orderedTris.Add(tris[0]);
        //print("Tris in:" + tris.Count);

        for (int i = 0; i <= tris.Count - 1; i++) {

            if (orderedTris.Contains(tris[i])) continue;

            if (TriAjacent(orderedTris[0], tris[i], site)) {
                orderedTris.Insert(0, tris[i]);
                i = 0;
            }
        }

        //print("Tris out: " + orderedTris.Count);


        for (int i = 0; i <= orderedTris.Count - 1; i++) {
            orderedVerts.Add(orderedTris[i].cicumCenter);
        }

        //print("Tris out:" + tris.Count);
        return orderedVerts;
    }


    /// <summary>
    /// This function finds all tris associated with a particular site
    /// </summary>
    /// <param name="site"> The site the tris are associated with </param>
    /// <returns> The list of associated tris </returns>
    List<DelaunyTriangle> FindTris(Vector3 site) {
        List<DelaunyTriangle> tris = new List<DelaunyTriangle>();
        for (int i = 0; i < triangles.Count; i++) {
            if (triangles[i].TriContains(site)) //TriContains(site))
            {
                tris.Add(triangles[i]);
            }
        }
        return tris;
    }

    /// <summary>
    /// This is a recursive function that returns a random point on a unit sphear as long as it is a certin distance from all other points
    /// </summary>
    /// <returns> The list of associated Tris </returns>
    public Vector3 GetRandomPoint() {
        Vector3 pt = Random.onUnitSphere;

        for (int i = pts.Length - 1; i >= 0; i--) {
            if (Vector3.Distance(pts[i], pt) < cellRadius) {
                return GetRandomPoint();
            }
        }

        return pt;
    }

    /// <summary>
    /// This function checks if one tri is ajacent to another by checking if they share a corner vert that is not their shared site
    /// </summary>
    /// <param name="tri1">one of the tris being compared</param>
    /// <param name="tri2">the other tri being compared</param>
    /// <param name="site">the shared site</param>
    /// <returns>true if the tris are ajacent, false if they are not</returns>
    bool TriAjacent(DelaunyTriangle tri1, DelaunyTriangle tri2, Vector3 site) {
        if (tri1.a == site) {
            if (tri1.b == tri2.a) return true;
            if (tri1.b == tri2.b) return true;
            if (tri1.b == tri2.c) return true;
            if (tri1.c == tri2.a) return true;
            if (tri1.c == tri2.b) return true;
            if (tri1.c == tri2.c) return true;
        } else if (tri1.b == site) {
            if (tri1.a == tri2.a) return true;
            if (tri1.a == tri2.b) return true;
            if (tri1.a == tri2.c) return true;
            if (tri1.c == tri2.a) return true;
            if (tri1.c == tri2.b) return true;
            if (tri1.c == tri2.c) return true;
        } else if (tri1.c == site) {
            if (tri1.a == tri2.a) return true;
            if (tri1.a == tri2.b) return true;
            if (tri1.a == tri2.c) return true;
            if (tri1.b == tri2.a) return true;
            if (tri1.b == tri2.b) return true;
            if (tri1.b == tri2.c) return true;
        }
        return false;
    }



    /// <summary>
    /// This function generates a mesh tri for each set of verts that share a common cell-Vert and generates a mesh tri for them. 
    /// </summary>
    void GenerateCornerTris() {
        List<Vector3> CompletedVerts = new List<Vector3>();

        //create a new meshtri for each vert without generating duplicates
        for (int i = 0; i < meshVerts.Count; i++) {
            List<Vert> tempVerts = new List<Vert>();
            tempVerts.Add(meshVerts[i]);
            if (CompletedVerts.Contains(meshVerts[i].cellVert)) continue;

            //if (VertsContains(unsortedVerts[i])) continue;
            for (int a = meshVerts.Count - 1; a >= 0; a--) {
                if (i == a) continue;


                if (meshVerts[i].cellVert == meshVerts[a].cellVert) {
                    bool shouldAdd = true;
                    for (int j = 0; j < tempVerts.Count; j++) {
                        if (tempVerts[j].site == meshVerts[a].site || tempVerts[j].siteIndex == meshVerts[a].siteIndex) {
                            shouldAdd = false;
                            break;
                        }
                    }

                    if (shouldAdd) {
                        tempVerts.Add(meshVerts[a]);
                    }
                }
            }
            CompletedVerts.Add(tempVerts[0].cellVert);
            //tempVerts = ClearDuplicateVerts(tempVerts);

            if (tempVerts.Count < 3) {
                //print("tempVerts Count: " + tempVerts.Count);
                //print("i: " + i);
                //badSite.Add(tempVerts[0].cellVert);
                for (int j = 0; j < tempVerts.Count; j++) {
                    //tempVerts[j].PrintAll();
                }
            }

            cornerTris.Add(new MeshTri(tempVerts.ToArray(), transform.position));
        }
        //cornerTris = ClearDuplicateTris(cornerTris);
    }



    List<Vert> ClearDuplicateVerts(List<Vert> verts) {

        //print("verts in: " + verts.Count);
        //List<Vert> checkVerts = verts;
        List<Vert> nonDupeVerts = new List<Vert>();//verts;
        //int showMeTheDupes = 0;
        nonDupeVerts.Add(verts[verts.Count - 1]);

        //print("List: " + numVertList);

        for (int i = verts.Count - 1; i >= 0; i--) {
            for (int j = nonDupeVerts.Count - 1; j >= 0; j--) {
                //print("Vert: " + i + ", Index: " + verts[i].index);
                if (i == j) continue;
                if (verts[i] != nonDupeVerts[j] && !nonDupeVerts.Contains(verts[i])) {

                    nonDupeVerts.Add(verts[i]);
                }
            }
        }

        /*
        if (nonDupeVerts.Count < 3)
        {
            print(" ");
            print("////////////////////////////////////////////////////////////////////////////////");
            print(" ");
            print("verts in: " + verts.Count);
            print("verts out: " + nonDupeVerts.Count);

            for (int i = verts.Count - 1; i >= 0; i--)
            {

                print(" ");
                print("vert" + i + ": " + "siteIndex" + verts[i].siteIndex);
                

            }
        }
        */

        //numVertList++;
        //print("verts out: " + nonDupeVerts.Count);
        return nonDupeVerts;
    }




    /// <summary>
    /// This function generates a vert for each vert of each cell. 
    /// </summary>
    /// <returns>A list of verts for each vert of each cell</returns>
    List<Vert> GenerateVerts() {
        List<Vert> verts = new List<Vert>();
        int vertCount = 0;

        for (int a = cells.Count - 1; a >= 0; a--) {
            if (cells[a].orderedVerts.Count > 5) {
                //print("a: " + cells[a].orderedVerts.Count);
            }
            for (int i = cells[a].orderedVerts.Count - 1; i >= 0; i--) {

                //Vert vert = new Vert(cells[a].orderedVerts[i], cells[a].site,);
                verts.Add(new Vert(cells[a].orderedVerts[i], cells[a].site, vertCount, a));
                vertCount++;
            }
        }

        /*
        for (int i = 0; i < verts.Count - 1; i++)
        {
            verts[i].SetIndex(i);
        }
        */

        //print("Total Verts:" + verts.Count + ", " + vertCount);
        //print();

        return verts;
    }



    void GenerateQuads() {
        //This a list ov vert arrays
        //each vert array contains two verts that share the same cellVert  
        List<Vert[]> vertPairs = PairVerts();
        List<Vert[]> detectedPairs = new List<Vert[]>();


        for (int a = 0; a < vertPairs.Count; a++) {
            if (detectedPairs.Contains(vertPairs[a])) continue;

            for (int b = 0; b < vertPairs.Count; b++) {
                if (a == b) continue;
                if (detectedPairs.Contains(vertPairs[b])) continue;
                if (CheckForParalell(vertPairs[a], vertPairs[b])) {
                    Vert[] combinedVerts = new Vert[4] { vertPairs[a][0], vertPairs[a][1], vertPairs[b][0], vertPairs[b][1] };
                    quads.Add(new MeshQuad(combinedVerts, transform.position));
                    detectedPairs.Add(vertPairs[b]);
                    detectedPairs.Add(vertPairs[a]);
                    continue;
                }
            }
        }
    }




    private List<Vert[]> PairVerts() {
        List<Vert[]> vertPairs = new List<Vert[]>();

        for (int i = 0; i < cornerTris.Count; i++) {
            Vert[] cv = cornerTris[i].vertisies;
            Vert[] a = new Vert[2] { cv[0], cv[1] };
            Vert[] b = new Vert[2] { cv[1], cv[2] };
            Vert[] c = new Vert[2] { cv[2], cv[0] };

            vertPairs.Add(a);
            vertPairs.Add(b);
            vertPairs.Add(c);
        }

        return vertPairs;
    }

    private bool CheckForParalell(Vert[] a, Vert[] b) {
        Vector3 aOne = a[0].site;
        Vector3 aTwo = a[1].site;
        Vector3 bOne = b[0].site;
        Vector3 bTwo = b[1].site;
        int numSites = 4;

        if (aOne == bOne) numSites--;
        if (aTwo == bTwo) numSites--;
        if (aOne == bTwo) numSites--;
        if (aTwo == bOne) numSites--;

        if (numSites == 2) {
            if (a[0].site == b[0].site && a[1].site == b[1].site) return true;
            if (a[0].site == b[1].site && a[1].site == b[0].site) return true;
        }

        return false;
    }

    //HACK: this function needs to be renamed/might not be nessicary/could be repurposed
    /// <summary>
    /// This function checks if a vert is stored within this function
    /// </summary>
    /// <param name="vert"></param>
    /// <returns></returns>
    bool VertsContains(Vert vert) {
        if (cornerTris.Count == 0) {
            return false;
        }

        for (int i = cornerTris.Count - 1; i >= 0; i--) {
            for (int a = 2; a >= 0; a--) {
                if (cornerTris[i].vertisies[a] == vert) {
                    return true;
                }
            }
        }

        return false;
    }

    void GenerateMesh() {

        //TODO: use the Triangles Built in Logic to set UVs


        Vector3[] finalVerts;



        List<Vector3> tempVertHolder = new List<Vector3>();

        for (int i = 0; i < meshVerts.Count; i++) {
            tempVertHolder.Add(meshVerts[i].position);
        }

        


        finalVerts = tempVertHolder.ToArray();


        List<Vector2> tempUVHolder = new List<Vector2>();
        int a = 0;
        for (int i = 0; i < finalVerts.Length; i++) {

            if (a == 0) {
                tempUVHolder.Add(new Vector2(0,0));
            } else if (a == 1) {
                tempUVHolder.Add(new Vector2(1, 0));
            } else if (a == 2) {
                tempUVHolder.Add(new Vector2(0, 1));
            } else if (a == 3) {
                tempUVHolder.Add(new Vector2(1, 1));
            }

        }

        Vector2[] UVs = tempUVHolder.ToArray(); //new Vector2[finalVerts.Length];

        int[] finalTris;
        //Vector2[] UVs;

        List<int> tempTriHolder = new List<int>();
       
        for (int i = 0; i < cornerTris.Count; i++) {
            tempTriHolder.AddRange(cornerTris[i].tri);
            //tempUVHolder.AddRange(cornerTris[i].UVs);

            //.Add(cornerTris[i].vertisies[0].index);
            //tempTriHolder.Add(cornerTris[i].vertisies[1].index);
            //tempTriHolder.Add(cornerTris[i].vertisies[2].index);


        }

        for (int i = 0; i < quads.Count; i++) {

            tempTriHolder.AddRange(quads[i].a.tri);
            //tempUVHolder.AddRange(quads[i].a.UVs);

            //tempTriHolder.Add(quads[i].a.vertisies[1].index);
            //tempTriHolder.Add(quads[i].a.vertisies[2].index);

            tempTriHolder.AddRange(quads[i].b.tri);
            //tempUVHolder.AddRange(quads[i].b.UVs);

            //tempTriHolder.Add(quads[i].b.vertisies[1].index);
            //tempTriHolder.Add(quads[i].b.vertisies[2].index);

        }

        finalTris = tempTriHolder.ToArray();
       // UVs = tempUVHolder.ToArray();


        

        voronoiMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = voronoiMesh;
        voronoiMesh.vertices = finalVerts;
        voronoiMesh.triangles = finalTris;
        voronoiMesh.uv = UVs;

    }



}


