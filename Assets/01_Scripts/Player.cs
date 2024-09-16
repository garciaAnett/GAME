using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Player : MonoBehaviour
{
    public float damage = 1f;
    public float speed = 2f;
    public float timeBtwShoot = 1.5f;
    public int ammo = 10;
    int currentAmmo;
    float life = 60;
    public float maxLife = 60;
    float timer = 0;
    bool canShoot = true;
    public Rigidbody2D rb;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public Bullet prefab;
    public float bulletSpeed = 5f;
    public ParticleSystem particle;
    
    public Image lifebar;


    void Start()
    {
        Debug.Log("Inició el juego");
        currentAmmo = ammo;
        life=maxLife;

        lifebar.fillAmount = life/maxLife; 
    }

    void Update()
    {
        
        Movement();
        Reload();
        CheckIfCanShoot();
        Shoot();
       
    }

    void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        rb.velocity = new Vector2(x, y) * speed;
    }

    void Shoot()
    {
        if(Input.GetKeyDown(KeyCode.Space) && canShoot && currentAmmo > 0)
        {
            Bullet b = Instantiate(prefab, firePoint.position, transform.rotation);
            b.damage= damage;
            b.speed = bulletSpeed;
         //   currentAmmo--;
            canShoot = false;
        }
    }

    void Reload()
    {
        if(currentAmmo == 0 && Input.GetKeyDown(KeyCode.R))
        {
            currentAmmo = ammo;
        }
    }

    void CheckIfCanShoot()
    {
        if (timer < timeBtwShoot)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0;
            canShoot = true;
        }
    }

    public void TakeDamage(float dmg)
    {
            life -= dmg;
          
            lifebar.fillAmount = life / maxLife;
            if (life <= 0)
            {
                Destroy(gameObject);
                Instantiate(particle, transform.position, Quaternion.identity);
              
            }
        
    }
  

}