using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour {

    [SerializeField]
    Transform platformCheck;

    [SerializeField]
    AudioClip jumpSound;

    private Rigidbody rb;
    private AudioSource audioSource;
    private Vector3 playerDefaultPos;
    private bool isOnPlatform;

    public delegate void PlayerAction();
    public static event PlayerAction OnJump;
    public static event PlayerAction OnLand;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start ()
    {
        playerDefaultPos = transform.position;
        Pause = false;
        IsDead = false;
        isOnPlatform = true;
    }

	void Update () // -9.81 real grav
    {
        if (GameManager.Instance.GameStarted)
        {
            if (!Pause)
            {
                if (!IsDead)
                {
                    ManageInput();
                }
                if (CheckIfNear(platformCheck.position, 0.1f))
                {
                    if (!isOnPlatform)
                    {
                        if (OnLand != null)
                            OnLand();
                    }
                    isOnPlatform = true;
                }
                else
                {
                    isOnPlatform = false;
                }
            }
        }
	}

    bool CheckIfNear(Vector3 position, float range)
    {
        Collider[] colliders = Physics.OverlapSphere(position, range);
        for (int i = 0; i < colliders.Length; i++)
            if (colliders[i].gameObject != gameObject)
                return true;
        return false;
    }

    void ManageInput()
    {
#if UNITY_IOS
                if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                DoJump();
            }
        }
#endif
#if UNITY_ANDROID

        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                DoJump();
            }
        }
#endif
#if UNITY_EDITOR
        if (Input.GetButtonDown("Jump"))
        {
            DoJump();
        }
#endif
    }

    void DoJump()
    {
        if (isOnPlatform)
        {
            Debug.Log("Jump");
            audioSource.PlayOneShot(jumpSound);
            if (OnJump != null)
                OnJump();
            rb.AddForce(Vector3.up * 12, ForceMode.Impulse);
        }
    }

    public void InitPlayer()
    {
        Pause = false;
        IsDead = false;
        isOnPlatform = true;
        transform.position = playerDefaultPos;
    }

    public bool Pause { get; set; }
    public bool IsDead { get; set; }
}
