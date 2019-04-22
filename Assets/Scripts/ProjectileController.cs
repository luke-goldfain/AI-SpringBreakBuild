using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    float worldLowerX, worldUpperX,
          worldLowerZ, worldUpperZ;

    float speed = 3f;

    // Start is called before the first frame update
    void Start()
    {
        worldLowerX = -50f;
        worldUpperX = 50f;
        worldLowerZ = -50f;
        worldUpperZ = 50f;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(Vector3.forward * speed);

        if (this.transform.position.x >= worldUpperX ||
            this.transform.position.x <= worldLowerX ||
            this.transform.position.z >= worldUpperZ ||
            this.transform.position.z <= worldLowerZ)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Prey" ||
            collision.gameObject.tag == "Predator")
        {
            Destroy(collision.gameObject);
        }

        Destroy(this.gameObject);
    }
}
