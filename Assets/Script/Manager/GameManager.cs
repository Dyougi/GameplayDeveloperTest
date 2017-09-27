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
    private e_posPlatform currentPosPlatform;

    private static GameManager instance;

    public enum e_posPlatform { BOT, BOTRIGHT, RIGHT, TOPRIGHT, TOP, TOPLEFT, LEFT, BOTLEFT };

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
        Pause = true;
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
                    lastInstanceTime = MyTimer.Instance.TotalTime;
                    GameObject currentInstance = Instantiate(platformDefault, positionPlatformStart[(int)currentPosPlatform]);
                    currentInstance.transform.parent = platformEnvironment.transform;
                    currentInstance.GetComponent<Platform>().Speed = speedPlatform;
                    currentInstance.GetComponent<Platform>().PosPlatform = currentPosPlatform;
                    instancePlatform.Add(currentInstance);
                    GetNewPosPlatform();
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
        currentInstance.GetComponent<Platform>().Init(0, posPlatformStart, posPlatformStart, false);
        instancePlatform.Add(currentInstance);
        for (int count = 0; count < 9; count++)
        {
            currentInstance = Instantiate(platformDefault, positionPlatformCreate[(int)currentPosPlatform].position, positionPlatformCreate[(int)currentPosPlatform].rotation);
            currentInstance.transform.parent = platformEnvironment.transform;
            currentInstance.GetComponent<Platform>().Init(speedPlatform, positionPlatformCreate[(int)currentPosPlatform].position, positionPlatformStart[(int)currentPosPlatform].position + posPlatform);
            currentInstance.GetComponent<Platform>().PosPlatform = currentPosPlatform;
            instancePlatform.Add(currentInstance);
            GetNewPosPlatform();
            offset += 5;
            posPlatform = Vector3.forward * offset;
        }
        currentPlatform = instancePlatform[0];
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

    void PlayerJumped()
    {
        if (instancePlatform[1].GetComponent<Platform>().PosPlatform - currentPlatform.GetComponent<Platform>().PosPlatform > 0)
            RotatePlatformEnvironment(45);
        if (instancePlatform[1].GetComponent<Platform>().PosPlatform - currentPlatform.GetComponent<Platform>().PosPlatform < 0)
            RotatePlatformEnvironment(-45);
    }

    void RotatePlatformEnvironment(int degree)
    {
        platformEnvironment.transform.Rotate(Vector3.forward * degree);
    }

    public bool Pause { get; set; }
    public bool GameStarted { get; set; }
}
