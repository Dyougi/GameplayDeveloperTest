using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private Transform positionDeathPlayer;

    [SerializeField]
    private Transform positionFirstPlatform;

    [SerializeField]
    private PlayerController playerInstance;

    [SerializeField]
    private float speedPlatformStart;

    [SerializeField]
    private float minDistanceBetweenPlatform;

    [SerializeField]
    private float maxDistanceBetweenPlatform;

    [SerializeField]
    private int flagPlatformStart;

    [SerializeField]
    private int stepPlatform;

    [SerializeField]
    private GameObject platformDefault;

    [SerializeField]
    private GameObject platformEnvironment;

    [SerializeField]
    private InterfaceManager interfaceManagerInstance;

    [SerializeField]
    private PlatformManager platformManagerInstance;

    [SerializeField]
    private AudioClip startGameSound;

    [SerializeField]
    private AudioClip deathSound;

    [SerializeField]
    private AudioClip stepSound;

    private float ratePlatform;
    private AudioSource audioSource;

    private float lastInstanceTime;
    private float startTime;
    private float initTime;
    private bool isPlayerDead;
    private float speedPlatform;
    private int currentFlagPlatform;
    private int flagPlatform;
    private int currentStepPlatform;
    private float distanceBetweenPlatform;
    private float currentMinDistanceBetweenPlatform;
    private float currentMaxDistanceBetweenPlatform;
    private bool isPaused;

    private static GameManager instance;

    private PathPlatform[] pathPlatform;

    public enum e_dirRotation { LEFT, RIGHT };
    public enum e_posPlatform { BOT, BOTRIGHT, RIGHT, TOPRIGHT, TOP, TOPLEFT, LEFT, BOTLEFT };

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.LogWarning("Singleton " + this.name + " : instance here already");
            Destroy(gameObject);
            return;
        }
        audioSource = GetComponent<AudioSource>();
        pathPlatform = new PathPlatform[3];
    }

    public static GameManager Instance
    {
        get { return instance; }
    }

    void Start()
    {
        GameStarted = false;
        isPaused = false;
        PlayerController.OnJump += PlayerJumped;
        PlayerController.OnLand += PlayerLanded;
        isPlayerDead = false;
        pathPlatform[0] = new PathPlatform(0, true);
        pathPlatform[1] = new PathPlatform(1);
        pathPlatform[2] = new PathPlatform(2);
        InitGame();
    }


    void Update()
    {
        if (GameStarted)
        {
            if (!Pause)
            {
                if (VerifPlayerDeath())
                    return;
                if (lastInstanceTime + ratePlatform < MyTimer.Instance.TotalTimeSecond)
                {
                    int scaleRandom = Random.Range(4, 8);
                    if (MyRandom.ThrowOfDice(30))
                        AddPath();
                    for (int count = 0; count < 3; count++)
                    {
                        if (pathPlatform[count].used == true)
                        {
                            platformManagerInstance.CreatePlatform(pathPlatform[count].currentPosPlatform, speedPlatform, Vector3.zero, scaleRandom);
                            GetNewPosPlatform(count);
                        }
                    }
                    distanceBetweenPlatform = Random.Range(currentMinDistanceBetweenPlatform, currentMaxDistanceBetweenPlatform);
                    ratePlatform = (distanceBetweenPlatform + scaleRandom) / speedPlatform;
                    lastInstanceTime = MyTimer.Instance.TotalTimeSecond;
                }
            }
        }
        else
        {
            ManageInput();
        }
    }

    void ManageInput()
    {
#if UNITY_IOS
 if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (isPlayerDead)
                {
                    if (initTime + 1 < MyTimer.Instance.TotalTimeSecond)
                    {
                        isPlayerDead = false;
                        platformEnvironment.transform.eulerAngles = Vector3.zero;
                        playerInstance.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        interfaceManagerInstance.ShowMenu(true);
                        InitGame();
                    }
                }
                else
                {
                    if (initTime + 1 < MyTimer.Instance.TotalTimeSecond)
                        StartGame();
                }
            }
        }
#endif

#if UNITY_ANDROID

        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (isPlayerDead)
                {
                    if (initTime + 1 < MyTimer.Instance.TotalTimeSecond)
                    {
                        isPlayerDead = false;
                        platformEnvironment.transform.eulerAngles = Vector3.zero;
                        playerInstance.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        interfaceManagerInstance.ShowMenu(true);
                        InitGame();
                    }
                }
                else
                {
                    if (initTime + 1 < MyTimer.Instance.TotalTimeSecond)
                        StartGame();
                }
            }
        }
#endif
#if UNITY_EDITOR
        if (Input.GetKeyDown("a"))
        {
            if (isPlayerDead)
            {
                if (initTime + 1 < MyTimer.Instance.TotalTimeSecond)
                {
                    isPlayerDead = false;
                    platformEnvironment.transform.eulerAngles = Vector3.zero;
                    playerInstance.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    interfaceManagerInstance.ShowMenu(true);
                    InitGame();
                }
            }
            else
            {
                if (initTime + 1 < MyTimer.Instance.TotalTimeSecond)
                    StartGame();
            }
        }
#endif
    }

    void InitGame()
    {
        Debug.Log("InitGame");
        speedPlatform = speedPlatformStart;
        playerInstance.InitPlayer();
        ratePlatform = 0;
        interfaceManagerInstance.InitInterface();
        initTime = MyTimer.Instance.TotalTimeSecond;
        ScorePoint = 0;
        currentFlagPlatform = 0;
        currentStepPlatform = 0;
        flagPlatform = flagPlatformStart;
        platformManagerInstance.InitPlatform();
        platformManagerInstance.ClearInstancesPlatform();
        pathPlatform[0].currentPosPlatform = e_posPlatform.BOT;
        pathPlatform[1].used = false;
        pathPlatform[2].used = false;
        currentMinDistanceBetweenPlatform = minDistanceBetweenPlatform;
        currentMaxDistanceBetweenPlatform = maxDistanceBetweenPlatform;
        InitTerrain();
    }

    void InitTerrain()
    {
        Debug.Log("InitTerrain");
        float offset = -45;
        Vector3 posPlatform = Vector3.forward * offset;

        posPlatform += Vector3.up * 70;
        platformManagerInstance.CreatePlatform(pathPlatform[0].currentPosPlatform, speedPlatform, posPlatform, 6, false, false);
        GetNewPosPlatform(0);
        distanceBetweenPlatform = Random.Range(currentMinDistanceBetweenPlatform, currentMaxDistanceBetweenPlatform);
        offset += (6 + distanceBetweenPlatform);
        posPlatform = Vector3.forward * offset;
        float scaleRandom;
        while (offset <= 0)
        {
            scaleRandom = Random.Range(4, 8);
            if (MyRandom.ThrowOfDice(30))
                AddPath();
            for (int count = 0; count < 3; count++)
            {
                if (pathPlatform[count].used == true)
                {
                    platformManagerInstance.CreatePlatform(pathPlatform[count].currentPosPlatform, speedPlatform, posPlatform, scaleRandom);
                    GetNewPosPlatform(count);
                }
            }
            lastInstanceTime = (distanceBetweenPlatform + scaleRandom + offset) / speedPlatform;
            offset += (scaleRandom + distanceBetweenPlatform);
            posPlatform = Vector3.forward * offset;
        }
        platformManagerInstance.CurrentPlatform = platformManagerInstance.InstancesPlatform[0];
        platformManagerInstance.SecondPlatform = platformManagerInstance.InstancesPlatform[1];
    }


    bool VerifPlayerDeath()
    {
        if (playerInstance.transform.position.y < 0.7f)
        {
            Debug.Log("Dead");
            if (PlayerPrefs.GetInt("bestScore") < ScorePoint)
                PlayerPrefs.SetInt("bestScore", ScorePoint);
            GameStarted = false;
            interfaceManagerInstance.ShowIngameUI(false);
            interfaceManagerInstance.UpdateStats();
            interfaceManagerInstance.ShowStats(true);
            isPlayerDead = true;
            initTime = MyTimer.Instance.TotalTimeSecond;
            audioSource.PlayOneShot(deathSound);
            return true;
        }
        return false;
    }

    void RotatePlatformEnvironment(e_dirRotation dir)
    {
        StartCoroutine(DoRotatePlatformEnvironment(dir));
    }

    void PlayerLanded(PlayerController.e_jump jump)
    {
        platformManagerInstance.CurrentPlatform = platformManagerInstance.SecondPlatform;
        platformManagerInstance.SecondPlatform = platformManagerInstance.InstancesPlatform[platformManagerInstance.InstancesPlatform.IndexOf(platformManagerInstance.SecondPlatform) + 1];
        ScorePoint += 1;
        interfaceManagerInstance.UpdateScore();
        if (currentStepPlatform < stepPlatform && currentFlagPlatform >= flagPlatform)
        {
            speedPlatform += 1f;
            platformManagerInstance.UpdateSpeedPlatform(speedPlatform);
            platformManagerInstance.UpdateColorPlatform();
            currentStepPlatform++;
            flagPlatform += 10;
            currentFlagPlatform = 0;
            audioSource.PlayOneShot(stepSound);
            currentMinDistanceBetweenPlatform += 0.2f;
            currentMaxDistanceBetweenPlatform += 0.5f;
        }
        currentFlagPlatform++;
    }

    void PlayerJumped(PlayerController.e_jump jump)
    {
        if (jump == PlayerController.e_jump.LEFT)
        {
            RotatePlatformEnvironment(e_dirRotation.LEFT);
        }
        else
        {
            RotatePlatformEnvironment(e_dirRotation.RIGHT);
        }
    }

    void GetNewPosPlatform(int pathID)
    {
        pathPlatform[pathID].lastPosPlatform = pathPlatform[pathID].currentPosPlatform;
        if (MyRandom.ThrowOfDice(50))
            pathPlatform[pathID].currentPosPlatform = pathPlatform[pathID].currentPosPlatform == e_posPlatform.BOTLEFT ? e_posPlatform.BOT : pathPlatform[pathID].currentPosPlatform + 1;
        else
            pathPlatform[pathID].currentPosPlatform = pathPlatform[pathID].currentPosPlatform == e_posPlatform.BOT ? e_posPlatform.BOTLEFT : pathPlatform[pathID].currentPosPlatform - 1;
        if (pathPlatform[2].used == true && (pathPlatform[2].currentPosPlatform == pathPlatform[1].currentPosPlatform ||
            pathPlatform[2].currentPosPlatform == pathPlatform[0].currentPosPlatform))
        {
            pathPlatform[2].used = false;
        }
        if (pathPlatform[1].used == true && pathPlatform[1].currentPosPlatform == pathPlatform[0].currentPosPlatform)
        {
            pathPlatform[1].used = false;
        }
    }

    void AddPath()
    {
        for (int count = 1; count < 3; count++)
        {
            if (pathPlatform[count].used == false)
            {
                pathPlatform[count].used = true;
                pathPlatform[count].currentPosPlatform = pathPlatform[count - 1].lastPosPlatform;
                pathPlatform[count].lastPosPlatform = pathPlatform[count - 1].currentPosPlatform;
                GetNewPosPlatform(count);
                return;
            }
        }
    }

    public void StartGame()
    {
        Debug.Log("StartGame");
        startTime = MyTimer.Instance.TotalTimeSecond;
        lastInstanceTime += MyTimer.Instance.TotalTimeSecond;
        interfaceManagerInstance.ShowMenu(false);
        interfaceManagerInstance.ShowIngameUI(true);
        GameStarted = true;
        audioSource.PlayOneShot(startGameSound);
    }

    public void DestroyPlatform(GameObject thisPlatform)
    {
        platformManagerInstance.DestroyPlatform(thisPlatform);
    }

    public void AddScore(int point)
    {
        ScorePoint += point;
        interfaceManagerInstance.UpdateScore();
    }

    public void DoPause()
    {
        if (!Pause)
            Pause = true;
        else
            Pause = false;
    }

    public bool Pause {
        get
        {
            return isPaused;
        }
        set
        {
            isPaused = value;
            interfaceManagerInstance.Pause = value;
            playerInstance.Pause = value;
            platformManagerInstance.Pause = value;
            MyTimer.Instance.Pause = value;
        }
    }
    public bool GameStarted { get; set; }
    public int ScorePoint { get; set; }
    public float ScoreTime
    {
        get
        {
            return MyTimer.Instance.TotalTimeSecond - startTime;
        }

    }

    IEnumerator DoRotatePlatformEnvironment(e_dirRotation dir)
    {
        float angle = 0.0f;
        float degree;
        Vector3 saveRotation = platformEnvironment.transform.eulerAngles;
        while (angle < 45.0f)
        {
            if (!Pause)
            {
                degree = Time.deltaTime * 250;
                if (dir == e_dirRotation.RIGHT)
                {
                    platformEnvironment.transform.Rotate(Vector3.back * degree);
                }
                else
                {
                    platformEnvironment.transform.Rotate(Vector3.forward * degree);

                }
                if (angle + degree > 45.0f)
                {
                    if (dir == e_dirRotation.RIGHT)
                    {
                        platformEnvironment.transform.eulerAngles = saveRotation + (Vector3.back * 45);
                    }
                    else
                    {
                        platformEnvironment.transform.eulerAngles = saveRotation + (Vector3.forward * 45);
                    }
                }
                angle += degree;
            }
            yield return null;
        }
    }
}