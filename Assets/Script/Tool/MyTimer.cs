using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTimer : MonoBehaviour
{

    private static MyTimer instance;

    float totalTime;

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
    }

    public static MyTimer Instance
    {
        get
        {
            return instance;
        }
    }

    void Start()
    {
        totalTime = 0;
        Pause = false;
    }

    void Update()
    {
        if (!Pause)
        {
            totalTime += Time.deltaTime;
        }
    }

    public void ResetTimer()
    {
        totalTime = 0;
    }

    public float TotalTimeMilliSecond
    {
        get
        {
            return totalTime * 1000;
        }
    }

    public float TotalTimeSecond
    {
        get
        {
            return totalTime;
        }
    }

    public float TotalTimeMinute
    {
        get
        {
            return totalTime / 60;
        }
    }

    public float TotalTimHour
    {
        get
        {
            return totalTime / 3600;
        }
    }

    public bool Pause { get; set; }
}