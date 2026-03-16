//using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// Civilina npc posible states
enum NPC_States
{
    Interfere, // chase criminal
    Involved, // find law enforcment
    Semi_Involved, // shout
    Leaving, // move away from crime scene
    lurking, //move around crime scene 
    idle
};


public class Npc_Civillian : MonoBehaviour
{
    /// 
    /// Varibales
    /// 

    NavMeshAgent agent;
    
    public SkinnedMeshRenderer mat;

   

    [SerializeField] LayerMask groundLayer;

    // This npc sensor for sight and hearing other agents
    public NPC_AI_Sensor sensor;

    // values for random wander
    Vector3 destPoint;
    bool walkPointSet;
    [SerializeField] float walkRange;

    // current "personality" traits or affecting factors
    public float empathy = 0.0f;
    public float fear = 0.0f;
    public float bystanderEffect = 0.0f;
  
    // current state
    NPC_States currentState;

    public string debugState;

    // boolean for if the npc is alerted to the crime
    public bool alert;

    bool justAlerted = false;



    Vector3 CrimainalPos;
    bool reachedCriminal = false;

    public List<GameObject> lawEnforcementLocations = new List<GameObject>();

    public int debugList = 0;

    bool foundClosestLawEnforment = false;

    GameObject obj;

    Npc_Civillian NPCs;
    public int shoutInterval;
    float shoutTimer;

    public bool reachedOtherNpc = false;
    public bool foundOtherNpc = false;
    public Vector3 npcPos;
    public Npc_Civillian currentNPCs;

    public Vector3 crimeScene;
    public bool beenToCrimeSceen = false;

    Npc_Guard guard;
    Vector3 guardPos;
    Npc_Guard targetedGuard;
    bool reachedGuard;


    public Player_Descriptions player_Description;

    public float chasingTimer = 5f;
    public float chasingInterval = 5f;

    public Vector3 lastKnownPlayerLocation;
    public bool beenToLKPL = false;

    private GameObject player;

    public bool seenPlayer = false;

    private Vector3 guardStation;


    public Animator animator;

    /// 
    /// Varibales
    /// 

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        sensor = GetComponent<NPC_AI_Sensor>();

       

       

       // mat.material.SetColor("_BaseColor", Color.green);

        // Getting stat values
        RandomlySetStats();


        animator = GetComponent<Animator>();

        shoutTimer = Random.Range(0, shoutInterval);
    }


 

    private void Update()
    {
        if(agent.isStopped)
        {
            animator.SetFloat("Speed", 0f);
        }
        else if(!agent.isStopped)
        {
            animator.SetFloat("Speed", 0.2f);
        }


        DeFuzzyFication();
        if (alert)
        {
            if(!justAlerted)
            {
                //agent.isStopped = true;
                agent.SetDestination(transform.position);

                bystanderEffect = (sensor.bystanderObjects.Count / 2) * 0.1f;

                justAlerted = true;
            }

            switch(currentState)
            {
                case NPC_States.Interfere:
                    agent.isStopped = false;

                    if (sensor.objects.Count == 0 && seenPlayer)
                    {
                        chasingTimer -= Time.deltaTime;
                        if (chasingTimer <= 0)
                        {

                            lastKnownPlayerLocation = player.transform.position;
                            player = null;
                            chasingTimer = chasingInterval;
                            seenPlayer = false;

                        }
                    }


                    if (sensor.objects.Count != 0 || player != null)
                    {


                        if (sensor.objects.Count != 0)
                        {
                            player = sensor.objects[0].transform.gameObject;
                            seenPlayer = true;


                            chasingTimer = chasingInterval;
                        }
                       
                        if(player.GetComponent<Player_Script>().GetDescription() == player_Description)
                        {

                            if (Vector3.Distance(transform.position, player.transform.position) < 5)
                            {
                                // print(Vector3.Distance(transform.position, CrimainalPos));
                                agent.SetDestination(transform.position);
                                reachedCriminal = true;
                            }
                            else
                            {
                                agent.SetDestination(player.transform.position);
                            }

                            Vector3 lookat;
                            lookat.x = player.transform.position.x;
                            lookat.y = 0;
                            lookat.z = player.transform.position.z;

                            transform.LookAt(lookat);
                        }
                        //CrimainalPos = sensor.objects[0].transform.position;

                      
                    }
                    else if(!beenToCrimeSceen && crimeScene!=null && !seenPlayer)
                    {
                        agent.SetDestination(crimeScene);

                        if(Vector3.Distance(transform.position,crimeScene)<5)
                        {
                            beenToCrimeSceen = true;

                        }
                    }
                    else
                    {
                        RandomWander();
                    }

                   // mat.color = Color.darkGreen;
                    mat.material.SetColor("_BaseColor", Color.darkGreen);

                    break;

                case NPC_States.Involved:

                    
               

                    if (targetedGuard != null)
                    {  
                        if(Vector3.Distance(transform.position,guardPos)<5)
                        {
                            targetedGuard.alert = true;
                            targetedGuard.crimeScene = crimeScene;
                            targetedGuard.player_Description = player_Description;

                            foundClosestLawEnforment = true;


                           // reachedGuard = true;
                            targetedGuard = null;
                        }
                        else
                        {
                            agent.SetDestination(guardPos);
                        }

                    }
                    else
                    {

                        if (sensor.soundObjects != null)
                        {
                            for (int i = 0; i < sensor.soundObjects.Count; i++)
                            {
                                if (sensor.soundObjects[i].GetComponent<Npc_Guard>() != null)
                                {
                                    guard = sensor.soundObjects[i].GetComponent<Npc_Guard>();

                                    if (guard.alert != true)
                                    {
                                        guardPos = guard.transform.position;
                                        targetedGuard = guard;

                                    }
                                    else
                                    {
                                        //foundClosestLawEnforment = true;
                                       // reachedGuard = true;
                                    }

                                }
                            }
                        }


                        if (targetedGuard==null && !reachedGuard)
                        {

                            for (int i = 0; i < lawEnforcementLocations.Count; i++)
                            {
                                obj = lawEnforcementLocations[i];
                            }

                            if (obj != null)
                            {
                                agent.isStopped = false;
                                if (obj != null)
                                {
                                    if (Vector3.Distance(transform.position, obj.transform.position) < 5)
                                    {
                                        reachedGuard = true;
                                        guardStation = obj.transform.position;

                                    }
                                    else
                                    {
                                        agent.SetDestination(obj.transform.position);
                                    }
                                }
                               
                                // debugList++;
                            }
                            foundClosestLawEnforment = true;

                        }
                      
                        if(reachedGuard)
                        {
                            walkRange = 5;
                            RandomWander();
                        }
                       
                           
                        
                    }

                    
                  
                  
                    

                    

                    mat.material.SetColor("_BaseColor", Color.red);

                    break;

                case NPC_States.Semi_Involved:
                    agent.isStopped = false;
                    //shoutTimer -= Time.deltaTime;
                    //if (shoutTimer < 0)
                    //{
                    //    shoutTimer += shoutInterval;
                    //    Shout();
                    //}

                    Shout();

                   // mat.color = Color.purple;
                    mat.material.SetColor("_BaseColor", Color.purple);
                    break;

                case NPC_States.Leaving:
                    agent.isStopped = false;

                    if(sensor.objects.Count!=0)
                    {
                        Vector3 direction = (transform.position - sensor.objects[0].transform.position).normalized;

                        agent.SetDestination(transform.position + direction);

                        Vector3 lookat;
                        lookat.x = sensor.objects[0].transform.position.x;
                        lookat.y = 0;
                        lookat.z = sensor.objects[0].transform.position.z;

                        transform.LookAt(lookat);

                    }
                    else
                    {
                        Vector3 direction = (transform.position - crimeScene).normalized;
                        if (Vector3.Distance(transform.position, crimeScene) < 30)
                        {
                            agent.SetDestination(transform.position + direction);

                        }
                        else
                        {
                            RandomWander();
                        }
                    }



                    //mat.color = Color.yellow;
                    mat.material.SetColor("_BaseColor", Color.yellow);
                    break;

                case NPC_States.lurking:

                    agent.isStopped = false;
                    if (sensor.objects.Count != 0)
                    {
                        Vector3 lookat;
                        lookat.x = sensor.objects[0].transform.position.x;
                        lookat.y = 0;
                        lookat.z = sensor.objects[0].transform.position.z;

                        transform.LookAt(lookat);        
                        
                        if(Vector3.Distance(sensor.objects[0].transform.position,transform.position)<15)
                        {
                            Vector3 direction = (transform.position - sensor.objects[0].transform.position).normalized;

                            agent.SetDestination(transform.position + direction);
                        }
                    }
                   else if (!beenToCrimeSceen)
                    {
                        agent.SetDestination(crimeScene);
                        if (Vector3.Distance(transform.position, crimeScene) < 10)
                        {
                            beenToCrimeSceen = true;

                        }
                    }
                    else
                    {
                        walkRange = 5;
                        RandomWander();
                    }




                    //mat.color = Color.beige;
                    mat.material.SetColor("_BaseColor", Color.beige);
                    break;
            }




        }
       

        // if the npc does not know of the crime then they will randomly wander the scene
        else
        {
            RandomWander();
            //currentState = NPC_States.idle;
        }
    }

    void Shout()
    {
        if(sensor.soundObjects.Count !=0 )
        {
            if(!foundOtherNpc)
            {
                for (int i = 0; i < sensor.soundObjects.Count; i++)
                {
                    if(sensor.soundObjects[i].GetComponent<Npc_Civillian>()!=null)
                    {
                        NPCs = sensor.soundObjects[i].GetComponent<Npc_Civillian>();

                        if (!NPCs.alert && !foundOtherNpc)
                        {
                            npcPos = NPCs.agent.transform.position;
                            currentNPCs = NPCs;
                            foundOtherNpc = true;

                        }
                    }

                   

                }
            }

            if(!reachedOtherNpc && foundOtherNpc)
            {
               
                agent.SetDestination(npcPos);

                if(Vector3.Distance(transform.position,npcPos)<2)
                {
                   
                    currentNPCs.alert = true;
                    currentNPCs.crimeScene = crimeScene;


                    if(currentNPCs.empathy+0.2<=1)
                    {
                        currentNPCs.empathy += 0.2f;

                    }
                    reachedOtherNpc = true;

                }
            }

            if(reachedOtherNpc && !foundOtherNpc)
            {
                RandomWander();
                reachedOtherNpc = false;
            }

            if(reachedOtherNpc && foundOtherNpc)
            {

                for (int i = 0; i < sensor.soundObjects.Count; i++)
                {
                    if (sensor.soundObjects[i].GetComponent<Npc_Civillian>() != null)
                    {
                        NPCs = sensor.soundObjects[i].GetComponent<Npc_Civillian>();
                        if (NPCs != null)
                        {
                            NPCs.alert = true;
                            NPCs.crimeScene = crimeScene;
                            NPCs.player_Description = player_Description;
                        }
                    }
                    else if (sensor.soundObjects[i].GetComponent<Npc_Guard>() != null)
                    {
                        Npc_Guard guard = sensor.soundObjects[i].GetComponent<Npc_Guard>();
                        if(guard.alert!=true)
                        {
                            guard.alert = true;
                            guard.crimeScene = crimeScene;
                            guard.player_Description = player_Description;

                        }
                    }
                }
                foundOtherNpc = false;
            }

           if(!foundOtherNpc && !reachedOtherNpc)
            {
                RandomWander();
            }
        }
        else
        {
           

            RandomWander();
        }
       
    }




    void RandomlySetStats()
    {
        // Getting random value for each float then rounding it up to one decible point 
        empathy = Random.Range(0.0f, 1.0f);
        empathy = Mathf.Round(empathy * 10.0f) * 0.1f;

        fear = Random.Range(0.0f, 1.0f);
        fear = Mathf.Round(fear * 10.0f) * 0.1f;

        //bystanderEffect = Random.Range(0.0f, 1.0f);
        //bystanderEffect = Mathf.Round(bystanderEffect * 10.0f) * 0.1f;
    }

    void DeFuzzyFication()
    {
        if (fear >= empathy)
        {
            if(bystanderEffect >= 0.5 && fear <= 0.5)
            {
                currentState = NPC_States.Semi_Involved;
                debugState = "Semi_Involved";
            }
            else if (bystanderEffect <= 0.5 && fear >= 0.5)
            {
                currentState = NPC_States.Leaving;
                debugState = "Leaving";
            }
            else if (bystanderEffect >= 0.5 && fear == 1)
            {
                currentState = NPC_States.lurking;
                debugState = "Lurking";
            }
            else if (bystanderEffect >= 0.5 && fear >= 0.5)
            {
                currentState = NPC_States.Leaving;
                debugState = "Leaving";
            }
            else if (bystanderEffect <= 0.5 && fear <= 0.5)
            {
                currentState = NPC_States.lurking;
                debugState = "Lurking";
            }
        }     
        else
        {
            if (bystanderEffect >= 0.5 && empathy <= 0.5)
            {
                currentState = NPC_States.Semi_Involved;
                debugState = "Semi_Involved";
            }
            else if (bystanderEffect <= 0.5 && empathy >= 0.5)
            {
                currentState = NPC_States.Interfere;
                debugState = "Interfere";
            }
            else if (bystanderEffect >= 0.5 && empathy == 1)
            {
                currentState = NPC_States.Involved;
                debugState = "Involved";
            }
            else if (bystanderEffect >= 0.5 && empathy >= 0.5)
            {
                currentState = NPC_States.Involved;
                debugState = "Involved";
            }
            else if (bystanderEffect <= 0.5 && empathy <= 0.5)
            {
                currentState = NPC_States.Semi_Involved;
                debugState = "Semi_Involved";
            }
        }
    }


    void SearchForDest()
    {
      


        if(currentState == NPC_States.Involved)
        {
            float z = Random.Range(-walkRange, walkRange);
            float x = Random.Range(-walkRange, walkRange);

            destPoint = new Vector3(guardStation.x + x, guardStation.y, guardStation.z + z);
          
            if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
            {
                walkPointSet = true;
            }
        }
        else if(currentState == NPC_States.lurking)
        {
            float z = Random.Range(-walkRange, walkRange);
            float x = Random.Range(-walkRange, walkRange);

            destPoint = new Vector3(crimeScene.x + x, crimeScene.y, crimeScene.z + z);


            if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
            {
                walkPointSet = true;
            }
        }
        else if (currentState == NPC_States.Leaving)
        {
            float z = Random.Range(-walkRange, walkRange);
            float x = Random.Range(-walkRange, walkRange);

            destPoint = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);
            do
            {
                float tempz = Random.Range(-walkRange, walkRange);
                float tempx = Random.Range(-walkRange, walkRange);

                destPoint = new Vector3(transform.position.x + tempx, transform.position.y, transform.position.z + tempz);
            } while (Vector3.Distance(crimeScene, destPoint) < 30);
            if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
            {
                walkPointSet = true;
            }
        }
        else
        {
            float tempz = Random.Range(-walkRange, walkRange);
            float tempx = Random.Range(-walkRange, walkRange);

            destPoint = new Vector3(transform.position.x + tempx, transform.position.y, transform.position.z + tempz);
            if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
            {
                walkPointSet = true;
            }

            
        }
    }

    void RandomWander()
    {
        if (!walkPointSet) SearchForDest();
        if (walkPointSet) agent.SetDestination(destPoint);
        if (Vector3.Distance(transform.position, destPoint) < 10) walkPointSet = false;
        if (agent.pathStatus == NavMeshPathStatus.PathPartial) walkPointSet = false;
    }


    public void SetPlayerDescription(Player_Descriptions playerDisc)
    {
        player_Description = playerDisc;
    }

}
