using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Input_Mangaer : MonoBehaviour
{
    private PlayerInputs playerInput;
    private PlayerInputs.OnFootActions onfoot;

    private Player_Script playerScript;

    private PlayerLook look;

    public float stealTimer = 0;
    public float stealInterval = 30;

    public bool movementLocked = false;

    




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerInput = new PlayerInputs();
        onfoot = playerInput.OnFoot;

        playerScript = GetComponent<Player_Script>();

        look = GetComponent<PlayerLook>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!movementLocked)
        {
            playerScript.ProcessMove(onfoot.Movement.ReadValue<Vector2>());
        }       
    }

    private void LateUpdate()
    {
        look.ProcessLook(onfoot.Look.ReadValue<Vector2>());      
    }

    private void Update()
    {
        if(stealTimer>0)
        {
            stealTimer -= Time.deltaTime;
        }

        if(onfoot.Steal.ReadValue<float>() == 1 && stealTimer <= 0)
        {
           playerScript.Steal(onfoot.Steal.ReadValue<float>());
            stealTimer += stealInterval;
        }


        
    }

    private void OnEnable()
    {
        onfoot.Enable();
    }
    private void OnDisable()
    {
        onfoot.Disable();
    }
}
