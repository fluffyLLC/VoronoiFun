using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi : MonoBehaviour {
    [Range(20, 100)] public int numberOfPoints = 20;
    List<DelaunyTriangle> triangles = new List<DelaunyTriangle>();
    List<Cell> cells;
    List<Vert> meshVerts;
    List<MeshTri> cornerTris;
    List<MeshQuad> quads;
    Vector3[] pts;
    //Vector3 center =  


    int radius = 10;
    public bool useCellRadius = true;
    float cellRadius = .25f;

    public bool drawDelauny = false;



    /////////////////////////////////////////////////////////////////////////////////////////////// Functions that are run by the engine

    void Start() {

    }

    void OnValidate() {
        pts = new Vector3[numberOfPoints];
        cells = new List<Cell>();
        cornerTris = new List<MeshTri>();
        quads = new List<MeshQuad>();

        //loop set the location of a ll the points
        for (int i = 0; i < pts.Length; i++) {
            //TODO:scale the mag of these unit vectors
            if (useCellRadius) {
                pts[i] = GetRandomPoint() + transform.position;
            } else {
                pts[i] = Random.onUnitSphere + transform.position;
            }

        }

        //triangulate the points
        Triangulate(pts);

        for (int i = pts.Length - 1; i >= 0; i--) {
            List<DelaunyTriangle> cellTris = FindTris(pts[i]);
            cells.Add(new Cell(pts[i], cellTris, OrderedTris(cellTris, pts[i])));
        }

        meshVerts = GenerateVerts();

        GenerateCornerTris();

        GenerateQuads();

    }

    /// <summary>
    /// Draws a line between each point within the triangles array
    /// </summary>
    void OnDrawGizmos()
    {

        Vector3 size = Vector3.one * .1f;
        if (drawDelauny)
        {
            foreach (DelaunyTriangle tri in triangles)
            {
                if (tri.TriContains(pts[0]))
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.white;
                }
                Gizmos.DrawLine(tri.a, tri.b);
                Gizmos.DrawLine(tri.b, tri.c);
                Gizmos.DrawLine(tri.c, tri.a);
            }
        }


        Gizmos.color = Color.red;
        //print(testCell.cellTris.Count);

        foreach (Cell c in cells)
        {
            for (int i = c.orderedVerts.Count - 1; i >= 0; i--)
            {
                if (i > 0)
                {
                    Gizmos.DrawLine(c.orderedVerts[i], c.orderedVerts[i - 1]);
                }
                else
                {
                    // Gizmos.DrawLine(c.orderedVerts[i], c.orderedVerts[c.orderedVerts.Count - 1]);
                }

            }

        }




        //print("Corner Tris:" + cornerTris.Count);

        for (int i = cornerTris.Count - 1; i >= 0; i--)
        {
            //print(verts[0][0].position);
           // print(cornerTris[i].vertisies.Length);
            for (int a = cornerTris[i].vertisies.Length - 1; a >= 0; a--)
            {
                //print(cornerTris[i].vertisies.Length);
                //print(a + ": " + verts[i][a].position);
                /*
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(verts[i][a].cellVert, verts[i][a].position);
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(verts[i][a].position, verts[i][a].site);
                */


                Gizmos.color = Color.green;
                if (a > 0)
                {
                    Gizmos.DrawLine(cornerTris[i].vertisies[a].position, cornerTris[i].vertisies[a - 1].position);
                }
                else if (a == 0)
                {
                    Gizmos.DrawLine(cornerTris[i].vertisies[a].position, cornerTris[i].vertisies[cornerTris[i].vertisies.Length-1].position);
                }

            }
        }

        
        for (int i = 0; i < quads.Count; i++)
        {
            Gizmos.color = Color.cyan;
            for (int v = quads[i].a.vertisies.Length - 1; v >= 0; v--)
            {

                if (v > 0)
                {
                    Gizmos.DrawLine(quads[i].a.vertisies[v].position, quads[i].a.vertisies[v - 1].position);
                }
                else if (v == 0)
                {
                    Gizmos.DrawLine(quads[i].a.vertisies[v].position, quads[i].a.vertisies[2].position);
                }
            }
            
            Gizmos.color = Color.yellow;
            for (int j = quads[i].b.vertisies.Length - 1; j >= 0; j--)
            {

                if (j > 0)
                {
                    Gizmos.DrawLine(quads[i].b.vertisies[j].position, quads[i].b.vertisies[j - 1].position);
                }
                else if (j == 0)
                {
                    Gizmos.DrawLine(quads[i].b.vertisies[j].position, quads[i].b.vertisies[2].position);
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
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////Structs

    struct DelaunyTriangle {
        //public int i1;
        //public int i2;
        //public int i3;
        public Vector3 a { get; }
        public Vector3 b { get; }
        public Vector3 c { get; }
        public Vector3 cicumCenter { get; }
        public float radiusSquared { get; }

        public DelaunyTriangle(Vector3 a, Vector3 b, Vector3 c) {
            this.a = a;
            this.b = b;
            this.c = c;
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
        public bool TriContains( Vector3 pt) {
            if (a == pt) return true;
            if (b == pt) return true;
            if (c == pt) return true;
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
        public Vert[] vertisies { get; } 
        //public Vector3 normal { get; }
        //public Vector2[] UVs { get; }
       //public Mesh tri { get; }

        public MeshTri(Vert[] vertisies ){
            this.vertisies = vertisies;
        }

        private void CalcNormal() {
            Vector3 center = Vector3.zero;

            for (int i = vertisies.Length - 1; i >= 0; i--) {
                center += vertisies[i].position;
            }

            center /= vertisies.Length;

            //Vector3 n = transform.position - center;
            //n.Normalize();
            //normal = n;

        }

        public static bool operator ==(MeshTri t1, MeshTri t2)
        {
            return t1.Equals(t2);
        }

        public static bool operator !=(MeshTri t1, MeshTri t2)
        {
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
        public MeshTri a;
        public MeshTri b;

        public MeshQuad(Vert[] vertisies) {
            this.vertisies = vertisies;

            Vert[] aVerts;
            Vert[] bVerts;
            if (vertisies[0].site == vertisies[2].site)
            {
                aVerts = new Vert[3] { vertisies[0], vertisies[1], vertisies[2] };
                bVerts = new Vert[3] { vertisies[2], vertisies[3], vertisies[1] };
            }
            else
            {
                aVerts = new Vert[3] { vertisies[0], vertisies[1], vertisies[3] };
                bVerts = new Vert[3] { vertisies[3], vertisies[2], vertisies[1] };
            }
            a = new MeshTri(aVerts);
            b = new MeshTri(bVerts);
        }



    }

    /// <summary>
    /// This structure contains all of the information that defines a vert, these vertes are designed to be generated in association with a vornoi cell.
    /// </summary>
    struct Vert {

        /// <summary>
        /// The location of the "cell vert" used to gnerate this verticie. It is used to determin the position of this vert. 
        /// </summary>
        public Vector3 cellVert { get;  }
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

        public Vert(Vector3 cellVert, Vector3 site) {
            this.cellVert = cellVert;
            this.site = site;
            position = Vector3.zero;//if we don't set position here it won't let us set it at the end of the function
            index = 0;
            position = SetPosition();
        }

        Vector3 SetPosition() {
            Vector3 vecToSite = cellVert - site;
            float p =  offset / vecToSite.magnitude;
            return Vector3.Lerp(cellVert, site, p);
            
        }

        public void SetIndex(int index) {
            this.index = index;
        }

        public static bool operator ==(Vert t1, Vert t2) {
            return t1.Equals(t2);
        }

        public static bool operator !=(Vert t1, Vert t2) {
            return !t1.Equals(t2);
        }

        public override bool Equals(object obj)
        {
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
            foreach (Vector3 p2 in points) {
                if (p1 == p2) continue;
                foreach (Vector3 p3 in points) {
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
        List<Vector3> orderedVerts = new List<Vector3>();
        List<DelaunyTriangle> orderedTris = new List<DelaunyTriangle>();
        orderedTris.Add(tris[0]);
        //print("Tris in:" + tris.Count);
       


        for (int i = 0; i <= tris.Count - 1; i++) {

            if (orderedTris.Contains(tris[i])) continue;

            if (TriAjacent(orderedTris[0], tris[i], site)) {
                orderedTris.Insert(0, tris[i]);
                i = 0;
            }
        }

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
        for (int i = triangles.Count - 1; i >= 0; i--) {
            if (triangles[i].TriContains(site)) {
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
        for (int i = meshVerts.Count - 1; i >= 0; i-- ) {
            List<Vert> tempVerts = new List<Vert>();
            tempVerts.Add(meshVerts[i]);
            if (CompletedVerts.Contains(meshVerts[i].cellVert)) continue; 

            //if (VertsContains(unsortedVerts[i])) continue;
            for (int a = meshVerts.Count - 1; a >= 0; a--) {
                if (i == a) continue;

                if (meshVerts[i].cellVert == meshVerts[a].cellVert ) {
                    //print(unsortedVerts[i].site + ", " + unsortedVerts[a].site);
                    if (meshVerts[i].site != meshVerts[a].site) {

                        tempVerts.Add(meshVerts[a]);

                    }
                }
            }
            CompletedVerts.Add(tempVerts[0].cellVert);
            tempVerts = ClearDuplicateVerts(tempVerts);
            cornerTris.Add(new MeshTri(tempVerts.ToArray()));
        }
        cornerTris = ClearDuplicateTris(cornerTris);
    }


    List<Vert> ClearDuplicateVerts(List<Vert> verts){
        print("verts in: " + verts.Count);
        List<Vert> nonDupeVerts = new List<Vert>();//verts;
        int showMeTheDupes = 0;
        nonDupeVerts.Add(verts[verts.Count-1]);
     
        for (int i = verts.Count - 1; i >= 0; i--)
        {
            for (int j = nonDupeVerts.Count - 1; j >= 0; j--)
            {
                if (i == j) continue;
                if (verts[i] != nonDupeVerts[j] && !nonDupeVerts.Contains(verts[i])){
                    nonDupeVerts.Add(verts[i]);
                }
            }
        }

        print("verts out: " + nonDupeVerts.Count);
        return nonDupeVerts;
    }

    List<MeshTri> ClearDuplicateTris(List<MeshTri> tris) {
        print("tris in: " + tris.Count);
        List<MeshTri> nonDupeTris = new List<MeshTri>();//verts;
        //int showMeTheDupes = 0;
        nonDupeTris.Add(tris[tris.Count - 1]);

        for (int i = tris.Count - 1; i >= 0; i--)
        {
            for (int j = nonDupeTris.Count - 1; j >= 0; j--)
            {
                if (i == j) continue;
                if (tris[i] != nonDupeTris[j] && !nonDupeTris.Contains(tris[i]))
                {
                    nonDupeTris.Add(tris[i]);
                }
            }
        }

        print("tris out: " + nonDupeTris.Count);
        return nonDupeTris;
    }


    /// <summary>
    /// This function generates a vert for each vert of each cell. 
    /// </summary>
    /// <returns>A list of verts for each vert of each cell</returns>
    List<Vert> GenerateVerts() {
        List<Vert> verts = new List<Vert>();
        int vertCount = 0;

        for (int a = cells.Count - 1; a >= 0; a--)
        {
            for (int i = cells[a].orderedVerts.Count - 1; i >= 0; i--)
            {
                //Vert vert = new Vert(cells[a].orderedVerts[i], cells[a].site,);
                verts.Add(new Vert(cells[a].orderedVerts[i], cells[a].site));
                vertCount++;

            }
        }

        for (int i = 0; i < verts.Count - 1; i++) {
            verts[i].SetIndex(i);
        }

        print("Total Verts:" + verts.Count);
        //print(vertCount/cells.Count);
        
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
                if (CheckForParalell(vertPairs[a],vertPairs[b])) {
                    Vert[] combinedVerts = new Vert[4] { vertPairs[a][0], vertPairs[a][1], vertPairs[b][0], vertPairs[b][1]};
                    quads.Add(new MeshQuad(combinedVerts));
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
            Vert[] a = new Vert[2] {cv[0], cv[1]};
            Vert[] b = new Vert[2] {cv[1], cv[2]};
            Vert[] c = new Vert[2] {cv[2], cv[0]};

            vertPairs.Add(a);
            vertPairs.Add(b);
            vertPairs.Add(c);
        }

        return vertPairs;
    }

    private bool CheckForParalell(Vert[] a ,Vert[] b) {
        Vector3 aOne = a[0].site;
        Vector3 aTwo = a[1].site;
        Vector3 bOne = b[0].site;
        Vector3 bTwo = b[1].site;
        int numSites = 4;

        if (aOne == bOne) numSites--;
        if (aTwo == bTwo) numSites--;
        if (aOne == bTwo) numSites--;
        if (aTwo == bOne) numSites--;

        if (numSites == 2)
        {
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

    
    
}
