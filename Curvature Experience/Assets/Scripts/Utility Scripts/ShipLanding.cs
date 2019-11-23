using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipLanding : MonoBehaviour
{
    private Vector3 originalPosition;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = this.transform.position;
        this.transform.position = this.transform.position + this.transform.up * 50;
    }

    /*
    private void Awake()
    {
        originalPosition = this.transform.position;
        this.transform.position = this.transform.position + this.transform.forward * 10;

    }*/

    // Update is called once per frame
    void Update()
    {
        if ((originalPosition - transform.position).magnitude > .1f)
        {
            transform.position += Time.deltaTime * -transform.up * (originalPosition - transform.position).magnitude;
            transform.localRotation *= Quaternion.Euler(0, (originalPosition - transform.position).magnitude / 10, 0);
        }
    }
}
