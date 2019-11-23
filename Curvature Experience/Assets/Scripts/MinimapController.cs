/* Tal Rastopchin
 * July 30, 2019
 * 
 * A script that facilitates the activation and deactivation of a minimap
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* MinimapController toggles the active state of the MeshRenderer according to
 * the controller trigger
 */
public class MinimapController : MonoBehaviour
{
    // controller input
    public GameObject controller;
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;

    // Start is called before the first frame update
    void Start()
    {
        trackedObject = controller.GetComponent<SteamVR_TrackedObject>();
    }

    // Update is called once per frame
    void Update()
    {
        // assigns the SteamVR_Controller.Device objects to the corresponding controller device
        device = SteamVR_Controller.Input((int)trackedObject.index);

        bool pressDownTrigger = device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger);

        // toggle minimap visibility
        if (pressDownTrigger)
        {
            GetComponent<MeshRenderer>().enabled = !GetComponent<MeshRenderer>().enabled;
        }
    }
}
