using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WindowsInput;

public class BotTest : MonoBehaviour
{
    private List<PathPlatform> paths;
    private Transform currentFlagTransform;
    private PathPlatform currentPath;
    private bool isInit;

    void Start ()
    {
        InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_Z);
        InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_E);
        InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_A);
        
        isInit = false;
        GameManager.OnPath += NewPathIt;
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
            if (transform.position.z > currentFlagTransform.Find("FlagJumpBotTest").position.z)
            {
                int index = GameManager.Instance.PlatformManagerInstance.InstancesPlatform.IndexOf(currentFlagTransform.gameObject) + 1;
                currentFlagTransform = GameManager.Instance.PlatformManagerInstance.InstancesPlatform[index].transform;
                currentPath = paths[0];
                paths.RemoveAt(0);

                if (WhereToJump(currentPath) == GameManager.e_dirRotation.LEFT)
                {
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_Z);
                }
                else
                {
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_E);
                }
            }
        }
        else
            InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_A);
    }

    void NewPathIt(PathPlatform path)
    {
        if (path.pathID == 0)
        {
            PathPlatform newPath = new PathPlatform(path);
            paths.Add(newPath);
        }       
    }

    GameManager.e_dirRotation WhereToJump(PathPlatform path)
    {
        if (path.currentPosPlatform == 0)
        {
            if (path.nextPosPlatform == (GameManager.e_posPlatform)7)
                return GameManager.e_dirRotation.LEFT;
            else
                return GameManager.e_dirRotation.RIGHT;
        }
        if (path.currentPosPlatform > path.nextPosPlatform)
            return GameManager.e_dirRotation.RIGHT;
        return GameManager.e_dirRotation.LEFT;
    }
}
