using UnityEngine;
using System.Collections;

public class Flyer : MonoBehaviour {

    float yRot;
    float xRot;
    float currentYrot;
    float currentXrot;
    float yRotV;
    float xRotV;
    float lookSmoothDamp;

    public float speedMod = 1;
    public float lookSensitivity = 1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        yRot -= Input.GetAxis("Mouse Y") * lookSensitivity;
        xRot += Input.GetAxis("Mouse X") * lookSensitivity;
        
        yRot = Mathf.Clamp(yRot, -90, 90);

        currentXrot = Mathf.SmoothDamp(currentXrot, xRot, ref xRotV, lookSmoothDamp);
        currentYrot = Mathf.SmoothDamp(currentYrot, yRot, ref yRotV, lookSmoothDamp);
    
        transform.rotation = Quaternion.Euler(currentYrot, currentXrot, 0);

        if (Input.GetKey("w"))
        {
            GetComponent<Rigidbody>().velocity += transform.forward * speedMod;
        }

        if (Input.GetKey("s"))
        {
            GetComponent<Rigidbody>().velocity -= transform.forward * speedMod;
        }

        if (Input.GetKey("d"))
        {
            GetComponent<Rigidbody>().velocity += transform.right * speedMod;
        }

        if (Input.GetKey("a"))
        {
            GetComponent<Rigidbody>().velocity -= transform.right * speedMod;
        }        


	}

}
