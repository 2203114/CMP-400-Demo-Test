//using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;




public class AiPatrol : MonoBehaviour
{

    public AI_Sensor sensor;

    NavMeshAgent agent;

    public Renderer mat;

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

    public bool alert;

    Vector3 CrimainalPos;
    bool atCrim = false;

    AiPatrol NPCS;

    public int shoutInterval;
    float shoutTimer;


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

        shoutTimer = Random.Range(0, shoutInterval);

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        CompairStats();

        sensor = GetComponent<AI_Sensor>();

        mat = GetComponent<Renderer>();

        mat.material.SetColor("_BaseColor",Color.green);
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
     

       if (alert)
        {
            if(state == "lurking")
            {
                if (sensor.objects.Count != 0)
                {
                    transform.LookAt(sensor.objects[0].transform);
                }

                shoutTimer -= Time.deltaTime;
                if(shoutTimer<0)
                {
                    shoutTimer += shoutInterval;
                    Shout();
                }

                walkRange = 10;
                Patrol();
                mat.material.SetColor("_BaseColor", Color.purple);
                

            }
            
            if (state == "helping")
            {
                if (sensor.objects.Count != 0)
                {
                   
                    CrimainalPos = sensor.objects[0].transform.position;

                    if (Vector3.Distance(transform.position, CrimainalPos) < 5)
                    {
                        // print(Vector3.Distance(transform.position, CrimainalPos));
                        agent.isStopped = true; ;
                        atCrim = true;
                    }
                    else
                    {
                        agent.isStopped = false; ;
                        atCrim = false;
                    }

                    if (!atCrim)
                    {
                        agent.SetDestination(CrimainalPos);
                    }
                }

                //shoutTimer -= Time.deltaTime;
                //if (shoutTimer < 0)
                //{
                //    shoutTimer += shoutInterval;
                //    Shout();
                //}

                mat.material.SetColor("_BaseColor", Color.blue);
            }

            if(state=="bussy")
            {
                mat.material.SetColor("_BaseColor", Color.orange);
                Patrol();
            }

        }
        else
        {
            Patrol();
        }



    }

    void Shout()
    {
        if (sensor.soundObjects.Count !=0)
        {
           for (int i = 0; i < sensor.soundObjects.Count; i++)
            {
                NPCS = sensor.soundObjects[i].GetComponent<AiPatrol>();
                if (NPCS != null)
                {
                    NPCS.alert = true;
                }
            }
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


    void CheckForTheft()
    {

    }
}
