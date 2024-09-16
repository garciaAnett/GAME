using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public EnemyType type;
    public float speed = 2;
    float life = 3;
    public float maxlife = 3;
    public float timeBtwShoot = 1.5f;
    float timer = 0;
    public float range = 4;
    bool targetInRange = false;
    Transform target;
    public Transform firePoint;
    public float damage = 2f;
   
    public Bullet prefab;
    public float bulletSpeed = 5f;
    public ParticleSystem particle;
    public float chance = 5f;
    public Image lifebar;
    void Start()
    {
        GameObject ship = GameObject.FindGameObjectWithTag("Player");
        target = ship.transform;
        life = maxlife;
        lifebar.fillAmount = life / maxlife;
    }

    void Update()
    {
        switch (type)
        {
            case EnemyType.Normal:
                MoveForward();
                break;
            case EnemyType.NormalShoot:
                MoveForward();
                Shoot();
                break;
            case EnemyType.Kamikase:
                if (targetInRange)
                {
                    RotateToTarget();
                    MoveForward(2);
                }
                else
                {
                    MoveForward();
                    SearchTarget();
                }
                break;
            case EnemyType.Sniper:
                if (targetInRange)
                {
                    RotateToTarget();
                    Shoot();
                }
                else
                {
                    MoveForward();
                    SearchTarget();
                }
                break;
        }

    }
    private void OnCollisionEnter2D(Collision2D obj)
    {
        if (obj.gameObject.CompareTag("Floor"))
        { Destroy(gameObject); }
    }
    void MoveForward()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    void MoveForward(float m)
    {
        transform.Translate(Vector2.up * speed * m * Time.deltaTime);
    }

    void RotateToTarget()
    {
        Vector2 dir = target.position - transform.position;
        float angleZ = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + 0;
        transform.rotation = Quaternion.Euler(0, 0, -angleZ);
    }

    void SearchTarget()
    {
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= range)
        {
            targetInRange = true;
        }
        else
        {
            targetInRange = false;
        }
    }

    void Shoot()
    {
        if (timer < timeBtwShoot)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0;
            Bullet b = Instantiate(prefab, firePoint.position, transform.rotation);
            b.damage = damage;
            b.speed = bulletSpeed;
        }
    }

    public void TakeDamage(float dmg)
    {
        life -= dmg;
        lifebar.fillAmount = life / maxlife;
        if (life <= 0)
        {
            

            Instantiate(particle, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player e = collision.gameObject.GetComponent<Player>(); // obtener ese enemigo su componente
           
            Instantiate(particle, transform.position, Quaternion.identity);


            e.TakeDamage(damage); // ejecutar el metodo danio al enemy
            Destroy(gameObject); //destroy bullet
        }

    }

}
public enum EnemyType
{
    Normal,
    NormalShoot,
    Kamikase,
    Sniper
}