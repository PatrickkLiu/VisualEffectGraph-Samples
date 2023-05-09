using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anim : MonoBehaviour
{
    public Animator anim;
    // Use this for initialization
    void Start () {
        anim = GetComponent<Animator>();
    }
    private void FixedUpdate()
    {
        if (Input.GetKeyUp("1"))
        {
            anim.Play("JumpingDown");
        }
    
        
        
    }
}
