//using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// Civilina npc posible states
public enum NPC_States
{
    Interfere, // chase criminal
    Involved, // find law enforcment
    Semi_Involved, // shout
    Leaving, // move away from crime scene
    lurking, //move around crime scene 
    idle
};
public enum NPC_Civ_SubStates
{
    Idle,
    Inform_Guards,          // alert guards
    Chase_Player,           // chase player
    Protect_Shopkeeper      // 
}

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

    public float trustInLaw = 0.0f;
    public float confidence = 0.0f;

    public float inflience = 0.0f;
    public NPC_States influencedState;

    // current state
    public NPC_States currentState;
    public NPC_Civ_SubStates currentSubState;


    public string debugState;

    // boolean for if the npc is alerted to the crime
    public bool alert;

    bool justAlerted = false;

    Vector3 CrimainalPos;

    public List<GameObject> lawEnforcementLocations = new List<GameObject>();

    public int debugList = 0;

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

    public GameObject dataCollectorObj;
    public DataCollector dataCollector;

    public bool isWitness;
    public float witnessTimer = 0;
    public float witnessTimerStopPoint = 2;

    NPC_ShopKeep shopKeep;

    float catchTimer = 3;
    float catchTimerReset = 3;

    /// 
    /// Varibales
    /// 

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        sensor = GetComponent<NPC_AI_Sensor>();            

        RandomlySetStats();

        animator = GetComponent<Animator>();

        shoutTimer = Random.Range(0, shoutInterval);

        dataCollector = dataCollectorObj.GetComponent<DataCollector>();
        dataCollector.civillians.Add(transform.gameObject);

        currentState = NPC_States.idle;
    }

    void GetInflence()
    {           
        int countOfLeaving = 0;
        int countOfLurking = 0;
         
        Npc_Civillian civillian;

        for (int i =0; i < sensor.bystanderObjects.Count; i++)
        {
            if(sensor.bystanderObjects[i].GetComponent<Npc_Civillian>())
            {
                civillian = sensor.bystanderObjects[i].GetComponent<Npc_Civillian>();

                if(civillian.alert)
                {                                  
                    if (civillian.currentState == NPC_States.Leaving)
                    {
                        countOfLeaving++;
                    }
                    else if (civillian.currentState == NPC_States.lurking)
                    {
                        countOfLurking++;
                    }
                }
            }
        }

        if(countOfLeaving>=countOfLurking)
        {
            inflience  = (countOfLeaving) * 0.1f;
            influencedState = NPC_States.Leaving;
        }
        else if(countOfLurking>= countOfLeaving)
        {
            inflience  = (countOfLurking) * 0.1f;        
            influencedState = NPC_States.lurking;
        }

        if (inflience > 1.0f)
        {
            inflience = 1.0f;
        }
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
              
        if (alert)
        {
            if(!justAlerted)
            {
                agent.SetDestination(transform.position);

                bystanderEffect = (sensor.bystanderObjects.Count / 2) * 0.1f;
                DeFuzzyFication();
                GetInflence();
                BystanderInflence();

                justAlerted = true;

                
            }
            if(isWitness)
            {
                witnessTimer += Time.deltaTime;
            }
            if (witnessTimer < witnessTimerStopPoint && isWitness)
            {             
                WitnessedTheCrime();
            }
            else if (!isWitness || witnessTimer >= witnessTimerStopPoint)
            {
                // Switch for npc states
                switch (currentState)
                {
                    // ----------------------------------- Interfere ---------------------------------------
                    case NPC_States.Interfere:
                        agent.isStopped = false;

                        switch (currentSubState)
                        {
                            case NPC_Civ_SubStates.Chase_Player:

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

                                    if (player.GetComponent<Player_Script>().GetDescription() == player_Description)
                                    {
                                        if (Vector3.Distance(transform.position, player.transform.position) < 3)
                                        {
                                            agent.SetDestination(transform.position);
                                            catchTimer -= Time.deltaTime;

                                            if (catchTimer <= 0)
                                            {
                                                // when civ catches player
                                                dataCollector.SetPlayerCaught(true);
                                                SceneManager.LoadScene(1);
                                            }
                                        }
                                        else
                                        {
                                            agent.SetDestination(player.transform.position);
                                            catchTimer = catchTimerReset;
                                        }

                                        Vector3 lookat;
                                        lookat.x = player.transform.position.x;
                                        lookat.y = 0;
                                        lookat.z = player.transform.position.z;

                                        transform.LookAt(lookat);
                                    }
                                }
                                else if (!beenToCrimeSceen && crimeScene != null && !seenPlayer)
                                {
                                    agent.SetDestination(crimeScene);

                                    if (Vector3.Distance(transform.position, crimeScene) < 5)
                                    {
                                        beenToCrimeSceen = true;
                                    }
                                }
                                else
                                {
                                    RandomWander();
                                }

                                break;


                            case NPC_Civ_SubStates.Protect_Shopkeeper:
                                
                                if(crimeScene != Vector3.zero)
                                {
                                    if(!beenToCrimeSceen)
                                    {
                                        if(Vector3.Distance(transform.position,crimeScene)>5)
                                        {
                                            agent.SetDestination(crimeScene);
                                        }
                                        else
                                        {
                                            beenToCrimeSceen = true;
                                        }
                                    }
                                    else
                                    {
                                        walkRange = 10;
                                        RandomWander();                                
                                    }

                                    if (sensor.soundObjects.Count != 0)
                                    {
                                        for (int i = 0; i < sensor.soundObjects.Count; i++)
                                        {
                                            if (sensor.soundObjects[i].GetComponent<NPC_ShopKeep>() != null)
                                            {
                                                shopKeep = sensor.soundObjects[i].GetComponent<NPC_ShopKeep>();
                                            }
                                        }
                                    }


                                    if (shopKeep != null)
                                    {
                                        if(sensor.objects.Count != 0)
                                        {
                                            if(Vector3.Distance(transform.position,shopKeep.transform.position)<15)
                                            {
                                                if(Vector3.Distance(transform.position,sensor.objects[0].transform.position)>2)
                                                {
                                                    agent.SetDestination(sensor.objects[0].transform.position);
                                                    catchTimer = catchTimerReset;
                                                }
                                                else
                                                {
                                                    catchTimer -= Time.deltaTime;

                                                    if(catchTimer<=0)
                                                    {
                                                        // when civ catches player
                                                        dataCollector.SetPlayerCaught(true);
                                                        SceneManager.LoadScene(1);
                                                    }                                              
                                                }
                                            }
                                            else
                                            {
                                                agent.SetDestination(shopKeep.transform.position);
                                            }
                                        }
                                        else
                                        {
                                            if(Vector3.Distance(transform.position,shopKeep.transform.position)>10)
                                            {
                                                agent.SetDestination(shopKeep.transform.position);
                                            }
                                            else
                                            {
                                                
                                                walkRange = 5;
                                                RandomWander();
                                            }
                                        }
                                    }
                                }

                                break;
                        }
                    
                        mat.material.SetColor("_BaseColor", Color.darkGreen);
                        break;
                    // ----------------------------------- Interfere ---------------------------------------


                    // ---------------------------------- Involved ------------------------------------------
                    case NPC_States.Involved:
                      
                        if (targetedGuard != null)
                        {
                            if (Vector3.Distance(transform.position, guardPos) < 5)
                            {
                                targetedGuard.alert = true;
                                targetedGuard.crimeScene = crimeScene;
                                targetedGuard.player_Description = player_Description;
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
                                        else if (guard.player_Description != player_Description)
                                        {
                                            guardPos = guard.transform.position;
                                            targetedGuard = guard;
                                        }
                                        else if (guard.crimeScene != crimeScene)
                                        {
                                            guardPos = guard.transform.position;
                                            targetedGuard = guard;
                                        }
                                    }
                                }
                            }

                            if (targetedGuard == null && !reachedGuard)
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
                                }
                            }

                            if (reachedGuard)
                            {
                                walkRange = 5;
                                RandomWander();
                            }
                        }

                        mat.material.SetColor("_BaseColor", Color.red);

                        break;
                    // ---------------------------------- Involved ------------------------------------------

                    // ------------------------------- Semi_Involved -----------------------------------------
                    case NPC_States.Semi_Involved:
                        agent.isStopped = false;

                        Shout();
                        walkRange = 10;
                        mat.material.SetColor("_BaseColor", Color.purple);
                        break;
                    // ------------------------------- Semi_Involved -----------------------------------------

                    // --------------------------------- Leaving ---------------------------------------------
                    case NPC_States.Leaving:
                        agent.isStopped = false;

                        if (sensor.objects.Count != 0)
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

                        mat.material.SetColor("_BaseColor", Color.yellow);
                        break;
                    // --------------------------------- Leaving ---------------------------------------------

                    // --------------------------------- Lurking ---------------------------------------------
                    case NPC_States.lurking:

                        agent.isStopped = false;
                        if (sensor.objects.Count != 0)
                        {
                            Vector3 lookat;
                            lookat.x = sensor.objects[0].transform.position.x;
                            lookat.y = 0;
                            lookat.z = sensor.objects[0].transform.position.z;

                            transform.LookAt(lookat);

                            if (Vector3.Distance(sensor.objects[0].transform.position, transform.position) < 15)
                            {
                                Vector3 direction = (transform.position - sensor.objects[0].transform.position).normalized;

                                agent.SetDestination(transform.position + direction);
                            }
                        }
                        else
                        {
                            if (Vector3.Distance(transform.position, crimeScene) < 10)
                            {
                                agent.SetDestination(transform.position);
                            }
                            else
                            {
                                agent.SetDestination(crimeScene);
                            }
                        }

                        mat.material.SetColor("_BaseColor", Color.beige);
                        break;
                        // --------------------------------- Lurking ---------------------------------------------
                }
            }
           
        }      
        // if the npc does not know of the crime then they will randomly wander the scene
        else
        {
            RandomWander();          
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
                        else if (NPCs.alert && NPCs.player_Description!=player_Description)
                        {
                            npcPos = NPCs.agent.transform.position;
                            currentNPCs = NPCs;
                            foundOtherNpc = true;
                        }
                        else if (NPCs.alert && NPCs.crimeScene != crimeScene)
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
                agent.SetDestination(currentNPCs.transform.position);

                if(Vector3.Distance(transform.position, currentNPCs.transform.position) <2)
                {                  
                    currentNPCs.alert = true;
                    currentNPCs.crimeScene = crimeScene;

                    //if(currentNPCs.empathy+0.2<=1)
                    //{
                    //    currentNPCs.empathy += 0.2f;
                    //}
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
                        else if (guard.player_Description!=player_Description)
                        {
                            guard.crimeScene = crimeScene;
                            guard.player_Description = player_Description;
                        }
                        else if (guard.crimeScene!=crimeScene)
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

        trustInLaw = Random.Range(0.0f, 1.0f);
        trustInLaw = Mathf.Round(trustInLaw * 10.0f) * 0.1f;

        confidence = Random.Range(0.0f, 1.0f);
        confidence = Mathf.Round(confidence * 10.0f) * 0.1f;
    }


    void BystanderInflence()
    {
        float inflenceRDMValue = Random.Range(0.0f, 1.0f);
        inflenceRDMValue = Mathf.Round(inflenceRDMValue * 10.0f) * 0.1f;
  
        if (inflience>=inflenceRDMValue)
        {           
            if (influencedState == NPC_States.Leaving)
            {
                if (currentState == NPC_States.Semi_Involved || currentState == NPC_States.Involved || currentState == NPC_States.Interfere)
                {
                    if(fear + 0.3f >= 1.0f)
                    {
                        fear = 1.0f;
                    }
                    else
                    {
                        fear += 0.3f;
                    }
                    DeFuzzyFication();
                    dataCollector.numberOfCivsInfluenced += 1;
                }
                else if(currentState == NPC_States.lurking)
                {
                    currentState = NPC_States.Leaving;
                    dataCollector.numberOfCivsInfluenced += 1;
                }
            }

            else if (influencedState == NPC_States.lurking)
            {
                if(currentState == NPC_States.Leaving)
                {
                    if(fear - 0.3 == 0.0f)
                    {
                        fear = 0.0f;
                    }
                    else
                    {
                        fear -= 0.3f;
                    }
                    DeFuzzyFication();
                    dataCollector.numberOfCivsInfluenced += 1;
                }
                else if (currentState == NPC_States.Semi_Involved)
                {
                    currentState = NPC_States.lurking;
                    dataCollector.numberOfCivsInfluenced += 1;
                }
            }          
        }      
    }

    void DeFuzzyFication()
    {
        // Trust in police
        // confidence
        // when checking for bystanders get how many npc are doing what then get a inflience value 

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
                currentState = NPC_States.Leaving;
                debugState = "Leaving";
            }
            else if (bystanderEffect >= 0.5 && fear >= 0.5)
            {
                currentState = NPC_States.lurking;
                debugState = "lurking";
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
                if (trustInLaw <= 0.5 && confidence > 0.5)
                {
                    currentState = NPC_States.Interfere;
                    debugState = "Interfere";
                    currentSubState = NPC_Civ_SubStates.Chase_Player;
                }
                else if (trustInLaw > 0.5 && confidence <= 0.5)
                {
                    currentState = NPC_States.Involved;
                    debugState = "Involved";
                    currentSubState = NPC_Civ_SubStates.Inform_Guards;
                }
                else if (trustInLaw > 0.5 && confidence > 0.5)
                {
                    currentState = NPC_States.Interfere;
                    debugState = "Interfere";
                    currentSubState = NPC_Civ_SubStates.Protect_Shopkeeper;
                }
            }
            else if (bystanderEffect >= 0.5 && empathy == 1)
            {              
                if (trustInLaw <= 0.5 && confidence > 0.5)
                {
                    currentState = NPC_States.Interfere;
                    debugState = "Interfere";
                    currentSubState = NPC_Civ_SubStates.Chase_Player;
                }
                else if (trustInLaw > 0.5 && confidence <= 0.5)
                {
                    currentState = NPC_States.Involved;
                    debugState = "Involved";
                    currentSubState = NPC_Civ_SubStates.Inform_Guards;
                }
                else if(trustInLaw > 0.5 && confidence > 0.5)
                {
                    currentState = NPC_States.Interfere;
                    debugState = "Interfere";
                    currentSubState = NPC_Civ_SubStates.Protect_Shopkeeper;
                }
            }
            else if (bystanderEffect >= 0.5 && empathy >= 0.5)
            {
                currentState = NPC_States.Involved;
                debugState = "Involved";

                if (trustInLaw <= 0.5 && confidence > 0.8)
                {
                    currentState = NPC_States.Interfere;
                    debugState = "Interfere";
                    currentSubState = NPC_Civ_SubStates.Chase_Player;
                }
                else if (trustInLaw > 0.5 && confidence <= 0.8)
                {
                    currentState = NPC_States.Involved;
                    debugState = "Involved";
                    currentSubState = NPC_Civ_SubStates.Inform_Guards;
                }
                else if (trustInLaw > 0.5 && confidence > 0.8)
                {
                    currentState = NPC_States.Interfere;
                    debugState = "Interfere";
                    currentSubState = NPC_Civ_SubStates.Protect_Shopkeeper;
                }
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
        else if (currentSubState == NPC_Civ_SubStates.Protect_Shopkeeper)
        {
            float z = Random.Range(-walkRange, walkRange);
            float x = Random.Range(-walkRange, walkRange);

            destPoint = new Vector3(crimeScene.x + x, crimeScene.y, crimeScene.z + z);

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
        if (!walkPointSet) 
            SearchForDest();

        if (walkPointSet) 
            agent.SetDestination(destPoint);

        if (Vector3.Distance(transform.position, destPoint) < 10) 
            walkPointSet = false;

        if (agent.pathStatus == NavMeshPathStatus.PathPartial) 
            walkPointSet = false;
    }

    public void SetPlayerDescription(Player_Descriptions playerDisc)
    {
        player_Description = playerDisc;
    }  


    void WitnessedTheCrime()
    {
        if(sensor.objects.Count != 0)
        {
            Vector3 lookat;
            lookat.x = sensor.objects[0].transform.position.x;
            lookat.y = 0;
            lookat.z = sensor.objects[0].transform.position.z;

            transform.LookAt(lookat);

            agent.SetDestination(transform.position);
        }
    }
}
