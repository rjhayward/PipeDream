using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PipeSeries : MonoBehaviour
{
    //so the program knows when to destroy the initial straight pipes
    public enum GameState {PreGame, InGame};
    GameState state;

    public Pipe pipePrefab;
    public Font font;
    //stores the current pipes as a queue such that when a new one is added, the oldest one is deleted (so the pipes don't overlap each other, which starts to happen when you keep a large amount of pipes)
    Queue<Pipe> PipeQueue;

    int framesSinceLastPipe;
    public Canvas canvas;

    public ShipMovement ship;
    
    // Use this for initialization
    void Start()
    {
        framesSinceLastPipe = 0;
        InitialisePipeSeries();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            ChangeState();
        }
        framesSinceLastPipe++;
    }

    //starts off the initial pipe series and adds them to a queue
    public void InitialisePipeSeries()
    {
        state = GameState.PreGame;

        if (PipeQueue != null)
        {
            foreach (Pipe p in PipeQueue)
            {
                Destroy(p.gameObject);
            }   
            PipeQueue.Clear();
        }

        PipeQueue = new Queue<Pipe>();
        Pipe previousPipe = null;

        float percentage = Random.Range(0.1f, 0.5f);
        //adds initial 5 pipes
        for (int i = 0; i < 7; i++)
        {
            Pipe newPipe = Instantiate(pipePrefab);

            newPipe.transform.parent = transform;

            if (i > 0) newPipe.RenderStraightPipe();//Vector3.zero, percentage);
            else newPipe.RenderStraightPipe();

            if (i > 0) newPipe.RenderVolume();

            if (i > 0) newPipe.AttachAsNewStraightPipe(previousPipe);

            PipeQueue.Enqueue(newPipe);

            previousPipe = newPipe;
            percentage = Random.Range(0.1f, 0.5f);
        }
    }

    void OnGUI()
    {
        if (state == GameState.PreGame)
        {

            Rect startPos = new Rect((Screen.width / 2.0f) - 300, (Screen.height / 2.0f) - 90, 600, 180);
            Rect labelPos = new Rect((Screen.width / 2.0f) - 300, (Screen.height / 2.0f) - 270, 600, 180);
            //CrossPlatformInputManager.SetAxisZero("Roll");

            GUIStyle styleBtn = GUI.skin.button;
            GUIStyle styleLbl = GUI.skin.label;

            styleBtn.font = font;
            styleLbl.font = font;

            styleBtn.fontSize = 100;
            styleLbl.fontSize = 130;
            styleLbl.alignment = TextAnchor.MiddleCenter;
            GUI.Label(labelPos, "PipeDream!!!", styleLbl);

            if (GUI.Button(startPos, "Start Game", styleBtn))
            {
                ChangeState();
                canvas.GetComponentInChildren<Text>().text = "Score: 0";
                ship.ResetAcceleration();
            }
        }
    }



    public void ChangeState()//player presses start/end
    {
        //state changed when this is called as it is only called when we want to generate a new pipe
        if (state == GameState.PreGame)
        {
            state = GameState.InGame;
        }
        else
        {
            state = GameState.PreGame;
        }
    }

    public GameState GetState()
    {
        return state;
    }
    

    //when a pipe is added, it is added to the queue and the first pipe on the queue is destroyed
    public int AddPipe(int score)
    {
        //so multiple collisions with the same pipe at a similar time don't generate more pipes than it should
        if (framesSinceLastPipe > 15) //dequeue and destroy first pipe
        {
            framesSinceLastPipe = 0;

            Pipe toDelete = PipeQueue.Dequeue();
            Destroy(toDelete.gameObject);

            Pipe[] pipeArray = PipeQueue.ToArray();

            Pipe previousPipe = pipeArray[pipeArray.Length - 1];

            float percentage = Random.Range(0.1f, 0.5f);

            Pipe newPipe = Instantiate(pipePrefab);

            newPipe.transform.parent = transform;
            if (state == GameState.PreGame)
            {
                newPipe.RenderStraightPipe();
                newPipe.AttachAsNewStraightPipe(previousPipe);
            }
            else
            {
                newPipe.RenderPipe(Vector3.zero, percentage);
                newPipe.AttachAsNewPipe(previousPipe);
                score++;
            }
            newPipe.RenderVolume();

            PipeQueue.Enqueue(newPipe); // add new pipe to queue
        }
        return score;
    }
}