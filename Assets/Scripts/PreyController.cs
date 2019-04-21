using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyController : MonoBehaviour {

    enum PreyState
    {
        roam,
        flee,
        flock,
        alert
    }
    private PreyState pState;
    private PreyState pPrevState;

    private bool oneTimeStateActionsExecuted;

    public Vector3 NextRoamPos;

    private Vector3 reroutePos;
    private bool isRerouting;

    private Quaternion targetRotation;

    private GameObject flockTarget;

    private Vector3 enemyDir;
    readonly float fleeTime = 3f;
    float fleeTimer;
    readonly float alertTime = 3f;
    float alertTimer;

    float worldLowerX, worldUpperX,
          worldLowerZ, worldUpperZ;
    
    private int raycastNum = 8;
    RaycastHit hit;

    private float speed = 0.16f;
    private float rotSpeed = 10f;

    public float maxFlockDist = 20f;
    //private float minFlockDist = 5f;

    Rigidbody rb;

	// Use this for initialization
	void Start ()
    {
        rb = this.GetComponent<Rigidbody>();

        pState = PreyState.roam;
        pPrevState = PreyState.alert;

        worldLowerX = -50f;
        worldUpperX = 50f;
        worldLowerZ = -50f;
        worldUpperZ = 50f;

        oneTimeStateActionsExecuted = false;
        isRerouting = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        // hack to force y position
        transform.position = new Vector3(transform.position.x, 1.2f, transform.position.z);
        // hack to force x, z rotation
        transform.rotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);

        UpdateCastVisionRays();

        if (pPrevState != pState) oneTimeStateActionsExecuted = false;
        else oneTimeStateActionsExecuted = true;

        if (!oneTimeStateActionsExecuted)
        {
            ExecuteOneTimeStateActions();
        }

        switch (pState)
        {
            case PreyState.roam:
                UpdateStateRoam();
                break;
            case PreyState.flee:
                UpdateStateFlee();
                break;
            case PreyState.flock:
                UpdateStateFlock();
                break;
            case PreyState.alert:
                UpdateStateAlert();
                break;
        }
    }

    private void ExecuteOneTimeStateActions()
    {
        print(this + " state from " + pPrevState + " to " + pState);

        switch (pState)
        {
            case PreyState.roam:
                {
                    NextRoamPos = new Vector3(transform.position.x + UnityEngine.Random.Range(-50f, 50f), transform.position.y, transform.position.z + UnityEngine.Random.Range(-50f, 50f));

                    targetRotation = Quaternion.LookRotation(NextRoamPos - rb.transform.position);
                    break;
                }
            case PreyState.flee:
                {
                    targetRotation = Quaternion.LookRotation(-enemyDir);

                    fleeTimer = 0f;
                    break;
                }
            case PreyState.flock:
                {

                    break;
                }
            case PreyState.alert:
                {
                    alertTimer = 0f;
                    targetRotation = Quaternion.LookRotation(enemyDir);
                    break;
                }
        }

        pPrevState = pState;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Sound")
        {
            enemyDir = other.transform.position - this.transform.position;

            if (pState != PreyState.flee)
            {
                pState = PreyState.alert;
                //print(this + " state set to alert");
            }
        }

        if (other.tag == "PredatorScent")
        {
            ScentController sCont = other.GetComponent<ScentController>();

            enemyDir = sCont.OrigPosition - this.transform.position;

            if (pState != PreyState.flee)
            {
                pState = PreyState.alert;
                //print(this + " state set to alert");
            }
        }
    }

    private void UpdateStateAlert()
    {
        alertTimer += Time.deltaTime;

        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, targetRotation, Time.deltaTime * rotSpeed);

        rb.transform.Translate(Vector3.forward * speed * 0.5f);

        if (alertTimer >= alertTime)
        {
            pState = PreyState.roam;
            NextRoamPos = this.transform.position;
            //print(this + "state set to roam");
        }
    }

    private void UpdateStateFlock()
    {
        // Break out of flock state if distance from flock target is higher than maxFlockDistance
        // Keep in mind that the agent can always switch to flee from this state as well.
        if (transform.position.x - flockTarget.transform.position.x > maxFlockDist &&
            transform.position.z - flockTarget.transform.position.x > maxFlockDist)
        {
            pState = PreyState.roam;
            //print(this + " state set to roam");
        }

        Vector3 tempRoamPos = NextRoamPos;

        // Get the flock target's next roam position
        if (flockTarget) { tempRoamPos = flockTarget.GetComponent<PreyController>().NextRoamPos; }

        // Set this agent's roam position randomly within a certain range of the flock target's roam position,
        // whether this agent's roam position is too far from it or it has reached its current roam position already.
        if (tempRoamPos.x - this.NextRoamPos.x > maxFlockDist ||
            tempRoamPos.z - this.NextRoamPos.z > maxFlockDist ||
            (transform.position.x - NextRoamPos.x < 10 && transform.position.z - NextRoamPos.z < 10))
        {
            tempRoamPos += new Vector3(UnityEngine.Random.Range(-maxFlockDist, maxFlockDist), 
                                       this.transform.position.y, 
                                       UnityEngine.Random.Range(-maxFlockDist, maxFlockDist));

            this.NextRoamPos = tempRoamPos;

            targetRotation = Quaternion.LookRotation(NextRoamPos - rb.transform.position);
        }

        // Finally, clamp the roam position to within the world boundaries
        if (NextRoamPos.x < worldLowerX || NextRoamPos.x > worldUpperX)
        {
            NextRoamPos = new Vector3(Mathf.Clamp(NextRoamPos.x, worldLowerX, worldUpperX), this.transform.position.y, NextRoamPos.z);
            targetRotation = Quaternion.LookRotation(NextRoamPos - rb.transform.position);
        }
        if (NextRoamPos.z < worldLowerZ || NextRoamPos.z > worldUpperZ)
        {
            NextRoamPos = new Vector3(NextRoamPos.x, this.transform.position.y, Mathf.Clamp(NextRoamPos.z, worldLowerZ, worldUpperZ));
            targetRotation = Quaternion.LookRotation(NextRoamPos - rb.transform.position);
        }

        // Account for rerouting as well
        if (isRerouting &&
            transform.position.x - reroutePos.x < 1 &&
            transform.position.z - reroutePos.z < 1)
        {
            targetRotation = Quaternion.LookRotation(NextRoamPos - rb.transform.position);

            isRerouting = false;
        }

        // Move a little bit faster than in roam state
        rb.transform.Translate(Vector3.forward * speed * 1.25f);

        // Same movement pattern as in roam state
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
    }

    private void UpdateStateFlee()
    {
        fleeTimer += Time.deltaTime;

        if (fleeTimer < fleeTime) // Flee opposite direction
        {
            rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, targetRotation, Time.deltaTime * rotSpeed);

            rb.transform.Translate(Vector3.forward * speed * 2);
        }
        else // switch to alert state
        {
            pState = PreyState.alert;
            ////print(this + "State set to alert");
        }

        if (isRerouting &&
            transform.position.x - reroutePos.x < 1 &&
            transform.position.z - reroutePos.z < 1)
        {
            targetRotation = Quaternion.LookRotation(-enemyDir);

            isRerouting = false;
        }
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

                if (hit.collider.tag == "Prey" && pState != PreyState.flee && pState != PreyState.flock)
                {
                    NextRoamPos = hit.transform.position;
                    flockTarget = hit.collider.gameObject;

                    pState = PreyState.flock;
                    //print(this + "state set to flock");

                    // debug
                    Debug.DrawRay(transform.position, (localForward + offset) * 30f, Color.blue, 0.5f);
                    Debug.DrawRay(transform.position, (localForward - offset) * 30f, Color.blue, 0.5f);
                }

                if (hit.collider.tag == "Player" || hit.collider.tag == "Predator")
                {
                    pState = PreyState.flee;
                    enemyDir = hit.transform.position - transform.position;
                    //print(this + "state set to flee");

                    // debug
                    Debug.DrawRay(transform.position, (localForward + offset) * 30f, Color.red, 0.5f);
                    Debug.DrawRay(transform.position, (localForward - offset) * 30f, Color.red, 0.5f);
                }

                // Redirect if there is an obstacle in the way, with slight changes to calculation based on state
                if (hit.collider.tag == "Obstacle")
                {
                    if (pState == PreyState.roam || pState == PreyState.flock)
                    {
                        if (AreSameDirection(NextRoamPos - this.transform.position, hit.transform.position - this.transform.position) &&
                            Vector3.Distance(this.transform.position, hit.transform.position) < Vector3.Distance(this.transform.position, NextRoamPos))
                        {
                            targetRotation = Quaternion.LookRotation(RerouteAroundObstacle());
                        }
                    }

                    if (pState == PreyState.flee)
                    {
                        if (AreSameDirection(-enemyDir, hit.transform.position - this.transform.position) &&
                            Vector3.Distance(this.transform.position, hit.transform.position) < Vector3.Distance(this.transform.position, -enemyDir))
                        {
                            targetRotation = Quaternion.LookRotation(RerouteAroundObstacle());
                        }
                    }
                }
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
                    if (hit.collider.tag == "Obstacle") // todo: This never executes, even though some of the reroute points should have the obstacle in the way.
                    {
                        j++;

                        // debug
                        Debug.DrawRay(reroutePos, (this.transform.position - reroutePos), Color.red, 0.5f);
                    }
                    else
                    {
                        // debug
                        Debug.DrawRay(reroutePos, (this.transform.position - reroutePos), Color.green, 1f);
                        //print(this + " redirecting to " + rerouteTransforms[j]);

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
