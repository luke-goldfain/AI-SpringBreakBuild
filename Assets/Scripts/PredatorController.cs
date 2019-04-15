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
    private PredatorState predPrevState;

    private bool oneTimeStateActionsExecuted;

    public Vector3 NextRoamPos;

    private Vector3 reroutePos;
    private bool isRerouting;

    private Quaternion targetRotation;

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
        predPrevState = PredatorState.alert;

        worldLowerX = -50f;
        worldUpperX = 50f;
        worldLowerZ = -50f;
        worldUpperZ = 50f;

        oneTimeStateActionsExecuted = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // hack to force y position
        transform.position = new Vector3(transform.position.x, 2.1f, transform.position.z);
        // hack to force x, z rotation
        transform.rotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);

        UpdateCastVisionRays();

        if (predPrevState != predState) oneTimeStateActionsExecuted = false;
        else oneTimeStateActionsExecuted = true;

        if (!oneTimeStateActionsExecuted)
        {
            ExecuteOneTimeStateActions();
        }

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

    private void ExecuteOneTimeStateActions()
    {
        print(this + " state from " + predPrevState + " to " + predState);

        switch (predState)
        {
            case PredatorState.roam:
                {
                    NextRoamPos = new Vector3(transform.position.x + UnityEngine.Random.Range(-50f, 50f), transform.position.y, transform.position.z + UnityEngine.Random.Range(-50f, 50f));

                    targetRotation = Quaternion.LookRotation(NextRoamPos - rb.transform.position);
                    break;
                }
            case PredatorState.hunt:
                {
                    targetRotation = Quaternion.LookRotation(preyDir);

                    huntTimer = 0f;
                    break;
                }
            case PredatorState.alert:
                {
                    targetRotation = Quaternion.LookRotation(preyDir);

                    alertTimer = 0f;
                    break;
                }
        }

        predPrevState = predState;
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
                //print(this + " state set to alert");
            }
        }
    }

    private void UpdateStateAlert()
    {
        alertTimer += Time.deltaTime;

        targetRotation = Quaternion.LookRotation(preyDir);
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
        huntTimer += Time.deltaTime;

        if (isRerouting &&
            transform.position.x - reroutePos.x < 1 &&
            transform.position.z - reroutePos.z < 1)
        {
            targetRotation = Quaternion.LookRotation(preyDir);

            isRerouting = false;
        }

        rb.transform.Translate(Vector3.forward * speed * 2f);

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

            targetRotation = Quaternion.LookRotation(NextRoamPos - rb.transform.position);
        }

        // Account for the agent rerouting and having reached their reroute point
        if (isRerouting &&
            transform.position.x - reroutePos.x < 1 &&
            transform.position.z - reroutePos.z < 1)
        {
            targetRotation = Quaternion.LookRotation(NextRoamPos - rb.transform.position);

            isRerouting = false;
        }

        rb.transform.Translate(Vector3.forward * speed);

        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
    }
    
    private void UpdateCastVisionRays()
    {
        Vector3 localForward = transform.rotation * Vector3.forward;

        for (float i = 0; i < raycastNum; i++)
        {
            Vector3 offset = new Vector3(i/25, 0f, i/25);

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
                    //print(this + " state set to hunt");

                    // debug
                    Debug.DrawRay(transform.position, (localForward + offset) * 30f, Color.green, 0.5f);
                    Debug.DrawRay(transform.position, (localForward - offset) * 30f, Color.green, 0.5f);
                }

                if (hit.collider.tag == "Obstacle")
                {
                    if (predState == PredatorState.roam)
                    {
                        if (AreSameDirection(NextRoamPos - this.transform.position, hit.transform.position - this.transform.position) &&
                            Vector3.Distance(this.transform.position, hit.transform.position) < Vector3.Distance(this.transform.position, NextRoamPos))
                        {
                            targetRotation = Quaternion.LookRotation(RerouteAroundObstacle());
                        }
                    }

                    if (predState == PredatorState.hunt)
                    {
                        if (AreSameDirection(preyDir, hit.transform.position - this.transform.position) &&
                            Vector3.Distance(this.transform.position, hit.transform.position) < Vector3.Distance(this.transform.position, preyDir))
                        {
                            targetRotation = Quaternion.LookRotation(RerouteAroundObstacle());
                        }
                    }
                }
            }
            else if (predState == PredatorState.hunt && huntTimer >= huntMinTime) // If the predator stops seeing prey, they go on alert
            {
                predState = PredatorState.alert;
                //print(this + " state set to alert");
            }
        }
    }

    private Vector3 RerouteAroundObstacle()
    {
        int layermask = ~(1 << 9);

        Transform[] rerouteTransforms = hit.collider.gameObject.GetComponentsInChildren<Transform>();

        for (int j = 0; j < rerouteTransforms.Length; j++)
        {
            if (rerouteTransforms[j].gameObject.tag != "Obstacle")
            {
                reroutePos = rerouteTransforms[j].transform.position;

                if (Physics.Raycast(reroutePos, this.NextRoamPos - reroutePos, out hit, Vector3.Distance(reroutePos, this.NextRoamPos), layermask) ||
                    Physics.Raycast(reroutePos, this.transform.position - reroutePos, out hit, Vector3.Distance(reroutePos, this.transform.position), layermask))
                {
                    if (hit.transform.tag == "Obstacle") // todo: This never executes, even though some of the reroute points should have the obstacle in the way.
                    {
                        j++;

                        // debug
                        Debug.DrawRay(reroutePos, (this.transform.position - reroutePos), Color.red, 0.5f);
                    }
                    else
                    {
                        // debug
                        Debug.DrawRay(reroutePos, (this.transform.position - reroutePos), Color.green, 1f);
                        print(this + " redirecting to " + rerouteTransforms[j]);

                        isRerouting = true;

                        return reroutePos - this.transform.position;
                    }
                }
            }
        }

        return NextRoamPos;
    }

    private bool AreSameDirection(Vector3 dir1, Vector3 dir2)
    {
        dir1.Normalize();
        dir2.Normalize();

        if (dir1.x - dir2.x < 0.2f && dir1.z - dir2.z < 0.2f)
        {
            return true;
        }

        return false;
    }
}
