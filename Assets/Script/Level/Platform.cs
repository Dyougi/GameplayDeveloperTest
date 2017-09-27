using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour {

    private bool isAtStart;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 endTranslatePlatform;

    public float Speed {get; set;}

    public enum e_platformColor { RED, BLUE, GREEN }

    void Awake()
    {
        isAtStart = false;
    }

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
                if (isAtStart)
                {
                    if (transform.position.x > endTranslatePlatform.x)
                    {
                        GameManager.Instance.DestroyPlatform(this.gameObject);
                        Destroy(this.gameObject);
                    }
                    transform.Translate(Vector3.back * Time.deltaTime * Speed);
                }
            }
        }
	}

    public void Init(float newSpeed, Vector3 newStartPosition, Vector3 newEndPosition, Vector3 newEndTranslatePlatform, bool doLerp = true)
    {
        Speed = newSpeed;
        startPosition = newStartPosition;
        endPosition = newEndPosition;
        endTranslatePlatform = newEndTranslatePlatform;
        if (doLerp)
            StartCoroutine(TranslatePlatformToStart());
        else
            isAtStart = true;
    }

    public void ChangeColor(e_platformColor newColor)
    {

    }

    IEnumerator TranslatePlatformToStart()
    {
        float ElapsedTime = 0.0f;
        while (ElapsedTime <= 1.0f)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, ElapsedTime);
            ElapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPosition;
        isAtStart = true;
    }

    public bool Pause { get; set; }
    public GameManager.e_posPlatform PosPlatform { get; set; }
}
