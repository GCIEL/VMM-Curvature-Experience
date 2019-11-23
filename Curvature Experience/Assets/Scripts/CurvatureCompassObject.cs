/* Tal Rastopchin
 * July 29, 2019
 * 
 * Implementation of a Curvature compass object that allows a player to use a
 * raycast to see the normal curvatures of a regular surface at the ray
 * intersection point.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* CurvatureCompassObject allows a player to use a raycast from a specified
 * controller towards the mesh component of a TessellationObject and measure the
 * corresponding normal curvatures at the intersection point. The compass is
 * either idling, where it displays the direction that it can raycast in, or it
 * is actively raycasting and rendering a compass displaying the normal and
 * principle curvatures at the intersection point.
 * 
 * surfaceController, playerController, and tessellationObject all correspond
 * to the SurfaceController, PlayerController, and TessellationObject GameObjects
 * used to facilitate the virtual reality experience. The controller field
 * corresponds to the controller that this compass should take input, position,
 * and orientation from. The transformOffsetObject should be some empty child
 * GameObject of the specified controller that allows the user to specify the
 * position and orientation of the raycst. The origin of the raycast is the
 * position of the transformOffsetObject, and the direction of the raycast is the
 * forward direction of the transformOffsetObject's transform. We use the
 * computedCurvature boolean to store the current state of the compass.
 * 
 * The compassSize field scales the entire compass directions and principle curvature
 * visualization. The tangentDirections and principleDirections arrays store references
 * to instantiated compassDirection prefabs.The directionRadius and
 * principleDirectionRadius  fields correspond to the radius' of the rendered tangent
 * directions and the principle directions. Likewise, the directionMaterial and
 * principleDirectionMaterials are assigned to the tangentDirections' and
 * principleDirections' elements in the Start method. numDirections determines how
 * many directions will be rendered and sampled in the RegularSurfaceCurvature
 * object. BezierCurveObject is a prefab designed to render an arc from the controller
 * to the compass hit location. Lastly, compassRaycastOrigin is a prefab that is
 * instantiated to create a transparent sphere at the start of the bezier curve arc.
 */
public class CurvatureCompassObject : MonoBehaviour
{
    // objects initializing regular surface and parametric surface mapping
    public GameObject surfaceController;
    public GameObject playerController;
    public GameObject tessellationObject;

    // controller input
    public GameObject controller;
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;

    // controller transform offset object for 
    public GameObject transformOffsetObject;

    // regular surface and parametric surface mapping objects
    private RegularSurface regularSurface;
    private ParametricSurfaceMapping compassMapping;
    private ParametricSurfaceMapping playerMapping;

    // meshCollider for tessellationObject raycast
    private MeshCollider meshCollider;

    // object for curvature computations
    private RegularSurfaceCurvature curvature;

    // parameters for raycast
    public float maxRayDistance = 5;
    public float pursuitDistanceThreshold = .1f;
    public int maxPursuitIterations = 64;

    // objects to render compass
    public float compassSize = 1;
    public float directionRadius = .01f;
    public float principleDirectionRadius = .025f;
    public float principleDirectionMultiplier = 1.1f;
    public GameObject directionObject;
    public Material directionMaterial;
    public Material principleDirectionMaterial;
    public int numDirections;
    private GameObject[] tangentDirections;
    private GameObject[] principleDirections;

    // variables for bezier curve visualization
    public GameObject compassRaycastOrigin;
    public GameObject bezierCurveObject;

    // a set of states that the curvature compass object is in
    private bool computedCurvature = false;

    /* Start
     * 
     * Gets, stores, and initializes all relevant fields of the CurvatureCompassObject
     * 
     *  Postconditions
     *      Gets and stores a reference to the RegularSurface object from the surfaceController
     *      Gets and stores a reference to the ParametricSurfaceMappin object from the
     *          playerController
     *      Gets and stores a reference to the MeshCollider object from the tessellation Object
     *      Gets and stores a reference to the SteamVR_TrackedObject from the designated
     *          controller
     *      Initializes the compassMapping as a copy of the current playerMapping
     *      Initializes the RegularSurfaceCurvature with the compassMapping as input
     *      Instantiates all of the compass directions as well as the bezierCurveObject
     * 
     * Called before the first frame update.
     */
    void Start()
    {
        // get our regular surface, mapping mesh collider, and tracked object components
        regularSurface = surfaceController.GetComponent<SurfaceController>().GetSurface();
        playerMapping = playerController.GetComponent<PlayerController>().GetMapping();
        meshCollider = tessellationObject.GetComponent<MeshCollider>();
        trackedObject = controller.GetComponent<SteamVR_TrackedObject>();

        // construct our compassMapping and regularSurfaceCurvature object
        compassMapping = new ParametricSurfaceMapping(regularSurface, playerMapping.LocalPoint);
        curvature = new RegularSurfaceCurvature(regularSurface, compassMapping, numDirections);

        // instantiate our compass directions
        InitializeDirections(ref tangentDirections, numDirections, directionMaterial);
        InitializeDirections(ref principleDirections, 2, principleDirectionMaterial);

        // instantiate our bezierCurveObject prefab
        bezierCurveObject = Instantiate(bezierCurveObject);
        compassRaycastOrigin = Instantiate(compassRaycastOrigin);

        // position and set the parent of our compass raycast origin
        compassRaycastOrigin.transform.position = transformOffsetObject.transform.position;
        compassRaycastOrigin.transform.parent = transformOffsetObject.transform;
    }

    /* InitializeDirections
     * 
     * Given a reference to an uninitialized GameObject array, a number of desired
     * directions, and a direction material, initialize the GameObject array and
     * instantiate the directions accordingly.
     * 
     *  Preconditions
     *      directions is an uninitialized GameObject array
     *      numDirections > 0
     *      material is a valid material
     * 
     *  Postconditions
     *      directions is initialized
     *      each GameObject in directions is a reference to an instantiated directionObject
     *          with the specified material
     */
    private void InitializeDirections (ref GameObject[] directions, int numDirections, Material material)
    {
        directions = new GameObject[numDirections];
        for (int i = 0; i < numDirections; i++)
        {
            GameObject newDirection = GameObject.Instantiate(directionObject);
            newDirection.transform.parent = transform;
            newDirection.GetComponent<MeshRenderer>().material = material;
            directions[i] = newDirection;
        }
    }

    /* Update
     * 
     * Gets the controller device and controller input. Then, performs the compass
     * raycast and compute the curvatures at that point. Creates a curvature compass
     * at the raycast hitpoint.
     * 
     *  Preconditions
     *      The Start was called
     *      
     *  Postconditions
     *      Calls the GetControllerDevice and ProcessControllerInput methods
     * 
     *  Called once per frame.
     */
    void Update()
    {
        GetControllerDevice();
        ProcessControllerInput();
    }

    /* ProcessControllerInput
     * 
     * Handles the controller input and updates the state of the CurvatureCompassObject
     * to faciliate the functionality of the CurvatureCompassObject.
     * 
     *  Preconditions:
     *      The Start method is called
     *      
     *  Postconditions:
     *      If the trigger is not pressed, the compass directions and bezier curve will
     *          not be visible and it will be recorded that computedCurvature is false;
     *      If the trigger is pressed, RayCast will be called. If there was not a ray hit,
     *          then the compass directions and bezier curve will not be visible and it
     *          will be recorded that computedCurvature is false. If there was a ray-mesh
     *          intersection, then we will compute the relevant curvature information at that
     *          intersection point and display the compass directions and bezier curve.
     *      The controller device will vibrate on a ray hit.
     */
    private void ProcessControllerInput()
    {     
        bool pressTrigger = device.GetPress(SteamVR_Controller.ButtonMask.Trigger);

        Vector3 rayOrigin = transformOffsetObject.transform.position;
        Vector3 rayDirection = transformOffsetObject.transform.forward;

        if (!pressTrigger)
        {
            // if we are displaying the compass directions stop
            ToggleCompassDirections(false);

            // render the idling bezier curve
            SetCurvePoints(rayOrigin, rayDirection, rayOrigin + rayDirection, -rayDirection, .25f);
        }
        else if (pressTrigger)
        {
            bool rayHit = RayCast(rayOrigin, rayDirection, maxRayDistance, pursuitDistanceThreshold, maxPursuitIterations);

            // if there was a ray mesh collider intersection
            if (rayHit)
            {
                // device vibrates on ray hit
                device.TriggerHapticPulse(500);

                // if we are not displaying the compass directions start
                ToggleCompassDirections(true);

                // render the active bezier curve
                SetCurvePoints(rayOrigin, rayDirection, compassMapping.GlobalPoint, compassMapping.GlobalNormal.normalized, .5f);

                // compute relevant curvature information
                curvature.ComputeCurvatures();
                float maxCurvature = curvature.GetPrincipleCurvature1().normalCurvature;

                // orient the compass directions
                for (int i = 0; i < numDirections; i++)
                {
                    Vector3 direction = compassSize * curvature.GetCurvature(i).normalCurvature / maxCurvature * curvature.GetCurvature(i).direction;
                    Vector3 end = compassMapping.GlobalPoint + direction;
                    Utils.OrientCube(tangentDirections[i], compassMapping.GlobalPoint, end, compassMapping.GlobalNormal, directionRadius);
                }

                // orient the principle directions
                Vector3 principleDirection1 = compassSize * curvature.GetPrincipleCurvature1().normalCurvature / maxCurvature * curvature.GetPrincipleCurvature1().direction;
                Vector3 direction1Start = compassMapping.GlobalPoint - principleDirectionMultiplier * principleDirection1;
                Vector3 direction1End = compassMapping.GlobalPoint + principleDirectionMultiplier * principleDirection1;

                Utils.OrientCube(principleDirections[0], direction1Start, direction1End, compassMapping.GlobalNormal, principleDirectionRadius);

                Vector3 principleDirection2 = compassSize * curvature.GetPrincipleCurvature2().normalCurvature / maxCurvature * curvature.GetPrincipleCurvature2().direction;
                Vector3 direction2Start = compassMapping.GlobalPoint - principleDirectionMultiplier * principleDirection2;
                Vector3 direction2End = compassMapping.GlobalPoint + principleDirectionMultiplier * principleDirection2;

                Utils.OrientCube(principleDirections[1], direction2Start, direction2End, compassMapping.GlobalNormal, principleDirectionRadius);
             }

            // if there was no ray mesh collider intersection
            else
            {
                // if we are displaying the compass directions and there is no ray hit stop displaying them
                ToggleCompassDirections(false);

                // render the idling bezier curve
                SetCurvePoints(rayOrigin, rayDirection, rayOrigin + rayDirection, -rayDirection, .25f);
            }
        }
    }

    /* DisplayCompassDirections
     * 
     * Sets all of the compass direction objects, including the principle directions
     * as active or inactive according to the boolean value display.
     * 
     *  Preconditions:
     *      tangentDirections is intialized and every element of tangentDirections
     *          is an instantiated GameObject.
     *      principleDirections is intialized and every element of principleDirections
     *          is an instantiated GameObject.
     *      
     *  Postconditions:
     *      every element of both the tangentDirections and principleDirections
     *          GameObeject array is either set active or inactive.  
     */
    private void DisplayCompassDirections (bool display)
    {
        for (int i = 0; i < numDirections; i++)
        {
            tangentDirections[i].SetActive(display);
            if (i < 2)
                principleDirections[i].SetActive(display);
        }
    }

    /* ToggleCompassDirections
     * 
     * Given the desired state of the computedCurvature field, determine whether or not the
     * current state matches the desired state. If they do match, do nothing. If they do not
     * match, set the desiredState and update display the compass directions accordingly.
     * 
     *  Preconditions
     *      Start has been called
     *  
     *  Postconditions:
     *      computedCurvature reflects the current state of the CurvatureCompassObject
     *      DisplayCompassDirections is called according to the state change
     */
    private void ToggleCompassDirections (bool desiredState)
    {
        if (computedCurvature != desiredState)
        {
            DisplayCompassDirections(desiredState);
            computedCurvature = desiredState;
        }
    }

    /* RayCast
     * 
     * Performs a raycast to intersect the collider mesh and determines the corresponding
     * local coordinate of the hit point using the TangentPlanePursuit class. Returns whether
     * or not the raycast intersected with the collider mesh.
     * 
     *  Preconditions
     *      rayOrigin is the start position of the ray.
     *      rayDirection is the direction in which to raycast.
     *      maxRayDistance is the maximum distance a ray hit can be considered a valid
     *          intersection.
     *      pursuitDistanceThreshold is the desired distance we would like the global point.
     *          corresponding to the local point in our tangent plane iteration approximatation
     *          to be from the destination point we are trying to approximate.
     *      maxPursuitIterations is the maximum number of number of tangent plane approximation
     *          iterations that the TangentPlanePursuit object should perform.
     *          
     *  Postconditions
     *      returns whether or not the raycast intersected with the collider mesh.
     *      if the raycast intersected with the collider mesh, updates the compassMapping
     *          local coordinate to that of the approximation of the local coordinate of the
     *          raycast hit.
     */
    private bool RayCast (Vector3 rayOrigin, Vector3 rayDirection, float maxRayDistance, float pursuitDistanceThreshold, int maxPursuitIterations)
    {
        Ray ray = new Ray(rayOrigin, rayDirection);
        RaycastHit hitInfo;

        bool rayHit = meshCollider.Raycast(ray, out hitInfo, maxRayDistance);

        // if raycast hit
        if (rayHit)
        {
            TangentPlanePursuit tangentPlanePursuit = new TangentPlanePursuit(playerMapping, hitInfo.point);
            Vector2 newLocalPoint = tangentPlanePursuit.Pursuit(pursuitDistanceThreshold, maxPursuitIterations);
            compassMapping.UpdateGlobalCoordinates(newLocalPoint);
        }

        return rayHit;
    }

    /* GetControllerDevice
     * 
     * Assigns the SteamVR_Controller.Device objects to the corresponding controller
     * device
     */
    private void GetControllerDevice()
    {
        device = SteamVR_Controller.Input((int)trackedObject.index);
    }

    /* SetCurvePoints
     * 
     * Sets the points of the visualization curve using the SetCurvePoints method of the
     * BezierCurveObject class
     * 
     *  Preconditions:
     *      bezierCurveObject is an instantiated BezierCurveObject prefab
     *      
     *  Postconditions:
     *      the curve points of the bezierCurveObject are set accordingly
     */
    private void SetCurvePoints(Vector3 point1, Vector3 offset1, Vector3 point2, Vector3 offset2, float offsetMultiplier)
    {
        bezierCurveObject.GetComponent<BezierCurveObject>().SetCurvePoints(point1, offset1, point2, offset2, offsetMultiplier);
    }

    /* GetUmbilicalDistance
     * 
     * Returns the a measure of `distance' from the player to the closest umbilical point.
     * 
     *  Preconditons:
     *      The RegularSurfaceCurvature object is properly initialized.
     *      
     *  Postconditions
     *      If the curvature was properly computed, return the difference of the compass's
     *          principle curvatures. This works because at an umbilical point th principle
     *          curvatures are equal, so the closer one gets to an umbilical point the smaller
     *          that difference will be.
     *      If the curvature was not properly computed, returns positive infinity.
     */
    public float GetUmbilicalDistance ()
    {
        if (computedCurvature)
            return curvature.GetPrincipleCurvature1().normalCurvature - curvature.GetPrincipleCurvature2().normalCurvature;
        else return float.PositiveInfinity;
    }

    /* GetGaussianCurvature
     * 
     * Returns the compass's Gaussian curvature reading
     * 
     *  Preconditons:
     *      The RegularSurfaceCurvature object is properly initialized.
     */
    public float GetGaussianCurvature ()
    {
        return curvature.GetGaussianCurvature();
    }

    /* GetCompassPosition
     * 
     * Returns the compass's mapping global position
     * 
     *  Preconditons:
     *      The RegularSurfaceCurvature object is properly initialized.
     */
    public Vector3 GetCompassPosition()
    {
        return compassMapping.GlobalPoint;
    }

    /* GetCompassNormal
     * 
     * Returns the compass's mapping global normal
     * 
     * Preconditons:
     *      The RegularSurfaceCurvature object is properly initialized.
     */
    public Vector3 GetCompassNormal()
    {
        return compassMapping.GlobalNormal;
    }
}
