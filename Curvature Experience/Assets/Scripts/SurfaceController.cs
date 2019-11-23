using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceController : MonoBehaviour
{
    public float a;
    public float b;
    public float c;

    private RegularSurface surface;

    public enum Surface {
        Ellipsoid,
        EllipticParaboloid,
        TwoSheetedHyperboloid
    }

    public Surface mySurface;

    // Start is called before the first frame update
    void Start()
    {
        if (mySurface == Surface.Ellipsoid)
        {
            surface = new Ellipsoid();
        }
        else if (mySurface == Surface.EllipticParaboloid)
        {
            surface = new EllipticParaboloid();
        }



        surface.A = a;
        surface.B = b;
        surface.C = c;
    }

    // Update is called once per frame
    void Update()
    {
        surface.A = a;
        surface.B = b;
        surface.C = c;
    }

    public RegularSurface GetSurface()
    {
        return surface;
    }
}
