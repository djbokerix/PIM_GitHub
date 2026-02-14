using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAmbience : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] sonidosSusto;

    public float tiempoMin = 20f;
    public float tiempoMax = 40f;

    private float timer;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            ReproducirSonidoRandom();
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        timer = Random.Range(tiempoMin, tiempoMax);
    }

    void ReproducirSonidoRandom()
    {
        if (sonidosSusto.Length == 0) return;

        // Elige uno al azar
        int index = Random.Range(0, sonidosSusto.Length);

        // Jugar con el volumen y posición para que parezca lejos
        audioSource.panStereo = Random.Range(-0.8f, 0.8f); // Izquierda o derecha
        audioSource.pitch = Random.Range(0.9f, 1.1f);      // Variar tono ligeramente

        audioSource.PlayOneShot(sonidosSusto[index], 0.7f);
    }
}
