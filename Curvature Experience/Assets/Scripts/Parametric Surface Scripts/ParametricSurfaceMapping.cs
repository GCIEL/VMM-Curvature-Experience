/* Tal Rastopchin
 * July 16, 2019
 * 
 * Implementation of a class used to keep track of a point on a parameterized regular suface
 * by mapping from a system of local coordinates of R2 into a system of global coordinates
 * in R3. Implementation of an interface used to provide the necessary parameterization and
 * mapping functions for this class.
 * 
 * M. P. do Carmo, Differential Geometry of Curves and Surfaces.
 * Prentice-Hall, Inc., Englewood Cliffs N.J., 1976. Translated from the Portuguese
 */

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/* RegularSurfaceMapping is a class that is used to keep track of a point on a
 * regular surface parameterized by a mapping from a system of local coordinates
 * of R2 into a coordinate neighborhood of R3.
 * 
 * This class keeps track of a point on a regular surface by using a parameterization
 * of a system of local (u, v) coordinates and keeping track of a point localPoint
 * in our local coordinates. This class also keeps track of the canonical u and v
 * (localUTangent and localVTangent, respectively) local tangent vectors. This class
 * lastly keeps track of an initial global forward vector, which it parallel transports
 * so that we can have accurate "geodesic movement" on our surfaces.
 * 
 * Our parameterization and mapping functions are provided to us by the IMappable
 * mapping field. A class that implements IMappable provides us with three functions;
 * namely, our mapping, pushforward, and pullback functions. We use these functions
 * to "transform" our system of local coordinates into our system of global coordinates.
 * To do this, we apply mapping to our localPoint to produce our globalPoint; apply
 * the pushfowrard to our localUTangent and localVTangent to produce our globalUTangent
 * and globalVTangent respectively, and apply the cross product to our resulting
 * globalUTangent and globalVTangent to produce our globalNormal vector.
 * 
 * To parallel transport our initial view vector, what we do is after updating our
 * system of global coordinates, we project the old globalForward vector onto our
 * new tangent plane. In this fashion, we approximate the parallel transport of the
 * view vector, and so we can maintain a correct orientation of our resulting system
 * of global coordinates.
 */
public class ParametricSurfaceMapping
{
    // our parameterization and mapping functions
    private readonly IMappable mapping;

    // system of local coordinates
    private Vector2 localPoint;
    private Vector2 localUTangent = new Vector2(1, 0);
    private Vector2 localVTangent = new Vector2(0, 1);

    // system of global coordinates
    private Vector3 globalPoint;
    private Vector3 globalUTangent;
    private Vector3 globalVTangent;
    private Vector3 globalNormal;

    // we will parallel transport this vector
    private Vector3 globalForward;

    /* A series of getter properties for the rest of our local and global coordinates
     */
    public Vector2 LocalPoint
    {
        get { return localPoint; }
    }
    public Vector2 LocalUTangent
    {
        get { return localUTangent; }
    }
    public Vector2 LocalVTangent
    {
        get { return localVTangent; }
    }
    public Vector3 GlobalPoint
    {
        get { return globalPoint; }
    }
    public Vector3 GlobalUTangent
    {
        get { return globalUTangent; }
    }
    public Vector3 GlobalVTangent
    {
        get { return globalVTangent; }
    }
    public Vector3 GlobalNormal
    {
        get { return globalNormal; }
    }
    public Vector3 GlobalForward
    {
        get { return globalForward; }
    }

    public IMappable Mapping
    {
        get { return mapping; }
    }

    /* RegularSurfaceMapping() constructor
     * 
     * Constructs a new RegularSurfaceMapping object with the given mapping and with the
     * localPoint initialized to (0,0). Updates the system of global coordinates accordingly.
     * 
     * IMappable mapping provides the definitions of our parameterization and mapping
     * functions
     */
    public ParametricSurfaceMapping(IMappable mapping)
    {
        this.mapping = mapping;
        this.localPoint = new Vector2(0, 0);
        UpdateGlobalCoordinates();
    }

    /* RegularSurfaceMapping constructor
     * 
     * Constructs a new RegularSurfaceMapping object with the given mapping and given
     * localPoint. Updates the system of global coordinates accordingly.
     * 
     * IMappable mapping provides the definitions of our parameterization and mapping
     * functions
     * 
     * Vector2 localPoint defines our initial localPoint
     */
    public ParametricSurfaceMapping(IMappable mapping, Vector2 localPoint)
    {
        this.mapping = mapping;
        
        this.localPoint = localPoint;

        this.globalForward = mapping.Pushforward(localPoint, localVTangent);
        UpdateGlobalCoordinates();
    }

    /* UpdateGlobalCoordinates updates the global coordinate system
     * 
     * Using the method outlined in the class description, we update our global coordinate
     * system applying the mapping and parameterization methods provided by our IMappable
     * mapping to our system of local coordinates.
     */
    public void UpdateGlobalCoordinates()
    {
        // update our system of global coordinates
        globalPoint = mapping.Mapping(localPoint);
        globalUTangent = mapping.Pushforward(localPoint, localUTangent);
        globalVTangent = mapping.Pushforward(localPoint, localVTangent);
        globalNormal = Vector3.Cross(globalUTangent, globalVTangent);

        // parallel transport the old forward vector
        globalForward = Vector3.ProjectOnPlane(globalForward, globalNormal);

    }

    /* UpdateGlobalCoordinates updates both the local and global coordinate systems
     * 
     * Using the method outlined in the class description, we update our global coordinate
     * system applying the mapping and parameterization methods provided by our IMappable
     * mapping to our system of local coordinates as well as regularize our local point
     * beforehand. Does not move if the new locatino is in a singular region.
     * 
     * Vector2 newLocalPoint defines our new localPoint
     */
    public void UpdateGlobalCoordinates(Vector2 newLocalPoint)
    {
        this.localPoint = newLocalPoint;
        UpdateGlobalCoordinates();
    }
    /* ToString produces a string representation of our coordinate systems
     * 
     * Produces a string representation of our systems of local and global coordinates.
     * Namely, creates a string representing our fields pertaining to both coordinate
     * systems, as well as the pullbacks of the global U and V tangent vectors.
     */
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("Local Point: " + localPoint + "\n");
        sb.Append("Local U Tangent: " + localUTangent + "\n");
        sb.Append("Local V Tangent: " + localVTangent + "\n\n");

        sb.Append("Global Point: " + globalPoint.normalized + "\n");
        sb.Append("Global U Tangent: " + globalUTangent.normalized + "\n");
        sb.Append("Global V Tangent : " + globalVTangent.normalized + "\n");
        sb.Append("Global Normal : " + globalNormal.normalized + "\n\n");

        sb.Append("Global U Tangent Pullback : " + mapping.Pullback(localPoint, globalUTangent).normalized + "\n");
        sb.Append("Global V Tangent Pullback : " + mapping.Pullback(localPoint, globalVTangent).normalized + "\n");

        return sb.ToString();
    }
}


/* IMappable is an interface that is used to provide an object with the needed functionality
 * in order to be used as the mapping field within a ParametricSurfaceMapping object. Namely,
 * this interface requires implementing the mapping, pushforward, pullback, and second
 * fundamental form needed for using differential geometry to study the Gauss map in local
 * coordinates.
 */
public interface IMappable
{
    /* Mapping takes our local point and gives us our global point
     * 
     * Equivalent to our parameterization function X from R2 to R3
     * 
     * Vector2 localPoint is our point in local coordinates of R2
     */
    Vector3 Mapping(Vector2 localPoint);

    /* Pushforward "pushes forward" local tangent vectors
     * 
     * Equivalent to applying the matrix of the differential (pushforward) to
     * a local tangent vector.  
     * 
     * Vector2 localPoint is our point in local coordinates of R2
     * 
     * Vector2 localTangent is our tangent vector to our local point in local
     * coordinates of R2
     */
    Vector3 Pushforward(Vector2 localPoint, Vector2 localTangent);

    /* Pullback "pulls back" global tangent vectors
     * 
     * Equivalent to applying the inverse matrix of the differential (pushforward)
     * to a global tangent vector.
     * 
     * Vector2 localPoint is our point in local coordinates of R2
     * 
     * Vector2 localTangent is our tangent vector to our local point in local
     * coordinates of R2
     */
    Vector2 Pullback(Vector2 localPoint, Vector3 globalTangent);

    /* Form2e is the first coefficient of the second fundamental form expressed
     * in local coordinates
     */
    float Form2e(Vector2 localPoint);

    /* Form2f is the second coefficient of the second fundamental form expressed
     * in local coordinates
     */
    float Form2f(Vector2 localPoint);

    /* Form2g is the third coefficient of the second fundamental form expressed
     * in local coordinates
     */
    float Form2g(Vector2 localPoint);

    /* Form2 is the second fundamental form function expressed in local coordinates
     * applied to a global tangent vector.
     */
    float Form2(Vector2 localPoint, Vector3 globalTangent);

    /* NormalCurvature applies Form2 to a normalized global tangent vector. We do this
     * to make sure that whenever we compute the normal curvature of a tangent direction,
     * we normalize the tangent vector.
     */
    float NormalCurvature(Vector2 localPoint, Vector3 globalTangent);

    /* InSingularRegion returns whether or not a local point in the system of local
     * coordinates is in a singular region (defined by a point and the boundsDelta).
     * We use this function to decide whether or not our movement in a certain direction
     * is valid.
     */
    bool InSingularRegion(Vector2 localPoint);
}
