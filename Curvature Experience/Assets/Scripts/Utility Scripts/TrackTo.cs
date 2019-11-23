/* Tal Rastopchin
 * July 31, 2019
 * 
 * A script to make a GameObject orient such that it is tracking towards the specified
 * object with the desired up
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* TrackTo calls the transform.LootAt method facing away from the desired track object
 * with the up parameter as the transform.up of the up GameObject. It faces away because
 * for some reason when applying this to a Canvas the Canvas would not face in the right
 * direction if one made it track to the track.transform.position.
 */
public class TrackTo : MonoBehaviour
{
    public GameObject track;
    public GameObject up;

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(2 * transform.position - track.transform.position, up.transform.up);
    }
}
