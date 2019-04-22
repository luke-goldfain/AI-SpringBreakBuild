using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyFoodGenerator : MonoBehaviour
{
    float worldLowerX, worldUpperX,
          worldLowerZ, worldUpperZ;

    readonly float foodCooldown = 10f;
    float foodTimer;

    public int startingFoods = 8;

    public GameObject PreyFood;

    // Start is called before the first frame update
    void Start()
    {
        worldLowerX = -50f;
        worldUpperX = 50f;
        worldLowerZ = -50f;
        worldUpperZ = 50f;

        // Instantiate a given amount of prey food objects at the start
        for (int i = 0; i < startingFoods; i++)
        {
            Instantiate(PreyFood, new Vector3(Random.Range(worldLowerX, worldUpperX), 0f, Random.Range(worldLowerZ, worldUpperZ)), Quaternion.identity);
        }

        foodTimer = foodCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        foodTimer += Time.deltaTime;

        // Instantiate a new prey food on a cooldown
        if (foodTimer >= foodCooldown)
        {
            Instantiate(PreyFood, new Vector3(Random.Range(worldLowerX, worldUpperX), 0f, Random.Range(worldLowerZ, worldUpperZ)), Quaternion.identity);

            foodTimer = 0f;
        }
    }
}
