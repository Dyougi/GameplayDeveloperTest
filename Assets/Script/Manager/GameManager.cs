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
    private float MinDistanceBetweenPlatform;

    [SerializeField]
    private float MaxDistanceBetweenPlatform;

    [SerializeField]
    private int flagPlatformStart;

    [SerializeField]
    private int stepPlatform;

    [SerializeField]
    private GameObject platformDefault;

    [SerializeField]
    private GameObject platformEnvironment;

    [SerializeField]
    private InterfaceManager instanceInterfaceManager;

    [SerializeField]
    private PlatformManager platformManagerInstance;

    [SerializeField]
    private Camera camera;

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

    private static GameManager instance;

    public enum e_dirRotation { LEFT, RIGHT };

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
    }

    public static GameManager Instance
    {
        get { return instance; }
    }

    void Start()
    {
        GameStarted = false;
        Pause = false;
        PlayerController.OnJump += PlayerJumped;
        PlayerController.OnLand += PlayerLanded;
        isPlayerDead = false;
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
                if (lastInstanceTime + ratePlatform < MyTimer.Instance.TotalTime)
                {
                    int scaleRandom = Random.Range(4, 8);
                    platformManagerInstance.CreatePlatform(speedPlatform, Vector3.zero, scaleRandom);
                    distanceBetweenPlatform = Random.Range(MinDistanceBetweenPlatform, MaxDistanceBetweenPlatform);
                    ratePlatform = (distanceBetweenPlatform + scaleRandom) / speedPlatform;
                    lastInstanceTime = MyTimer.Instance.TotalTime;
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
            if (Input.GetTouch(0).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                if (isPlayerDead)
                {
                    if (initTime + 1 < MyTimer.Instance.TotalTime)
                    {
                        isPlayerDead = false;
                        platformEnvironment.transform.eulerAngles = Vector3.zero;
                        playerInstance.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        instanceInterfaceManager.ShowMenu(true);
                        InitGame();
                    }
                }
                else
                {
                    if (initTime + 1 < MyTimer.Instance.TotalTime)
                        StartGame();
                }
            }
        }
#endif

#if UNITY_ANDROID

        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                if (isPlayerDead)
                {
                    if (initTime + 1 < MyTimer.Instance.TotalTime)
                    {
                        isPlayerDead = false;
                        platformEnvironment.transform.eulerAngles = Vector3.zero;
                        playerInstance.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        instanceInterfaceManager.ShowMenu(true);
                        InitGame();
                    }
                }
                else
                {
                    if (initTime + 1 < MyTimer.Instance.TotalTime)
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
                if (initTime + 1 < MyTimer.Instance.TotalTime)
                {
                    isPlayerDead = false;
                    platformEnvironment.transform.eulerAngles = Vector3.zero;
                    playerInstance.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    instanceInterfaceManager.ShowMenu(true);
                    InitGame();
                }
            }
            else
            {
                if (initTime + 1 < MyTimer.Instance.TotalTime)
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
        instanceInterfaceManager.InitInterface();
        initTime = MyTimer.Instance.TotalTime;
        ScorePoint = 0;
        currentFlagPlatform = 0;
        currentStepPlatform = 0;
        flagPlatform = flagPlatformStart;
        platformManagerInstance.InitPlatform();
        platformManagerInstance.ClearInstancesPlatform();
        InitTerrain();
    }

    void InitTerrain()
    {
        Debug.Log("InitTerrain");
        float offset = -45;
        Vector3 posPlatform = Vector3.forward * offset;

        posPlatform += Vector3.up * 70;
        platformManagerInstance.CreatePlatform(speedPlatform, posPlatform, 6, false, false);
        distanceBetweenPlatform = Random.Range(MinDistanceBetweenPlatform, MaxDistanceBetweenPlatform);
        offset += (6 + distanceBetweenPlatform);
        posPlatform = Vector3.forward * offset;
        float scaleRandom;
        while (offset <= 0)
        {
            scaleRandom = Random.Range(4, 8);
            platformManagerInstance.CreatePlatform(speedPlatform, posPlatform, scaleRandom);
            lastInstanceTime = (distanceBetweenPlatform + scaleRandom + offset) / speedPlatform;
            offset += (scaleRandom + distanceBetweenPlatform);
            posPlatform = Vector3.forward * offset;
        }
        platformManagerInstance.CurrentPlatform = platformManagerInstance.InstancesPlatform[0];
        platformManagerInstance.SecondPlatform = platformManagerInstance.InstancesPlatform[1];
    }

    bool VerifPlayerDeath()
    {
        if (playerInstance.transform.position.y < 0)
        {
            Debug.Log("Dead");
            if (PlayerPrefs.GetInt("bestScore") < ScorePoint)
                PlayerPrefs.SetInt("bestScore", ScorePoint);
            GameStarted = false;
            instanceInterfaceManager.ShowIngameUI(false);
            instanceInterfaceManager.UpdateStats();
            instanceInterfaceManager.ShowStats(true);
            isPlayerDead = true;
            initTime = MyTimer.Instance.TotalTime;
            audioSource.PlayOneShot(deathSound);
            return true;
        }
        return false;
    }

    void RotatePlatformEnvironment(e_dirRotation dir)
    {
        StartCoroutine(DoRotatePlatformEnvironment(dir));
    }

    void PlayerLanded()
    {
        platformManagerInstance.CurrentPlatform = platformManagerInstance.SecondPlatform;
        platformManagerInstance.SecondPlatform = platformManagerInstance.InstancesPlatform[platformManagerInstance.InstancesPlatform.IndexOf(platformManagerInstance.SecondPlatform) + 1];
        ScorePoint += 1;
        instanceInterfaceManager.UpdateScore();
        if (currentStepPlatform < stepPlatform && currentFlagPlatform >= flagPlatform)
        {
            speedPlatform += 1f;
            platformManagerInstance.UpdateSpeedPlatform(speedPlatform);
            platformManagerInstance.UpdateColorPlatform();
            currentStepPlatform++;
            flagPlatform += 10;
            currentFlagPlatform = 0;
            audioSource.PlayOneShot(stepSound);
        }
        currentFlagPlatform++;
    }

    void PlayerJumped()
    {
        if (platformManagerInstance.CurrentPlatform.GetComponent<Platform>().PosPlatform == PlatformManager.e_posPlatform.BOT)
        {
            if (platformManagerInstance.SecondPlatform.GetComponent<Platform>().PosPlatform == PlatformManager.e_posPlatform.BOTRIGHT)
                RotatePlatformEnvironment(e_dirRotation.RIGHT);
            else
                RotatePlatformEnvironment(e_dirRotation.LEFT);
            return;
        }
        if (platformManagerInstance.CurrentPlatform.GetComponent<Platform>().PosPlatform == PlatformManager.e_posPlatform.BOTLEFT)
        {
            if (platformManagerInstance.SecondPlatform.GetComponent<Platform>().PosPlatform == PlatformManager.e_posPlatform.BOT)
                RotatePlatformEnvironment(e_dirRotation.RIGHT);
            else
                RotatePlatformEnvironment(e_dirRotation.LEFT);
            return;
        }
        if (platformManagerInstance.CurrentPlatform.GetComponent<Platform>().PosPlatform < platformManagerInstance.SecondPlatform.GetComponent<Platform>().PosPlatform)
        {
            RotatePlatformEnvironment(e_dirRotation.RIGHT);
            return;
        }
        if (platformManagerInstance.CurrentPlatform.GetComponent<Platform>().PosPlatform > platformManagerInstance.SecondPlatform.GetComponent<Platform>().PosPlatform)
        {
            RotatePlatformEnvironment(e_dirRotation.LEFT);
            return;
        }
    }

    public void StartGame()
    {
        Debug.Log("StartGame");
        startTime = MyTimer.Instance.TotalTime;
        lastInstanceTime += MyTimer.Instance.TotalTime;
        instanceInterfaceManager.ShowMenu(false);
        instanceInterfaceManager.ShowIngameUI(true);
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
        instanceInterfaceManager.UpdateScore();
    }
    public bool Pause { get; set; }
    public bool GameStarted { get; set; }
    public int ScorePoint { get; set; }
    public float ScoreTime
    {
        get
        {
            return MyTimer.Instance.TotalTime - startTime;
        }

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


    IEnumerator DoLerpColorCamera(Color start, Color end)
    {
        float ElapsedTime = 0.0f;
        while (ElapsedTime <= 1.0f)
        {
            camera.backgroundColor = Color.Lerp(start, end, ElapsedTime);
            ElapsedTime += Time.deltaTime * 2;
            yield return null;
        }
        camera.backgroundColor = end;
    }
}