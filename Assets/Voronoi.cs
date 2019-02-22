using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi : MonoBehaviour {
    [Range(20, 100)] public int numberOfPoints = 20;
    List<Triangle> triangles = new List<Triangle>();
    List<Cell> cells = new List<Cell>();
    Vector3[] pts;



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

        for (int i = pts.Length-1; i >= 0; i--) {
            List<Triangle> cellTris = FindTris(pts[i]);
            cells.Add(new Cell(pts[i], cellTris));
        }
        
    }

    struct Triangle {
        //public int i1;
        //public int i2;
        //public int i3;
        public Vector3 a { get; }
        public Vector3 b { get; }
        public Vector3 c { get; }
        public Vector3 cicumCenter { get; }
        public float radiusSquared { get; }

        public Triangle(Vector3 a, Vector3 b, Vector3 c) {
            this.a = a;
            this.b = b;
            this.c = c;
            cicumCenter = Circumscribe(a, b, c);
            radiusSquared = (cicumCenter - a).sqrMagnitude;
        }

        public bool CloserThanVerts(Vector3 other) {
            return (cicumCenter - other).sqrMagnitude < radiusSquared;
        }

        public static bool operator ==(Triangle t1, Triangle t2) {
            return t1.Equals(t2);
        }

        public static bool operator !=(Triangle t1, Triangle t2) {
            return !t1.Equals(t2);
        }

    }

    struct Cell {

        public List<Triangle> cellTris { get; set; }
        public Vector3 site { get; }

        public Cell(Vector3 site, List<Triangle> cellTris) : this() {
            this.site = site;
            this.cellTris = cellTris;
        }

    }




    /// <summary>
    /// Delauney triangulation for a set of points
    /// </summary>
    /// <param name="points"></param>
    void Triangulate(Vector3[] points) {
        triangles = new List<Triangle>();
        int calculations = 0;
        foreach (Vector3 p1 in points) {
            foreach (Vector3 p2 in points) {
                if (p1 == p2) continue;
                foreach (Vector3 p3 in points) {
                    if (p1 == p3) continue;
                    if (p2 == p3) continue;

                    Triangle tri = new Triangle(p1, p2, p3);
                    bool success = true;
                    foreach (Vector3 p4 in points) {
                        if (p1 == p4) continue;
                        if (p2 == p4) continue;
                        if (p3 == p4) continue;
                        calculations++;
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
        /*
        int[] result = new int[triangles.Count * 3];
        int i = 0;
        foreach(Triangle tri in triangles)
        {
            result[i++] = tri.i1;
            result[i++] = tri.i2;
            result[i++] = tri.i3;
        }
        return result;
        */
    }

    /// <summary>
    /// Draws a line between each point within the triangles array
    /// </summary>
    void OnDrawGizmos() {

        Vector3 size = Vector3.one * .1f;
        if (drawDelauny) {
            foreach (Triangle tri in triangles) {
                if (TriContains(tri, pts[0])) {
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



    List<Triangle> FindTris(Vector3 site) {
        List<Triangle> tris = new List<Triangle>();
        for (int i = triangles.Count - 1; i >= 0; i--) {
            if (TriContains(triangles[i], site)) {
                tris.Add(triangles[i]);
            }
        }
        return tris;
    }


    public Vector3 GetRandomPoint() {
        Vector3 pt = Random.onUnitSphere;
        for (int i = pts.Length - 1; i >= 0; i--) {
            if (Vector3.Distance(pts[i], pt) < cellRadius) {
                return GetRandomPoint();
            }
        }
        return pt;
    }

    bool TriAjacent(Triangle tri1, Triangle tri2, Vector3 site) {
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


    bool TriContains(Triangle tri, Vector3 pt) {
        if (tri.a == pt) return true;
        if (tri.b == pt) return true;
        if (tri.c == pt) return true;
        return false;
    }
}
