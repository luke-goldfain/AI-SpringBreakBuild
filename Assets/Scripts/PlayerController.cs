using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    Rigidbody rb;
    float dirX, dirZ;
    float speed;
    bool isAttacking;
    float attackCooldown = 0.5f;
    float attackTimer;

    public GameObject soundSphere;
    private List<GameObject> soundSpheres;
    private float soundSphereCD = 1f;
    private float soundSphereTime;
    private float soundScale;

    public GameObject Projectile;
    public GameObject ProjectileSpawn;


	// Use this for initialization
	void Start ()
    {
        rb = this.GetComponent<Rigidbody>();

        soundSpheres = new List<GameObject>();

        soundScale = 1f;

        speed = 0.2f;

        attackTimer = attackCooldown;

        soundSphereTime = soundSphereCD;
	}
	
	// Update is called once per frame
	void Update ()
    {
        GetInput();

        MovePlayer();
        
        attackTimer += Time.deltaTime;

        soundSphereTime += Time.deltaTime;


        if (isAttacking)
        {
            AttackOnCooldown();
        }
	}

    private void AttackOnCooldown()
    {
        if (attackTimer >= attackCooldown)
        {
            Instantiate(Projectile, ProjectileSpawn.transform);

            SpawnSoundSphere(20f);

            attackTimer = 0f;
        }
    }

    private void MovePlayer()
    {
        if (dirX != 0 || dirZ != 0)
        {
            rb.transform.localRotation = Quaternion.LookRotation(new Vector3(dirX, 0, dirZ));

            SpawnSoundSphereOnCooldown();

            speed = 0.2f;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed = 0.5f;
            }
        }
        else
        {
            DestroyAllSounds();
        }

        rb.transform.Translate(Vector3.forward * speed);
    }

    private void GetInput()
    {
        dirX = 0;
        dirZ = 0;

        speed = 0f;

        // Right/left
        if (Input.GetKey(KeyCode.RightArrow))
        {
            dirX = 1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            dirX = -1;
        }

        // Up/down
        if (Input.GetKey(KeyCode.UpArrow))
        {
            dirZ = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            dirZ = -1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            isAttacking = true;
        }
        else
        {
            isAttacking = false;
        }
    }

    private void SpawnSoundSphereOnCooldown()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            soundSphereTime += (Time.deltaTime * 2);

            soundScale = 30f;
        }
        else
        {
            soundScale = 20f;
        }

        // Spawn a single sound sphere on the player's location at the cooldown specified.
        if (soundSphereTime >= soundSphereCD)
        {
            SpawnSoundSphere(soundScale);
        }

        // Destroy all sound spheres (there should only ever be 1)
        // when the timer hits 1/5 of the sphere spawn time.
        if (soundSphereTime >= soundSphereCD / 5)
        {
            DestroyAllSounds();
        }
    }

    private void SpawnSoundSphere(float scale)
    {
        soundSpheres.Add(Instantiate(soundSphere, this.transform));
        soundSphereTime = 0f;

        foreach (GameObject sp in soundSpheres)
        {
            sp.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    // Destroy all sound spheres attached to this game object.
    private void DestroyAllSounds()
    {
        foreach (GameObject sp in soundSpheres)
        {
            Destroy(sp);
        }

        soundSpheres.Clear();
    }
}
