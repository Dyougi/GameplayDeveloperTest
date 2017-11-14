using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlatformManager : MonoBehaviour
{

    [SerializeField]
    private GameObject platform;

    [SerializeField]
    private GameObject bonus;

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
    private int idPlatform;
    private float[,] arrayColor;

    public enum e_platformType { START, DEFAULT };
    public enum e_colorPlatform { COLOR1, COLOR2, COLOR3, COLOR4, COLOR5, COLOR6, COLOR7, COLOR8 };

    private void Awake()
    {
        instancePlatform = new List<GameObject>();
        arrayColor = new float[8, 3] {
            {0, 0, 255},
            {255, 0, 0},
            {255, 128, 0},
            {255, 255, 0},
            {0, 255, 0},
            {127, 0, 255},
            {255, 0, 127},
            {128, 128, 128},
        };
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

    public void CreatePlatform(GameManager.e_posPlatform posPlatform, float newSpeed, Vector3 newOffset, float newScale, bool doLerp = true, bool instanceBonus = true)
    {
        Vector3 platformStartPosition;
        if (!doLerp)
            platformStartPosition = positionPlatformStart[(int)posPlatform].position + newOffset;
        else
            platformStartPosition = positionPlatformStart[(int)posPlatform].position;
        GameObject currentInstance = Instantiate(platform, platformStartPosition, positionPlatformStart[(int)posPlatform].rotation);
        currentInstance.name = "Platform " + idPlatform;
        currentInstance.transform.parent = platformEnvironment.transform;
        currentInstance.GetComponent<Platform>().PosPlatform = posPlatform;
        currentInstance.GetComponent<Platform>().Id = idPlatform;
        currentInstance.GetComponent<Platform>().ColorPlatform = CurrentColorPlatform;
        currentInstance.GetComponentInChildren<MeshRenderer>().material = platformMaterial;
        if (instanceBonus)
        {
            if (MyRandom.ThrowOfDice(40))
            {
                Vector3 offset = new Vector3(0, 0.6f, newScale / 2);
                GameObject currentInstanceBonus = Instantiate(bonus, currentInstance.transform.position, currentInstance.transform.rotation);
                currentInstanceBonus.transform.parent = currentInstance.transform;
                currentInstanceBonus.transform.localPosition += offset;
            }
        }
        currentInstance.GetComponent<Platform>().Init(newSpeed, positionPlatformStart[(int)posPlatform], positionPlatformEnd[(int)posPlatform], positionPlatformDestroy.position, newOffset, doLerp);
        currentInstance.transform.GetChild(0).localScale = new Vector3(currentInstance.transform.GetChild(0).localScale.x, currentInstance.transform.GetChild(0).localScale.y, newScale);
        instancePlatform.Add(currentInstance);
        idPlatform++;
    }

    public void UpdatePausePlatform(bool pause)
    {
        instancePlatform.ForEach(delegate (GameObject obj)
        {
            obj.GetComponent<Platform>().Pause = pause;
        });
    }

    public void UpdateSpeedPlatform(float newSpeed)
    {
        instancePlatform.ForEach(delegate (GameObject obj)
        {
            obj.GetComponent<Platform>().Speed = newSpeed;
        });
    }

    public void UpdateColorPlatform()
    {
        CurrentColorPlatform = (int)CurrentColorPlatform + 1 == arrayColor.Length ? 0 : CurrentColorPlatform + 1;
        if ((int)CurrentColorPlatform < arrayColor.GetLength(0))
        {
            Color start = GetColorFromArray((int)CurrentColorPlatform - 1);
            Color end = GetColorFromArray((int)CurrentColorPlatform);
            StartCoroutine(DoLerpColorCamera(start, end));
        }
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
    public GameObject CurrentPlatform { get; set; }
    public GameObject SecondPlatform { get; set; }
    public List<GameObject> InstancesPlatform
    {
        get
        {
            return instancePlatform;
        }

    }

    IEnumerator DoLerpColorCamera(Color start, Color end)
    {
        float ElapsedTime = 0.0f;
        while (ElapsedTime <= 1.0f)
        {
            platformMaterial.color = Color.Lerp(start, end, ElapsedTime);
            ElapsedTime += Time.deltaTime * 2;
            yield return null;
        }
        platformMaterial.color = end;
    }
}