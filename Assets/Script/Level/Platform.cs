using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour {

    public float Speed {get; set;}

	void Start ()
    {
        Speed = 1;

    }
	
	void Update ()
    {
        transform.Translate(Vector3.back * Time.deltaTime * Speed);
	}
}
