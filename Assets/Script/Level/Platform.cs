using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour {
    
    private Transform startPosition;
    private Transform endPosition;
    private Vector3 endTranslatePlatform;

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
                if (transform.position.z < endTranslatePlatform.z)
                {
                    GameManager.Instance.DestroyPlatform(gameObject);
                    Destroy(gameObject);
                }
                transform.Translate(Vector3.back * Time.deltaTime * Speed);
            }
        }
	}

    public void Init(float newSpeed, Transform newStartPosition, Transform newEndPosition, Vector3 newEndTranslatePlatform, Vector3 offset, bool doLerp = true)
    {
        Speed = newSpeed;
        startPosition = newStartPosition;
        endPosition = newEndPosition;
        endTranslatePlatform = newEndTranslatePlatform;

        if (doLerp)
            StartCoroutine(TranslatePlatformToStart(offset));
    }

    public bool Pause { get; set; }

    public float Speed { get; set; }

    public float Id { get; set; }

    public float HeightScale { get; set; }

    public PlatformManager.e_colorPlatform ColorPlatform { get; set; }

    public GameManager.e_posPlatform PosPlatform { get; set; }

    public GameManager.e_posPlatform NextPosPlatform { get; set; }

    public GameManager.e_posPlatform LastPosPlatform { get; set; }

    IEnumerator TranslatePlatformToStart(Vector3 offset)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < 1.0f)
        {
            if (!Pause)
            {
                Vector3 lerpVec = Vector3.Lerp(startPosition.position, endPosition.position + offset, elapsedTime);
                if (GameManager.Instance.GameStarted)
                    lerpVec.z = transform.position.z;
                transform.position = lerpVec;
                elapsedTime += Time.deltaTime * 0.5f;
            }

            yield return null;
        }

        Vector3 newPos = new Vector3(endPosition.position.x, endPosition.position.y, transform.position.z);
        transform.position = newPos;
    }
}
