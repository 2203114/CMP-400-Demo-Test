
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Player_Descriptions
{
    black_Clothes,
    yellow_Clothes,
    purple_Clothes,
    green_Clothes
}


public class Player_Script : MonoBehaviour
{
    /// 
    /// Variables
    /// 

    private Camera cam;

    private CharacterController controller;

    private PlayerLook playerLook;

    private Input_Mangaer input_Mangaer;


    private Vector3 playerVelocity;
    private bool isGrounded;

    public float gravity = -9.8f;
    public float speed = 5f;

    public bool tryingTosteal = false;
    public float stealingTimer = 10;
    public float stealingInterval = 10;


    public float lockMovementTimer = 3;
    public float lockMovementInterval = 3;
    public bool stealing = false;


    public Player_Descriptions clothes;


    public GameObject endPoint;


    /// 
    /// Variables
    /// 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponent<Camera>();

        controller = GetComponent<CharacterController>();

        playerLook = GetComponent<PlayerLook>();

        input_Mangaer = GetComponent<Input_Mangaer>();

        SetRandomClothes(); 

    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(transform.position,endPoint.transform.position)<3)
        {
            SceneManager.LoadScene(2);
        }

        isGrounded = controller.isGrounded;

        if(tryingTosteal)
        {
            playerLook.StealCheck();

            stealingTimer -= Time.deltaTime;
            if(stealingTimer<=0)
            {
                stealingTimer = stealingInterval;
                tryingTosteal = false;
            }
        }
        
        if(stealing)
        {
            lockMovementTimer -= Time.deltaTime;

            if(lockMovementTimer<=0)
            {
                stealing = false;
                lockMovementTimer = lockMovementInterval;
                input_Mangaer.movementLocked = false;
            }
            else
            {
                input_Mangaer.movementLocked = true;
            }

        }



    }

    void SetRandomClothes()
    {
        int rnd = Random.Range(1, 4);

        switch(rnd)
        {
            case 1:
                clothes = Player_Descriptions.black_Clothes;
                break;

            case 2:
                clothes = Player_Descriptions.yellow_Clothes;
                break;

            case 3:
                clothes = Player_Descriptions.purple_Clothes;
                break;

            case 4:
                clothes = Player_Descriptions.green_Clothes;
                break;
        }
    }

    public Player_Descriptions GetDescription()
    {
        return clothes;
    }

    // receive inputs from input manager
    public void ProcessMove(Vector2 inputs)
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = inputs.x;
        moveDirection.z = inputs.y;

        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);

        if(isGrounded&& playerVelocity.y<0)
        {
            playerVelocity.y = -2f;
        }

        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void Steal(float s)
    {

        tryingTosteal = true;
        
        
    }

    public void Stealing(GameObject shopKeep)
    {
       
        stealing = true;

        shopKeep.GetComponent<NPC_ShopKeep>().GettenStolenFrom(transform.gameObject);

    }

}
