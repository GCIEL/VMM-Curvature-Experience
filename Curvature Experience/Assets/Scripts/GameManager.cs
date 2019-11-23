/* Tal Rastopchin
 * July 30, 2019
 * 
 * GameManager is the script that keeps track of how many umbilical points are
 * discovered as well as manages the transitions from one Unity scene to the next.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/* GameManager
 * 
 */
public class GameManager : MonoBehaviour
{
    // references to the playerController and curvatureCompass
    public PlayerController playerController;
    public CurvatureCompassObject curvatureCompass;

    public int totalUmbilicalPoints = 4;

    // parameters for detecting and keeping track of umbilical points
    private float currentUmbilicalDistance = float.PositiveInfinity;
    private Vector3[] umbilicalPoints = new Vector3[4];
    private int numUmbilicalPoints = 0;
    public float umbilicalPointDistanceThreshold = .001f;
    public float umbilicalPointDifferenceThreshold = 10f;
    public float umbilicalDistanceMultiplier = 1000;

    // reference to text object
    public TextMeshProUGUI displayText;
    public TextMeshProUGUI displayOutline;

    // prefab to instantiate at each discovered umbilical point
    public GameObject spaceShipPrefab;

    private void Update()
    {
        GetUmbilicalDistance();
        DetectUmbilicalPoint();
        UpdateTextDisplay();

        if (numUmbilicalPoints == totalUmbilicalPoints) {
            playerController.enabled = false;
            playerController.transform.position += 8 * Time.deltaTime * playerController.transform.up;
        }
            
    }

    /* UpdateTextDisplay
     *
     *  Postconditions
     *      If all four umbilical points are found, display that you have found them all
     *      If we get an umbilical distance reading that is valid, where the value is not
     *          float.PositiveInfinity, display this distance scaled by the umbilicalDistanceMultiplier
     *      If we get an invalid umbilical distance reading of float.PositiveInfinity,
     *          display "???"
     */
    private void UpdateTextDisplay ()
    {
        if (numUmbilicalPoints == totalUmbilicalPoints)
            SetTextDisplay("You have found them all !");

        if (currentUmbilicalDistance != float.PositiveInfinity)
        {
            float scaledUmbilicalDistance = (currentUmbilicalDistance - umbilicalPointDistanceThreshold) * umbilicalDistanceMultiplier;
            if (scaledUmbilicalDistance > 0)
                SetTextDisplay(scaledUmbilicalDistance.ToString("0.00"));
            else
                SetTextDisplay("!!!");
        }
        else
            SetTextDisplay("???");

        // helper function
        void SetTextDisplay (string text)
        {
            displayText.text = text;
            displayOutline.text = text;
        }
    }

    /* GetUmbilicalDistance
     * 
     * Stores the current umbilical distance reading from the curvatureCompass in the
     * currentUmbilicalDistance field
     */
    private void GetUmbilicalDistance ()
    {
        currentUmbilicalDistance = curvatureCompass.GetUmbilicalDistance();
    }

    /* DetectUmbilicalPoint
     * 
     * Determines whether the player is at an umbilical point according to the
     * umbilicalDistancePointThreshold, and keeps track of that point if it is
     * a new umbilic.
     * 
     *  Preconditions
     *      GetUmbilicalDistance should be called directly before this method to ensure
     *          the latest umbilical distance reading is used
     *          
     *  Postconditions
     *      If the player is at an umbilical point, determines whether or not they have
     *          already discovered this point according to the umbilicalPointDifferenceThreshold
     *      If it is a new umbilical point, stores the point in the umbilicalPoints array
     *          and updates the number of umbilical points.
     *      If a new umbilical point is discovered, instantiates the spaceShipPrefab at the
     *          location of the umbilical point.
     */
    private void DetectUmbilicalPoint ()
    {
        if (currentUmbilicalDistance < umbilicalPointDistanceThreshold)
        {
            bool newUmbilicalPoint = true;
            Vector3 candidateUmbilicalPoint = curvatureCompass.GetCompassPosition();

            // if first umbilical point
            if (numUmbilicalPoints == 0)
            {
                umbilicalPoints[0] = candidateUmbilicalPoint;
            }

            // if not first, make sure it is far enough from the previously discovered points
            else
            {
                for (int i = 0; i < numUmbilicalPoints; i++)
                {
                    float dist = (umbilicalPoints[i] - candidateUmbilicalPoint).magnitude;
                    if (dist < umbilicalPointDifferenceThreshold)
                    {
                        newUmbilicalPoint = false;
                        break;
                    }
                }
            }

            // if it s a new umbilical point, keep track of it and instantiate the spaceship prefab
            if (newUmbilicalPoint)
            {
                umbilicalPoints[numUmbilicalPoints] = candidateUmbilicalPoint;
                InstantiateSpaceShip(candidateUmbilicalPoint);
                numUmbilicalPoints++;
            }
        }
    }

    /* InstantiateSpaceShip
     * 
     * Instantiates the spaceShipPrefab at the specified point. Since the spaceShipPrefab
     * I am using has the spaceship land at a the location of the prefab, after instantiation
     * I need to position the prefab at the umbilical point location as well as orient it
     * such that its up vector is the normal vector of the surface at the compass' reading point.
     */
    private void InstantiateSpaceShip (Vector3 umbilicalPoint)
    {
        GameObject newUmbilicalPointObject = GameObject.Instantiate(spaceShipPrefab);
        newUmbilicalPointObject.transform.position = umbilicalPoint;
        newUmbilicalPointObject.transform.up = curvatureCompass.GetCompassNormal();
    }
}

/* Sources
 * https://answers.unity.com/questions/1027642/vr-how-to-display-a-canvas-with-oculus.html
 */