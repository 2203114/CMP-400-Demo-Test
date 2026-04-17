using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public Test test;

    public bool stealing;

    public Renderer rend;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        test = GetComponent<Test>();
        rend = GetComponent<Renderer>();
        rend.material.SetColor("_BaseColor", Color.blueViolet);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            stealing = false;
            rend.material.SetColor("_BaseColor", Color.blueViolet);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            stealing = true;
            rend.material.SetColor("_BaseColor", Color.red);
        }
    }
}
