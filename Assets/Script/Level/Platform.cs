using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour {

    private bool isAtStart;
    private Vector3 startPosition;
    private Vector3 endPosition;


    public float Speed {get; set;}

    public enum e_platformColor { RED, BLUE, GREEN }

	void Start ()
    {
        isAtStart = false;
        Pause = false;
    }
	
	void Update ()
    {
        if (GameManager.Instance.GameStarted)
        {
            if (!Pause)
            {
                if (isAtStart)
                {
                    transform.Translate(Vector3.back * Time.deltaTime * Speed);
                }
            }
        }
	}

    public void Init(float newSpeed, Vector3 newStartPosition, Vector3 newEndPosition, bool doLerp = true)
    {
        Speed = newSpeed;
        startPosition = newStartPosition;
        endPosition = newEndPosition;
        if (doLerp)
            StartCoroutine(TranslatePlatformToStart());
    }

    public void ChangeColor(e_platformColor newColor)
    {

    }

    IEnumerator TranslatePlatformToStart()
    {
        float ElapsedTime = 0.0f;
        while (ElapsedTime < 1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, ElapsedTime);
            ElapsedTime += Time.deltaTime;
            yield return null;
        }
        isAtStart = true;
    }

    public bool Pause { get; set; }
    public GameManager.e_posPlatform PosPlatform { get; set; }
}
