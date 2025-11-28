using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public Test test;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        test = GetComponent<Test>();
       

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            test.testBool = false;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            test.testBool = true;
        }
    }
}
