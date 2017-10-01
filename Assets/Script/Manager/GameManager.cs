using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {

    [SerializeField]
    private Transform[] positionPlatformStart;

    [SerializeField]
    private Transform[] positionPlatformEnd;

    [SerializeField]
    private Material[] platformMaterial;

    [SerializeField]
    private Transform positionPlatformDestroy;

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
    private Camera camera;

    [SerializeField]
    private AudioClip deathSound;

    [SerializeField]
    private AudioClip startGameSound;

    private float ratePlatform;
    private List<GameObject> instancePlatform;
    private AudioSource audioSource;

    private float lastInstanceTime;
    private GameObject currentPlatform;
    private GameObject secondPlatform;
    private e_posPlatform currentPosPlatform;
    private int idPlatform;
    private float startTime;
    private float initTime;
    private bool isPlayerDead;
    private float speedPlatform;

    private e_colorPlatform currentColorPlatform;

    private static GameManager instance;

    public enum e_posPlatform { BOT, BOTRIGHT, RIGHT, TOPRIGHT, TOP, TOPLEFT, LEFT, BOTLEFT };
    public enum e_dirRotation { LEFT, RIGHT };
    public enum e_colorPlatform { BLUE, GREEN, RED };

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

    void Start ()
    {
        GameStarted = false;
        Pause = false;
        instancePlatform = new List<GameObject>();
        PlayerController.OnJump += PlayerJumped;
        PlayerController.OnLand += PlayerLanded;
        isPlayerDead = false;
        InitGame();
    }
	
	void Update ()
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
                    Debug.Log("INSTANCE " + idPlatform);
                    GameObject currentInstance = Instantiate(platformDefault, positionPlatformStart[(int)currentPosPlatform].position, positionPlatformStart[(int)currentPosPlatform].rotation);
                    currentInstance.name = "Platform " + idPlatform;
                    currentInstance.transform.parent = platformEnvironment.transform;
                    currentInstance.GetComponent<Platform>().Init(speedPlatform, positionPlatformStart[(int)currentPosPlatform], positionPlatformEnd[(int)currentPosPlatform], positionPlatformDestroy.position, Vector3.zero);
                    currentInstance.GetComponent<Platform>().PosPlatform = currentPosPlatform;
                    currentInstance.GetComponent<Platform>().Id = idPlatform;
                    int randomNumber = Random.Range(0, 3);
                    currentInstance.GetComponentInChildren<MeshRenderer>().material = platformMaterial[randomNumber];
                    currentInstance.GetComponent<Platform>().ColorPlatform = (e_colorPlatform)randomNumber;
                    if (currentInstance.GetComponent<Platform>().ColorPlatform != currentColorPlatform)
                        currentInstance.GetComponent<Platform>().ChangeOpacity(true);
                    else
                        currentInstance.GetComponent<Platform>().ChangeOpacity(false);
                    randomNumber = Random.Range(3, 6);
                    currentInstance.transform.localScale = new Vector3(currentInstance.transform.localScale.x, currentInstance.transform.localScale.y, randomNumber);
                    ratePlatform = (distanceBetweenPlatform + randomNumber) / speedPlatform;
                    idPlatform++;
                    instancePlatform.Add(currentInstance);
                    GetNewPosPlatform();
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
        speedPlatform = speedPlatformStart;
        playerInstance.InitPlayer();
        ratePlatform = (distanceBetweenPlatform + 4) / speedPlatform;
        currentPosPlatform = e_posPlatform.BOT;
        idPlatform = 0;
        instanceInterfaceManager.InitInterface();
        initTime = MyTimer.Instance.TotalTime;
        ScorePoint = 0;
        currentColorPlatform = e_colorPlatform.BLUE;
        camera.backgroundColor = GetColorFromEnum(e_colorPlatform.BLUE);
        instancePlatform.ForEach(delegate (GameObject obj)
        {
            Destroy(obj);
        });
        instancePlatform.Clear();
        InitTerrain();
    }

    void InitTerrain()
    {
        float offset = -45;
        Vector3 posPlatform = Vector3.forward * offset;
        Vector3 posPlatformStart = new Vector3(0, 0, -1);

        GameObject currentInstance = Instantiate(platformStart, posPlatformStart, Quaternion.identity);
        currentInstance.transform.parent = platformEnvironment.transform;
        currentInstance.name = "Platform START " + idPlatform;
        currentInstance.GetComponent<Platform>().Init(speedPlatform, positionFirstPlatform, positionFirstPlatform, positionPlatformDestroy.position, Vector3.zero, false);
        currentInstance.GetComponent<Platform>().PosPlatform = currentPosPlatform;
        currentInstance.GetComponent<Platform>().Id = idPlatform;
        currentInstance.GetComponent<Platform>().ChangeOpacity(false);
        offset += (4 + distanceBetweenPlatform);
        idPlatform++;
        GetNewPosPlatform();
        instancePlatform.Add(currentInstance);
        posPlatform = Vector3.forward * offset;
        int randomNumber;
        while (offset <= 0)
        {
            currentInstance = Instantiate(platformDefault, positionPlatformStart[(int)currentPosPlatform].position, positionPlatformStart[(int)currentPosPlatform].rotation);
            currentInstance.transform.parent = platformEnvironment.transform;
            currentInstance.name = "Platform START " + idPlatform;
            currentInstance.GetComponent<Platform>().Init(speedPlatform, positionPlatformStart[(int)currentPosPlatform], positionPlatformEnd[(int)currentPosPlatform], positionPlatformDestroy.position, posPlatform);
            currentInstance.GetComponent<Platform>().PosPlatform = currentPosPlatform;
            currentInstance.GetComponent<Platform>().Id = idPlatform;
            randomNumber = Random.Range(0, 3);
            currentInstance.GetComponentInChildren<MeshRenderer>().material = platformMaterial[randomNumber];
            currentInstance.GetComponent<Platform>().ColorPlatform = (e_colorPlatform)randomNumber;
            idPlatform++;
            instancePlatform.Add(currentInstance);
            GetNewPosPlatform();
            lastInstanceTime = (((distanceBetweenPlatform + 4) - Mathf.Abs(offset)) / speedPlatform);
            randomNumber = Random.Range(3, 6);
            currentInstance.transform.localScale = new Vector3(currentInstance.transform.localScale.x, currentInstance.transform.localScale.y, randomNumber);
            offset += (randomNumber + distanceBetweenPlatform);
            posPlatform = Vector3.forward * offset;
        }
        currentPlatform = instancePlatform[0];
        secondPlatform = instancePlatform[1];
        UpdateStatPlatform();
    }

    bool VerifPlayerDeath()
    {
        if (playerInstance.transform.position.y < 0)
        {
            if (PlayerPrefs.GetInt("bestScore") < ScorePoint)
                PlayerPrefs.SetInt("bestScore", ScorePoint);
            GameStarted = false;
            instanceInterfaceManager.ShowButtonsColor(false);
            instanceInterfaceManager.UpdateStats();
            instanceInterfaceManager.ShowStats(true);
            isPlayerDead = true;
            Debug.Log("Dead !");
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

    void UpdateStatPlatform()
    {
        instancePlatform.ForEach(delegate(GameObject obj)
        {
            if (currentPlatform != obj)
            {
                if (obj.GetComponent<Platform>().ColorPlatform == currentColorPlatform)
                {
                    obj.GetComponent<Platform>().ChangeOpacity(false);
                }
                else
                {
                    obj.GetComponent<Platform>().ChangeOpacity(true);
                }
            }
        });
    }


    void PlayerLanded()
    {
        currentPlatform = secondPlatform;
        secondPlatform = instancePlatform[instancePlatform.IndexOf(secondPlatform) + 1];
        ScorePoint += 2;
    }

    void PlayerJumped()
    {
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

    void GetNewPosPlatform()
    {
        if (MyRandom.ThrowOfDice(50))
            currentPosPlatform = currentPosPlatform == e_posPlatform.BOTLEFT ? e_posPlatform.BOT : currentPosPlatform + 1;
        else
            currentPosPlatform = currentPosPlatform == e_posPlatform.BOT ? e_posPlatform.BOTLEFT : currentPosPlatform - 1;
    }

    Color GetColorFromEnum(e_colorPlatform newColor)
    {
        Color result = Color.blue;
        float color = 200f / 255f;
        if (newColor == e_colorPlatform.BLUE)
        {
            result = new Color(0.1f, 0.1f, color);
        }
        if (newColor == e_colorPlatform.GREEN)
        {
            result = new Color(0.1f, color, 0.1f);
        }
        if (newColor == e_colorPlatform.RED)
        {
            result = new Color(color, 0.1f, 0.1f);
        }
        return result;
    }
    public void StartGame()
    {
        lastInstanceTime += MyTimer.Instance.TotalTime - ratePlatform;
        startTime = MyTimer.Instance.TotalTime;
        instanceInterfaceManager.ShowMenu(false);
        instanceInterfaceManager.ShowButtonsColor(true);
        GameStarted = true;
        audioSource.PlayOneShot(startGameSound);
    }

    public void ButtonLeftPressed()
    {
        if (instanceInterfaceManager.CurrentButtonLeftColor == e_colorPlatform.BLUE &&
            instanceInterfaceManager.CurrentButtonRightColor == e_colorPlatform.GREEN)
        {
            instanceInterfaceManager.ChangeColorButtonLeft(e_colorPlatform.RED);
            StartCoroutine(DoLerpColor(e_colorPlatform.BLUE));
            currentColorPlatform = e_colorPlatform.BLUE;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == e_colorPlatform.BLUE &&
            instanceInterfaceManager.CurrentButtonRightColor == e_colorPlatform.RED)
        {
            instanceInterfaceManager.ChangeColorButtonLeft(e_colorPlatform.GREEN);
            StartCoroutine(DoLerpColor(e_colorPlatform.BLUE));
            currentColorPlatform = e_colorPlatform.BLUE;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == e_colorPlatform.GREEN &&
            instanceInterfaceManager.CurrentButtonRightColor == e_colorPlatform.BLUE)
        {
            instanceInterfaceManager.ChangeColorButtonLeft(e_colorPlatform.RED);
            StartCoroutine(DoLerpColor(e_colorPlatform.GREEN));
            currentColorPlatform = e_colorPlatform.GREEN;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == e_colorPlatform.GREEN &&
            instanceInterfaceManager.CurrentButtonRightColor == e_colorPlatform.RED)
        {
            instanceInterfaceManager.ChangeColorButtonLeft(e_colorPlatform.BLUE);
            StartCoroutine(DoLerpColor(e_colorPlatform.GREEN));
            currentColorPlatform = e_colorPlatform.GREEN;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == e_colorPlatform.RED &&
            instanceInterfaceManager.CurrentButtonRightColor == e_colorPlatform.BLUE)
        {
            instanceInterfaceManager.ChangeColorButtonLeft(e_colorPlatform.GREEN);
            StartCoroutine(DoLerpColor(e_colorPlatform.RED));
            currentColorPlatform = e_colorPlatform.RED;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == e_colorPlatform.RED &&
            instanceInterfaceManager.CurrentButtonRightColor == e_colorPlatform.GREEN)
        {
            instanceInterfaceManager.ChangeColorButtonLeft(e_colorPlatform.BLUE);
            StartCoroutine(DoLerpColor(e_colorPlatform.RED));
            currentColorPlatform = e_colorPlatform.RED;
        }
        UpdateStatPlatform();
    }

    public void ButtonRightPressed()
    {
        StartCoroutine(DoLerpColor(currentColorPlatform));
        if (instanceInterfaceManager.CurrentButtonLeftColor == e_colorPlatform.BLUE &&
            instanceInterfaceManager.CurrentButtonRightColor == e_colorPlatform.GREEN)
        {
            instanceInterfaceManager.ChangeColorButtonRight(e_colorPlatform.RED);
            StartCoroutine(DoLerpColor(e_colorPlatform.GREEN));
            currentColorPlatform = e_colorPlatform.GREEN;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == e_colorPlatform.BLUE &&
            instanceInterfaceManager.CurrentButtonRightColor == e_colorPlatform.RED)
        {
            instanceInterfaceManager.ChangeColorButtonRight(e_colorPlatform.GREEN);
            StartCoroutine(DoLerpColor(e_colorPlatform.GREEN));
            StartCoroutine(DoLerpColor(e_colorPlatform.RED));
            currentColorPlatform = e_colorPlatform.RED;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == e_colorPlatform.GREEN &&
            instanceInterfaceManager.CurrentButtonRightColor == e_colorPlatform.BLUE)
        {
            instanceInterfaceManager.ChangeColorButtonRight(e_colorPlatform.RED);
            StartCoroutine(DoLerpColor(e_colorPlatform.BLUE));
            currentColorPlatform = e_colorPlatform.BLUE;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == e_colorPlatform.GREEN &&
            instanceInterfaceManager.CurrentButtonRightColor == e_colorPlatform.RED)
        {
            instanceInterfaceManager.ChangeColorButtonRight(e_colorPlatform.BLUE);
            StartCoroutine(DoLerpColor(e_colorPlatform.RED));
            currentColorPlatform = e_colorPlatform.RED;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == e_colorPlatform.RED &&
            instanceInterfaceManager.CurrentButtonRightColor == e_colorPlatform.BLUE)
        {
            instanceInterfaceManager.ChangeColorButtonRight(e_colorPlatform.GREEN);
            StartCoroutine(DoLerpColor(e_colorPlatform.BLUE));
            currentColorPlatform = e_colorPlatform.BLUE;
        }
        else
        if (instanceInterfaceManager.CurrentButtonLeftColor == e_colorPlatform.RED &&
            instanceInterfaceManager.CurrentButtonRightColor == e_colorPlatform.GREEN)
        {
            instanceInterfaceManager.ChangeColorButtonRight(e_colorPlatform.BLUE);
            StartCoroutine(DoLerpColor(e_colorPlatform.GREEN));
            currentColorPlatform = e_colorPlatform.GREEN;
        }
        UpdateStatPlatform();
    }

    public void DestroyPlatform(GameObject thisPlatform)
    {
        instancePlatform.Remove(thisPlatform);
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


    IEnumerator DoLerpColor(e_colorPlatform newColorPlat)
    {
        float ElapsedTime = 0.0f;
        Color beforeColor = GetColorFromEnum(currentColorPlatform);
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
