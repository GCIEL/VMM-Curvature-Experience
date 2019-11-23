/* Tal Rastopchin
 * June 12, 2019
 * 
 * Implementation of a class used to facilitate the tessellation of parametric
 * surfaces in Unity. Implementation of an interface used to provide the necessary
 * parameterization function for this class.
 * 
 * Adapted from the tessellation project that I completed in high school as well
 * as Christopher French's tesselation implementation.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ParametricSurfaceTessellation is a class that abstracts away the tesselation
 * of parametric surfaces.
 * 
 * This class creates a mesh of a parametric surface via tessellation. This class
 * models parametric surfaces of the form f(u, v) where f maps a tesselated plane
 * in our (u, v) coordinates to our parametric surface in our (x, y, z) coordinates.
 * Our surface parameterization for the tessellation is provided by the tessellation
 * object. We specify the boundaries of the initial (u, v) plane with the uMin, uMax,
 * vMin, and vMax parameters. We specify how "detailed" the plane is along each axis
 * with the uRes and vRes parameters. The reverseOrientation field, when true, will
 * reverse the order that triangles are specified, effectively reversing the
 * "orientation" of the resulting mesh.
 * 
 * TODO We can extend the functionality of this class by having turning the
 * tesselated (u, v) plane into a tesselated (u, v) cylinder by "wrapping" around
 * the last set of triangles along each axis. This would allow for the smooth
 * closing of seams on parametric surfaces like ellipsoids and toroids.
 */
public class ParametricSurfaceTessellation
{
    // our tessellation parameterization functions
    private readonly ITessellatable tessellation;

    // mesh to store tessellation
    private readonly Mesh mesh;

    // relevant parametric surface tessellation parameters
    private int uRes = 1;
    private int vRes = 1;

    private float uMin = 0;
    private float uMax = 1;
    private float vMin = 0;
    private float vMax = 1;

    private bool reverseOrientation = false;

    /* URes and VRes getter and setter properties
     */
    public int URes
    {
        get { return uRes; }
        set
        {
            if (value > 0)
                uRes = value;
            else
                uRes = 1;
        }
    }
    public int VRes
    {
        get { return vRes; }
        set
        {
            if (value > 0)
                vRes = value;
            else
                vRes = 1;
        }
    }

    /* UMin, UMax, VMin, VMax getter and setter properties
     */
    public float UMin
    {
        get { return uMin; }
        set { uMin = value; }
    }
    public float UMax
    {
        get { return uMax; }
        set { uMax = value; }
    }
    public float VMin
    {
        get { return vMin; }
        set { vMin = value; }
    }
    public float VMax
    {
        get { return vMax; }
        set { vMax = value; }
    }

    /* ReverseOrientation getter and setter property
     */
    public bool ReverseOrientation
    {
        get
        {
            return reverseOrientation;
        }
        set
        {
            reverseOrientation = value;
        }
    }

    /* ParametricSurfaceTessellation() constructor
     * 
     * Constructs a new ParametricSurfaceTessellation with the given tessellation.
     * Initializes the mesh and marks it as a "dynamic mesh" for Unity so that
     * it is optimized for frequent changes.
     */
    public ParametricSurfaceTessellation(ITessellatable tessellation)
    {
        this.tessellation = tessellation;

        mesh = new Mesh();
        mesh.MarkDynamic();
    }

    /* SetParameters() sets the relevant parametric surface tesselation parameters
     * 
     * The uRes and vRes parameters denote the amount of subdivisions along each
     * of the u and v axes respectively. The uMin and uMax parameters denote the
     * boundary along the u axis, and the vMin and vMax parameters denote the
     * boundary along the v axis.   
     */
    public void SetParameters(int uRes, int vRes, float uMin, float uMax, float vMin, float vMax, bool reverseOrientation)
    {
        this.uRes = uRes;
        this.vRes = vRes;
        this.uMin = uMin;
        this.uMax = uMax;
        this.vMin = vMin;
        this.vMax = vMax;
        this.reverseOrientation = reverseOrientation;
    }

    /* GetMesh() returns a reference to the mesh
     */
    public Mesh GetMesh()
    {
        return mesh;
    }

    /* Update() tessellates the mesh according to the given parameters
     * 
     * We generate the mesh via the tesselation process as follows:
     *    
     *  1)  First, we assert our uRes and vRes are greater than 1
     *    
     *  2)  Then, we clear the mesh data
     *    
     *  3)  Then, we calculate the total number of vertices and triangles that we
     *      will have to allocate arrays for. Sinc a uRes of 1 will create two
     *      vertices along the u axis, the total number of vertices in our
     *      subdivided plane is (uRes + 1) * (vRes + 1). Since we have uRes * vRes
     *      quads on our plane, 2 triangles a quad, and 3 indices a triangle, we
     *      allocate space for 2 * 3 * uRes * vRes triangle indices.
     * 
     *  4)  We next give ourselves indices for counting within our arrays, as well
     *      as calculate the width and height of each "rectangular peice" of our
     *      subdivided (u, v) plane. We call these deltas.
     * 
     *  5) After all that setup, we proceed to tessellate the mesh as follows:
     *    
     *      a.  We use indices i and j to calculate the position of the lower
     *          left corner of each "rectangular peice." The position of this
     *          corner in our local coordinates is (u, v).
     * 
     *      b.  We add our SurfacePoint(u, v) to the vertices array, incrementing
     *          the vertexIndex accordingly
     * 
     *      c.  For each vertex that we add to the vertex array, we add the two
     *          corresponding triangles that form the "rectangular peice" emanating
     *          from (u, v) as the lower left corner of that rectangular peice.
     * 
     *  6)  Finally, we set the new vertices and triangles and recalculate the mesh
     *      normals.
     * 
     * Notice that even though we index vertices that do not necessarily exist in
     * the newVertices array, because we do not add triangles when i < uRes and
     * j < uRes, we perfectly over our parametric surface with triangles.
     */
    public void Tessellate()
    {
        // return if params not good
        if (uRes < 1 || vRes < 1)
            return;

        // clear mesh vertex and triangle data
        this.mesh.Clear();

        // generate "rectangular" uv plane vertices and triangle indices
        int numVertices = (uRes + 1) * (vRes + 1);
        int numTriangles = 2 * 3 * uRes * vRes;
        Vector3[] newVertices = new Vector3[numVertices];
        Vector2[] newUVs = new Vector2[numVertices];
        int[] newTriangles = new int[numTriangles];

        // indices for array computations
        int vertexIndex = 0;
        int triangleIndex = 0;

        // compute deltas
        float uDelta = (uMax - uMin) / uRes;
        float vDelta = (vMax - vMin) / vRes;

        // actual mesh tesselation
        for (int i = 0; i < uRes + 1; i++)
        {
            for (int j = 0; j < vRes + 1; j++)
            {
                // compute (u, v)
                float u = uMin + uDelta * i;
                float v = vMin + vDelta * j;

                // add new vertex and texture coordinate
                newVertices[vertexIndex] = tessellation.SurfacePoint(u, v);
                newUVs[vertexIndex] = new Vector2((float)(i) / uRes, (float)(vRes - j) / vRes);
                vertexIndex++;

                // add two corresponding triangles for each quad
                if (i < uRes && j < vRes)
                {
                    if (reverseOrientation)
                    {
                        newTriangles[triangleIndex + 0] = TriangleIndex(0, i, j);
                        newTriangles[triangleIndex + 1] = TriangleIndex(1, i, j);
                        newTriangles[triangleIndex + 2] = TriangleIndex(2, i, j);

                        newTriangles[triangleIndex + 3] = TriangleIndex(3, i, j);
                        newTriangles[triangleIndex + 4] = TriangleIndex(2, i, j);
                        newTriangles[triangleIndex + 5] = TriangleIndex(1, i, j);
                    }
                    else
                    {
                        newTriangles[triangleIndex + 0] = TriangleIndex(0, i, j);
                        newTriangles[triangleIndex + 1] = TriangleIndex(2, i, j);
                        newTriangles[triangleIndex + 2] = TriangleIndex(1, i, j);

                        newTriangles[triangleIndex + 3] = TriangleIndex(3, i, j);
                        newTriangles[triangleIndex + 4] = TriangleIndex(1, i, j);
                        newTriangles[triangleIndex + 5] = TriangleIndex(2, i, j);
                    }

                    triangleIndex += 6;
                }
            }
        }

        this.mesh.vertices = newVertices;
        this.mesh.triangles = newTriangles;
        this.mesh.uv = newUVs;
        this.mesh.RecalculateNormals();
    }

    /* TriangleIndex() computes the local triangle index for the given quad
     * 
     * We assume that we are following the tessellation algorithm described in the
     * comments of the Tessellate() function. Then, we use the given (i, j) indices
     * to compute the triangle indices for the quad generated by a given vertex.
     * 
     * 0 : returns lower left corner index
     * 1 : returns upper left corner index
     * 2 : returns lower right corner index
     * 3 : returns upper right corner index
     */
    private int TriangleIndex(int triangleIndex, int i, int j)
    {
        switch (triangleIndex)
        {
            case 0:
                return (vRes + 1) * i + j;
            case 1:
                return (vRes + 1) * i + (j + 1);
            case 2:
                return (vRes + 1) * (i + 1) + j;
            case 3:
                return (vRes + 1) * (i + 1) + (j + 1);
            default:
                Debug.LogError("Invalid triangle index. Triangle index must be between 0 and 3 inclusive.");
                return -1;
        }
    }
}

/* ITessellatable is an interface that is used to provide an object with the needed functionality
 * in order to be used as the tessellation field within a ParametricSurfaceTessellation object.
 * Namely, this interface requires implementing SurfacePoint function.
 */
public interface ITessellatable
{
    /* SurfacePoint() gives us our parametric surface point
     * 
     * Given (u, v) in R2, this function gives us the corresponding point
     * (x(u,v), y(u,v), z(u,v) in R3   
     */
    Vector3 SurfacePoint(float u, float v);
}