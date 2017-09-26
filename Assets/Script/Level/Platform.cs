using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour {

    public float Speed {get; set;}

    public enum e_platformColor { RED, BLUE, GREEN }

	void Start ()
    {
        Pause = false;
    }
	
	void Update ()
    {
        if (GameManager.Instance.GameStarted)
        {
            if (!Pause)
            {
                transform.Translate(Vector3.back * Time.deltaTime * Speed);
            }
        }
	}

    void Init(float newSpeed)
    {
        Speed = newSpeed;
    }

    public void ChangeColor(e_platformColor newColor)
    {

    }

    public bool Pause { get; set; }
    public GameManager.e_posPlatform PosPlatform { get; set; }
}
