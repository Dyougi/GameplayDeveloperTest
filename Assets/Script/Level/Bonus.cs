using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonus : MonoBehaviour
{

    [SerializeField]
    private int pointGived;

    [SerializeField]
    private float offsetPosition;

    public int PointGived
    {
        get
        {
            return pointGived;
        }
    }

    public float OffsetPosition
    {
        get
        {
            return offsetPosition;
        }
    }
}
