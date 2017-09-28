using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [SerializeField]
    private Transform[] positionPlatformStart;

    [SerializeField]
    private Transform[] positionPlatformCreate;

    [SerializeField]
    private Transform positionPlatformEnd;

    [SerializeField]
    private PlayerController playerInstance;

    [SerializeField]
    private float speedPlatform;

    [SerializeField]
    private float distanceBetweenPlatform;

    [SerializeField]
    private GameObject platformDefault;

    [SerializeField]
    private GameObject platformStart;

    [SerializeField]
    private GameObject platformEnvironment;

    private int score;
    private float ratePlatform;
    private List<GameObject> instancePlatform;

    private float lastInstanceTime;
    private GameObject currentPlatform;
    private GameObject secondPlatform;
    private e_posPlatform currentPosPlatform;

    private static GameManager instance;

    public enum e_posPlatform { BOT, BOTRIGHT, RIGHT, TOPRIGHT, TOP, TOPLEFT, LEFT, BOTLEFT };
    public enum e_dirRotation { LEFT, RIGHT };

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.LogWarning("Singleton " + this.name + " : instance here already");
            Destroy(gameObject);
            return;
        }
    }

    public static GameManager Instance
    {
        get { return instance; }
    }

    void Start ()
    {
        GameStarted = false;
        Pause = false;
        instancePlatform = new List<GameObject>();
        PlayerController.OnJump += PlayerJumped;
        InitGame();
    }
	
	void Update ()
    {
        if (GameStarted)
        {
            if (!Pause)
            {
                if (lastInstanceTime + ratePlatform < MyTimer.Instance.TotalTime)
                {
                    Debug.Log("INSTANCE time: " + MyTimer.Instance.TotalTime);
                    GameObject currentInstance = Instantiate(platformDefault, positionPlatformStart[(int)currentPosPlatform]);
                    currentInstance.transform.parent = platformEnvironment.transform;
                    currentInstance.GetComponent<Platform>().Init(speedPlatform, positionPlatformCreate[(int)currentPosPlatform].position, positionPlatformStart[(int)currentPosPlatform].position, positionPlatformEnd.position);
                    currentInstance.GetComponent<Platform>().Speed = speedPlatform;
                    currentInstance.GetComponent<Platform>().PosPlatform = currentPosPlatform;
                    instancePlatform.Add(currentInstance);
                    GetNewPosPlatform();
                    lastInstanceTime = MyTimer.Instance.TotalTime;
                }
            }
        }
	}

    void InitGame()
    {
        playerInstance.SetToStartPos();
        ratePlatform = distanceBetweenPlatform / speedPlatform;
        currentPosPlatform = e_posPlatform.BOT;
        InitTerrain();
        //StartGame();
    }

    void InitTerrain()
    {
        int offset = -40;
        Vector3 posPlatform = Vector3.forward * offset;
        Vector3 posPlatformStart = new Vector3(0, 0, -1);

        GameObject currentInstance = Instantiate(platformStart, posPlatformStart, Quaternion.identity);
        currentInstance.transform.parent = platformEnvironment.transform;
        currentInstance.GetComponent<Platform>().Init(speedPlatform, posPlatformStart, posPlatformStart, positionPlatformEnd.position, false);
        currentInstance.GetComponent<Platform>().PosPlatform = currentPosPlatform;
        GetNewPosPlatform();
        instancePlatform.Add(currentInstance);
        for (int count = 0; count < 9; count++)
        {
            currentInstance = Instantiate(platformDefault, positionPlatformCreate[(int)currentPosPlatform].position, positionPlatformCreate[(int)currentPosPlatform].rotation);
            currentInstance.transform.parent = platformEnvironment.transform;
            currentInstance.GetComponent<Platform>().Init(speedPlatform, positionPlatformCreate[(int)currentPosPlatform].position, positionPlatformStart[(int)currentPosPlatform].position + posPlatform, positionPlatformEnd.position);
            currentInstance.GetComponent<Platform>().PosPlatform = currentPosPlatform;
            instancePlatform.Add(currentInstance);
            GetNewPosPlatform();
            offset += 5;
            posPlatform = Vector3.forward * offset;
        }
        currentPlatform = instancePlatform[0];
        secondPlatform = instancePlatform[1];
    }

    public void StartGame()
    {
        GameStarted = true;
        lastInstanceTime = 0;
    }

    void RestartGame()
    {

    }

    void GetNewPosPlatform()
    {
        if (MyRandom.ThrowOfDice(50))
            currentPosPlatform = currentPosPlatform == e_posPlatform.BOTLEFT ? e_posPlatform.BOT : currentPosPlatform + 1;
        else
            currentPosPlatform = currentPosPlatform == e_posPlatform.BOT ? e_posPlatform.BOTLEFT : currentPosPlatform - 1;
    }
    //                        0       1      2         3      4      5       6       7
    void PlayerJumped() // { BOT, BOTRIGHT, RIGHT, TOPRIGHT, TOP, TOPLEFT, LEFT, BOTLEFT };
    {
        Debug.Log("currentPlatform: " + currentPlatform.GetComponent<Platform>().PosPlatform);
        Debug.Log("secondPlatform: " + secondPlatform.GetComponent<Platform>().PosPlatform);
        if (currentPlatform.GetComponent<Platform>().PosPlatform == 0)
        {
            if (secondPlatform.GetComponent<Platform>().PosPlatform == (e_posPlatform)1)
                RotatePlatformEnvironment(e_dirRotation.RIGHT);
            else
                RotatePlatformEnvironment(e_dirRotation.LEFT);
            return;
        }
        if (currentPlatform.GetComponent<Platform>().PosPlatform == (e_posPlatform)7)
        {
            if (secondPlatform.GetComponent<Platform>().PosPlatform == 0)
                RotatePlatformEnvironment(e_dirRotation.RIGHT);
            else
                RotatePlatformEnvironment(e_dirRotation.LEFT);
            return;
        }
        if (currentPlatform.GetComponent<Platform>().PosPlatform < secondPlatform.GetComponent<Platform>().PosPlatform)
        {
            RotatePlatformEnvironment(e_dirRotation.RIGHT);
            return;
        }
        if (currentPlatform.GetComponent<Platform>().PosPlatform > secondPlatform.GetComponent<Platform>().PosPlatform)
        {
            RotatePlatformEnvironment(e_dirRotation.LEFT);
            return;
        }
    }

    void RotatePlatformEnvironment(e_dirRotation dir)
    {
        Debug.Log("RotatePlatformEnvironment: " + dir.ToString());
        StartCoroutine(DoRotatePlatformEnvironment(dir));
        currentPlatform = secondPlatform;
        secondPlatform = instancePlatform[instancePlatform.IndexOf(secondPlatform) + 1];
    }

    IEnumerator DoRotatePlatformEnvironment(e_dirRotation dir)
    {
        float angle = 0.0f;
        float degree;
        Vector3 saveRotation = platformEnvironment.transform.eulerAngles;
        while (angle <= 45.0f)
        {
            degree = Time.deltaTime * 400;
            if (dir == e_dirRotation.RIGHT)
            {
                platformEnvironment.transform.Rotate(Vector3.back * degree);
            }
            else
            {
                platformEnvironment.transform.Rotate(Vector3.forward * degree);

            }
            angle += degree;
            yield return null;
        }
        if (dir == e_dirRotation.RIGHT)
        {
            platformEnvironment.transform.eulerAngles = saveRotation + (Vector3.back * 45);
        }
        else
        {
            platformEnvironment.transform.eulerAngles = saveRotation + (Vector3.forward * 45);
        }
    }

    public void DestroyPlatform(GameObject thisPlatform)
    {
        instancePlatform.Remove(thisPlatform);
    }

    public bool Pause { get; set; }
    public bool GameStarted { get; set; }
}
