using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popcorn : MonoBehaviour
{
    public int frameDistance = 45;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % frameDistance == 0)
        {
            GameObject myObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            myObject.transform.position = this.transform.position;
            myObject.transform.localScale = new Vector3(.5f, .5f, .5f);
        }
    }
}
