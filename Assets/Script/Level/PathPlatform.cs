using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPlatform {

    public PathPlatform(int newId, bool newUsed = false, bool newJusteCreated = true)
    {
        pathID = newId;
        used = newUsed;
        justCreated = newJusteCreated;
    }

    public PathPlatform(PathPlatform other)
    {
        pathID = other.pathID;
        used = other.used;
        justCreated = other.justCreated;
        nextPosPlatform = other.nextPosPlatform;
        currentPosPlatform = other.currentPosPlatform;
        lastPosPlatform = other.lastPosPlatform;
    }

    public int pathID;
    public bool used;
    public bool justCreated;
    public GameManager.e_posPlatform nextPosPlatform;
    public GameManager.e_posPlatform currentPosPlatform;
    public GameManager.e_posPlatform lastPosPlatform;
}
