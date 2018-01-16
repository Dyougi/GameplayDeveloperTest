using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
    private bool haveToRestartAfterBackground;

    private Coroutine restartAfterBackground;

    private static GameManager instance;

    private PathPlatform[] pathPlatform;

    public delegate void PathAction(PathPlatform path);
    public static event PathAction OnPath;

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

        Log.ClearFile();
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
        pathPlatform[0] = new PathPlatform(0, true, false);
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
                    float scaleRandom = Random.Range(4, 8);
                    for (int count = 0; count < 3; count++)
                    {
                        if (pathPlatform[count].used && !pathPlatform[count].justCreated)
                        {
                            GameObject obj = platformManagerInstance.CreatePlatform(pathPlatform[count].nextPosPlatform, speedPlatform, Vector3.zero, scaleRandom);
                        
                            pathPlatform[count].lastPosPlatform = pathPlatform[count].currentPosPlatform;
                            pathPlatform[count].currentPosPlatform = pathPlatform[count].nextPosPlatform;

                            if (MyRandom.ThrowOfDice(30))
                            {
                                AddPath(count);
                                if (OnPath != null)
                                    OnPath(pathPlatform[count]);
                            }
                        }
                    }
                    
                    for (int count = 0; count < 3; count++)
                    {
                        if (pathPlatform[count].used == true)
                        {
                            GetNewPosPlatform(count);
                            if (pathPlatform[count].justCreated)
                            {
                                pathPlatform[count].justCreated = false;
                            }
                        }
                    }
                    
                    VerifIfSimilarPath();
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

    void OnApplicationFocus(bool pauseStatus)
    {
        if (pauseStatus) // regain focus
        {
            if (GameStarted)
            {
                if (haveToRestartAfterBackground)
                {
                    interfaceManagerInstance.ShowRestartBackground(true);
                    restartAfterBackground = StartCoroutine(RestartAfterbackground());
                }
            }
        }
        else // losing focus
        {
            if (haveToRestartAfterBackground)
                StopCoroutine(restartAfterBackground);
            if (!Pause && GameStarted)
            {
                interfaceManagerInstance.ShowIngameUI(false);
                Pause = true;
                haveToRestartAfterBackground = true;
            }
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
        pathPlatform[0].lastPosPlatform = e_posPlatform.BOT;
        pathPlatform[1].used = false;
        pathPlatform[2].used = false;
        pathPlatform[1].justCreated = false;
        pathPlatform[2].justCreated = false;
        currentMinDistanceBetweenPlatform = minDistanceBetweenPlatform;
        currentMaxDistanceBetweenPlatform = maxDistanceBetweenPlatform;
        InitTerrain();
        platformManagerInstance.CurrentPlatformPlayer = platformManagerInstance.InstancesPlatform[0];
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
            
            for (int count = 0; count < 3; count++)
            {
                if (pathPlatform[count].used && !pathPlatform[count].justCreated)
                {
                    platformManagerInstance.CreatePlatform(pathPlatform[count].nextPosPlatform, speedPlatform, posPlatform, scaleRandom);

                    pathPlatform[count].lastPosPlatform = pathPlatform[count].currentPosPlatform;
                    pathPlatform[count].currentPosPlatform = pathPlatform[count].nextPosPlatform;

                    if (MyRandom.ThrowOfDice(30))
                        AddPath(count);
                }
            }
            
            for (int count = 0; count < 3; count++)
            {
                if (pathPlatform[count].used == true)
                {
                    GetNewPosPlatform(count);
                    if (pathPlatform[count].justCreated)
                    {
                        pathPlatform[count].justCreated = false;
                    }
                }
            }
            
            VerifIfSimilarPath();
            lastInstanceTime = (distanceBetweenPlatform + scaleRandom + offset) / speedPlatform;
            offset += (scaleRandom + distanceBetweenPlatform);
            posPlatform = Vector3.forward * offset;
        }
        platformManagerInstance.CurrentPlatform = platformManagerInstance.InstancesPlatform[0];
        platformManagerInstance.SecondPlatform = platformManagerInstance.InstancesPlatform[1];
    }


    bool VerifPlayerDeath()
    {
        if (playerInstance.transform.position.y < 0.6f)
        {
            Debug.Log("Dead");

            if (PlayerPrefs.GetInt("bestScore") < ScorePoint)
                PlayerPrefs.SetInt("bestScore", ScorePoint);

            GameStarted = false;
            interfaceManagerInstance.ShowIngameUI(false);
            interfaceManagerInstance.UpdateStats(ScorePoint);
            interfaceManagerInstance.ShowStats(true);
            isPlayerDead = true;
            initTime = MyTimer.Instance.TotalTimeSecond;
            audioSource.PlayOneShot(deathSound);
            playerInstance.StopVelocity();
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
        if (MyRandom.ThrowOfDice(50))
            pathPlatform[pathID].nextPosPlatform = pathPlatform[pathID].currentPosPlatform == e_posPlatform.BOTLEFT ? e_posPlatform.BOT : pathPlatform[pathID].currentPosPlatform + 1;
        else
            pathPlatform[pathID].nextPosPlatform = pathPlatform[pathID].currentPosPlatform == e_posPlatform.BOT ? e_posPlatform.BOTLEFT : pathPlatform[pathID].currentPosPlatform - 1;
    }

    void VerifIfSimilarPath()
    {
        for (int countPath = 2; countPath >= 0; countPath--)
        {
            for (int countComparePath = 2; countComparePath >= 0; countComparePath--)
            {
                if (countPath != countComparePath
                    && pathPlatform[countPath].used
                    && pathPlatform[countComparePath].used
                    && pathPlatform[countPath].nextPosPlatform == pathPlatform[countComparePath].nextPosPlatform)
                {
                    pathPlatform[countComparePath].used = false;
                }
            }
        }
    }

    void AddPath(int pathId)
    {
        for (int count = 1; count < 3; count++)
        {
            if (pathPlatform[count].used == false)
            {
                pathPlatform[count].used = true;
                pathPlatform[count].justCreated = true;
                pathPlatform[count].currentPosPlatform = pathPlatform[pathId].currentPosPlatform;
                pathPlatform[count].lastPosPlatform = pathPlatform[pathId].lastPosPlatform;
                return;
            }
        }
    }

    int ActivatePath()
    {
        int result = 0;
        for (int count = 1; count < pathPlatform.Length; count++)
        {
            if (pathPlatform[count].justCreated == true)
                pathPlatform[count].justCreated = false;
        }
        return result;
    }

    int CountNumberPath()
    {
        int result = 0;
        for (int count = 0; count < pathPlatform.Length; count++)
        {
            if (pathPlatform[count].used == true)
                result++;
        }
        return result;
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
        interfaceManagerInstance.UpdateScore(ScorePoint);
    }

    public void DoPause()
    {
        if (!Pause)
            Pause = true;
        else
            Pause = false;
    }

    public bool Pause
    {
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

    public PlatformManager PlatformManagerInstance
    {
        get
        {
            return platformManagerInstance;
        }
    }

    public InterfaceManager InterfaceManagerInstance
    {
        get
        {
            return interfaceManagerInstance;
        }
    }

    IEnumerator RestartAfterbackground()
    {
        int count = 3;

        while (count != 0)
        {
            interfaceManagerInstance.UpdateRestartBackground(count.ToString());
            count--;
            yield return new WaitForSeconds(1);
        }

        interfaceManagerInstance.UpdateRestartBackground("GO !");
        yield return new WaitForSeconds(1);
        interfaceManagerInstance.ShowRestartBackground(false);
        interfaceManagerInstance.ShowIngameUI(true);
        Pause = false;
        haveToRestartAfterBackground = false;
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