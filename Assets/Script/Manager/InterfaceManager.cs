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
    GameObject buttons;

    [SerializeField]
    GameObject buttonLeft;

    [SerializeField]
    GameObject buttonRight;

    [SerializeField]
    GameObject bestScore;

    [SerializeField]
    Sprite[] spriteButton;

    void Start ()
    {
        if (!PlayerPrefs.HasKey("bestScore"))
            PlayerPrefs.SetInt("bestScore", 0);
        InitInterface();
    }
	
	void Update ()
    {
		
	}

    public void InitInterface()
    {
        bestScore.GetComponentInChildren<Text>().text = "BEST SCORE: " + PlayerPrefs.GetInt("bestScore").ToString();
        CurrentButtonLeftColor = GameManager.e_colorPlatform.GREEN;
        CurrentButtonRightColor = GameManager.e_colorPlatform.RED;
        buttonLeft.GetComponent<Image>().sprite = spriteButton[(int)GameManager.e_colorPlatform.GREEN];
        buttonRight.GetComponent<Image>().sprite = spriteButton[(int)GameManager.e_colorPlatform.RED];
        ShowButtonsColor(false);
        ShowStats(false);
    }

    public void ShowMenu(bool show)
    {
        menuTitle.SetActive(show);
        menuPlay.SetActive(show);
        bestScore.SetActive(show);
    }

    public void ShowButtonsColor(bool show)
    {
        buttons.SetActive(show);
    }

    public void ShowStats(bool show)
    {
        stats.SetActive(show);
    }

    public void UpdateStats()
    {
        point.GetComponent<Text>().text = "SCORE: " + GameManager.Instance.ScorePoint;
        time.GetComponent<Text>().text = "TIME: " + GameManager.Instance.ScoreTime.ToString("0.0") + " SEC";
    }

    public void ChangeColorButtonLeft(GameManager.e_colorPlatform newColor)
    {
        buttonLeft.GetComponent<Image>().sprite = spriteButton[(int)newColor];
        CurrentButtonLeftColor = newColor;
    }

    public void ChangeColorButtonRight(GameManager.e_colorPlatform newColor)
    {
        buttonRight.GetComponent<Image>().sprite = spriteButton[(int)newColor];
        CurrentButtonRightColor = newColor;
    }

    public GameManager.e_colorPlatform CurrentButtonLeftColor { get; set; }
    public GameManager.e_colorPlatform CurrentButtonRightColor { get; set; }
}
