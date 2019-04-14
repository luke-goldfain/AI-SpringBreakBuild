using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorController : MonoBehaviour {


    enum PredatorState
    {
        roam,
        hunt,
        alert
    }
    private PredatorState predState;

    public Vector3 NextRoamPos;

    private Vector3 preyDir;

    readonly float alertTime = 3f;
    float alertTimer;

    readonly float huntMinTime = 3f;
    float huntTimer;

    float worldLowerX, worldUpperX,
          worldLowerZ, worldUpperZ;

    private int raycastNum = 8;
    RaycastHit hit;

    private float speed = 0.2f;
    private float rotSpeed = 10f;

    Rigidbody rb;

    // Use this for initialization
    void Start ()
    {
        rb = this.GetComponent<Rigidbody>();

        predState = PredatorState.roam;

        worldLowerX = -50f;
        worldUpperX = 50f;
        worldLowerZ = -50f;
        worldUpperZ = 50f;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // hack to force y position
        transform.position = new Vector3(transform.position.x, 2.1f, transform.position.z);
        // hack to force x, z rotation
        transform.rotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);

        UpdateCastVisionRays();

        // todo: hearing, smell

        switch (predState)
        {
            case PredatorState.roam:
                UpdateStateRoam();
                break;
            case PredatorState.hunt:
                UpdateStateHunt();
                break;
            case PredatorState.alert:
                UpdateStateAlert();
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag == "Prey" || collision.gameObject.tag == "Player")
        {
            if (predState == PredatorState.hunt)
            {
                collision.gameObject.SetActive(false);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Sound")
        {
            preyDir = other.transform.position - this.transform.position;

            if (predState != PredatorState.hunt)
            {
                predState = PredatorState.alert;
                alertTimer = 0f;
                //print(this + " state set to alert");
            }
        }

        if (other.tag == "PreyScent")
        {
            ScentController sCont = other.GetComponent<ScentController>();

            preyDir = sCont.OrigPosition - this.transform.position;

            if (predState != PredatorState.hunt)
            {
                predState = PredatorState.alert;
                alertTimer = 0f;
                //print(this + " state set to alert");
            }
        }
    }

    private void UpdateStateAlert()
    {
        alertTimer += Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(preyDir);
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, targetRotation, Time.deltaTime * rotSpeed);

        rb.transform.Translate(Vector3.forward * speed * 0.5f);

        if (alertTimer >= alertTime)
        {
            predState = PredatorState.roam;
            NextRoamPos = this.transform.position;
            //print(this + " state set to roam");
        }
    }

    private void UpdateStateHunt()
    {
        huntTimer++;

        rb.transform.Translate(Vector3.forward * speed * 2f);

        Quaternion targetRotation = Quaternion.LookRotation(preyDir);
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
    }

    private void UpdateStateRoam()
    {
        if (transform.position.x - NextRoamPos.x < 5 &&
            transform.position.z - NextRoamPos.z < 5)
        {
            NextRoamPos = new Vector3(transform.position.x + UnityEngine.Random.Range(-50f, 50f), transform.position.y, transform.position.z + UnityEngine.Random.Range(-50f, 50f));

            // Correct for values outside of intended worldspace. Currently this will result in
            // the assignment of completely random values within the range.
            if (NextRoamPos.x > worldUpperX || NextRoamPos.x < worldLowerX)
            {
                NextRoamPos.x = UnityEngine.Random.Range(worldLowerX, worldUpperX);
            }
            if (NextRoamPos.z > worldUpperZ || NextRoamPos.z < worldLowerZ)
            {
                NextRoamPos.z = UnityEngine.Random.Range(worldLowerZ, worldUpperZ);
            }
        }

        rb.transform.Translate(Vector3.forward * speed);

        Quaternion targetRotation = Quaternion.LookRotation(NextRoamPos - rb.transform.position);
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
    }
    
    private void UpdateCastVisionRays()
    {
        Vector3 localForward = transform.rotation * Vector3.forward;

        for (float i = 0; i < raycastNum; i++)
        {
            Vector3 offset = new Vector3(0f, 0f, i / 15);

            Debug.DrawRay(transform.position, (localForward + offset) * 30f, Color.yellow);
            Debug.DrawRay(transform.position, (localForward - offset) * 30f, Color.yellow);

            int layermask = ~(1 << 9);

            // Cast a ray forward and on a slightly larger angle for each iteration. This creates a cone of rays.
            if (Physics.Raycast(transform.position, localForward + offset, out hit, 30f, layermask) ||
                Physics.Raycast(transform.position, localForward - offset, out hit, 30f, layermask))
            {

                if (hit.collider.tag == "Prey")
                {
                    preyDir = hit.transform.position - this.transform.position;

                    predState = PredatorState.hunt;
                    huntTimer = 0f;
                    //print(this + " state set to hunt");

                    // debug
                    Debug.DrawRay(transform.position, (localForward + offset) * 30f, Color.green, 0.5f);
                    Debug.DrawRay(transform.position, (localForward - offset) * 30f, Color.green, 0.5f);
                }

                if (hit.collider.tag == "Player")
                {
                    // subject to change, but for now the predator will also hunt the player
                    preyDir = hit.transform.position - this.transform.position;

                    predState = PredatorState.hunt;
                    huntTimer = 0f;
                    //print(this + " state set to hunt");

                    // debug
                    Debug.DrawRay(transform.position, (localForward + offset) * 30f, Color.green, 0.5f);
                    Debug.DrawRay(transform.position, (localForward - offset) * 30f, Color.green, 0.5f);
                }
            }
            else if (predState == PredatorState.hunt && huntTimer >= huntMinTime) // If the predator stops seeing prey, they go on alert
            {
                predState = PredatorState.alert;
                alertTimer = 0f;
                //print(this + " state set to alert");
            }
        }
    }
}
