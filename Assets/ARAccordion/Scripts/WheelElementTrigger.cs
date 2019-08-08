using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class WheelElementTrigger : MonoBehaviour
{
    private AudioSource RotationWheelSound;

    void OnEnable()
    {
        RotationWheelSound = GameObject.Find("Sounds/RotationWheel").GetComponent<AudioSource>();
        RotationWheelSound.mute = true;

        Invoke("Unmute", 0.1f);
    }

    void Unmute()
    {
        RotationWheelSound.mute = false;
    }

    public void OnTriggerEnter2D(Collider2D obj)
    {
        RotationWheelSound.Play();
    }

}
