using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Bullet : MonoBehaviour
{
    public float speed = 5f;
    public float timeToDestroy = 4f;
    public float damage = 1;
    public bool playerBullet = false;
    public ParticleSystem particle;
    void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && playerBullet)
        {
            Enemy e = collision.gameObject.GetComponent<Enemy>(); // obtener ese enemigo su componente
            Instantiate(particle, transform.position, Quaternion.identity);
            e.TakeDamage(damage); // ejecutar el metodo danio al enemy
            Destroy(gameObject); //destroy bullet
        } else if (collision.gameObject.CompareTag("Player") && !playerBullet)
        {
            Player e = collision.gameObject.GetComponent<Player>(); // obtener ese enemigo su componente
            Instantiate(particle, transform.position, Quaternion.identity);
            e.TakeDamage(damage); 
            Destroy(gameObject); //destroy bullet

        }
    }



}