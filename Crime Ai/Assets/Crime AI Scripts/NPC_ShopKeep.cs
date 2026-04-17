using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum NPC_SHOPKEEP_STATES
{
    Idle,
    SeekHelp,           // Look for guards
    BeingStolenFrom,    
    Shouting,           // shouting
    ChaseOfPlayer,
    BackUpFromPlayer
}

public class NPC_ShopKeep : MonoBehaviour
{
    NavMeshAgent agent;

    public SkinnedMeshRenderer mat;

    public bool isBeingStolenFrom;

    public Animator animator;

    [SerializeField] LayerMask groundLayer;

    // This npc sensor for sight and hearing other agents
    public NPC_AI_Sensor sensor;

    private Vector3 shopLocation;

    // values for fuzzy logic
    public float itemValue = 0.0f;
    public float fear = 0.0f;
    public float bystanderEffect = 0.0f;

    NPC_SHOPKEEP_STATES currentState;

    public bool alert;

    Player_Descriptions player_Description;


    public List<GameObject> lawEnforcementLocations = new List<GameObject>();
    public List<Vector3> perviousLawELocations = new List<Vector3>();
    Vector3 closestLawELocations;
    bool foundClosestLawLocation;
    bool foundGuard;

    bool beenToGuardHouse;
    Npc_Guard targetGuard;

    public  float shoutTimer = 1;
    float shoutTimerInterval = 1;
    int numberOfShoutsmax = 5;
    public  int currentAmountOfShouts = 0;

    public float chaseTimer = 0;
    float chaseTimerInterval = 10;

   public float stealTimer = 8;
    float stealTimerInterval = 8;

    public GameObject dataObject;
    DataCollector dataCollector;
    bool dataHasState = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

        agent = GetComponent<NavMeshAgent>();

        sensor = GetComponent<NPC_AI_Sensor>();

        shopLocation = transform.position;

        currentState = NPC_SHOPKEEP_STATES.Idle;

        GetRandomValues();

        dataCollector = dataObject.GetComponent<DataCollector>();
        dataCollector.numberOfShopkeeps +=1;
    }

    void GetRandomValues()
    {
        fear = Random.Range(0.0f, 1.0f);
        fear = Mathf.Round(fear * 10.0f) * 0.1f;

        itemValue = Random.Range(0.0f, 1.0f);
        itemValue = Mathf.Round(itemValue * 10.0f) * 0.1f;
    }

    void DeFuzzyfication()
    {
        if(fear==1)
        {
            if(itemValue==1)
            {
                if(bystanderEffect==1)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
                else if(bystanderEffect<=0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.SeekHelp;
                    Debug.Log("seekhelp");
                }
                else if (bystanderEffect>0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
            }

            else if (itemValue<=0.5)
            {
                if (bystanderEffect == 1)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
                else if (bystanderEffect <= 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.BackUpFromPlayer;
                    Debug.Log("backup");
                }
                else if (bystanderEffect > 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
            }

            else if(itemValue>0.5)
            {
                if (bystanderEffect == 1)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
                else if (bystanderEffect <= 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.BackUpFromPlayer;
                    Debug.Log("backup");
                }
                else if (bystanderEffect > 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
            }
        }
        else if(fear<=0.5)
        {
            if (itemValue == 1)
            {
                if (bystanderEffect == 1)
                {
                    currentState = NPC_SHOPKEEP_STATES.ChaseOfPlayer;
                    Debug.Log("chase");
                }
                else if (bystanderEffect <= 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.SeekHelp;
                    Debug.Log("seek");
                }
                else if (bystanderEffect > 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
            }

            else if (itemValue <= 0.5)
            {
                if (bystanderEffect == 1)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
                else if (bystanderEffect <= 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
                else if (bystanderEffect > 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
            }

            else if (itemValue > 0.5)
            {
                if (bystanderEffect == 1)
                {
                    currentState = NPC_SHOPKEEP_STATES.ChaseOfPlayer;
                    Debug.Log("chase");
                }
                else if (bystanderEffect <= 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.SeekHelp;
                    Debug.Log("seek");
                }
                else if (bystanderEffect > 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
            }
        }

        else if(fear>0.5)
        {
            if (itemValue == 1)
            {
                if (bystanderEffect == 1)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
                else if (bystanderEffect <= 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.SeekHelp;
                    Debug.Log("seek");
                }
                else if (bystanderEffect > 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
            }

            else if (itemValue <= 0.5)
            {
                if (bystanderEffect == 1)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
                else if (bystanderEffect <= 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.SeekHelp;
                    Debug.Log("seek");
                }
                else if (bystanderEffect > 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.BackUpFromPlayer;
                    Debug.Log("backup");
                }
            }

            else if (itemValue > 0.5)
            {
                if (bystanderEffect == 1)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
                else if (bystanderEffect <= 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.SeekHelp;
                    Debug.Log("seek");
                }
                else if (bystanderEffect > 0.5)
                {
                    currentState = NPC_SHOPKEEP_STATES.Shouting;
                    Debug.Log("Shouting");
                }
            }
        }

        if(!dataHasState)
        {
            dataCollector.AddShopKeepState(currentState);
            dataHasState = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if(isBeingStolenFrom)
        {
            animator.SetBool("Stealing", true);

        }
        else
        {
            animator.SetBool("Stealing", false);
        }


        switch(currentState)
        {
            case NPC_SHOPKEEP_STATES.Idle:
                mat.material.SetColor("_BaseColor", Color.aliceBlue);
                animator.SetBool("Stealing", false);
                animator.SetFloat("Speed", 0.0f);
                break;

            case NPC_SHOPKEEP_STATES.BeingStolenFrom:

                stealTimer -= Time.deltaTime;

                if(stealTimer <= 0)
                {
                    stealTimer = stealTimerInterval;
                    DeFuzzyfication();
                }
                animator.SetBool("Stealing", true);
                animator.SetFloat("Speed", 0.0f);
                mat.material.SetColor("_BaseColor", Color.crimson);

                break;

            case NPC_SHOPKEEP_STATES.SeekHelp:
                
                animator.SetBool("Stealing", false);
                animator.SetFloat("Speed", 0.2f);

                if (!foundClosestLawLocation)
                {
                    if (lawEnforcementLocations != null)
                    {
                        if (lawEnforcementLocations.Count > 1)
                        {
                            for (int i = 0; i < lawEnforcementLocations.Count; i++)
                            {
                                if (perviousLawELocations.Count != 0)
                                {
                                    if (Vector3.Distance(lawEnforcementLocations[i].transform.position, transform.position) < Vector3.Distance(perviousLawELocations[i - 1], transform.position))
                                    {
                                        closestLawELocations = lawEnforcementLocations[i].transform.position;
                                    }
                                    else
                                    {
                                        closestLawELocations = perviousLawELocations[i - 1];
                                    }
                                }
                                else
                                {
                                    perviousLawELocations.Add(lawEnforcementLocations[i].transform.position);
                                }
                            }
                        }
                    }

                    if (closestLawELocations != null)
                    {
                        foundClosestLawLocation = true;
                    }
                }

                if (targetGuard == null && !foundGuard)
                {
                    if (sensor.soundObjects.Count != 0)
                    {
                        for (int i = 0; i < sensor.soundObjects.Count; i++)
                        {
                            if (sensor.soundObjects[i].GetComponent<Npc_Guard>())
                            {
                                targetGuard = sensor.soundObjects[i].GetComponent<Npc_Guard>();
                            }
                        }
                    }
                }

                if (!foundGuard && !beenToGuardHouse && targetGuard == null)
                {
                    if (Vector3.Distance(transform.position, closestLawELocations) > 5)
                    {
                        agent.SetDestination(closestLawELocations);
                    }
                    else
                    {
                        beenToGuardHouse = true;
                    }
                }
                else if (targetGuard != null && !foundGuard)
                {
                    if (Vector3.Distance(transform.position, targetGuard.transform.position) > 5)
                    {
                        agent.SetDestination(targetGuard.transform.position);
                    }
                    else
                    {
                        targetGuard.alert = true;
                        targetGuard.player_Description = player_Description;
                        targetGuard.crimeScene = shopLocation;

                        foundGuard = true;
                        targetGuard = null;
                    }
                }

                if (foundGuard || beenToGuardHouse)
                {
                    if (Vector3.Distance(transform.position, shopLocation) > 2)
                    {
                        agent.SetDestination(shopLocation);
                    }
                    else
                    {
                        currentState = NPC_SHOPKEEP_STATES.Idle;
                    }
                }

                mat.material.SetColor("_BaseColor", Color.blueViolet);

                break;

            case NPC_SHOPKEEP_STATES.Shouting:

                animator.SetBool("Stealing", true);
                if (shoutTimer > 0)
                {
                    shoutTimer -= Time.deltaTime;
                }


                if (currentAmountOfShouts < numberOfShoutsmax)
                {
                    if (shoutTimer <= 0)
                    {
                        if (sensor.soundObjects.Count != 0)
                        {
                            for (int i = 0; i < sensor.soundObjects.Count; i++)
                            {
                                if (sensor.soundObjects[i].GetComponent<Npc_Civillian>() != null)
                                {
                                    if (!sensor.soundObjects[i].GetComponent<Npc_Civillian>().alert)
                                    {
                                        sensor.soundObjects[i].GetComponent<Npc_Civillian>().alert = true;
                                        sensor.soundObjects[i].GetComponent<Npc_Civillian>().crimeScene = shopLocation;
                                        sensor.soundObjects[i].GetComponent<Npc_Civillian>().player_Description = player_Description;
                                    }
                                    else if (sensor.soundObjects[i].GetComponent<Npc_Civillian>().player_Description != player_Description)
                                    {
                                        sensor.soundObjects[i].GetComponent<Npc_Civillian>().crimeScene = shopLocation;
                                        sensor.soundObjects[i].GetComponent<Npc_Civillian>().player_Description = player_Description;
                                    }
                                    else if (sensor.soundObjects[i].GetComponent<Npc_Civillian>().crimeScene != shopLocation)
                                    {
                                        sensor.soundObjects[i].GetComponent<Npc_Civillian>().crimeScene = shopLocation;
                                        sensor.soundObjects[i].GetComponent<Npc_Civillian>().player_Description = player_Description;
                                    }

                                }
                                if (sensor.soundObjects[i].GetComponent<Npc_Guard>() != null)
                                {
                                    if (!sensor.soundObjects[i].GetComponent<Npc_Guard>().alert)
                                    {
                                        sensor.soundObjects[i].GetComponent<Npc_Guard>().alert = true;
                                        sensor.soundObjects[i].GetComponent<Npc_Guard>().crimeScene = shopLocation;
                                        sensor.soundObjects[i].GetComponent<Npc_Guard>().player_Description = player_Description;
                                    }
                                    else if (sensor.soundObjects[i].GetComponent<Npc_Guard>().player_Description != player_Description)
                                    {
                                        sensor.soundObjects[i].GetComponent<Npc_Guard>().crimeScene = shopLocation;
                                        sensor.soundObjects[i].GetComponent<Npc_Guard>().player_Description = player_Description;
                                    }
                                    else if (sensor.soundObjects[i].GetComponent<Npc_Guard>().crimeScene != shopLocation)
                                    {
                                        sensor.soundObjects[i].GetComponent<Npc_Guard>().crimeScene = shopLocation;
                                        sensor.soundObjects[i].GetComponent<Npc_Guard>().player_Description = player_Description;
                                    }
                                }
                            }

                            currentAmountOfShouts++;
                            shoutTimer = shoutTimerInterval;
                        }
                    }
                }






                if (currentAmountOfShouts >= numberOfShoutsmax)
                {
                    currentState = NPC_SHOPKEEP_STATES.Idle;
                    currentAmountOfShouts = 0;
                }

                mat.material.SetColor("_BaseColor", Color.darkOrange);

                break;

            case NPC_SHOPKEEP_STATES.BackUpFromPlayer:

                animator.SetBool("Stealing", false);
                animator.SetFloat("Speed", 0.2f);

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
                    if (Vector3.Distance(transform.position, shopLocation) > 2)
                    {
                        agent.SetDestination(shopLocation);
                    }
                    else
                    {
                        currentState = NPC_SHOPKEEP_STATES.Idle;
                    }
                }

                mat.material.SetColor("_BaseColor", Color.gray);
                break;

            case NPC_SHOPKEEP_STATES.ChaseOfPlayer:

                animator.SetBool("Stealing", false);
                animator.SetFloat("Speed", 0.2f);

                chaseTimer += Time.deltaTime;
                
                if(chaseTimer<chaseTimerInterval)
                {
                    if(sensor.objects.Count != 0)
                    {
                        Vector3 lookat;
                        lookat.x = sensor.objects[0].transform.position.x;
                        lookat.y = 0;
                        lookat.z = sensor.objects[0].transform.position.z;

                        transform.LookAt(lookat);

                        if(Vector3.Distance(transform.position,sensor.objects[0].transform.position)>5)
                        {
                            agent.SetDestination(sensor.objects[0].transform.position);
                        }
                        else
                        {
                            agent.SetDestination(transform.position);
                        }
                    }
                }
                else if (chaseTimer>=chaseTimerInterval)
                {
                    if (Vector3.Distance(transform.position, shopLocation) > 2)
                    {
                        agent.SetDestination(shopLocation);
                    }
                    else
                    {
                        currentState = NPC_SHOPKEEP_STATES.Idle;
                    }
                }

                mat.material.SetColor("_BaseColor", Color.lightSalmon);

                break;
        }
    }

    private void RandomlySetStats()
    {
        itemValue = Random.Range(0.0f, 1.0f);
        itemValue = Mathf.Round(itemValue * 10.0f) * 0.1f;

        fear = Random.Range(0.0f, 1.0f);
        fear = Mathf.Round(fear * 10.0f) * 0.1f;

    }


    public void GettenStolenFrom(GameObject player)
    {
        isBeingStolenFrom = true;

        Vector3 lookat;
        lookat.x = player.transform.position.x;
        lookat.y = 0;
        lookat.z = player.transform.position.z;

        transform.LookAt(lookat);

        bystanderEffect = (sensor.bystanderObjects.Count / 2) * 0.1f;

        // Debug.Log(bystanderEffect);
        currentState = NPC_SHOPKEEP_STATES.BeingStolenFrom;
    }

    public void SetPlayerDescription(Player_Descriptions playerDisc)
    {
        player_Description = playerDisc;
    }
}
