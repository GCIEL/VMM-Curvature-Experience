/* Tal Rastopchin
 * July 16, 2019
 * 
 * RegularSurface is a class that we derive from when we want to create a new
 * surface for the player to explore
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* RegularSurface is an abstract class that models a parameterized regular surface
 * within Unity
 * 
 * RegularSurface implements IMappable and ITessellatable, as well as provides three
 * constants that can affect our underlying parameterizations.
 */
public abstract class RegularSurface : IMappable, ITessellatable
{
    /* Getter and setter properties for generic constants A, B, and C
     */
    public float A { get; set; }
    public float B { get; set; }
    public float C { get; set; }

    /* Functions implemented from IMappable and ITessellatable
     */
    public abstract Vector3 Mapping(Vector2 localPoint);
    public abstract Vector2 Pullback(Vector2 localPoint, Vector3 globalTangent);
    public abstract Vector3 Pushforward(Vector2 localPoint, Vector2 localTangent);
    public abstract Vector3 SurfacePoint(float u, float v);

    public abstract float Form2e(Vector2 localPoint);
    public abstract float Form2f(Vector2 localPoint);
    public abstract float Form2g(Vector2 localPoint);

    public float Form2(Vector2 localPoint, Vector3 globalTangent)
    {
        Vector2 localTangent = Pullback(localPoint, globalTangent);

        float ut = localTangent.x;
        float vt = localTangent.y;

        return Form2e(localPoint) * ut * ut + 2 * Form2f(localPoint) * ut * vt + Form2g(localPoint) * vt * vt;
    }
    public float NormalCurvature(Vector2 localPoint, Vector3 globalTangent)
    {
        return Form2(localPoint, globalTangent.normalized);
    }
    public abstract bool InSingularRegion(Vector2 localPoint);
}
