using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PipeSeries : MonoBehaviour
{
    //so the program knows when to destroy the initial straight pipes
    public enum GameState {PreGame, InGame};
    GameState state;

    public Canvas canvas;
    public ShipMovement ship;
    public Button button;
    public Font font;
    public Pipe pipePrefab;
    //stores the current pipes as a queue such that when a new one is added, the oldest one is deleted (so the pipes don't overlap each other, which starts to happen when you keep a large amount of pipes)
    Queue<Pipe> PipeQueue;

    int pipeCount;
    int framesSinceLastPipe;

    
    // Use this for initialization
    void Start()
    {
        //button.onClick.AddListener(PauseToggle);
        framesSinceLastPipe = 0;
        pipeCount = 0;
        InitialisePipeSeries();
    }
    
    // Update is called once per frame
    void Update()
    {
 

        //if (Input.GetKeyDown("space"))
        //{
        //    PauseToggle();
        //    // ChangeState();
        //}

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
        //adds initial 17 pipes
        for (int i = 0; i < 17; i++)
        {
            Pipe newPipe = Instantiate(pipePrefab);

            newPipe.transform.parent = transform;

            if (i > 0) newPipe.RenderStraightPipe(); //Vector3.zero, percentage);
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

            Rect buttonPos = new Rect((Screen.width / 2.0f) - 300, (Screen.height / 2.0f) - 90, 600, 180);
            
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

            if (GUI.Button(buttonPos, "Start Game", styleBtn))
            {
                ChangeState();
                canvas.GetComponentInChildren<Text>().text = "Score: 0";
                ship.ResetAcceleration();
            }
            
        }
        //else
        //{

        //    /* 
            
        //    GUIStyle styleBtn = GUI.skin.button;
            
        //    styleBtn.font = font;
            
        //    styleBtn.fontSize = 100;
            

        //    Rect pausePos = new Rect(300, 90, 300, 300);

        //    GUI.contentColor = Color.green;
        //    if (GUI.Button(pausePos, "| |", styleBtn))
        //    {
        //        PauseToggle();
        //    }

        //    */
        //}
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

    public void SetState(GameState s)
    {
        state = s;
    }


    //when a pipe is added, it is added to the queue and the first pipe on the queue is destroyed
    public int AddPipe(int score)
    {
        pipeCount++;
        //so multiple collisions with the same pipe at a similar time don't generate more pipes than it should
        if (framesSinceLastPipe > 15) //dequeue and destroy first pipe
        {
            framesSinceLastPipe = 0;

            Pipe toDelete = PipeQueue.Dequeue();
            Destroy(toDelete.gameObject);

            

            
            if (state == GameState.PreGame)
            {
                Pipe newPipe = Instantiate(pipePrefab);
                newPipe.transform.parent = transform;
                Pipe[] pipeArray = PipeQueue.ToArray();

                Pipe previousPipe = pipeArray[pipeArray.Length - 1];

                newPipe.RenderStraightPipe();
                newPipe.AttachAsNewStraightPipe(previousPipe);
                PipeQueue.Enqueue(newPipe);
            }
            else
            {
                int pipesToAdd = 3;
                score++;

                if (true)//pipeCount % pipesToAdd == 0)
                {
                    int tries = 0;
                    while (!AttemptNPipeAddition(pipesToAdd) && tries < 10)
                    {
                        tries++;
                        Debug.Log("unsuccessful add, trying again");

                        RollbackNPipeAddition(pipesToAdd);
                    }

                }
            }


             // add new pipe to queue
        }
        return score;
    }

    public bool AttemptNPipeAddition(int n)
    {
        bool collision = false;

        for (int i = 0; i < n; i++)
        {

            Pipe[] pipeArray = PipeQueue.ToArray();

            Pipe previousPipe = pipeArray[pipeArray.Length - 1];

            Pipe newPipe = Instantiate(pipePrefab);
            newPipe.transform.parent = transform;
            float percentage = 0.6f; //Random.Range(0.1f, 0.5f);
            newPipe.RenderVolume();
            newPipe.RenderPipe(Vector3.zero, percentage);
            newPipe.AttachAsNewPipe(previousPipe);
            PipeQueue.Enqueue(newPipe);


            Collider[] colliders = Physics.OverlapSphere(Vector3.zero, newPipe.torusRadius * 1.5f);
            
            for (int j = 0; j < colliders.Length; j++)
            {
               if ((colliders[j].GetComponentInParent<Pipe>().gameObject != newPipe.gameObject) && (colliders[j].GetComponentInParent<Pipe>().gameObject != previousPipe.gameObject))
               {
                    if (colliders[j].GetComponentInParent<Pipe>() != null) // TODO figure out a way to check if the collision is of type Pipe only -> currently this triggers when PipeVolume 
                    {
                        Debug.Log(colliders[j].gameObject.name);
                        collision = true;
                    }
               }
            }
        }
        
        return !collision; // returns whether there has been a successful addition with no collisions
    }

    public void RollbackNPipeAddition(int n)
    {
        Pipe[] pipeArray = PipeQueue.ToArray();

        for (int i = 1; i <= n; i++)
        {
            Destroy(pipeArray[pipeArray.Length - i].gameObject);//SetActive(false);
            //pipeArray[pipeArray.Length - i].GetComponent<MeshFilter>().sharedMesh = null;
            pipeArray[pipeArray.Length - i] = null;
        }

        pipeArray = pipeArray.Where(val => val != null).ToArray();
        PipeQueue = new Queue<Pipe>(pipeArray);
    }
}