/* Tal Rastopchin
 * June 20, 2019
 * 
 * Implementation of a player controller object that facilitates a player "walking"
 * along a regular parameterized surface.
 * 
 * Adapted from the CameraController.cs script in the GraphColoring final project
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* PlayerController uses a ParametricSurfaceMapping object as the basis for a player
 * controller where a player "walks" along a parameterized regular surface.
 * 
 * Using the left controller, the vertical axis of the joystick corresponds to moving
 * forward in the direction the player is looking, and the horizontal axis corrsponds to
 * moving to the right or left of the player.
 * 
 * This script should be the component of an empty game object. The [CameraRig] prefab
 * should inherit the transforms of this object. The surfaceController object should
 * be set to an object that initializes the corresponding RegularSurface object. The
 * cameraEye object should be set to the Camera (eye) child of the [CameraRig] prefab.
 * The controllerLeft object should be set to the Controller (left) child of the
 * [CameraRig] prefab.
 */
public class PlayerController : MonoBehaviour
{
    // object initializing the regularSurface
    public GameObject surfaceController;

    // camera eye for player movement
    public GameObject cameraEye;

    // controller input
    public GameObject controllerLeft;
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;

    // player controller settings
    public Vector2 initialLocalPoint = new Vector2(0, Mathf.PI / 4);
    public float speed = 4;
    public bool usePlayerCoordinates = true;
    public bool drawGlobalCoordinates = true;
    public bool drawCameraCoordinates = true;

    // regular surface and parametric surface mapping objects
    private RegularSurface surface;
    public ParametricSurfaceMapping mapping;

    public GameObject shipPrefab;

    /* Start is called before the first frame update
     * 
     * Retrieves the surface from the surfaceController object and initializes the
     * mapping accordingly. Positions our player at the initial mapping global point
     * as well as orients the player with forwards aligning to the globalVTangent
     * and upwards aligned to the globalNormal
     */
    public void Start()
    {
        trackedObject = controllerLeft.GetComponent<SteamVR_TrackedObject>();

        surface = surfaceController.GetComponent<SurfaceController>().GetSurface();
        mapping = new ParametricSurfaceMapping(surface, initialLocalPoint);

        Quaternion orientation = Quaternion.LookRotation(mapping.GlobalForward, mapping.GlobalNormal.normalized);
        transform.SetPositionAndRotation(mapping.GlobalPoint, orientation);

        Instantiate(shipPrefab, transform.position, transform.rotation);
    }

    /* Update updates the player's position and orientation based on user input
     * 
     * We use the camera's forward direction to compute the pullback of the camera's
     * tangent vectors. To do this, we first project the camera's forward direction
     * into the global tangent plane. Then, we use our pullback function to compute
     * the pullback of the global tangent vector back into local coordinates. To
     * compute the local right tangent, we apply the pullback function to the cross
     * product of our forward projection and global normal vector.
     * 
     * We lastly update our local point by linearly combining our pulled back tangent
     * vectors with the posH and posV inputs as coefficients.   
     * 
     * Note: Update is called once per frame
     */
    public void Update()
    {
        // get the left controller device via index
        device = SteamVR_Controller.Input((int)trackedObject.index);

        // get controller input and smooth it
        float posH = Smooth(device.GetAxis().x);
        float posV = Smooth(device.GetAxis().y);

        // we pullback our projected cameraEye forward vector to move in our parameterized surface mapping
        Vector3 forwardProjection = Vector3.ProjectOnPlane(cameraEye.transform.forward, mapping.GlobalNormal);
        Vector3 rightProjection = -forwardProjection.magnitude * Vector3.Cross(forwardProjection, mapping.GlobalNormal).normalized;
        Vector2 forwardPullback = surface.Pullback(mapping.LocalPoint, forwardProjection);
        Vector2 rightPullback = surface.Pullback(mapping.LocalPoint, rightProjection);

        // if we are not using player coordinates, then use coordinate lines
        if (!usePlayerCoordinates)
        {
            rightPullback = surface.Pullback(mapping.LocalPoint, mapping.GlobalUTangent.normalized);
            forwardPullback = surface.Pullback(mapping.LocalPoint, mapping.GlobalVTangent.normalized);
        }

        Vector2 newLocalPoint = mapping.LocalPoint + speed * Time.deltaTime * (posH * rightPullback + posV * forwardPullback);

        // if our new local coordinate is not in the surface's defined singular region
        if (!surface.InSingularRegion(newLocalPoint))
        {
            mapping.UpdateGlobalCoordinates(newLocalPoint);

            Quaternion orientation = Quaternion.LookRotation(mapping.GlobalForward.normalized, mapping.GlobalNormal.normalized);
            transform.SetPositionAndRotation(mapping.GlobalPoint, orientation);
        }
    }

    /* Smooth smoothes our our input
     */
    private float Smooth(float input)
    {
        float sign = (input > 0) ? 1 : -1;
        return sign * Mathf.Pow(input, 2);
    }

    /* GetMapping returns a reference to the ParametricSurfaceMapping mapping objec
     */
    public ParametricSurfaceMapping GetMapping ()
    {
        return mapping;
    }

    /* OnDrawGizmos() draws relevant vectors for visualization purposes
     * 
     * Specifically, this draws vectors corresponding to:
     * 
     * The global coordinate frame:
     *  1) A red GlobalUTangent
     *  2) A green GlobalVTangent
     *  3) A blue GlobalNormal
     * 
     * The camera vectors    
     *  4) A cyan camera forward vector
     *  5) a magenta camera forward projection into the global tangent plane
     *  6) a yellow camera global tangent right
     */
    public void OnDrawGizmos()
    {
        if (mapping != null)
        {
            if (drawGlobalCoordinates)
            {
                DrawVector(mapping.GlobalPoint, mapping.GlobalUTangent.normalized, Color.red);
                DrawVector(mapping.GlobalPoint, mapping.GlobalVTangent.normalized, Color.green);
                DrawVector(mapping.GlobalPoint, mapping.GlobalNormal.normalized, Color.blue);
            }

            if (drawCameraCoordinates)
            {
                Vector3 forwardProjection = Vector3.ProjectOnPlane(cameraEye.transform.forward, mapping.GlobalNormal);
                Vector3 rightProjection = forwardProjection.magnitude * Vector3.Cross(forwardProjection, mapping.GlobalNormal).normalized;

                DrawVector(mapping.GlobalPoint, cameraEye.transform.forward, Color.cyan);
                DrawVector(mapping.GlobalPoint, forwardProjection, Color.magenta);
                DrawVector(mapping.GlobalPoint, rightProjection, Color.yellow);
            }
        }
        else
        {
            DrawVector(transform.position, transform.right, Color.red);
            DrawVector(transform.position, transform.forward, Color.green);
            DrawVector(transform.position, transform.up, Color.blue);
        }
    }

    /* DrawVector is a Gizmos.DrawLine helper function
     */
    private void DrawVector(Vector3 point, Vector3 direction, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(point, point + direction);
    }
}
