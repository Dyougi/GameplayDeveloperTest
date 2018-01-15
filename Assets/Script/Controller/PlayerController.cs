﻿using System.Collections;
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
    private BoxCollider coll;
    private AudioSource audioSource;
    private Vector3 playerDefaultPos;
    private bool isJumping;
    private float speedRotation;
    private bool isPaused;

    public delegate void PlayerAction(e_jump jump);
    public static event PlayerAction OnJump;
    public static event PlayerAction OnLand;

    public enum e_jump { RIGHT, LEFT, NONE};

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();
        playerDefaultPos = transform.position;
    }

    void Start ()
    {
        isPaused = false;
        IsDead = false;
        speedRotation = 100;
    }

	void Update ()
    {
        if (GameManager.Instance.GameStarted)
        {
            if (!Pause)
            {
                if (isJumping && rb.velocity.y != 0)
                {
                    Log.WriteStringDate("Landing");
                    if (OnLand != null)
                        OnLand(e_jump.NONE);
                    isJumping = false;
                }

                transform.GetChild(0).Rotate(Vector3.right * speedRotation * Time.deltaTime);

                if (!IsDead && rb.velocity.y == 0)
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

    void ManageInput()
    {
        Vector2 tapPoint = Vector2.zero;

#if UNITY_IOS

        if (Input.touchCount > 0)
        {
            tapPoint = Input.GetTouch(0).position;
            tapPoint.y = Screen.height - tapPoint.y;
            if (Input.GetTouch(0).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                if (tapPoint.x < Screen.width / 2)
                    DoJump(e_jump.LEFT);
                else
                    DoJump(e_jump.RIGHT);
            }
        }

#endif
#if UNITY_ANDROID

        if (Input.touchCount > 0)
        {
            tapPoint = Input.GetTouch(0).position;
            tapPoint.y = Screen.height - tapPoint.y;
            if (Input.GetTouch(0).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                if (tapPoint.x < Screen.width / 2)
                    DoJump(e_jump.LEFT);
                else
                    DoJump(e_jump.RIGHT);
            }
        }

#endif
#if UNITY_EDITOR

        if (Input.GetButtonDown("JumpLeft"))
        {
            DoJump(e_jump.LEFT);
        }
        if (Input.GetButtonDown("JumpRight"))
        {
            DoJump(e_jump.RIGHT);
        }

#endif
    }

    void DoJump(e_jump jump)
    {
        Log.WriteStringDate("Try to jump");
        if (!isJumping)
        {
            audioSource.PlayOneShot(jumpSound);
            Log.WriteStringDate("Jump !");
            if (OnJump != null)
            {
                OnJump(jump);
            }
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
        coll.enabled = true;
    }

    public void StopVelocity()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        coll.enabled = false;
    }

    public bool Pause
    {
        get
        {
            return isPaused;
        }

        set
        {
            isPaused = value;

            if (value)
                rb.constraints = RigidbodyConstraints.FreezeAll;
            else
                rb.constraints = ~RigidbodyConstraints.FreezePositionY;
        }
    }

    public bool IsDead { get; set; }
}
