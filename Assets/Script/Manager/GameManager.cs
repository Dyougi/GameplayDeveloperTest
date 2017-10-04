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
    private float distanceBetweenPlatform;

    [SerializeField]
    private GameObject platformDefault;

    [SerializeField]
    private GameObject platformStart;

    [SerializeField]
    private GameObject platformEnvironment;

    [SerializeField]
    private InterfaceManager instanceInterfaceManager;

    [SerializeField]
    private PlatformManager platformManagerInstance;

    [SerializeField]
    private Camera camera;

    [SerializeField]
    private AudioClip deathSound;

    [SerializeField]
    private AudioClip startGameSound;

    private float ratePlatform;
    private AudioSource audioSource;

    private float lastInstanceTime;
    private float startTime;
    private float initTime;
    private bool isPlayerDead;
    private float speedPlatform;

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
                    speedPlatform += 0.05f;
                    platformManagerInstance.UpdateSpeedPlatform(speedPlatform);
                    int scaleRandom = Random.Range(4, 8);
                    int colorRandom = Random.Range(0, 3);
                    platformManagerInstance.CreatePlatform(speedPlatform, Vector3.zero, (PlatformManager.e_colorPlatform)colorRandom, scaleRandom, true);
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
        camera.backgroundColor = GetColorFromEnum(PlatformManager.e_colorPlatform.BLUE);
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
        platformManagerInstance.CreatePlatform(speedPlatform, posPlatform, PlatformManager.e_colorPlatform.BLUE, 6, false);

        offset += (6 + distanceBetweenPlatform);
        posPlatform = Vector3.forward * offset;
        float scaleRandom;
        int colorRandom;
        while (offset <= 0)
        {
            scaleRandom = Random.Range(4, 8);
            colorRandom = Random.Range(0, 3);
            platformManagerInstance.CreatePlatform(speedPlatform, posPlatform, (PlatformManager.e_colorPlatform)colorRandom, scaleRandom, true);
            lastInstanceTime = (distanceBetweenPlatform + scaleRandom + offset) / speedPlatform;
            offset += (scaleRandom + distanceBetweenPlatform);
            posPlatform = Vector3.forward * offset;
        }
        platformManagerInstance.CurrentPlatform = platformManagerInstance.InstancesPlatform[0];
        platformManagerInstance.SecondPlatform = platformManagerInstance.InstancesPlatform[1];
        platformManagerInstance.UpdateOpacityPlatform();
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
        ScorePoint += 2;
        instanceInterfaceManager.UpdateScore();
    }

    void PlayerJumped()
    {
        if (platformManagerInstance.CurrentPlatform.GetComponent<Platform>().PosPlatform == 0)
        {
            if (platformManagerInstance.SecondPlatform.GetComponent<Platform>().PosPlatform == (PlatformManager.e_posPlatform)1)
                RotatePlatformEnvironment(e_dirRotation.RIGHT);
            else
                RotatePlatformEnvironment(e_dirRotation.LEFT);
            return;
        }
        if (platformManagerInstance.CurrentPlatform.GetComponent<Platform>().PosPlatform == (PlatformManager.e_posPlatform)7)
        {
            if (platformManagerInstance.SecondPlatform.GetComponent<Platform>().PosPlatform == 0)
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

    Color GetColorFromEnum(PlatformManager.e_colorPlatform newColor)
    {
        Color result = Color.blue;
        float color = 200f / 255f;
        if (newColor == PlatformManager.e_colorPlatform.BLUE)
        {
            result = new Color(0.1f, 0.1f, color);
        }
        if (newColor == PlatformManager.e_colorPlatform.GREEN)
        {
            result = new Color(0.1f, color, 0.1f);
        }
        if (newColor == PlatformManager.e_colorPlatform.RED)
        {
            result = new Color(color, 0.1f, 0.1f);
        }
        return result;
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

    public void ButtonLeftPressed()
    {
        if (instanceInterfaceManager.CurrentButtonLeftColor == PlatformManager.e_colorPlatform.BLUE &&
            instanceInterfaceManager.CurrentButtonRightColor == PlatformManager.e_colorPlatform.GREEN)
        {
            instanceInterfaceManager.ChangeColorButtonLeft(PlatformManager.e_colorPlatform.RED);
            StartCoroutine(DoLerpColor(PlatformManager.e_colorPlatform.BLUE));
            platformManagerInstance.CurrentColorPlatform = PlatformManager.e_colorPlatform.BLUE;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == PlatformManager.e_colorPlatform.BLUE &&
            instanceInterfaceManager.CurrentButtonRightColor == PlatformManager.e_colorPlatform.RED)
        {
            instanceInterfaceManager.ChangeColorButtonLeft(PlatformManager.e_colorPlatform.GREEN);
            StartCoroutine(DoLerpColor(PlatformManager.e_colorPlatform.BLUE));
            platformManagerInstance.CurrentColorPlatform = PlatformManager.e_colorPlatform.BLUE;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == PlatformManager.e_colorPlatform.GREEN &&
            instanceInterfaceManager.CurrentButtonRightColor == PlatformManager.e_colorPlatform.BLUE)
        {
            instanceInterfaceManager.ChangeColorButtonLeft(PlatformManager.e_colorPlatform.RED);
            StartCoroutine(DoLerpColor(PlatformManager.e_colorPlatform.GREEN));
            platformManagerInstance.CurrentColorPlatform = PlatformManager.e_colorPlatform.GREEN;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == PlatformManager.e_colorPlatform.GREEN &&
            instanceInterfaceManager.CurrentButtonRightColor == PlatformManager.e_colorPlatform.RED)
        {
            instanceInterfaceManager.ChangeColorButtonLeft(PlatformManager.e_colorPlatform.BLUE);
            StartCoroutine(DoLerpColor(PlatformManager.e_colorPlatform.GREEN));
            platformManagerInstance.CurrentColorPlatform = PlatformManager.e_colorPlatform.GREEN;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == PlatformManager.e_colorPlatform.RED &&
            instanceInterfaceManager.CurrentButtonRightColor == PlatformManager.e_colorPlatform.BLUE)
        {
            instanceInterfaceManager.ChangeColorButtonLeft(PlatformManager.e_colorPlatform.GREEN);
            StartCoroutine(DoLerpColor(PlatformManager.e_colorPlatform.RED));
            platformManagerInstance.CurrentColorPlatform = PlatformManager.e_colorPlatform.RED;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == PlatformManager.e_colorPlatform.RED &&
            instanceInterfaceManager.CurrentButtonRightColor == PlatformManager.e_colorPlatform.GREEN)
        {
            instanceInterfaceManager.ChangeColorButtonLeft(PlatformManager.e_colorPlatform.BLUE);
            StartCoroutine(DoLerpColor(PlatformManager.e_colorPlatform.RED));
            platformManagerInstance.CurrentColorPlatform = PlatformManager.e_colorPlatform.RED;
        }
        platformManagerInstance.UpdateOpacityPlatform();
    }

    public void ButtonRightPressed()
    {
        StartCoroutine(DoLerpColor(platformManagerInstance.CurrentColorPlatform));
        if (instanceInterfaceManager.CurrentButtonLeftColor == PlatformManager.e_colorPlatform.BLUE &&
            instanceInterfaceManager.CurrentButtonRightColor == PlatformManager.e_colorPlatform.GREEN)
        {
            instanceInterfaceManager.ChangeColorButtonRight(PlatformManager.e_colorPlatform.RED);
            StartCoroutine(DoLerpColor(PlatformManager.e_colorPlatform.GREEN));
            platformManagerInstance.CurrentColorPlatform = PlatformManager.e_colorPlatform.GREEN;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == PlatformManager.e_colorPlatform.BLUE &&
            instanceInterfaceManager.CurrentButtonRightColor == PlatformManager.e_colorPlatform.RED)
        {
            instanceInterfaceManager.ChangeColorButtonRight(PlatformManager.e_colorPlatform.GREEN);
            StartCoroutine(DoLerpColor(PlatformManager.e_colorPlatform.GREEN));
            StartCoroutine(DoLerpColor(PlatformManager.e_colorPlatform.RED));
            platformManagerInstance.CurrentColorPlatform = PlatformManager.e_colorPlatform.RED;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == PlatformManager.e_colorPlatform.GREEN &&
            instanceInterfaceManager.CurrentButtonRightColor == PlatformManager.e_colorPlatform.BLUE)
        {
            instanceInterfaceManager.ChangeColorButtonRight(PlatformManager.e_colorPlatform.RED);
            StartCoroutine(DoLerpColor(PlatformManager.e_colorPlatform.BLUE));
            platformManagerInstance.CurrentColorPlatform = PlatformManager.e_colorPlatform.BLUE;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == PlatformManager.e_colorPlatform.GREEN &&
            instanceInterfaceManager.CurrentButtonRightColor == PlatformManager.e_colorPlatform.RED)
        {
            instanceInterfaceManager.ChangeColorButtonRight(PlatformManager.e_colorPlatform.BLUE);
            StartCoroutine(DoLerpColor(PlatformManager.e_colorPlatform.RED));
            platformManagerInstance.CurrentColorPlatform = PlatformManager.e_colorPlatform.RED;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == PlatformManager.e_colorPlatform.RED &&
            instanceInterfaceManager.CurrentButtonRightColor == PlatformManager.e_colorPlatform.BLUE)
        {
            instanceInterfaceManager.ChangeColorButtonRight(PlatformManager.e_colorPlatform.GREEN);
            StartCoroutine(DoLerpColor(PlatformManager.e_colorPlatform.BLUE));
            platformManagerInstance.CurrentColorPlatform = PlatformManager.e_colorPlatform.BLUE;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == PlatformManager.e_colorPlatform.RED &&
            instanceInterfaceManager.CurrentButtonRightColor == PlatformManager.e_colorPlatform.GREEN)
        {
            instanceInterfaceManager.ChangeColorButtonRight(PlatformManager.e_colorPlatform.BLUE);
            StartCoroutine(DoLerpColor(PlatformManager.e_colorPlatform.GREEN));
            platformManagerInstance.CurrentColorPlatform = PlatformManager.e_colorPlatform.GREEN;
        }
        platformManagerInstance.UpdateOpacityPlatform();
    }

    public void DestroyPlatform(GameObject thisPlatform)
    {
        platformManagerInstance.DestroyPlatform(thisPlatform);
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


    IEnumerator DoLerpColor(PlatformManager.e_colorPlatform newColorPlat)
    {
        float ElapsedTime = 0.0f;
        Color beforeColor = GetColorFromEnum(platformManagerInstance.CurrentColorPlatform);
        Color afterColor = GetColorFromEnum(newColorPlat);
        while (ElapsedTime <= 1.0f)
        {
            camera.backgroundColor = Color.Lerp(beforeColor, afterColor, ElapsedTime);
            ElapsedTime += Time.deltaTime * 2;
            yield return null;
        }
        camera.backgroundColor = afterColor;
    }
}