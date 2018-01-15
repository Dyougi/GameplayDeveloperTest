using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WindowsInput;

public class BotTest : MonoBehaviour
{
    private static BotTest instance;

    private List<GameObject> listPlatform;
    private Transform currentFlagZ;

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

    void Start ()
    {
        InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_Z);
        InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_E);
        InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_A);

        listPlatform = GameManager.Instance.PlatformManagerInstance.InstancesPlatform;
    }
	
	void Update ()
    {
	}
}
