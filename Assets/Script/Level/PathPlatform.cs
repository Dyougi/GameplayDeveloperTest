using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPlatform {

    public PathPlatform(int newId, bool newUsed = false)
    {
        pathID = newId;
        used = newUsed;
    }

    public int pathID;
    public bool used;
    public GameManager.e_posPlatform currentPosPlatform;
    public GameManager.e_posPlatform lastPosPlatform;
}
