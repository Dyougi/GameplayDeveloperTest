using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    private Rigidbody rb;
    private Vector3 playerDefaultPos;

	void Start ()
    {
        rb = GetComponent<Rigidbody>();
        playerDefaultPos = transform.position;
    }
	
	void Update () // -9.81 real grav
    {
		if (Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * 8, ForceMode.Impulse);
        }
	}
}
