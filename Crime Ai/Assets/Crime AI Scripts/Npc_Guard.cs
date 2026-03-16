//using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// Guard npc posible states
enum NPC_Guard_States
{
   Guarding,    // idle state of guard posted at a place
   Patroling,   // randomly patroling
   Chasing,     // chase player
   Investigate  // investigate crime scene
};


public class Npc_Guard : MonoBehaviour
{
    /// 
    /// Variables 
    /// 

    NavMeshAgent agent;

    public Material mat;

    [SerializeField] LayerMask groundLayer;

    // This npc sensor for sight and hearing other agents
    public NPC_AI_Sensor sensor;

    NPC_Guard_States currentState;



    // boolean for if the npc is alerted to the crime
    public bool alert;

    public Vector3 crimeScene;
    public bool beenToCrimeSceen = false;

    // if patrolling assingn a patrol leader
    public GameObject patrolLeader;

    // set if this guard is patrol leader
    public bool isPatrolLeader;

    public bool isPartoling;

    Vector3 destPoint;
    bool walkPointSet;
    [SerializeField] float walkRange;

    public Animator animator;

    public string debugState;


    public float chasingTimer = 5f;
    public float chasingInterval = 5f;

    public Vector3 lastKnownPlayerLocation;
    public bool beenToLKPL = false;

    private GameObject player;


    public Player_Descriptions player_Description;

    
    /// 
    /// Variables 
    /// 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        sensor = GetComponent<NPC_AI_Sensor>();

        mat = GetComponent<Material>();

        animator = GetComponent<Animator>();

        //mat.material.SetColor("_BaseColor", Color.blue);

        if(isPartoling)
        {
            currentState = NPC_Guard_States.Patroling;
        }
        else
        {
            currentState = NPC_Guard_States.Guarding;
        }


        if(isPatrolLeader)
        {
            walkRange = 30;
        }
        else
        {
            walkRange = 10;

        }

        lastKnownPlayerLocation = Vector3.zero;

    }

    // Update is called once per frame
    void Update()
    {

        //if (agent.velocity.x != 0 && agent.velocity.y != 0 && agent.velocity.y != 0)
        //{
        //    animator.SetFloat("Speed", 0.2f);
        //}
        //else
        //{
        //    animator.SetFloat("Speed", 0f);
        //}

        if (agent.isStopped)
        {
            animator.SetFloat("Speed", 0f);
        }
        else if (!agent.isStopped)
        {
            animator.SetFloat("Speed", 0.2f);
        }


        if (alert)
        {
            if(currentState == NPC_Guard_States.Chasing && sensor.objects.Count == 0)
            {
                chasingTimer -= Time.deltaTime;
                if(chasingTimer<=0)
                {
                   
                    lastKnownPlayerLocation = player.transform.position;
                    player = null;
                    chasingTimer = chasingInterval;

                }
            }
            
            // chase player
            if (sensor.objects.Count != 0 || player != null)
            {
                currentState = NPC_Guard_States.Chasing;

                debugState = "Chasing";


                if(sensor.objects.Count != 0)
                {
                    player = sensor.objects[0].transform.gameObject;

                    
                    chasingTimer = chasingInterval;
                }


                if(player!=null)
                {

                    if (player.GetComponent<Player_Script>().GetDescription() == player_Description)
                    {
                        if (Vector3.Distance(transform.position, player.transform.position) < 2)
                        {
                            agent.SetDestination(transform.position);
                            SceneManager.LoadScene(1);

                        }
                        else
                        {
                            agent.SetDestination(player.transform.position);
                        }
                    }

                    Vector3 lookat;
                    lookat.x = player.transform.position.x;
                    lookat.y = 0;
                    lookat.z = player.transform.position.z;

                    transform.LookAt(lookat);
                }     
            }
            // investigate crime scene
            else if (crimeScene!=null)
            {
                currentState = NPC_Guard_States.Investigate;

                debugState = "investigating";

                if(!beenToLKPL)
                {
                    agent.SetDestination(lastKnownPlayerLocation);
                    if (Vector3.Distance(transform.position, lastKnownPlayerLocation) < 5)
                    {
                        beenToLKPL = true;
                    }

                }


                else if(!beenToCrimeSceen && lastKnownPlayerLocation == Vector3.zero)
                {
                    agent.SetDestination(crimeScene);

                    if(Vector3.Distance(transform.position,crimeScene)<5)
                    {
                        beenToCrimeSceen = true;
                    }
                }
                else
                {
                    walkRange = 10;
                    Patroling();

                }


            }
        }
        else
        {
            switch (currentState)
            {

                // guard post mode
                case NPC_Guard_States.Guarding:

                    debugState = "guarding";

                   // animator.SetFloat("Speed", 0.0f);
                    Guarding();

                    break;

                // patroling mode
                case NPC_Guard_States.Patroling:

                    debugState = "patroling";
                    //animator.SetFloat("Speed", 0.1f);
                    Patroling();

                    break;

            }
        }
    }

    void Patroling()
    {

        switch(currentState)
        {
            case NPC_Guard_States.Patroling:

                if (isPatrolLeader)
                {
                    if (!walkPointSet) SearchForDest();
                    if (walkPointSet) agent.SetDestination(destPoint);
                    if (Vector3.Distance(transform.position, destPoint) < 5) walkPointSet = false;
                    if (agent.pathStatus == NavMeshPathStatus.PathPartial) walkPointSet = false;
                }
                else
                {
                    if (!walkPointSet) SearchForDest();
                    if (walkPointSet) agent.SetDestination(destPoint);
                    if (Vector3.Distance(transform.position, destPoint) < 10) walkPointSet = false;
                    if (agent.pathStatus == NavMeshPathStatus.PathPartial) walkPointSet = false;
                }
                break;

            case NPC_Guard_States.Investigate:

                if (!walkPointSet) SearchForDest();
                if (walkPointSet) agent.SetDestination(destPoint);
                if (Vector3.Distance(transform.position, destPoint) < 5) walkPointSet = false;
                if (agent.pathStatus == NavMeshPathStatus.PathPartial) walkPointSet = false;

                break;
        }
    
    }

    void Guarding()
    {

    }

    // get the guard position
    public Vector3 GetGuardPosition()
    {
        return this.transform.position;
    }

    void SearchForDest()
    {

        float z = Random.Range(-walkRange, walkRange);
        float x = Random.Range(-walkRange, walkRange);

        switch (currentState)
        {

            case NPC_Guard_States.Patroling:
                
                if (isPatrolLeader)
                {
                    //float z = Random.Range(-walkRange, walkRange);
                    //float x = Random.Range(-walkRange, walkRange);

                    destPoint = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);


                    if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
                    {
                        walkPointSet = true;
                    }
                }
                else
                {
                    //float z = Random.Range(-walkRange, walkRange);
                    //float x = Random.Range(-walkRange, walkRange);

                    destPoint = new Vector3(patrolLeader.transform.position.x + x, patrolLeader.transform.position.y, patrolLeader.transform.position.z + z);


                    if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
                    {
                        walkPointSet = true;
                    }
                }

                break;


            case NPC_Guard_States.Investigate:

                //float z = Random.Range(-walkRange, walkRange);
                //float x = Random.Range(-walkRange, walkRange);
                if(lastKnownPlayerLocation==Vector3.zero)
                {
                    destPoint = new Vector3(crimeScene.x + x, crimeScene.y, crimeScene.z + z);
                }
                else
                {
                    destPoint = new Vector3(lastKnownPlayerLocation.x + x, lastKnownPlayerLocation.y, lastKnownPlayerLocation.z + z);
                }
               


                if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
                {
                    walkPointSet = true;
                }


                break;
        }      
    }

    public void SetPlayerDescription(Player_Descriptions playerDisc)
    {
        player_Description = playerDisc;
    }

}
