using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

//class for main shipmovement behaviour

public class ShipMovement : MonoBehaviour
{
    bool desktop;
    float pitch;
    float yaw;
    float roll;
    int score;
    public Text highScore;
    public Text pauseText;
    float originalTorusRadius;
    float ChangeInPitch;
    float ChangeInRoll;
    Vector3 vel;
    Animator anim;
    Rigidbody rb;
    Vector3 originalPos;
    Vector3 originalAcceleration;
    Quaternion originalRot;

    public Font font;
    public Camera cam;

    public PipeSeries pipeSeries;

    // Use this for initialization
    void Start()
    {
        originalTorusRadius = pipeSeries.pipePrefab.torusRadius;
        score = 0;
        desktop = false;
        pitch = 0;
        yaw = 0;
        roll = 0;
        ChangeInPitch = 0;
        ChangeInRoll = 0;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        originalRot = transform.rotation;
        originalPos = transform.position;
        originalAcceleration = Input.acceleration;
        originalAcceleration.x = 0;
        originalAcceleration.y = 0;


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
        int speedMultiplier = 100;


        if (pipeSeries.GetState() == PipeSeries.GameState.InGame)
        {

            //when we aren't using mouse + keyboard to debug
            if (!desktop && Mathf.Approximately(Time.timeScale, 1.0f))
            {
                if (Input.touchCount > 0)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began) Pause();
                }

                //use this for calculating 
                Vector3 oldPYR = new Vector3(pitch, yaw, roll);

                if (new Vector3(pitch, yaw, roll) != oldPYR)
                {
                    rb.isKinematic = true;
                }

                rb.isKinematic = false;
                _ = Vector3.zero;
                Vector3 acceleration = Input.acceleration;
                acceleration /= acceleration.magnitude;
                acceleration -= originalAcceleration;

                ChangeInPitch = 5 * (pitch / -400); //makes it between -1 and 1
                ChangeInRoll = (1 / 0.15f) * (roll / -500); //makes it between -1 and 1
                pitch = -400 * Mathf.Clamp(acceleration.z, -0.2f, 0.2f);
                roll = -500 * Mathf.Clamp(acceleration.x, -0.15f, 0.15f);

                float absPitch = ChangeInPitch;
                float absRoll = ChangeInRoll;

                ChangeInPitch -= 5 * (pitch / -400);
                ChangeInRoll -= (1 / 0.15f) * (roll / -500);

                float pitchMultiplier = Mathf.Pow(3 * absPitch / 4, 2) + 0.75f;
                float rollMultiplier = Mathf.Pow(3 * absRoll / 4, 2) + 0.75f;

                // if not paused rotate camera slightly on movement
                if (Mathf.Approximately(Time.timeScale, 1.0f)) cam.transform.RotateAround(transform.position, transform.right, -1 * ChangeInPitch);

                if (pitch != 0 || roll != 0) transform.Rotate(new Vector3(pitch * pitchMultiplier * Time.deltaTime, 0f, roll * rollMultiplier * Time.deltaTime));

                speedMultiplier = 100;
            }
            else //when we are using mouse + keyboard to debug  
            {

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Pause();
                }

                Vector3 oldPYR = new Vector3(pitch, yaw, roll);

                if (new Vector3(pitch, yaw, roll) != oldPYR)
                {
                    rb.isKinematic = true;
                }
                rb.isKinematic = false;

                pitch = CrossPlatformInputManager.GetAxis("Pitch") * 85f;
                roll = CrossPlatformInputManager.GetAxis("Roll") * 150f;

                if (pitch != 0 || roll != 0) transform.Rotate(new Vector3(pitch * Time.deltaTime, 0f, roll * Time.deltaTime));


                speedMultiplier = 100;
            }
        }
        float speedFunction = (float)(1 + 2 * (Math.Log10((score / 8) + 1) / 3));

        float torusRadiusFunction = (float)(1 - (Math.Log10((score / 16) + 1) / 8));
        transform.position += speedFunction * speedMultiplier * transform.forward * Time.deltaTime;

        //limits the decrease in torus radius to half 
        if (pipeSeries.pipePrefab.torusRadius > originalTorusRadius / 2)
        {
            pipeSeries.pipePrefab.torusRadius = originalTorusRadius * torusRadiusFunction;
        }
    }


    void OnGUI()
    {
        if (Mathf.Approximately(Time.timeScale, 0.0f))
        {

            Rect buttonPos = new Rect((Screen.width / 2.0f) - 300, (Screen.height / 2.0f) + 40, 600, 180);
            //CrossPlatformInputManager.SetAxisZero("Roll");

            GUIStyle styleBtn = GUI.skin.button;

            styleBtn.font = font;

            styleBtn.fontSize = 100;
            styleBtn.fixedHeight = 150;

            if (GUI.Button(buttonPos, "Resume", styleBtn))
            {
                Time.timeScale = 1.0f;
                pauseText.text = "";
            }
        }
    }


    void Pause()
    {
        //Debug.Log("pausing");
        
        Time.timeScale = 0.0f;
        pauseText.text = "Paused";

        //if (Mathf.Approximately(Time.timeScale, 0.0f))
        //{
        //    Time.timeScale = 1.0f;
        //    pauseText.text = "";
        //}
        //else
        //{

        //}
    }

    public void ResetAcceleration()
    {
        if (!desktop) originalAcceleration = Input.acceleration;
        originalAcceleration.x = 0;
        originalAcceleration.y = 0;
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

            pipeSeries.pipePrefab.torusRadius = originalTorusRadius;
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