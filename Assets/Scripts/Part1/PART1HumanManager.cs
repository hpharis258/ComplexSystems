using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PART1HumanManager : MonoBehaviour
{
    private List<GameObject> humanWaypoints = new List<GameObject>();
    private GameObject currentTarget = null;
    private float currentSpeed = 3;
    private Animator animator;
    private AnimationClip[] myClips;

    // Start is called before the first frame update
    void Start()
    {
        // Find All Human Waypoints
        GameObject[] GameObjectsWithHumanWaypointTag;
        GameObjectsWithHumanWaypointTag = GameObject.FindGameObjectsWithTag("HumanWaypoint");
        //
        foreach(GameObject waypoint in GameObjectsWithHumanWaypointTag)
        {
            VisGraphWaypointManager tmpWaypoint = waypoint.GetComponent<VisGraphWaypointManager>();
            if(tmpWaypoint)
            {
                humanWaypoints.Add(waypoint);
            }
        }
        currentTarget = FindClosest();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(currentTarget != null)
        {
            // Find direction to current target,
            Vector3 direction = currentTarget.transform.position - transform.position;
            direction.y = 0;
            float distance = direction.magnitude;
            //
            if(direction.magnitude > 0)
            {
                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = rotation;
            }
            //
            Vector3 normDirection = direction / distance;
            transform.position = transform.position + normDirection* currentSpeed *Time.deltaTime;
           
            //animator.SetInteger("Walk", 1);
            if (distance < 1)
            {
                VisGraphWaypointManager tmpWaypoint = currentTarget.GetComponent<VisGraphWaypointManager>();
                if(tmpWaypoint)
                {
                    if(tmpWaypoint.connections.Count == 0)
                    {
                        currentTarget = null;
                    }
                    if(tmpWaypoint.connections.Count == 1)
                    {
                        currentTarget = tmpWaypoint.connections[0].ToNode;
                    }
                    if(tmpWaypoint.connections.Count > 1)
                    {
                        int rndIndx = Random.Range(0, tmpWaypoint.connections.Count);  
                        currentTarget= tmpWaypoint.connections[rndIndx].ToNode;
                    }
                }
            }
        }
       
    }
    //
    private GameObject FindClosest()
    {
        GameObject closest = null;
        float distanceSqr = Mathf.Infinity;
        //
        foreach(GameObject waypoint in humanWaypoints)
        {
            if(waypoint != null)
            {
                Vector3 direction = waypoint.transform.position - transform.position;
                float tmpDistanceSqr = direction.sqrMagnitude;
                if(tmpDistanceSqr < distanceSqr)
                {
                    closest = waypoint;
                    distanceSqr = tmpDistanceSqr;
                }
            }
        }
        return closest; 
    }
}
