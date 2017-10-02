using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlatformManager : MonoBehaviour
{

    [SerializeField]
    private GameObject platform;

    [SerializeField]
    private Transform[] positionPlatformStart;

    [SerializeField]
    private Transform[] positionPlatformEnd;

    [SerializeField]
    private GameObject platformEnvironment;

    [SerializeField]
    private Transform positionPlatformDestroy;

    [SerializeField]
    private Material[] platformMaterial;

    private List<GameObject> instancePlatform;
    private e_posPlatform currentPosPlatform;
    private int idPlatform;

    public enum e_platformType { START, DEFAULT };
    public enum e_posPlatform { BOT, BOTRIGHT, RIGHT, TOPRIGHT, TOP, TOPLEFT, LEFT, BOTLEFT };
    public enum e_colorPlatform { BLUE, GREEN, RED };

    void Start()
    {
        instancePlatform = new List<GameObject>();
    }

    void GetNewPosPlatform()
    {
        if (MyRandom.ThrowOfDice(50))
            currentPosPlatform = currentPosPlatform == e_posPlatform.BOTLEFT ? e_posPlatform.BOT : currentPosPlatform + 1;
        else
            currentPosPlatform = currentPosPlatform == e_posPlatform.BOT ? e_posPlatform.BOTLEFT : currentPosPlatform - 1;
    }

    public void InitPlatform()
    {
        Debug.Log("InitPlatform");
        CurrentColorPlatform = e_colorPlatform.BLUE;
        currentPosPlatform = e_posPlatform.BOT;
        idPlatform = 0;
    }

    public void CreatePlatform(float newSpeed, Vector3 newOffset, e_colorPlatform newColorPlatform, float newScale, bool newDoLerp = true)
    {
        Vector3 platformStartPosition;
        if (!newDoLerp)
            platformStartPosition = positionPlatformStart[(int)currentPosPlatform].position + newOffset;
        else
            platformStartPosition = positionPlatformStart[(int)currentPosPlatform].position;
        GameObject currentInstance = Instantiate(platform, platformStartPosition, positionPlatformStart[(int)currentPosPlatform].rotation);
        currentInstance.name = "Platform " + idPlatform;
        currentInstance.transform.parent = platformEnvironment.transform;
        currentInstance.GetComponent<Platform>().PosPlatform = currentPosPlatform;
        currentInstance.GetComponent<Platform>().Id = idPlatform;
        currentInstance.GetComponent<Platform>().ColorPlatform = newColorPlatform;
        currentInstance.GetComponentInChildren<MeshRenderer>().material = platformMaterial[(int)newColorPlatform];
        currentInstance.GetComponent<Platform>().Init(newSpeed, positionPlatformStart[(int)currentPosPlatform], positionPlatformEnd[(int)currentPosPlatform], positionPlatformDestroy.position, newOffset, newDoLerp);

        if (currentInstance.GetComponent<Platform>().ColorPlatform != CurrentColorPlatform)
            currentInstance.GetComponent<Platform>().ChangeOpacity(true);
        else
            currentInstance.GetComponent<Platform>().ChangeOpacity(false);
        currentInstance.transform.localScale = new Vector3(currentInstance.transform.localScale.x, currentInstance.transform.localScale.y, newScale);
        instancePlatform.Add(currentInstance);
        idPlatform++;
        GetNewPosPlatform();
    }

    public void UpdateSpeedPlatform(float newSpeed)
    {
        instancePlatform.ForEach(delegate (GameObject obj)
        {
            obj.GetComponent<Platform>().Speed = newSpeed;
        });
    }

    public void UpdateOpacityPlatform()
    {
        instancePlatform.ForEach(delegate (GameObject obj)
        {
            if (CurrentPlatform != obj)
            {
                if (obj.GetComponent<Platform>().ColorPlatform == CurrentColorPlatform)
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

}