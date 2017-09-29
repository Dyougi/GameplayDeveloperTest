using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [SerializeField]
    private Transform[] positionPlatformStart;

    [SerializeField]
    private Transform[] positionPlatformEnd;

    [SerializeField]
    private Transform positionPlatformDestroy;

    [SerializeField]
    private Transform positionDeathPlayer;

    [SerializeField]
    private Transform positionFirstPlatform;

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
    private int idPlatform;

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
                    Debug.Log("INSTANCE: " + MyTimer.Instance.TotalTime);
                    GameObject currentInstance = Instantiate(platformDefault, positionPlatformStart[(int)currentPosPlatform].position, positionPlatformStart[(int)currentPosPlatform].rotation);
                    currentInstance.name = "Platform " + idPlatform;
                    currentInstance.transform.parent = platformEnvironment.transform;
                    currentInstance.GetComponent<Platform>().Init(speedPlatform, positionPlatformStart[(int)currentPosPlatform], positionPlatformEnd[(int)currentPosPlatform], Vector3.zero, positionPlatformDestroy.position);
                    currentInstance.GetComponent<Platform>().PosPlatform = currentPosPlatform;
                    currentInstance.GetComponent<Platform>().Id = idPlatform;
                    idPlatform++;
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
        ratePlatform = (distanceBetweenPlatform + 4) / speedPlatform;
        currentPosPlatform = e_posPlatform.BOT;
        idPlatform = 0;
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
        currentInstance.GetComponent<Platform>().Init(speedPlatform, positionFirstPlatform, positionFirstPlatform, positionPlatformDestroy.position, Vector3.zero, false);
        currentInstance.GetComponent<Platform>().PosPlatform = currentPosPlatform;
        currentInstance.GetComponent<Platform>().Id = idPlatform;
        idPlatform++;
        GetNewPosPlatform();
        instancePlatform.Add(currentInstance);
        for (int count = 0; count < 9; count++)
        {
            currentInstance = Instantiate(platformDefault, positionPlatformStart[(int)currentPosPlatform].position, positionPlatformStart[(int)currentPosPlatform].rotation);
            currentInstance.transform.parent = platformEnvironment.transform;
            currentInstance.GetComponent<Platform>().Init(speedPlatform, positionPlatformStart[(int)currentPosPlatform], positionPlatformEnd[(int)currentPosPlatform], positionPlatformDestroy.position, posPlatform);
            currentInstance.GetComponent<Platform>().PosPlatform = currentPosPlatform;
            currentInstance.GetComponent<Platform>().Id = idPlatform;
            idPlatform++;
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
        lastInstanceTime = MyTimer.Instance.TotalTime;
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
        Debug.Log("################################################");
        instancePlatform.ForEach(delegate (GameObject obj)
        {
            Debug.Log("Platform id: " + obj.GetComponent<Platform>().Id);
        });
        Debug.Log("################################################");
        Debug.Log("BEFORE currentPlatform dir: " + currentPlatform.GetComponent<Platform>().PosPlatform);
        Debug.Log("BEFORE currentPlatform id: " + currentPlatform.GetComponent<Platform>().Id);
        Debug.Log("BEFORE secondPlatform dir: " + secondPlatform.GetComponent<Platform>().PosPlatform);
        Debug.Log("BEFORE secondPlatform id: " + secondPlatform.GetComponent<Platform>().Id);
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
        Debug.Log("AFTER currentPlatform dir: " + currentPlatform.GetComponent<Platform>().PosPlatform);
        Debug.Log("AFTER currentPlatform id: " + currentPlatform.GetComponent<Platform>().Id);
        Debug.Log("AFTER secondPlatform dir: " + secondPlatform.GetComponent<Platform>().PosPlatform);
        Debug.Log("AFTER secondPlatform id: " + secondPlatform.GetComponent<Platform>().Id + "\n");
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
