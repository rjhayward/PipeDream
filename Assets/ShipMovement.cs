using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class ShipMovement : MonoBehaviour
{
    bool desktop;
    float pitch;
    float yaw;
    float roll;
    int score;
    public Text highScore;

    float ChangeInPitch;
    Vector3 vel;
    Animator anim;
    Rigidbody rb;
    Vector3 originalPos;
    Vector3 originalAcceleration;
    Quaternion originalRot;

    public Camera cam;

    public PipeSeries pipeSeries;
    
    // Use this for initialization
    void Start ()
    {
        score = 0;
        desktop = false;
        pitch = 0;
        yaw = 0;
        roll = 0;
        ChangeInPitch = 0;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        originalRot = transform.rotation;
        originalPos = transform.position;
        originalAcceleration = Input.acceleration;
        originalAcceleration.x = 0;

        if (PlayerPrefs.HasKey("HighScore"))
        {
            highScore.text = "High Score: " + PlayerPrefs.GetInt("HighScore");
            Debug.Log("High Score: " + PlayerPrefs.GetInt("HighScore"));
        }
        else
        {
            highScore.text = "High Score: 0";
            PlayerPrefs.SetInt("HighScore", 0);

            Debug.Log("High Score: 0");
        }
    }



    // Update is called once per frame
    void Update()
    {
        int speedMultiplier = 170;

        if (pipeSeries.GetState() == PipeSeries.GameState.InGame)
        {
            //when we aren't using mouse + keyboard to debug
            if (!desktop)
            {
                //use this for calculating 
                Vector3 oldPYR = new Vector3(pitch, yaw, roll);

                if (new Vector3(pitch, yaw, roll) != oldPYR)
                {
                    rb.isKinematic = true;
                }

                rb.isKinematic = false;

                Vector3 acceleration = Vector3.zero;
                acceleration = Input.acceleration;
                acceleration /= acceleration.magnitude;
                acceleration -= originalAcceleration;

                ChangeInPitch = 5 * (pitch / -400); //makes it between -1 and 1
                pitch = -400 * Mathf.Clamp(acceleration.z, -0.2f, 0.2f);
                roll = -500 * Mathf.Clamp(acceleration.x, -0.15f, 0.15f);
                ChangeInPitch -= 5 * (pitch / -400);

                cam.transform.RotateAround(transform.position, transform.right, -1 * ChangeInPitch);

                if (pitch != 0 || roll != 0) transform.Rotate(new Vector3(pitch * Time.deltaTime, 0f, roll * Time.deltaTime));

                speedMultiplier = 170;
            }
            else
            {
                Vector3 oldPYR = new Vector3(pitch, yaw, roll);

                if (new Vector3(pitch, yaw, roll) != oldPYR)
                {
                    rb.isKinematic = true;
                }
                rb.isKinematic = false;

                pitch = CrossPlatformInputManager.GetAxis("Pitch") * 85f;
                roll = CrossPlatformInputManager.GetAxis("Roll") * 150f;

                if (pitch != 0 || roll != 0) transform.Rotate(new Vector3(pitch * Time.deltaTime, 0f, roll * Time.deltaTime));


                speedMultiplier = 200;
            }
        }

        transform.position += speedMultiplier * transform.forward * Time.deltaTime;
    }

    public void ResetAcceleration()
    {
        if (!desktop) originalAcceleration = Input.acceleration;
    }

    void OnCollisionEnter(Collision col)
    {
        //reset to beginning if pipe is hit
        if (col.gameObject.tag == "Pipe")
        {
            score = 0;
            GetComponentInChildren<Text>().text = "";
            if (PlayerPrefs.HasKey("HighScore"))
            {
                highScore.text = "High Score: " + PlayerPrefs.GetInt("HighScore");
            }
            transform.position = originalPos;
            transform.rotation = originalRot;
            rb.isKinematic = true;
            rb.isKinematic = false;

            pipeSeries.InitialisePipeSeries();
        }
    }


    void OnTriggerEnter(Collider col)
    {
        //whenever the pipevolume is entered 
        if (col.gameObject.tag == "PipeVolume")
        {
            Destroy(col.gameObject);
            int oldScore = score;
            score = pipeSeries.AddPipe(score); //This is being called more than once, so in the method we limit it to only once per collision by counting frames

            if (oldScore != score) GetComponentInChildren<Text>().text = "Score: " + score; //update score text

            if (score > PlayerPrefs.GetInt("HighScore"))
            {
                PlayerPrefs.SetInt("HighScore", score);
            }

            highScore.text = "High Score: " + PlayerPrefs.GetInt("HighScore");
        }
    }
}