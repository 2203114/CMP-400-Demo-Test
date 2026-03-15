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
    
    public Renderer mat;

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





    /// 
    /// Varibales
    /// 

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        sensor = GetComponent<NPC_AI_Sensor>();

        mat = GetComponent<Renderer>();

        mat.material.SetColor("_BaseColor", Color.green);

        // Getting stat values
        RandomlySetStats();
    

        shoutTimer = Random.Range(0, shoutInterval);
    }


 

    private void Update()
    {
        DeFuzzyFication();
        if (alert)
        {
            if(!justAlerted)
            {
                agent.isStopped = true;
                justAlerted = true;
            }

            switch(currentState)
            {
                case NPC_States.Interfere:
                    agent.isStopped = false;
                    if (sensor.objects.Count != 0)
                    {

                        CrimainalPos = sensor.objects[0].transform.position;

                        if (Vector3.Distance(transform.position, CrimainalPos) < 5)
                        {
                            // print(Vector3.Distance(transform.position, CrimainalPos));
                            agent.isStopped = true; ;
                            reachedCriminal = true;
                        }
                        else
                        {
                            agent.isStopped = false; ;
                            reachedCriminal = false;
                        }

                        if (!reachedCriminal)
                        {
                            agent.SetDestination(CrimainalPos);
                        }
                    }
                    else if(!beenToCrimeSceen && crimeScene!=null)
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

                    mat.material.SetColor("_BaseColor", Color.darkGreen);

                    break;

                case NPC_States.Involved:

                    
               

                    if (targetedGuard != null)
                    {  
                        if(Vector3.Distance(transform.position,guardPos)<5)
                        {
                            targetedGuard.alert = true;
                            targetedGuard.crimeScene = crimeScene;
                            foundClosestLawEnforment = true;
                            reachedGuard = true;
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
                                        foundClosestLawEnforment = true;
                                        reachedGuard = true;
                                    }

                                }
                            }
                        }


                        if (!foundClosestLawEnforment)
                        {

                            for (int i = 0; i < lawEnforcementLocations.Count; i++)
                            {
                                obj = lawEnforcementLocations[i];
                            }

                            if (obj != null)
                            {
                                agent.isStopped = false;
                                agent.SetDestination(obj.transform.position);
                                // debugList++;
                            }
                            foundClosestLawEnforment = true;

                        }
                        else
                        {
                            if (obj != null)
                            {
                                if (Vector3.Distance(transform.position, obj.transform.position) < 5)
                                {
                                    reachedGuard = true;
                                }
                            }

                            
                        }
                        if(reachedGuard)
                        {
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
                    mat.material.SetColor("_BaseColor", Color.purple);
                    break;

                case NPC_States.Leaving:
                    //agent.isStopped = false;


                    Vector3 direction = (transform.position - crimeScene).normalized;
                    mat.material.SetColor("_BaseColor", Color.yellow);
                    break;

                case NPC_States.lurking:

                    agent.isStopped = false;
                    if (sensor.objects.Count != 0)
                    {
                        transform.LookAt(sensor.objects[0].transform);
                    }
                    if (!beenToCrimeSceen)
                    {
                        agent.SetDestination(crimeScene);
                        if (Vector3.Distance(transform.position, crimeScene) < 10)
                        {
                            beenToCrimeSceen = true;

                        }
                    }
                    else
                    {
                        walkRange = 10;
                        RandomWander();
                    }


                   
                    

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
                        }
                    }
                    else
                    {
                        Npc_Guard guard = sensor.soundObjects[i].GetComponent<Npc_Guard>();
                        if(guard.alert!=true)
                        {
                            guard.alert = true;
                            guard.crimeScene = crimeScene;

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

        bystanderEffect = Random.Range(0.0f, 1.0f);
        bystanderEffect = Mathf.Round(bystanderEffect * 10.0f) * 0.1f;
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
        float z = Random.Range(-walkRange, walkRange);
        float x = Random.Range(-walkRange, walkRange);

        destPoint = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);


        if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
        {
            walkPointSet = true;
        }
    }

    void RandomWander()
    {
        if (!walkPointSet) SearchForDest();
        if (walkPointSet) agent.SetDestination(destPoint);
        if (Vector3.Distance(transform.position, destPoint) < 10) walkPointSet = false;
    }
  

}
