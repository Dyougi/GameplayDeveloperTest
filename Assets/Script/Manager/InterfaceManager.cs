using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour {

    [SerializeField]
    GameObject menuTitle;

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

    [SerializeField]
    GameObject tuto;

    [SerializeField]
    GameObject pause;

    [SerializeField]
    GameObject restartBackground;

    [SerializeField]
    Sprite pauseOn;

    [SerializeField]
    Sprite pauseOff;

    private bool isPaused;

    void Start ()
    {
        isPaused = false;

        if (!PlayerPrefs.HasKey("bestScore"))
            PlayerPrefs.SetInt("bestScore", 0);
    }
	
    public void InitInterface()
    {
        Debug.Log("InitInterface");
        bestScore.GetComponentInChildren<Text>().text = "BEST SCORE: " + PlayerPrefs.GetInt("bestScore").ToString();
        score.GetComponentInChildren<Text>().text = "0";
        ShowIngameUI(false);
        ShowStats(false);
        ShowRestartBackground(false);
    }

    public void ShowMenu(bool show)
    {
        menuTitle.SetActive(show);
        bestScore.SetActive(show);
        tuto.SetActive(show);
    }

    public void ShowIngameUI(bool show)
    {
        score.SetActive(show);
        pause.SetActive(show);
    }

    public void ShowStats(bool show)
    {
        stats.SetActive(show);
    }

    public void ShowRestartBackground(bool show)
    {
        restartBackground.SetActive(show);
    }

    public void UpdateRestartBackground(string str)
    {
        restartBackground.GetComponent<Text>().text = str;
    }

    public void UpdateStats(int newScore)
    {
        point.GetComponent<Text>().text = "SCORE: " + GameManager.Instance.ScorePoint;
        time.GetComponent<Text>().text = "BEST SCORE: " + PlayerPrefs.GetInt("bestScore").ToString();
    }

    public void UpdateScore(int newScore)
    {
        score.GetComponent<Text>().text = newScore.ToString();
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

            if (value)
                pause.GetComponent<Image>().sprite = pauseOn;
            else
                pause.GetComponent<Image>().sprite = pauseOff;
        }
    }
}
