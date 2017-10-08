using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour {

    [SerializeField]
    GameObject menuTitle;

    [SerializeField]
    GameObject menuPlay;

    [SerializeField]
    GameObject stats;

    [SerializeField]
    GameObject point;

    [SerializeField]
    GameObject time;

    [SerializeField]
    GameObject bestScore;

    [SerializeField]
    GameObject score;

    void Start ()
    {
        if (!PlayerPrefs.HasKey("bestScore"))
            PlayerPrefs.SetInt("bestScore", 0);
    }
	
	void Update ()
    {
		
	}

    public void InitInterface()
    {
        Debug.Log("InitInterface");
        bestScore.GetComponentInChildren<Text>().text = "BEST SCORE: " + PlayerPrefs.GetInt("bestScore").ToString();
        score.GetComponentInChildren<Text>().text = "0";
        ShowIngameUI(false);
        ShowStats(false);
    }

    public void ShowMenu(bool show)
    {
        menuTitle.SetActive(show);
        menuPlay.SetActive(show);
        bestScore.SetActive(show);
    }

    public void ShowIngameUI(bool show)
    {
        score.SetActive(show);
    }

    public void ShowStats(bool show)
    {
        stats.SetActive(show);
    }

    public void UpdateStats()
    {
        point.GetComponent<Text>().text = "SCORE: " + GameManager.Instance.ScorePoint;
        time.GetComponent<Text>().text = "BEST SCORE: " + PlayerPrefs.GetInt("bestScore").ToString();
    }

    public void UpdateScore()
    {
        score.GetComponent<Text>().text = GameManager.Instance.ScorePoint.ToString();
    }

    public PlatformManager.e_colorPlatform CurrentButtonLeftColor { get; set; }
    public PlatformManager.e_colorPlatform CurrentButtonRightColor { get; set; }
}
