/* Tal Rastopchin
 * July 19, 2019
 * 
 * A script component that is used to create bezier curve and render it using
 * the Unity LineRenderer component.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* The BezierCurveObject facilitates the creation and rendering of a third
 * degree Bezier curve using the Unity LineRenderer component.
 * 
 * This class represents a Bezier curve using four points. We use the third
 * degree Bezier curve equation to evaluate points along the curve defined by
 * these four points and pass these resulting points to the LineRenderer. 
 */
[RequireComponent(typeof(LineRenderer))]
public class BezierCurveObject : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Vector3[] points = new Vector3[4];
    public int resolution = 64;
    public float startWidth = .1f;
    public float endWidth = .1f;

    /* Start is called before the first frame update
     * 
     * Stores a reference to the LineRenderer component of this GameObject. Sets
     * the lineRenderer number of points, start width and end width, as well as
     * an initial set of curve points.
     */
    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = resolution;
        SetWidth(startWidth, endWidth);
        SetCurvePoints(transform.position, transform.up, transform.position + transform.forward, transform.up, 1);
    }

    /* Update is called once per frame
     * 
     * Generates the curve each frame
     */
    void Update()
    {
        GenerateCurve();
    }

    /* SetWidth sets the start and end width of the curve
     */
    public void SetWidth (float start, float end)
    {
        lineRenderer.startWidth = start;
        lineRenderer.endWidth = end;
    }

    /* SetCurvePoints specially sets the four Bezier control points
     * 
     * A Bezier curve with the control points p1, p2, p3, and p4 will start
     * at p1, travel towards p2, and terminate at p4 coming from the direction
     * of p3. This method sets these points given the start and end points, point1
     * and point2, and their corresponding 'directional offset' vectors for
     * computing p2 and p3. offsetMulitplier is a scalar coefficient for these
     * directional offsets.
     * 
     * For example, let us say that we pass to this function two points on a surface
     * with their corresponding normals. Then, the resulting Bezier curve will arc from
     * point1 up and then arc down towards point2.
     */
    public void SetCurvePoints (Vector3 point1, Vector3 offset1, Vector3 point2, Vector3 offset2, float offsetMultiplier)
    {
        points[0] = point1;
        points[1] = point1 + offsetMultiplier * offset1;
        points[2] = point2 + offsetMultiplier * offset2;
        points[3] = point2;
        
    }

    /* GenerateCurve computes and sets the Bezier curve points
     * 
     * Samples resolution points along the Bezier curve and sets their
     * resulting positions as points within the LineRenderer component
     */
    private void GenerateCurve()
    {
        for (int i = 0; i < resolution; i++)
        {
            float t = i * 1.0f / (resolution - 1);
            lineRenderer.SetPosition(i, EvaluateCurve(t));
        }
    }

    /* EvaluateCurve computes a point along our Bezier curve
     * 
     * Given a float t in the domain [0, 1], compute the corresponding point
     * along our Bezier curve. Uses the third degree Bezier curve equation
     * given in the wikipedia page for Bezier curves.
     * 
     */
    private Vector3 EvaluateCurve(float t)
    {
        Vector3 term1 = Mathf.Pow(1 - t, 3) * points[0];
        Vector3 term2 = 3 * Mathf.Pow(1 - t, 2) * t * points[1];
        Vector3 term3 = 3 * (1 - t) * Mathf.Pow(t, 2) * points[2];
        Vector3 term4 = Mathf.Pow(t, 3) * points[3];

        return term1 + term2 + term3 + term4;
    }
}

/* Sources
 * https://en.wikipedia.org/wiki/B%C3%A9zier_curve
 */
