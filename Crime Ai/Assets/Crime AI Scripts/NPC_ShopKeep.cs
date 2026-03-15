using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

enum NPC_SHOPKEEP_STATES
{
    Idle,
    SeekHelp,           // Look for guards
    BeingStolenFrom,    
    Shouting            // shouting
}

public class NPC_ShopKeep : MonoBehaviour
{
    NavMeshAgent agent;

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


    public List<GameObject> lawEnforcementLocations = new List<GameObject>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

        agent = GetComponent<NavMeshAgent>();

        sensor = GetComponent<NPC_AI_Sensor>();


        shopLocation = transform.position;

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

                break;

            case NPC_SHOPKEEP_STATES.BeingStolenFrom:

                break;

            case NPC_SHOPKEEP_STATES.SeekHelp:

                break;

            case NPC_SHOPKEEP_STATES.Shouting:

                break;

        }




    }

    public void GettenStolenFrom(GameObject player)
    {
        isBeingStolenFrom = true;

        Vector3 lookat;
        lookat.x = player.transform.position.x;
        lookat.y = 0;
        lookat.z = player.transform.position.z;

        transform.LookAt(lookat);

    }
}
