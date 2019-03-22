using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi : MonoBehaviour {
    [Range(20, 100)] public int numberOfPoints = 20;
    List<DelaunyTriangle> triangles = new List<DelaunyTriangle>();
    List<Cell> cells;
    List<Vert> meshVerts;
    List<MeshTri> cornerTris = new List<MeshTri>();
    Vector3[] pts;
    //Vector3 center =  


    int radius = 10;
    public bool useCellRadius = true;
    float cellRadius = .25f;

    public bool drawDelauny = false;

    void Start() {

    }

    void OnValidate() {
        pts = new Vector3[numberOfPoints];


        //loop set the location of a ll the points
        for (int i = 0; i < pts.Length; i++) {
            //TODO:scale the mag of these unit vectors
            if (useCellRadius) {
                pts[i] = GetRandomPoint();
            } else {
                pts[i] = Random.onUnitSphere;
            }

        }

        //triangulate the points
        Triangulate(pts);

        cells = new List<Cell>();

        for (int i = pts.Length - 1; i >= 0; i--) {
            List<DelaunyTriangle> cellTris = FindTris(pts[i]);
            cells.Add(new Cell(pts[i], cellTris, OrderedTris(cellTris, pts[i])));
        }

        cornerTris = new List<MeshTri>();

        meshVerts = GenerateVerts();

        GenerateCornerTris();

    }

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

            //Vector3 n = traansform.position - center;
            //n.Normalize();
            //normal = n;

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
        private float offset { get { return 0.025f; } }

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
    /// Draws a line between each point within the triangles array
    /// </summary>
    void OnDrawGizmos() {

        Vector3 size = Vector3.one * .1f;
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


        Gizmos.color = Color.red;
        //print(testCell.cellTris.Count);

        foreach (Cell c in cells) {
            for (int i = c.orderedVerts.Count - 1; i >= 0; i--) {
                if (i > 0) {
                    Gizmos.DrawLine(c.orderedVerts[i], c.orderedVerts[i - 1]);
                } else {
                    // Gizmos.DrawLine(c.orderedVerts[i], c.orderedVerts[c.orderedVerts.Count - 1]);
                }

            }

        }

        
        for (int i = cornerTris.Count-1; i >= 0; i--) {
            //print(verts[0][0].position);
            
            for (int a = cornerTris[i].vertisies.Length-1; a >= 0; a--) {
                //print(a + ": " + verts[i][a].position);
                /*
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(verts[i][a].cellVert, verts[i][a].position);
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(verts[i][a].position, verts[i][a].site);
                */


                Gizmos.color = Color.green;
                if (a > 0 ) {
                    Gizmos.DrawLine(cornerTris[i].vertisies[a].position, cornerTris[i].vertisies[a - 1].position);
                } else if ( a == 0) {
                    Gizmos.DrawLine(cornerTris[i].vertisies[a].position, cornerTris[i].vertisies[2].position);
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
        }*/
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

        //create a new meshtri for each vert without generating duplicates
        for (int i = meshVerts.Count - 1; i >= 0; i-- ) {
            List<Vert> tempVerts = new List<Vert>();

            //if (VertsContains(unsortedVerts[i])) continue;
            for (int a = meshVerts.Count - 1; a >= 0; a--) {
                
                if (meshVerts[i].cellVert == meshVerts[a].cellVert ) {
                    //print(unsortedVerts[i].site + ", " + unsortedVerts[a].site);
                    if (meshVerts[i].site != meshVerts[a].site) {
                        
                        if (tempVerts.Count == 0) {
                            //print("r");
                            tempVerts.Add(meshVerts[i]);
                            tempVerts.Add(meshVerts[a]);
                        } else { // if (verts[2] == null) {
                            tempVerts.Add(meshVerts[a]);
                        }
                    }
                }
            }

            cornerTris.Add(new MeshTri(tempVerts.ToArray()));
        }
    }


    /// <summary>
    /// This function generates a vert for each vert of each cell. 
    /// </summary>
    /// <returns>A list of verts for each vert of each cell</returns>
    List<Vert> GenerateVerts() {
        List<Vert> verts = new List<Vert>();

        for (int a = cells.Count - 1; a >= 0; a--)
        {
            for (int i = cells[a].orderedVerts.Count - 1; i >= 0; i--)
            {
                //Vert vert = new Vert(cells[a].orderedVerts[i], cells[a].site,);
                verts.Add(new Vert(cells[a].orderedVerts[i], cells[a].site));

            }
        }

        for (int i = 0; i < verts.Count - 1; i++) {
            verts[i].SetIndex(i);
        } 

        return verts;
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
