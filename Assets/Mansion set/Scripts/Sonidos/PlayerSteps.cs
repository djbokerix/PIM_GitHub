using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSteps : MonoBehaviour
{
    public AudioSource audioSource; 
    public AudioClip sonidoPasos;   

    [Range(0f, 1f)] public float volumenPasos = 0.4f; 

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Si nos movemos y estamos tocando el suelo y no está sonando ya
        if (controller.velocity.magnitude > 0.1f && controller.isGrounded)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = sonidoPasos;
                audioSource.volume = volumenPasos;
                audioSource.loop = true; // Que se repita mientras andas
                audioSource.Play();
            }
        }
        else
        {
            // Si paramos, el sonido para
            if (audioSource.isPlaying && audioSource.clip == sonidoPasos)
            {
                audioSource.Stop();
            }
        }
    }
}
