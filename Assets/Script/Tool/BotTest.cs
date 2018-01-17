using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WindowsInput;

public class BotTest : MonoBehaviour
{
    private Transform player;
    private List<GameManager.e_posPlatform> paths;
    private List<GameObject> platforms;
    private Transform currentFlagTransform;
    private GameManager.e_posPlatform currentPath;
    private GameManager.e_posPlatform lastPath;
    private bool isInit;

    void Start ()
    {
        GameManager.OnPath += NewPathIt;
        GameManager.OnDeath += PlayerDead;
        paths = new List<GameManager.e_posPlatform>();
        platforms = new List<GameObject>();
        player = GameObject.Find("Player").transform;
        lastPath = GameManager.e_posPlatform.BOT;
    }
	
	void Update ()
    {
        if (GameManager.Instance.GameStarted)
        {
            if (!isInit)
            {
                currentFlagTransform = GameManager.Instance.PlatformManagerInstance.InstancesPlatform[0].transform;
                isInit = true;
            }
            //Debug.Log("player.position.z: " + player.position.z + " - currentFlagTransform.Find(FlagJumpBotTest).position.z: " + currentFlagTransform.GetChild(0).Find("FlagJumpBotTest").position.z);
            if (player.position.z > currentFlagTransform.GetChild(0).Find("FlagJumpBotTest").position.z)
            {
                Debug.Log("platform " + currentFlagTransform.gameObject.name);
                currentFlagTransform = platforms[0].transform;
                platforms.RemoveAt(0);
                currentPath = paths[0];
                paths.RemoveAt(0);
                Debug.Log("########################################");
                if (WhereToJump(currentPath) == GameManager.e_dirRotation.LEFT)
                {
                    BotJumpLeft();
                }
                else
                {
                    BotJumpRight();
                }
                Debug.Log("########################################");
                lastPath = currentPath;
            }
        }
        else
        {
            InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_A);
        }
    }

    void PlayerDead()
    {
        isInit = false;
        paths.Clear();
        platforms.Clear();
        lastPath = GameManager.e_posPlatform.BOT;
    }

    void NewPathIt(GameObject obj, GameManager.e_posPlatform pos)
    {
        paths.Add(pos);
        platforms.Add(obj);
    }

    GameManager.e_dirRotation WhereToJump(GameManager.e_posPlatform pos)
    {
        Debug.Log("lastPath est à " + lastPath.ToString() + " (" + (int)lastPath + ")");
        Debug.Log("pos est à " + pos.ToString() + " (" + (int)pos + ")");
        if (lastPath == 0)
        {
            if (pos == (GameManager.e_posPlatform)7)
            {
                Debug.Log("On saute à gauche");
                return GameManager.e_dirRotation.LEFT;
            }
            else
            {
                Debug.Log("On saute à droite");
                return GameManager.e_dirRotation.RIGHT;
            }
        }

        if (lastPath == (GameManager.e_posPlatform)7)
        {
            if (pos == 0)
            {
                Debug.Log("On saute à gauche !");
                return GameManager.e_dirRotation.RIGHT;
            }
            else
            {
                Debug.Log("On saute à droite !");
                return GameManager.e_dirRotation.LEFT;
            }
        }

        if (pos > lastPath)
        {
            Debug.Log("On saute à droite !!");
            return GameManager.e_dirRotation.RIGHT;
        }
        Debug.Log("On saute à gauche !!");
        return GameManager.e_dirRotation.LEFT;
    }

    void BotJumpRight()
    {
        InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_E);
    }

    void BotJumpLeft()
    {
        InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_Z);
    }
}
