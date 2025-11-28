//using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;




public class AiPatrol : MonoBehaviour
{

    public AI_Sensor sensor;

    NavMeshAgent agent;

    [SerializeField] LayerMask groundLayer;

    Vector3 destPoint;
    bool walkPointSet;
    [SerializeField] float walkRange;

    public AnimationCurve curiosity;
    public AnimationCurve willingnessToHelp;
    public AnimationCurve bussyness;


    public float curiosityStat, willingnessToHelpStat, bussynessStat = 0.0f;

    float curiosityValue, willingnessToHelpValue, bussynessValue = 0.0f;
    string state;


    void CompairStats()
    {
        curiosityValue = curiosity.Evaluate(curiosityStat);
        willingnessToHelpValue = willingnessToHelp.Evaluate(willingnessToHelpStat);
        bussynessValue = bussyness.Evaluate(bussynessStat);

        if(curiosityValue > bussynessValue)
        {
            if(curiosityValue > willingnessToHelpValue)
            {
                state = "lurking";
            }
            else
            {
                state = "helping";
            }
        }
        else
        {
            if (bussynessValue > willingnessToHelpValue)
            {
                state = "bussy";
            }
            else
            {
                state = "helping";
            }
        }
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        CompairStats();

        sensor = GetComponent<AI_Sensor>();
    }

    // Update is called once per frame
    void Update()
    {
       // if (state != null)
       // {
       //     if (state == "lurking")
       //     {
       //         walkRange = 5; 
       //     }
       //     if (state == "bussy")
       //     {
       //         walkRange = 150;
       //     }
       // }
       //if (state != "helping")
       // {
       //     Patrol();
       // }
     

       if (sensor.objects.Count>0)
        {
            if(state == "lurking")
            {
                transform.LookAt(sensor.objects[0].transform);
            }
            
            if (state == "helping")
            {
                agent.SetDestination(sensor.objects[0].transform.position);
            }

            else
            {
               // Patrol();
            }

        }
        else
        {
           // Patrol();
        }



    }

    void Patrol()
    {
        if (!walkPointSet) SearchForDest();
        if (walkPointSet) agent.SetDestination(destPoint);
        if (Vector3.Distance(transform.position, destPoint)<10) walkPointSet = false;
    }

    void SearchForDest()
    {
        float z = Random.Range(-walkRange, walkRange);
        float x = Random.Range(-walkRange, walkRange);

        destPoint = new Vector3(transform.position.x + x,transform.position.y, transform.position.z + z);
        

        if(Physics.Raycast(destPoint,Vector3.down, groundLayer))
        {
            walkPointSet = true;
        }
    }
}
