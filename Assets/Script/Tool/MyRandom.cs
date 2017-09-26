using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyRandom
{
    public static bool ThrowOfDice(int percentage)
    {
        if (Random.Range(0, 100) < percentage)
            return true;
        return false;
    }
}