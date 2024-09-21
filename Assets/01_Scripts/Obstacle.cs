using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{

    private AudioSource m_AudioSource; // Referencia al componente de AudioSource
    // Start is called before the first frame update
    void Start()
    {
        // Obtener el componente AudioSource en el objeto Flag
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
            // Reproducir el sonido al alcanzar la bandera
            m_AudioSource.Play();
            // AQUI RECIBIO DAÑO
            // Ejemplo: Cambiar de nivel o mostrar un mensaje de victoria.
            Debug.Log("Uyy me dio, q tonto soy!");
        }
    }
}
