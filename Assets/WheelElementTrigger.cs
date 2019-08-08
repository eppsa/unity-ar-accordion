using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class WheelElementTrigger : MonoBehaviour
{
    private AudioSource RotationWheelSound;

    void Start()
    {
        RotationWheelSound = GameObject.Find("Sounds/RotationWheel").GetComponent<AudioSource>();
    }
    public void OnTriggerEnter2D(Collider2D obj)
    {
        RotationWheelSound.Play();
    }

}
