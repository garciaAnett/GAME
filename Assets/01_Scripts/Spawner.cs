using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public float timeBtwSpawn = 1.5f;

    float timer = 0;
    public Transform leftPoint;
    public Transform rightPoint;
    public List<GameObject> enemyPrefabs;

    void Start()
    {
        
    }

    void Update()
    {
        SpawnEnemy();
        
    }

    void SpawnEnemy()
    {
        if (timer < timeBtwSpawn)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0;
            float x = Random.Range(leftPoint.position.x, rightPoint.position.x);
            int enemy = Random.Range(0, enemyPrefabs.Count);
       
            Vector3 newPos = new Vector3(x, transform.position.y, 0);
            Instantiate(enemyPrefabs[enemy], newPos, Quaternion.Euler(0, 0, 180));
          
        }
    }
  
}
