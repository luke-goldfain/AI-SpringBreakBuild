using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindDirection : MonoBehaviour {

    public GameObject PredatorScentCylinder;
    public GameObject PreyScentCylinder;

    public Vector3 WindDir;

    private List<GameObject> predators;
    private List<GameObject> preys;

    private List<GameObject> scentCylinders;

    private readonly float scentCooldown = 1f;
    private float scentTimer;

    float worldLowerX, worldUpperX,
          worldLowerZ, worldUpperZ;

    // Use this for initialization
    void Start ()
    {
        predators = new List<GameObject>();
        preys = new List<GameObject>();
        scentCylinders = new List<GameObject>();

		foreach (GameObject c in GameObject.FindGameObjectsWithTag("Player"))
        {
            predators.Add(c);
        }
        foreach (GameObject c in GameObject.FindGameObjectsWithTag("Predator"))
        {
            predators.Add(c);
        }
        foreach (GameObject c in GameObject.FindGameObjectsWithTag("Prey"))
        {
            preys.Add(c);
        }

        worldLowerX = -50f;
        worldUpperX = 50f;
        worldLowerZ = -50f;
        worldUpperZ = 50f;

        scentTimer = 0f;

        WindDir = Vector3.forward;
	}
	
	// Update is called once per frame
	void Update ()
    {
        UpdateSpawnScentCylindersOnCooldown();

        UpdateChangeWindDirection();

        UpdateBlowScentCylinders();

        UpdateDeleteOutOfBoundsCylinders();
	}

    private void UpdateDeleteOutOfBoundsCylinders()
    {
        foreach (GameObject c in scentCylinders)
        {
            if (c.transform.position.x < worldLowerX - c.transform.localScale.x ||
                c.transform.position.x > worldUpperX + c.transform.localScale.x ||
                c.transform.position.z < worldLowerZ - c.transform.localScale.z ||
                c.transform.position.z > worldUpperZ + c.transform.localScale.z)
            {
                c.SetActive(false);
            }
        }
    }

    private void UpdateChangeWindDirection()
    {
        // Allow the x and z values of WindDir to change randomly
        WindDir.x += UnityEngine.Random.Range(-0.05f, 0.05f);
        WindDir.z += UnityEngine.Random.Range(-0.05f, 0.05f);

        // Keep the values between -1 and 1
        Mathf.Clamp(WindDir.x, -1f, 1f);
        Mathf.Clamp(WindDir.z, -1f, 1f);
    }

    private void UpdateSpawnScentCylindersOnCooldown()
    {
        scentTimer += Time.deltaTime;

        if (scentTimer >= scentCooldown)
        {
            foreach (GameObject p in predators)
            {
                SpawnPredatorScentCylinder(p);
            }
            foreach (GameObject p in preys)
            {
                SpawnPreyScentCylinder(p);
            }

            scentTimer = 0f;
        }
    }


    private void UpdateBlowScentCylinders()
    {
        foreach(GameObject c in scentCylinders)
        {
            c.transform.parent = null;

            c.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

            c.transform.Translate(WindDir * 0.5f);
        }
    }

    private void SpawnPredatorScentCylinder(GameObject predator)
    {
        if (predator.activeInHierarchy)
        {
            GameObject sc = Instantiate(PredatorScentCylinder, predator.transform);

            sc.GetComponent<ScentController>().ScentSource = predator;

            scentCylinders.Add(sc);
        }
    }

    private void SpawnPreyScentCylinder(GameObject prey)
    {
        if (prey.activeInHierarchy)
        {
            GameObject sc = Instantiate(PreyScentCylinder, prey.transform);

            sc.GetComponent<ScentController>().ScentSource = prey;

            scentCylinders.Add(sc);
        }
    }
}
