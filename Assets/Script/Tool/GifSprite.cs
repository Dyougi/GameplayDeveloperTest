using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GifSprite : MonoBehaviour {

    [SerializeField]
    private Sprite[] sprite;

    [SerializeField]
    private float delay;

    private Image refImage;
    private float lastSpriteUpdate;
    private int countSprite;

    private void Start()
    {
        refImage = GetComponent<Image>();
        lastSpriteUpdate = 0;
        countSprite = 0;
    }

    private void Update ()
    {
		if (lastSpriteUpdate < MyTimer.Instance.TotalTimeSecond)
        {
            refImage.sprite = sprite[countSprite];
            countSprite = countSprite == sprite.Length - 1 ? 0 : countSprite + 1;
            lastSpriteUpdate = MyTimer.Instance.TotalTimeSecond + delay;
        }
	}
}
