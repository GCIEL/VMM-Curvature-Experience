/* Tal Rastopchin
 * June 20, 2019
 * 
 * Implementation of a script component that creates a mesh from a tessellation
 * 
 * TODO: call the Tessellate() method only when relevant tessellation parameters
 * change
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* TessellationObject creates a mesh from a given ParametricSurfaceTessellation object
 */

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TessellationObject: MonoBehaviour
{
    public GameObject surfaceController;

    private ParametricSurfaceTessellation tessellation;

    public int uRes = 1;
    public int vRes = 1;
    public float uMin = 0;
    public float uMax = 1;
    public float vMin = 0;
    public float vMax = 1;
    public bool reverseOrientation = false;

    // Start is called before the first frame update
    void Start()
    {
        RegularSurface surface = surfaceController.GetComponent<SurfaceController>().GetSurface();
        tessellation = new ParametricSurfaceTessellation(surface);
        GetComponent<MeshFilter>().mesh = tessellation.GetMesh();

        tessellation.SetParameters(uRes, vRes, uMin, uMax, vMin, vMax, reverseOrientation);
        tessellation.Tessellate();

        gameObject.AddComponent<MeshCollider>();

        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        tessellation.SetParameters(uRes, vRes, uMin, uMax, vMin, vMax, reverseOrientation);
        tessellation.Tessellate();
        */
    }
}
