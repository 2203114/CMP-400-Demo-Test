
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;

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


    public float lockMovementTimer = 10;
    public float lockMovementInterval = 10;
    public bool stealing = false;





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

    }

    // Update is called once per frame
    void Update()
    {
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
        Debug.Log("yippi");

        stealing = true;

        shopKeep.GetComponent<NPC_ShopKeep>().GettenStolenFrom(transform.gameObject);

    }

}
