using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCars : MonoBehaviour
{
    public GameObject car1, car2, car3;

    public float spawnRate = 8.0f;
    float nextSpawn = 0f;

    int whatToSpawn;

    void Update()
    {
        if (Time.time > nextSpawn)
        {
            whatToSpawn = Random.Range(1, 4);
            switch (whatToSpawn)
            {
                case 1:
                    Instantiate(car1, transform.position, Quaternion.identity);
                    break;
                case 2:
                    Instantiate(car2, transform.position, Quaternion.identity);
                    break;
                case 3:
                    Instantiate(car3, transform.position, Quaternion.identity);
                    break;

            }
            nextSpawn = Time.time + spawnRate;
            
        }
    }
}
