using UnityEngine;

public class MyTimer : MonoBehaviour
{

    private static MyTimer instance;

    float totalTime;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.LogWarning("Singleton: instance here already");
            Destroy(gameObject);
            return;
        }
    }

    public static MyTimer Instance
    {
        get { return instance; }
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

    public void resetTimer()
    {
        totalTime = 0;
    }

    public float TotalTime
    {
        get
        {
            return totalTime;
        }
    }

    public bool Pause {get; set;}
}
