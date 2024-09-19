using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Aquí puedes agregar lo que sucede cuando Mario llega al final.
            // Ejemplo: Cambiar de nivel o mostrar un mensaje de victoria.
            Debug.Log("¡Nivel completado!");
        }
    }
}
