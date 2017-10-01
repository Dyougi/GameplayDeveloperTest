using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour {
    
    private Transform startPosition;
    private Transform endPosition;
    private Vector3 endTranslatePlatform;
    private MeshRenderer meshRender;
    private BoxCollider coll;

    public float Speed {get; set;}
    public float Id { get; set; }
    public GameManager.e_colorPlatform ColorPlatform { get; set; }

    void Awake()
    {
        meshRender = GetComponentInChildren<MeshRenderer>();
        coll = GetComponentInChildren<BoxCollider>();
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
                if (transform.position.z < endTranslatePlatform.z)
                {
                    GameManager.Instance.DestroyPlatform(this.gameObject);
                    //Debug.Log("DESTROY Platform id: " + Id);
                    Destroy(this.gameObject);
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

    public void ChangeOpacity(bool opacity)
    {
        Color newColor;
        if (opacity)
        {
            newColor = new Color(meshRender.material.color.r, meshRender.material.color.g, meshRender.material.color.b, 0.3f);
            coll.enabled = false;
        }
        else
        {
            newColor = new Color(meshRender.material.color.r, meshRender.material.color.g, meshRender.material.color.b, 1f);
            coll.enabled = true;
        }
        meshRender.material.color = newColor;
    }

    public bool Pause { get; set; }
    public GameManager.e_posPlatform PosPlatform { get; set; }

    IEnumerator TranslatePlatformToStart(Vector3 offset)
    {
        float ElapsedTime = 0.0f;
        while (ElapsedTime <= 1.0f)
        {
            Vector3 lerpVec = Vector3.Lerp(startPosition.position, endPosition.position + offset, ElapsedTime);
            if (GameManager.Instance.GameStarted)
                lerpVec.z = transform.position.z;
            transform.position = lerpVec;
            ElapsedTime += Time.deltaTime;
            yield return null;
        }
        Vector3 newPos = new Vector3(endPosition.position.x, endPosition.position.y, transform.position.z);
        transform.position = newPos;
    }
}
