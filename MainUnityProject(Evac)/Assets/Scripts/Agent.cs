using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    public float radius;
    public float mass;
    public float perceptionRadius;
    //public float destination;

    private List<Vector3> path;
    private NavMeshAgent nma;
    private Rigidbody rb;

    private HashSet<GameObject> perceivedNeighbors = new HashSet<GameObject>();
    private HashSet<GameObject> touchedWalls = new HashSet<GameObject>();

    void Start()
    {
        path = new List<Vector3>();
        nma = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        gameObject.transform.localScale = new Vector3(2 * radius, 1, 2 * radius);
        //gameObject.transform.localScale = new Vector3(1,1,1);
        nma.radius = radius;
        rb.mass = mass;
        GetComponent<SphereCollider>().radius = perceptionRadius / 2;
    }

    private void Update()
    {
        if (path.Count > 1 && Vector3.Distance(transform.position, path[0]) < 0.3f)
        {
            path.RemoveAt(0);
        } else if (path.Count == 1 && Vector3.Distance(transform.position, path[0]) < 1.3f)
        {
            path.RemoveAt(0);

            if (path.Count == 0)
            {
                gameObject.SetActive(false);
                AgentManager.RemoveAgent(gameObject);
            }
        }

        #region Visualization

        if (false)
        {
            if (path.Count > 0)
            {
                Debug.DrawLine(transform.position, path[0], Color.green);
            }
            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(path[i], path[i + 1], Color.yellow);
            }
        }

        if (false)
        {
            foreach (var neighbor in perceivedNeighbors)
            {
                Debug.DrawLine(transform.position, neighbor.transform.position, Color.yellow);
            }
        }

        if (false)
        {
            foreach (var neighbor in touchedWalls)
            {
                Debug.DrawLine(transform.position, neighbor.transform.position, Color.yellow);
            }
        }

        #endregion
    }

    #region Public Functions

    public void ComputePath(Vector3 destination)
    {
        nma.enabled = true;
        var nmPath = new NavMeshPath();
        nma.CalculatePath(destination, nmPath);
        path = nmPath.corners.Skip(1).ToList();
        
        //path = new List<Vector3>() { destination };
        //nma.SetDestination(destination);
        
        nma.enabled = false;
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    #endregion

    #region Incomplete Functions

    private Vector3 ComputeForce()
    {
        if (true) //Goal Force Mode
        {
            var force = CalculateGoalForce(maxSpeed: 1) + CalculateWallForce(); //+ CalculateAgentForce();// + CalculateWallForce();

            if (force != Vector3.zero)
            {
                return force.normalized * Mathf.Min(force.magnitude, Parameters.maxSpeed);
            }
            else
            {
                return Vector3.zero;
            }
        }

        if (false) //Pursue and Evade Mode
        {
            if (this.tag == "Chaser")
            {
                var goalDirection = (FindClosestTag("Agent").transform.position - transform.position).normalized;
                Debug.Log(FindClosestTag("Agent").transform.position.x);
                var prefForce = (((goalDirection * Mathf.Min(goalDirection.magnitude, 1)) - rb.velocity) / Parameters.T);
                var force = prefForce + CalculateWallForce();

                if (force != Vector3.zero)
                {
                    return force.normalized * Mathf.Min(force.magnitude, Parameters.maxSpeed);
                }
                else
                {
                    return Vector3.zero;
                }
            }
            else
            {
                var goalDirection = (transform.position - FindClosestTag("Chaser").transform.position).normalized;
                var prefForce = (((goalDirection * Mathf.Min(goalDirection.magnitude, 1)) - rb.velocity) / Parameters.T);
                var force = prefForce + CalculateAgentForce() + CalculateWallForce();

                if (force != Vector3.zero)
                {
                    return force.normalized * Mathf.Min(force.magnitude, Parameters.maxSpeed);
                }
                else
                {
                    return Vector3.zero;
                }
            }

        }

        if (false) //Wall Follower Mode
        {
            Vector3 movingForward = new Vector3(-1,0,1);
            var force = CalculateGoalForce(maxSpeed:1) - CalculateWallForce();

            if (force != Vector3.zero)
            {
                return force.normalized * Mathf.Min(force.magnitude, Parameters.maxSpeed);
            }
            else
            {
                return Vector3.zero;
            }
        }

        if (false) //Leader Following Mode
        {

            if (this.tag == "Leader")
            {
                var force = CalculateGoalForce(maxSpeed: 1) + CalculateAgentForce()+ CalculateWallForce();

                if (force != Vector3.zero)
                {
                    return force.normalized * Mathf.Min(force.magnitude, Parameters.maxSpeed);
                }
                else
                {
                    return Vector3.zero;
                }
            }
            else
            {
                var goalDirection = (FindClosestTag("Leader").transform.position - transform.position).normalized;
                var prefForce = (((goalDirection * Mathf.Min(goalDirection.magnitude, 1)) - rb.velocity) / Parameters.T);
                var force =  prefForce + CalculateAgentForce() + CalculateWallForce();

                if (force != Vector3.zero)
                {
                    return force.normalized * Mathf.Min(force.magnitude, Parameters.maxSpeed);
                }
                else
                {
                    return Vector3.zero;
                }
            }
        }

        if (false) //Crowd Following Mode
        {
            var panicParameter = 0.7f;
            var goalDirection = ((1 - panicParameter) * (path[0] - transform.position));

            var neighborvel = Vector3.zero;
            foreach (var n in perceivedNeighbors)
            {
                neighborvel += ((path[0] - transform.position) * Mathf.Min((path[0] - transform.position).magnitude,1));
            }

            neighborvel = neighborvel / perceivedNeighbors.Count;
            goalDirection = (goalDirection + panicParameter * neighborvel).normalized;
            
            var prefForce = (((goalDirection * Mathf.Min(goalDirection.magnitude, 1)) - rb.velocity) / Parameters.T);
            var force = prefForce + CalculateAgentForce() + CalculateWallForce();

            if (force != Vector3.zero)
            {
                return force.normalized * Mathf.Min(force.magnitude, Parameters.maxSpeed);
            }
            else
            {
                return Vector3.zero;
            }
        }

        return Vector3.zero;

    }
    
    private Vector3 CalculateGoalForce(float maxSpeed)
    {
        if (path.Count == 0)
        {
            return Vector3.zero;
        }

        //var goalDirection = (path[0] - transform.position).normalized;
        //var prefForce = (((goalDirection * Mathf.Min(goalDirection.magnitude, maxSpeed)) - rb.velocity) / Parameters.T);
        //return prefForce;

        var temp = path[0] - transform.position;
        var desiredVel = temp.normalized * Mathf.Min(temp.magnitude, maxSpeed);
        var actualVelocity = rb.velocity; 
        return (desiredVel - actualVelocity)/ Parameters.T;

    }

    private Vector3 CalculateAgentForce()
    {
        var agentForce = Vector3.zero;

        foreach (var n in perceivedNeighbors)
        {
            if (!AgentManager.IsAgent(n))
            {
                continue;
            }

            var neighbor = AgentManager.agentsObjs[n];
            var dir = (transform.position - neighbor.transform.position).normalized;
            var overlap = (radius + neighbor.radius) - Vector3.Distance(transform.position, n.transform.position);
            
            agentForce += Parameters.A * Mathf.Exp(overlap / Parameters.B) * dir;
            agentForce += Parameters.k * (overlap > 0f ? 1 : 0) * dir;
            
            var tangent = Vector3.Cross(Vector3.up, dir);
            agentForce += Parameters.Kappa * (overlap > 0f ? overlap : 0) * Vector3.Dot(rb.velocity - neighbor.GetVelocity(), tangent)* tangent;
           
            /*var proximityForce = Parameters.A * Mathf.Exp(radiusDiff / Parameters.B) * dir;
            var repulsionForce = Parameters.k * (radiusDiff > 0f ? radiusDiff : 0) * dir;
            var slidingForce = Parameters.Kappa * (radiusDiff > 0f ? radiusDiff : 0) * Vector3.Dot(rb.velocity - neighbor.GetVelocity(), Vector3.Cross(Vector3.up,dir)) * Vector3.Cross(Vector3.up,dir);*/

            //agentForce = proximityForce + slidingForce;//+repulsionForce;
        }

        return agentForce;
    }

    private Vector3 CalculateWallForce()
    {
        var wallForce = Vector3.zero;

        foreach (var wall in touchedWalls)
        {
            if (!WallManager.IsWall(wall))
            {
                continue;
            }

            var dir = (transform.position - wall.transform.position).normalized;
            var radiusDiff = (radius) - Vector3.Distance(transform.position, wall.transform.position);
            var proximityForce = Parameters.WALL_A * Mathf.Exp(radiusDiff / Parameters.WALL_B) * dir;
            var repulsionForce = Parameters.WALL_k * (radiusDiff > 0f ? radiusDiff : 0) * dir;
            var slidingForce = Parameters.WALL_Kappa * (radiusDiff > 0f ? radiusDiff : 0) * Vector3.Dot(rb.velocity, Vector3.Cross(Vector3.up, dir)) * Vector3.Cross(Vector3.up, dir);

            wallForce = proximityForce + repulsionForce - slidingForce;
        }
        return wallForce;

    }

    public void ApplyForce()
    {
        var force = ComputeForce();
        force.y = 0;

        rb.AddForce(force * 10, ForceMode.Force);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (AgentManager.IsAgent(other.gameObject))
        {
            perceivedNeighbors.Add(other.gameObject);
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        if (perceivedNeighbors.Contains(other.gameObject))
        {
            perceivedNeighbors.Remove(other.gameObject);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (WallManager.IsWall(collision.gameObject))
        {
            touchedWalls.Add(collision.gameObject);
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        if (touchedWalls.Contains(collision.gameObject))
        {
            touchedWalls.Remove(collision.gameObject);
        }
    }

    #endregion

    public GameObject FindClosestTag(string tag)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }
}
