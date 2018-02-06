using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlatformManager : MonoBehaviour
{

    [SerializeField]
    private GameObject platform;

    [SerializeField]
    private GameObject[] bonus;

    [SerializeField]
    private Transform[] positionPlatformStart;

    [SerializeField]
    private Transform[] positionPlatformEnd;

    [SerializeField]
    private GameObject platformEnvironment;

    [SerializeField]
    private Transform positionPlatformDestroy;

    [SerializeField]
    private Material platformMaterial;

    private List<GameObject> instancePlatform;
    private GameObject currentPlatformPlayer;

    private int idPlatform;
    private float[,] arrayColor;
    private bool isPaused;

    public enum e_platformType { START, DEFAULT };
    public enum e_bonus { BONUS1, BONUS2, BONUS3 };
    public enum e_colorPlatform { COLOR1, COLOR2, COLOR3, COLOR4, COLOR5, COLOR6, COLOR7, COLOR8 };

    private void Awake()
    {
        instancePlatform = new List<GameObject>();
        arrayColor = new float[8, 3] {
            {0, 0, 255},        // blue
            {255, 0, 0},        // red
            {255, 127, 0},      // orange
            {255, 255, 0},      // yellow
            {0, 255, 0},        // green
            {127, 0, 255},      // purple
            {255, 0, 127},      // pink
            {127, 127, 127},    // grey
        };
    }

    private void Start()
    {
        isPaused = false;
    }

    Color GetColorFromArray(int dim)
    {
        return new Color(arrayColor[dim, 0] / 255, arrayColor[dim, 1] / 255, arrayColor[dim, 2] / 255);
    }

    public void InitPlatform()
    {
        Debug.Log("InitPlatform");
        CurrentColorPlatform = e_colorPlatform.COLOR1;
        platformMaterial.color = GetColorFromArray(0);
        idPlatform = 0;
    }

    public GameObject CreatePlatform(GameManager.e_posPlatform posPlatform, float newSpeed, Vector3 newOffset, float newScale, bool doLerp = true, bool instanceBonus = true)
    {
        Vector3 platformStartPosition;

        if (!doLerp)
            platformStartPosition = positionPlatformStart[(int)posPlatform].position + newOffset;
        else
            platformStartPosition = positionPlatformStart[(int)posPlatform].position;

        GameObject currentInstance = Instantiate(platform, platformStartPosition, positionPlatformStart[(int)posPlatform].rotation);
        currentInstance.name = "Platform " + idPlatform;
        currentInstance.transform.parent = platformEnvironment.transform;

        Platform currentPlatform = currentInstance.GetComponent<Platform>();

        currentPlatform.PosPlatform = posPlatform;
        currentPlatform.Id = idPlatform;
        currentPlatform.ColorPlatform = CurrentColorPlatform;
        currentInstance.GetComponentInChildren<MeshRenderer>().material = platformMaterial;
        currentPlatform.HeightScale = newScale;

        if (instanceBonus)
        {
            if (MyRandom.ThrowOfDice(40))
            {
                int rando = Random.Range(0, bonus.Length);
                GameObject currentInstanceBonus = Instantiate(bonus[rando], currentInstance.transform.position, currentInstance.transform.rotation);
                Vector3 offset = new Vector3(0, currentInstanceBonus.GetComponent<Bonus>().OffsetPosition, newScale / 2);

                currentInstanceBonus.transform.parent = currentInstance.transform;
                currentInstanceBonus.transform.localPosition += offset;
            }
        }

        currentPlatform.Init(newSpeed, positionPlatformStart[(int)posPlatform], positionPlatformEnd[(int)posPlatform], positionPlatformDestroy.position, newOffset, doLerp);
        currentInstance.transform.GetChild(0).localScale = new Vector3(currentInstance.transform.GetChild(0).localScale.x, currentInstance.transform.GetChild(0).localScale.y, newScale);
        instancePlatform.Add(currentInstance);
        idPlatform++;
        return currentInstance;
    }

    public void UpdatecurrentSpeedPlatform(float newSpeed)
    {
        instancePlatform.ForEach(delegate (GameObject obj)
        {
            obj.GetComponent<Platform>().Speed = newSpeed;
        });
    }

    public void UpdateColorPlatform()
    {
        CurrentColorPlatform = (int)CurrentColorPlatform + 1 == arrayColor.Length ? 0 : CurrentColorPlatform + 1;
     
        Color start = GetColorFromArray((int)CurrentColorPlatform - 1);
        Color end = GetColorFromArray((int)CurrentColorPlatform);
        StartCoroutine(DoLerpColorPlatform(start, end));
    }

    public void ClearInstancesPlatform()
    {
        instancePlatform.ForEach(delegate (GameObject obj)
        {
            Destroy(obj);
        });

        instancePlatform.Clear();
    }

    public void DestroyPlatform(GameObject thisPlatform)
    {
        instancePlatform.Remove(thisPlatform);
    }

    public e_colorPlatform CurrentColorPlatform { get; set; }

    public List<GameObject> InstancesPlatform
    {
        get
        {
            return instancePlatform;
        }
    }

    public GameObject CurrentPlatformPlayer
    {
        get
        {
            return currentPlatformPlayer;
        }

        set
        {
            currentPlatformPlayer = value;
        }
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
            instancePlatform.ForEach(delegate (GameObject obj)
            {
                obj.GetComponent<Platform>().Pause = value;
            });
        }
    }

    IEnumerator DoLerpColorPlatform(Color start, Color end)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < 1.0f)
        {
            if (!Pause)
            {
                platformMaterial.color = Color.Lerp(start, end, elapsedTime);
                elapsedTime += Time.deltaTime * 2;
            }
            yield return null;
        }
        platformMaterial.color = end;
    }
}