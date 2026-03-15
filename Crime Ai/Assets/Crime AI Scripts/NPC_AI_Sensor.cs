using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NPC_AI_Sensor : MonoBehaviour
{
    /// 
    /// Varibales
    /// 

    // distances for sight and hearing
    public float distance = 10;
    public float soundDistance = 20;

    // sight cone
    public float angle = 30.0f;
    public float height = 1.0f;
    public Color debugMeshColor = Color.red;

    public int scanFrequency = 30;

    public LayerMask layers;
    public LayerMask soundLayer;

    // lists for the game object that are seen and heard
    public List<GameObject> objects = new List<GameObject>();
    public List<GameObject> soundObjects = new List<GameObject>();

    Collider[] colliders = new Collider[50];
    Collider[] soundColliders = new Collider[25];

    int count;
    int soundCount;

    float scanInterval;
    float scanTimer;

    Mesh mesh;

    public bool isGuard;

    /// 
    /// Varibales
    /// 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scanInterval = 1.0f / scanFrequency;
    }

    // Update is called once per frame
    void Update()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer < 0)
        {
            scanTimer += scanInterval;
            Scan();

        }
    }


    private void Scan()
    {
        count = Physics.OverlapSphereNonAlloc(transform.position, distance, colliders, layers, QueryTriggerInteraction.Collide);

        objects.Clear();
        for (int i = 0; i < count; i++)
        {
            GameObject obj = colliders[i].gameObject;
            if (IsInSight(obj))
            {
                objects.Add(obj);
            }
        }

        soundCount = Physics.OverlapSphereNonAlloc(transform.position, soundDistance, soundColliders, soundLayer, QueryTriggerInteraction.Collide);
        soundObjects.Clear();

        for (int i = 0; i < soundCount; i++)
        {

            GameObject sObj = soundColliders[i].gameObject;

            if (sObj != this.gameObject)
            {
                soundObjects.Add(sObj);
            }

        }


    }
    public bool IsInSight(GameObject obj)
    {
        Vector3 origin = transform.position;
        Vector3 dest = obj.transform.position;
        Vector3 direction = dest - origin;



        if (direction.y < 0 || direction.y > height)
        {
            return false;
        }

        direction.y = 0;

        float deltaAngle = Vector3.Angle(direction, transform.forward);
        if (deltaAngle > angle)
        {
            return false;
        }

        Player player = obj.GetComponent<Player>();

        if (player != null)
        {
            if (player.stealing == true)
            {
                if (isGuard)
                {
                    Npc_Guard NPC = GetComponent<Npc_Guard>();
                    NPC.crimeScene = player.transform.position;
                    NPC.alert = true;
                }
                else
                {
                    Npc_Civillian NPC = GetComponent<Npc_Civillian>();
                    NPC.crimeScene = player.transform.position;
                    NPC.alert = true;
                }

               
               
            }
        }
        return true;
    }
    Mesh CreateWedgeMesh()
    {
        Mesh mesh = new Mesh();

        int segments = 10;
        int numOfTriangles = (segments * 4) + 2 + 2;
        int numVertices = numOfTriangles * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        Vector3 bottomCenter = Vector3.zero;
        Vector3 bottomLeft = Quaternion.Euler(0, -angle, 0) * Vector3.forward * distance;
        Vector3 bottomRight = Quaternion.Euler(0, angle, 0) * Vector3.forward * distance;

        Vector3 topCentre = bottomCenter + Vector3.up * height;
        Vector3 topRight = bottomRight + Vector3.up * height;
        Vector3 topLeft = bottomLeft + Vector3.up * height;

        int vert = 0;

        // left side 

        vertices[vert++] = bottomCenter;
        vertices[vert++] = bottomLeft;
        vertices[vert++] = topLeft;

        vertices[vert++] = topLeft;
        vertices[vert++] = topCentre;
        vertices[vert++] = bottomCenter;

        // right side

        vertices[vert++] = bottomCenter;
        vertices[vert++] = topCentre;
        vertices[vert++] = topRight;

        vertices[vert++] = topRight;
        vertices[vert++] = bottomRight;
        vertices[vert++] = bottomCenter;


        float currentAngle = -angle;
        float deltaAngle = (angle * 2) / segments;
        for (int i = 0; i < segments; i++)
        {


            bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * distance;
            bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * distance;


            topRight = bottomRight + Vector3.up * height;
            topLeft = bottomLeft + Vector3.up * height;

            // far side

            vertices[vert++] = bottomLeft;
            vertices[vert++] = bottomRight;
            vertices[vert++] = topRight;

            vertices[vert++] = topRight;
            vertices[vert++] = topLeft;
            vertices[vert++] = bottomLeft;

            // top 

            vertices[vert++] = topCentre;
            vertices[vert++] = topLeft;
            vertices[vert++] = topRight;

            // bottom

            vertices[vert++] = bottomCenter;
            vertices[vert++] = bottomRight;
            vertices[vert++] = bottomLeft;

            currentAngle += deltaAngle;
        }

        for (int i = 0; i < numVertices; i++)
        {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }


    private void OnValidate()
    {
        mesh = CreateWedgeMesh();
        scanInterval = 1.0f / scanFrequency;
    }

    //private void OnDrawGizmos()
    //{
    //    if (mesh)
    //    {
    //        Gizmos.color = debugMeshColor;
    //        Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
    //    }

    //    Gizmos.DrawWireSphere(transform.position, distance);
    //    for (int i = 0; i < count; i++)
    //    {
    //        Gizmos.DrawSphere(colliders[i].transform.position, 1.0f);
    //    }

    //    Gizmos.color = Color.green;
    //    foreach (var obj in objects)
    //    {
    //        Gizmos.DrawSphere(obj.transform.position, 1.0f);
    //    }

    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, soundDistance);

    //    foreach (var obj in soundObjects)
    //    {
    //        Gizmos.DrawSphere(obj.transform.position, 1.0f);
    //    }
    //}
}
