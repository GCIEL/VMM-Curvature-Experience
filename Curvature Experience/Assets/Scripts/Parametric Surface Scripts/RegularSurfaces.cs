/* Tal Rastopchin
 * June 11, 2019
 * 
 * Implementations of regular surfaces that are both mappable and tessellatable.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Ellipsoid is a IMappable and ITessellatable class that models the ellipsoid
 * as a regular surface.
 * 
 * Properties A, B, and C refer to the constants in the ellipsoid equation given
 * in cartesian coordinates by (x*x)/(a*a) + (y*y)/(b*b) + (z*z)/(c*c) = 1.
 * 
 * Our mapping functions are as defined in the documentation of the IMappable
 * interface; our tessellation functions are as defined in the documentation
 * of the ITessellatable interface.
 * 
 * http://mathworld.wolfram.com/Ellipsoid.html
 */
public class Ellipsoid : RegularSurface
{
    /* Ellipsoid() constructor
     * 
     * Defaults A, B, and C to 1
     */
    public Ellipsoid ()
    {
        A = 1;
        B = 1;
        C = 1;
    }
    /* Ellipsoid() constructor
     * 
     * Sets A, B, and C to a, b, and c respectively
     */
    public Ellipsoid(float a, float b, float c)
    {
        A = a;
        B = b;
        C = c;
    }

    public override Vector3 SurfacePoint(float u, float v)
    {
        float x = A * Mathf.Cos(u) * Mathf.Sin(v);
        float y = B * Mathf.Sin(u) * Mathf.Sin(v);
        float z = C * Mathf.Cos(v);

        return new Vector3(x, z, y);
    }

    public override Vector3 Mapping(Vector2 localPoint)
    {
        return SurfacePoint(localPoint.x, localPoint.y);
    }

    public override Vector2 Pullback(Vector2 localPoint, Vector3 globalTangent)
    {
        float u = localPoint.x;
        float v = localPoint.y;

        float yt = globalTangent.z;
        float zt = globalTangent.y;

        float vt = zt / (-C * Mathf.Sin(v));
        float ut = (yt - (B * Mathf.Sin(u) * Mathf.Cos(v)) * vt) / (B * Mathf.Cos(u) * Mathf.Sin(v));

        return new Vector2(ut, vt);
    }

    public override Vector3 Pushforward(Vector2 localPoint, Vector2 localTangent)
    {
        float u = localPoint.x;
        float v = localPoint.y;

        float ut = localTangent.x;
        float vt = localTangent.y;

        float xt = (-A * Mathf.Sin(u) * Mathf.Sin(v)) * ut + (A * Mathf.Cos(u) * Mathf.Cos(v)) * vt;
        float yt = (B * Mathf.Cos(u) * Mathf.Sin(v)) * ut + (B * Mathf.Sin(u) * Mathf.Cos(v)) * vt;
        float zt = -C * Mathf.Sin(v) * vt;

        return new Vector3(xt, zt, yt);
    }

    public override float Form2e(Vector2 localPoint)
    {
        float u = localPoint.x;
        float v = localPoint.y;

        return A * B * C * Mathf.Pow(Mathf.Sin(v), 2) * Form2Helper(u, v);
    }

    public override float Form2f(Vector2 localPoint)
    {
        return 0;
    }

    public override float Form2g(Vector2 localPoint)
    {
        float u = localPoint.x;
        float v = localPoint.y;

        return A * B * C * Form2Helper(u, v);
    }

    private float Form2Helper(float u, float v)
    {
        float term1 = A * A * B * B * Mathf.Pow(Mathf.Cos(v), 2);
        float term2 = A * A * C * C * Mathf.Pow(Mathf.Sin(u), 2) * Mathf.Pow(Mathf.Sin(v), 2);
        float term3 = B * B * C * C * Mathf.Pow(Mathf.Cos(u), 2) * Mathf.Pow(Mathf.Sin(v), 2);
        return Mathf.Pow(term1 + term2 + term3, -.5f);
    }

    /* The singular points of the ellipsoid in this parameterization are those of the north and
     * south pole. While technically one of the meridians is a line of singular points, due to
     * the wrapping nature of sin and cos we do not have to accound for this in our system of
     * local coordinates. We need to account for the places where the system of local coordinates
     * can be 'sign reversed' (i.e. falling through), and those occur where these lines of singular
     * points intersect, namely, the north and south poles.
     * 
     * The points themselves occur in a radius around (0,0) and (0, PI). If we think about these
     * regions as 'polar caps,' then we can see in local coordinates the regions we are describing
     * are those of ([0, 2PI], 0 + delta, ) and ([0, 2PI], PI + delta) where the delta is the 'radius'
     * of our regions and we have that v can be any value from 0 to 2PI because it is a circular
     * region.
     */
    public override bool InSingularRegion(Vector2 localPoint)
    {
        float delta = .00001f;
        float v = localPoint.y % (Mathf.PI);

        if (v < 0 + delta)
            return true;

        if (v > Mathf.PI - delta)
            return true;

        return false;
    }
}

public class EllipticParaboloid : RegularSurface
{
    private float Form2Helper (Vector2 localPoint)
    {
        float u = localPoint.x;
        float v = localPoint.y;

        return Mathf.Sqrt(A * A * B * B + 2 * u * (A * A + B * B) + 2 * (B * B - A * A) * u * Mathf.Cos(2 * v));
    }
    public override float Form2e(Vector2 localPoint)
    {
        float u = localPoint.x;
        return A * B / (2 * u * Form2Helper(localPoint));
    }

    public override float Form2f(Vector2 localPoint)
    {
        return 0;
    }

    public override float Form2g(Vector2 localPoint)
    {
        float u = localPoint.x;

        return 2 * A * B * u / Form2Helper(localPoint);
    }

    public override bool InSingularRegion(Vector2 localPoint)
    {
        return (localPoint.x < .001f);
    }

    public override Vector3 Mapping(Vector2 localPoint)
    {
        float u = localPoint.x;
        float v = localPoint.y;

        float x = A * Mathf.Sqrt(u) * Mathf.Cos(v);
        float y = B * Mathf.Sqrt(u) * Mathf.Sin(v);
        float z = u;

        return new Vector3(x, z, y);

    }

    public override Vector2 Pullback(Vector2 localPoint, Vector3 globalTangent)
    {
        float u = localPoint.x;
        float v = localPoint.y;

        float xt = globalTangent.x;
        float yt = globalTangent.z;
        float zt = globalTangent.z;

        float ut = zt;
        float vt = (yt - (B * Mathf.Sin(v) / (2 * Mathf.Sqrt(u))) * ut) / (B * Mathf.Sqrt(u) * Mathf.Cos(v));

        return new Vector2(ut, vt);
    }

    public override Vector3 Pushforward(Vector2 localPoint, Vector2 localTangent)
    {
        float u = localPoint.x;
        float v = localPoint.y;

        float ut = localTangent.x;
        float vt = localTangent.y;

        float xt = (A * Mathf.Cos(v) / (2 * Mathf.Sqrt(u))) * ut + (-A * Mathf.Sqrt(u) * Mathf.Sin(v)) * vt;
        float yt = (B * Mathf.Sin(v) / (2 * Mathf.Sqrt(u))) * ut + (B * Mathf.Sqrt(u) * Mathf.Cos(v)) * vt;
        float zt = ut;

        return new Vector3(xt, zt, yt);
    }

    public override Vector3 SurfacePoint(float u, float v)
    {
        return Mapping(new Vector2(u, v));
    }
}



/* OneSheetedHyperboloid is a IMappable and ITessellatable class that models the one-
 * sheeted hyperboloid as a regular surface
 * 
 * Properties A, B, and C refer to the constants in the ellipsoid equation given
 * in cartesian coordinates by (x*x)/(a*a) + (y*y)/(a*a) - (z*z)/(c*c) = 1.
 * 
 * Our mapping functions are as defined in the documentation of the IMappable
 * interface; our tessellation functions are as defined in the documentation
 * of the ITessellatable interface.
 * 
 * http://mathworld.wolfram.com/One-SheetedHyperboloid.html
 */
public class OneSheetedHyperboloid : RegularSurface
{
    public override Vector3 SurfacePoint(float u, float v)
    {
        float x = A * Mathf.Sqrt(1 + u * u) * Mathf.Cos(v);
        float y = A * Mathf.Sqrt(1 + u * u) * Mathf.Sin(v);
        float z = C * u;

        return new Vector3(x, z, y);
    }

    public override Vector3 Mapping(Vector2 localPoint)
    {
        return SurfacePoint(localPoint.x, localPoint.y);
    }

    public override Vector2 Pullback(Vector2 localPoint, Vector3 globalTangent)
    {
        float u = localPoint.x;
        float v = localPoint.y;

        float yt = globalTangent.z;
        float zt = globalTangent.y;

        float term = Mathf.Sqrt(1 + u * u);

        float ut = zt / C;
        float vt = (yt - (A * u * Mathf.Sin(v) / term) * ut) / (A * term * Mathf.Cos(v));

        return new Vector2(ut, vt);
    }

    public override Vector3 Pushforward(Vector2 localPoint, Vector2 localTangent)
    {
        float u = localPoint.x;
        float v = localPoint.y;

        float ut = localTangent.x;
        float vt = localTangent.y;

        float term = Mathf.Sqrt(1 + u * u);

        float xt = (A * u * Mathf.Cos(v) / term) * ut + (-A * term * Mathf.Sin(v)) * vt;
        float yt = (A * u * Mathf.Sin(v) / term) * ut + (A * term * Mathf.Cos(v)) * vt;
        float zt = C * ut;

        return new Vector3(xt, zt, yt);
    }

    public override float Form2e(Vector2 localPoint)
    {
        float u = localPoint.x;
        float v = localPoint.y;

        return (-A * C) / ((u * u + 1) * Mathf.Sqrt(u * u * (A * A + C * C) + C * C));
    }

    public override float Form2f(Vector2 localPoint)
    {
        return 0;
    }

    public override float Form2g(Vector2 localPoint)
    {
        float u = localPoint.x;
        float v = localPoint.y;

        return (A * C * (u * u + 1)) / Mathf.Sqrt(u * u * (A * A + C * C) + C * C);
    }

    public override bool InSingularRegion(Vector2 localPoint)
    {
        throw new System.NotImplementedException();
    }
}
