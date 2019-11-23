/* Tal Rastopchin
 * July 22, 2019
 * 
 * Given a RegularSurface and corresponding ParametricSurfaceMapping,
 * computes and keeps track of the normal curvatures, principle directions
 * and curvatures, as well as Gaussian curvature.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* RegularSurfaceCurvature computes and keeps track of the curvature information
 * at a given point on a RegularSurface corrsponding to it's point representation
 * in a ParametricSurfaceMapping.
 */
public class RegularSurfaceCurvature
{
    // We use this struct to pass around our curvature, direction pairs
    public struct Curvature
    {
        public float normalCurvature;
        public Vector3 direction;

        public Curvature (float normalCurvature, Vector3 direction)
        {
            this.normalCurvature = normalCurvature;
            this.direction = direction;
        }
    }

    private RegularSurface regularSurface;
    private ParametricSurfaceMapping parametricSurfaceMapping;

    private Vector3 tangentBasis1;
    private Vector3 tangentBasis2;

    private int numDirections;
    private Curvature[] curvatures;

    // curvature information
    private int principleCurvature1Index = 0; // max
    private int principleCurvature2Index = 0; // min

    public RegularSurfaceCurvature (RegularSurface regularSurface, ParametricSurfaceMapping parametricSurfaceMapping, int numDirections)
    {
        this.regularSurface = regularSurface;
        this.parametricSurfaceMapping = parametricSurfaceMapping;
        this.numDirections = numDirections;

        curvatures = new Curvature[numDirections];

        tangentBasis1 = parametricSurfaceMapping.GlobalUTangent.normalized;
        tangentBasis2 = Vector3.Cross(parametricSurfaceMapping.GlobalNormal, tangentBasis1).normalized;
    }

    public Vector3 TangentBasis1
    {
        get { return tangentBasis1; }
    }

    public Vector3 TangentBasis2
    {
        get { return tangentBasis2; }
    }

    public void ComputeCurvatures ()
    {
        float minNormalCurvature = 0;
        float maxNormalCurvature = 0;

        tangentBasis1 = parametricSurfaceMapping.GlobalUTangent.normalized;
        float angle = 360.0f / numDirections;

        for (int i = 0; i  < numDirections; i++)
        {
            Vector3 direction = (Quaternion.AngleAxis(i * angle, parametricSurfaceMapping.GlobalNormal) * tangentBasis1).normalized;
            float normalCurvature = regularSurface.NormalCurvature(parametricSurfaceMapping.LocalPoint, direction);
            curvatures[i] = new Curvature(normalCurvature, direction);

            // finding k1 and k2
            if (i == 0)
            {
                minNormalCurvature = normalCurvature;
                maxNormalCurvature = normalCurvature;
            }
            else
            {
                if (normalCurvature > maxNormalCurvature)
                {
                    maxNormalCurvature = normalCurvature;
                    principleCurvature1Index = i;
                }

                if (normalCurvature < minNormalCurvature)
                {
                    minNormalCurvature = normalCurvature;
                    principleCurvature2Index = i;
                }
            }
        }
    }

    public Curvature GetPrincipleCurvature1()
    {
        return curvatures[principleCurvature1Index];
    }

    public Curvature GetPrincipleCurvature2()
    {
        return curvatures[principleCurvature2Index];
    }

    public Curvature GetCurvature(int i)
    {
        if (i < numDirections)
        {
            return curvatures[i];
        }
        else
            throw new System.IndexOutOfRangeException();
    }

    public float GetGaussianCurvature()
    {
        return GetPrincipleCurvature1().normalCurvature * GetPrincipleCurvature2().normalCurvature;
    }
}

/* Sources
 * https://answers.unity.com/questions/46770/rotate-a-vector3-direction.html
 */
