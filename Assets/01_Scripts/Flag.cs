using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    public AudioSource m_AudioSource; // AudioSource de la bandera
    public CameraFollow cameraFollow; // Referencia al script de la cámara

    // Start is called before the first frame update
    void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Reproducir el sonido de la bandera
            m_AudioSource.Play();

            // Mensaje de nivel completado
            Debug.Log("¡Nivel completado!");
        }
    }
}
