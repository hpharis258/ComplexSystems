using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
public class PART1TruckPathFinder : MonoBehaviour
{
    // The A* manager.
    private AStarManager AStarManager = new AStarManager(); 
    // List of all possible waypoints.
    private List<GameObject> Waypoints = new List<GameObject>();
    // List of waypoint map connections. Represents a path.
    private List<Connection> ConnectionArray = new List<Connection>();
    //
    GameObject[] GameObjectsWithWaypointTag;
    // The start and end nodes.
    [SerializeField]
    private GameObject start;
    [SerializeField]
    private List<GameObject> end;
    [SerializeField]
    private GameObject HumanAgent = null;
    [SerializeField]
    private GameObject HumanAgent1 = null;
    [SerializeField]
    private GameObject HumanAgent2 = null;
    [SerializeField]
    private GameObject StaticObstacle = null;
    // Time Taken
    private float timer = 0;
    // Distance Covered 
    private float totalDistance = 0;
    private Vector3 lastPosition = new Vector3(0,0,0);
    private bool outputTimeAndDistance = true;
    // Debug line offset.
    Vector3 OffSet = new Vector3(0, 0.3f, 0);
    // Movement variables.
    private float currentSpeed = 30;
    private int currentTarget = 0;
    private int currentEnd = 0;
    private Vector3 currentTargetPos;
    private int moveDirection = 1;
    private bool agentMove = true;
    bool goBack = false;
    bool endCurrentTrip = false;
    bool stop = false;
    // Start is called before the first frame update
    void Start()
    {
        if (start == null || end[currentEnd] == null)
        {
            Debug.Log("No start or end waypoints.");
            return;
        }
        VisGraphWaypointManager tmpWpM = start.GetComponent<VisGraphWaypointManager>();
        if (tmpWpM == null)
        {
            Debug.Log("Start is not a waypoint.");
            return;
        }
        tmpWpM = end[currentEnd].GetComponent<VisGraphWaypointManager>();
        if (tmpWpM == null)
        {
            Debug.Log("End is not a waypoint.");
            return;
        }
        // Find all the waypoints in the level.
       
        GameObjectsWithWaypointTag = GameObject.FindGameObjectsWithTag("Waypoint");
        foreach (GameObject waypoint in GameObjectsWithWaypointTag)
        {
            VisGraphWaypointManager tmpWaypointMan = waypoint.GetComponent<VisGraphWaypointManager>();
            if (tmpWaypointMan)
            {
                Waypoints.Add(waypoint);
                //Debug.Log("Waypoints Count: " + Waypoints.Count.ToString());
            }
        }
        // Go through the waypoints and create connections.
        foreach (GameObject waypoint in Waypoints)
        {
            VisGraphWaypointManager tmpWaypointMan = waypoint.GetComponent<VisGraphWaypointManager>();
            // Loop through a waypoints connections.
            foreach (VisGraphConnection aVisGraphConnection in tmpWaypointMan.Connections)
            {
                if (aVisGraphConnection.ToNode != null)
                {
                    Connection aConnection = new Connection();
                    aConnection.FromNode = waypoint;
                    aConnection.ToNode = aVisGraphConnection.ToNode;
                    AStarManager.AddConnection(aConnection);
                    //Debug.Log("Added Connection" + aConnection.ToNode.ToString());
                }
                else
                {
                    Debug.Log("Warning, " + waypoint.name + " has a missing to node for a connection!");
                }
            }
        }
        
            // Run A Star...
            // ConnectionArray stores all the connections in the route to the goal / end node.
            ConnectionArray = AStarManager.PathfindAStar(start, end[currentEnd]);
            if (ConnectionArray.Count == 0)
            {
                Debug.Log("Warning, A* did not return a path between the start and end node.");
                
            }
        lastPosition = transform.position;
      
        
       
    }
    // Draws debug objects in the editor and during editor play (if option set).
    void OnDrawGizmos()
    {
        // Draw path.
        foreach (Connection aConnection in ConnectionArray)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine((aConnection.FromNode.transform.position + OffSet),
           (aConnection.ToNode.transform.position + OffSet));
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(goBack)
        {
            //Debug.Log("I am Going Back!");
            ConnectionArray = AStarManager.PathfindAStar(start, end[currentEnd]);
            if (ConnectionArray.Count == 0)
            {
                Debug.Log("Did Not Find a Path Back!");
                return;
            }
            endCurrentTrip = true;
            goBack = false;
        }
        if (agentMove && goBack == false && stop == false)
        {
            Vector3 tmpDir = lastPosition - transform.position;
            float tempDistance = tmpDir.magnitude;
            totalDistance += tempDistance;
            lastPosition = transform.position;
            timer += Time.deltaTime;
            if(HumanAgent)
            {
                float DistanceToObject = Vector3.Distance(transform.position, HumanAgent.transform.position);
                if (DistanceToObject < 6)
                {
                    return;
                }
            }
            if (HumanAgent1)
            {
                float DistanceToObject = Vector3.Distance(transform.position, HumanAgent1.transform.position);
                if (DistanceToObject < 7)
                {
                    return;
                }
            }
            if (HumanAgent2)
            {
                float DistanceToObject = Vector3.Distance(transform.position, HumanAgent2.transform.position);
                if (DistanceToObject < 8)
                {
                    return;
                }
            }
            if(StaticObstacle)
            {
                float DistanceToObject = Vector3.Distance(transform.position, StaticObstacle.transform.position);
                if (DistanceToObject < 50)
                {
                    currentSpeed = 10;
                }
                else
                {
                    currentSpeed = 30;
                }
            }
            // Determine the direction to first node in the array.
            if (moveDirection > 0)
            {
                currentTargetPos = ConnectionArray[currentTarget].ToNode.transform.position;
            }
            else
            {
                currentTargetPos = ConnectionArray[currentTarget].FromNode.transform.position;
            }
            // Clear y to avoid up/down movement. Assumes flat surface.
            currentTargetPos.y = transform.position.y;
            Vector3 direction = currentTargetPos - transform.position;
            float distance = direction.magnitude;
            // Face in the right direction.
            direction.y = 0;
          
            if (direction.magnitude > 0)
            {
                if(currentTarget == 0)
                {
                    // 
                    currentTargetPos = ConnectionArray[0].FromNode.transform.position;
                }
                //Debug.Log("Direction magnitude bigger than 0");
                //Debug.Log("Direction magnitude " + direction.magnitude.ToString());
                float degreesPerSecond = 360 * Time.deltaTime;
                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                // 
                //rotation.y = rotation.y;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation *= Quaternion.Euler(0, 270, 0), degreesPerSecond);
               

            }

            // Calculate the normalised direction to the target from a game object.
            Vector3 normDirection = direction / distance;
            // Move the game object.
            transform.position = transform.position + normDirection * currentSpeed * Time.deltaTime;
            // Check if close to current target.
            if (distance < 1)
            {
                //Debug.Log("Connection Count: " + ConnectionArray.Count.ToString());
                //Debug.Log("Current Target: " + currentTarget.ToString());
//Debug.Log("Distance less than one");
                if(ConnectionArray.Count - 1 != currentTarget)
                {
                    currentTarget++;
                    distance = 1;
                }else
                {
                    if(endCurrentTrip == false)
                    {
                        Debug.Log("Made the delivery");
                        int sleepTime = 3000;
                        
                        Thread.Sleep(sleepTime);
                        //Debug.Log("Going Back!");
                        var temp = start;
                        start = end[currentEnd];
                        end[currentEnd] = temp;
                        ConnectionArray.Clear();
                        currentTarget = 0;
                        //print("CURRENT TARGET IN THE END: " + currentTarget.ToString());
                        //currentTarget--;
                        goBack = true;
                        //ConnectionArray.Reverse();
                        //

                        distance = 1;
                    }else
                    {
                        Debug.Log("Current Trip Ended!");
                        stop = true;
                        agentMove = false;
                        return;
                    }
                   
                }
               
                //currentTargetPos = ConnectionArray[currentTarget].FromNode.transform.position;
               
                
                // Add code here to set the next target index and handle the end and start of the
                // waypoint path array(ConnectionArray).
                // Close to target, so move to the next target in the list (if there is one).
            }
        }
        else
        {
            if(outputTimeAndDistance)
            {
                Debug.Log("Time Taken: " + timer);
                Debug.Log("Distance: " + totalDistance);
                timer = 0;
                totalDistance = 0;
                outputTimeAndDistance = false;
            }
        }
    }
}