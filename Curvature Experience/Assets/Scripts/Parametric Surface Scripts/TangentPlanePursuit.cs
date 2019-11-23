/* Tal Rastopchin
 * July 18, 2019
 * 
 * The TangentPlanePursuit class provides an algorithm for determining the 2D
 * local coordinate of a 3D point on a surface given a starting parametric
 * surface mapping.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* TangentPlanePursuit is a class that provides an algorithm for determining
 * the 2D local coordinate corresponding to a 3D point on some regular surface.
 * 
 * We use this class to determine the local coordinate of the 3D point resulting
 * in a raycast from the player's controller onto the parametric surface mesh.
 */
public class TangentPlanePursuit
{
    // the surface mapping as well as initial conditions
    private IMappable mapping;
    private ParametricSurfaceMapping parametricSurfaceMapping;

    // our destination and current local point approximation
    private Vector3 globalDestination;

    /* A getter property for the current local point approximation
     */
    public Vector2 LocalPointApproximation
    {
        get { return parametricSurfaceMapping.LocalPoint; }
    }

    /* A getter propery for the current distance from the approximation to the
     * destination point
     */
    public float DestinationDistance
    {
        get { return (parametricSurfaceMapping.GlobalPoint - globalDestination).magnitude; }
    }

    /* TangentPlanePursuit constructor
     * 
     * Given the initial condition ParametricSurfaceMapping (i.e. where the player is
     * currently located within the system of local coordinates) and a destination
     * (the point on the surface for which we want it's corresponding local coordinate),
     * contruct a TangentPlanePursuit object.
     * 
     * We store a reference to the IMappable stored within the initial conditions and
     * create a copy of the ParametricSurfaceMapping.
     */
    public TangentPlanePursuit (ParametricSurfaceMapping parametricSurfaceMapping, Vector3 globalDestination)
    {
        this.mapping = parametricSurfaceMapping.Mapping;
        this.parametricSurfaceMapping = new ParametricSurfaceMapping(mapping, parametricSurfaceMapping.LocalPoint);
        this.globalDestination = globalDestination;
    }

    /* PursuitIteration runs one iteration of the tangent plane pursuit algorithm
     * 
     * An iteration of the tangent plane pursuit algorithm works as follows. First, 
     * we compute the direction vector beginning at our current global position to
     * the global destination. Then, we project this vector onto the local tangent
     * plane. We pull back the resulting tangent vector and use it to compute
     * our new local point; that is, we use that tangent vector to compute the tangent
     * plane approximation of the global destination in local coordinates. We then
     * update our parametricSurfaceMapping to reflect this new local coordinate
     * and we return the distance between the global destination and our resulting
     * approximation of this point in our new system of local coordinates.
     */
    public Vector2 PursuitIteration ()
    {
        // visual debug
        //Vector3 oldLocation = parametricSurfaceMapping.GlobalPoint;
        Vector3 destinationDirection = globalDestination - parametricSurfaceMapping.GlobalPoint;
        Vector3 globalDestinationProjection = Vector3.ProjectOnPlane(destinationDirection, parametricSurfaceMapping.GlobalNormal);

        Vector2 localDestinationTangent = mapping.Pullback(parametricSurfaceMapping.LocalPoint, globalDestinationProjection);
        Vector2 newLocalPoint = parametricSurfaceMapping.LocalPoint + localDestinationTangent;

        parametricSurfaceMapping.UpdateGlobalCoordinates(newLocalPoint);
        // visual debug
        //InstantiateCylinder(parametricSurfaceMapping.GlobalPoint, oldLocation); 
        return newLocalPoint;
    }

    /* Pursuit facilitates the tangent plane pursuit algorithm with a desired distance
     * threshold and max number of iterationsl
     * 
     * Pursuit will simply call PursuitIteration() either until the distance from the
     * current approximation to the global point is within the desired threshold or
     * until we have called PursuitIteration() the maximum number of specified times
     */
    public Vector2 Pursuit(float distanceThreshold, int maxIterations)
    {
        int iteration = 0;
        float distance = DestinationDistance;

        while (distance > distanceThreshold && iteration < maxIterations)
        {
            PursuitIteration();
            distance = DestinationDistance;
            iteration++;
        }

        return LocalPointApproximation;
    }

    private static void InstantiateCylinder(Vector3 point1, Vector3 point2)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        float radius = .01f;
        Vector3 midpoint = Vector3.Lerp(point1, point2, .5f);

        cylinder.transform.position = midpoint;
        cylinder.transform.up = (point2 - point1).normalized;
        cylinder.transform.localScale = new Vector3(radius, (point2 - point1).magnitude / 2, radius);

    }
}
