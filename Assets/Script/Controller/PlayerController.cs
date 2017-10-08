using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour {

    [SerializeField]
    Transform platformCheck;

    [SerializeField]
    AudioClip jumpSound;

    [SerializeField]
    AudioClip bonusSound;

    private Rigidbody rb;
    private AudioSource audioSource;
    private Vector3 playerDefaultPos;
    private bool isJumping;
    private float speedRotation;

    public delegate void PlayerAction();
    public static event PlayerAction OnJump;
    public static event PlayerAction OnLand;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        playerDefaultPos = transform.position;
    }

    void Start ()
    {
        Pause = false;
        IsDead = false;
        speedRotation = 100;
    }

	void Update ()
    {
        if (GameManager.Instance.GameStarted)
        {
            if (!Pause)
            {
                if (isJumping && rb.velocity.y < 0)
                {
                    if (CheckIfNear(platformCheck.position, 0.05f))
                    {
                        if (OnLand != null)
                            OnLand();
                        isJumping = false;
                    }
                }
                transform.GetChild(0).Rotate(Vector3.right * speedRotation * Time.deltaTime);
                if (!IsDead)
                {
                    ManageInput();
                }
            }
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bonus")
        {
            audioSource.PlayOneShot(bonusSound, 0.4f);
            Destroy(other.gameObject);
            GameManager.Instance.AddScore(2);
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
        if (!isJumping)
        {
            audioSource.PlayOneShot(jumpSound);
            if (OnJump != null)
                OnJump();
            isJumping = true;
            rb.AddForce(Vector3.up * 12, ForceMode.Impulse);
        }
    }

    public void InitPlayer()
    {
        Debug.Log("InitPlayer");
        Pause = false;
        IsDead = false;
        isJumping = false;
        transform.position = playerDefaultPos;
    }

    public bool Pause { get; set; }
    public bool IsDead { get; set; }
}
