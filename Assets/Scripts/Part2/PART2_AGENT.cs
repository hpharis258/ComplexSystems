using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
public class PART2TruckPathFinder : MonoBehaviour
{
    // The A* manager.
    private AStarManager AStarManager = new AStarManager();
    // List of all possible waypoints.
    private List<GameObject> Waypoints = new List<GameObject>();
    // List of waypoint map connections. Represents a path.
    private List<Connection> ConnectionArray = new List<Connection>();
    //
    public bool run = true;
    GameObject[] GameObjectsWithWaypointTag;
    // The start and end nodes.
    [SerializeField]
    private string AgentName;
    [SerializeField]
    private GameObject start;
    private GameObject OriginalStart;
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
    [SerializeField]
    private GameObject AnotherAgent = null;
    public PART2TruckPathFinder AnotherAgentScript = null;
    [SerializeField]
    private GameObject AnotherAgent1 = null;
    public PART2TruckPathFinder AnotherAgentScript1 = null;
    [SerializeField]
    private GameObject AnotherAgent2 = null;
    public PART2TruckPathFinder AnotherAgentScript2 = null;
    [SerializeField]
    private GameObject AnotherAgent3 = null;
    public PART2TruckPathFinder AnotherAgentScript3 = null;
    [SerializeField]
    private GameObject AnotherAgent4 = null;
    public PART2TruckPathFinder AnotherAgentScript4 = null;
    // Time Taken
    private float timer = 0;
    // Distance Covered 
    private float totalDistance = 0;
    private Vector3 lastPosition = new Vector3(0, 0, 0);
    private bool outputTimeAndDistance = true;
    // Debug line offset.
    Vector3 OffSet = new Vector3(0, 0.3f, 0);
    // Movement variables.
    private float currentSpeed = 80;
    private float lastSpeed = 80;
    private int currentTarget = 0;
    private int currentEnd = 0;
    private Vector3 currentTargetPos;
    private int moveDirection = 1;
    private bool agentMove = true;
    bool goBack = false;
    bool endCurrentTrip = false;
    bool stop = false;
    bool goToNextLocation = false;
    int DeliveryCount = 0;

    public void ThreadProc()
    {
        //Debug.Log("THREAD STARTED");
        Thread.Sleep(1000);
        run = true;
        //Debug.Log("THREAD END");
    }
    public void WaitForFasterAgentToPass()
    {
        //Debug.Log("THREAD STARTED");
        Thread.Sleep(3000);
        run = true;
        //Debug.Log("THREAD END");
    }
    public float GetSpeed()
    {
        return currentSpeed;
    }

    // Start is called before the first frame update
    void Start()
    {
        OriginalStart = start;
        // Calculate Starting Speed Based on How many Parcels the truck is carrying
        var deliveryAmount = end.Count;
        for (int i = 0; i < deliveryAmount; i++)
        {
            if (currentSpeed >= 30 && lastSpeed >= 30)
            {
                currentSpeed -= 5;
                lastSpeed -= 5;
            }

        }
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
        if (run)
        {
            if (goToNextLocation)
            {
                currentTarget = 0;
                ConnectionArray = AStarManager.PathfindAStar(start, end[currentEnd]);
                if (ConnectionArray.Count == 0)
                {
                    Debug.Log("Did Not Find a Path To the next Node!");
                    return;
                }
                goToNextLocation = false;
                endCurrentTrip = false;
                goBack = false;
            }
            if (goBack)
            {
                currentTarget = 0;
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
                if (HumanAgent)
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
                    if (DistanceToObject < 7)
                    {
                        return;
                    }
                }
                if (StaticObstacle)
                {
                    float DistanceToObject = Vector3.Distance(transform.position, StaticObstacle.transform.position);
                    if (DistanceToObject < 50)
                    {
                        currentSpeed = 30;
                    }
                    else
                    {
                        currentSpeed = lastSpeed;
                    }
                }
                if (AnotherAgent)
                {
                    if (AnotherAgentScript)
                    {
                        float DistanceToObject = Vector3.Distance(transform.position, AnotherAgent.transform.position);
                        //Debug.Log("Distance to another agent: " + DistanceToObject.ToString());
                        if (DistanceToObject < 10)
                        {
                            //transform.Translate(Vector3.forward);
                            // Move the object forward along its z axis 1 unit/second.
                            //transform.Translate(Vector3.forward * Time.deltaTime);
                            //transform.Translate(Vector3.forward);
                            float OtherAgentSpeed = AnotherAgentScript.GetSpeed();
                            if (OtherAgentSpeed > currentSpeed)
                            {
                                Thread t = new Thread(new ThreadStart(WaitForFasterAgentToPass));
                                t.Start();
                                run = false;
                                //currentSpeed = 15;
                                //return;
                            }
                            //return;
                        }
                    }

                }
                if (AnotherAgent1)
                {
                    if (AnotherAgentScript1)
                    {
                        float DistanceToObject = Vector3.Distance(transform.position, AnotherAgent1.transform.position);
                        if (DistanceToObject < 10)
                        {

                            //transform.Translate(Vector3.forward);
                            // Move the object forward along its z axis 1 unit/second.
                            //transform.Translate(Vector3.forward * Time.deltaTime);
                            // transform.Translate(Vector3.forward);
                            float OtherAgentSpeed = AnotherAgentScript1.GetSpeed();
                            if (OtherAgentSpeed > currentSpeed)
                            {
                                Thread t = new Thread(new ThreadStart(WaitForFasterAgentToPass));
                                t.Start();
                                run = false;
                                //return;
                            }
                        }
                    }
                }
                if (AnotherAgent2)
                {
                    if (AnotherAgentScript2)
                    {
                        float DistanceToObject = Vector3.Distance(transform.position, AnotherAgent2.transform.position);
                        if (DistanceToObject < 10)
                        {
                            //transform.Translate(Vector3.forward);
                            //transform.LookAt(AnotherAgent2.transform.position);
                            //transform.Rotate(0, 180, 0);
                            //transform.Translate(Vector3.forward);
                            //// Move the object forward along its z axis 1 unit/second.
                            ////transform.Translate(Vector3.forward * Time.deltaTime);
                            ////transform.Translate(Vector3.forward);
                            float OtherAgentSpeed = AnotherAgentScript2.GetSpeed();
                            if (OtherAgentSpeed > currentSpeed)
                            {
                                Thread t = new Thread(new ThreadStart(WaitForFasterAgentToPass));
                                t.Start();
                                run = false;
                                //return;
                            }
                            //return;
                        }
                    }

                }
                if (AnotherAgent3)
                {
                    if (AnotherAgentScript3)
                    {
                        float DistanceToObject = Vector3.Distance(transform.position, AnotherAgent3.transform.position);
                        if (DistanceToObject < 10)
                        {
                            //transform.Translate(Vector3.forward);
                            //transform.LookAt(AnotherAgent3.transform.position);
                            //transform.Rotate(0, 180, 0);
                            //transform.Translate(Vector3.forward);
                            // Move the object forward along its z axis 1 unit/second.
                            //transform.Translate(Vector3.forward * Time.deltaTime);
                            //transform.Translate(Vector3.forward);
                            float OtherAgentSpeed = AnotherAgentScript3.GetSpeed();
                            if (OtherAgentSpeed > currentSpeed)
                            {
                                Thread t = new Thread(new ThreadStart(WaitForFasterAgentToPass));
                                t.Start();
                                run = false;
                                // return;
                            }
                        }
                    }

                }
                if (AnotherAgent4)
                {
                    if (AnotherAgentScript4)
                    {
                        float DistanceToObject = Vector3.Distance(transform.position, AnotherAgent4.transform.position);
                        if (DistanceToObject < 10)
                        {
                            
                            // Move the object forward along its z axis 1 unit/second.
                            //transform.Translate(Vector3.forward * Time.deltaTime);
                            //transform.Translate(Vector3.forward);
                            float OtherAgentSpeed = AnotherAgentScript4.GetSpeed();
                            if (OtherAgentSpeed > currentSpeed)
                            {
                                Thread t = new Thread(new ThreadStart(WaitForFasterAgentToPass));
                                t.Start();
                                run = false;

                                //return;
                            }

                        }
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
                        if (currentTarget == 0)
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
                        if (ConnectionArray.Count - 1 != currentTarget)
                        {
                            currentTarget++;
                            distance = 1;
                        }
                        else
                        {
                            if (endCurrentTrip == false)
                            {
                                DeliveryCount++;
                                //Debug.Log("********************************");
                                //Debug.Log("Agent Name: " + AgentName);
                                Debug.Log(AgentName + ": Completed Delivery: " + DeliveryCount);
                                //Debug.Log(AgentName + ": Time Taken: " + timer);
                                //Debug.Log(AgentName + ": Distance Covered: " + totalDistance);
                                Thread t = new Thread(new ThreadStart(ThreadProc));
                                t.Start();
                                run = false;
                               


                                currentSpeed += ((float)(lastSpeed * 0.10));
                                lastSpeed += (float)(lastSpeed * 0.1);
                                Debug.Log(AgentName + ": speed Increased: " + currentSpeed);
                                //Debug.Log("********************************");
                                //await Task.Delay(sleepTime);

                                //Debug.Log(end.Count);
                                //Debug.Log(currentEnd);
                                if (end.Count > DeliveryCount)
                                {
                                    start = end[currentEnd];
                                    currentEnd++;
                                    goToNextLocation = true;

                                    return;

                                }
                                start = end[currentEnd];
                                currentEnd = 0;

                                end[currentEnd] = OriginalStart;
                                ConnectionArray.Clear();
                                currentTarget = 0;
                                //print("CURRENT TARGET IN THE END: " + currentTarget.ToString());
                                //currentTarget--;
                                goBack = true;
                                //ConnectionArray.Reverse();
                                //

                                distance = 1;
                            }
                            else
                            {
                               
                                //Debug.Log("********************************");
                                //Debug.Log(AgentName + " Has completed all deliveries.");
                                ////Debug.Log("Agent Name: " + AgentName);
                                //Debug.Log(AgentName + ": Total Time Taken: " + timer);
                                //Debug.Log(AgentName + ": Total Distance Covered: " + totalDistance);
                                //Debug.Log(AgentName + ": Total Deliveries Completed: " + DeliveryCount.ToString());
                                //Debug.Log("********************************");
                                stop = true;
                                //goBack = true;
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
                


                if (outputTimeAndDistance)
                    {
                    //
                    if (AgentName == "AGENT 1")
                    {
                        transform.position += new Vector3(10, 0, 0);
                    }
                    if (AgentName == "AGENT 2")
                    {
                        transform.position += new Vector3(20, 0, 0);
                    }
                    if (AgentName == "AGENT 3")
                    {
                        transform.position += new Vector3(30, 0, 0);
                    }
                    if (AgentName == "AGENT 4")
                    {
                        transform.position += new Vector3(-10, 0, 0);
                    }
                    if (AgentName == "AGENT 5")
                    {
                        transform.position += new Vector3(-20, 0, 0);
                    }
                    if (AgentName == "AGENT 6")
                    {
                        transform.position += new Vector3(-30, 0, 0);
                    }
                    //

                    Debug.Log("********************************");
                        Debug.Log(AgentName + " Has completed all deliveries.");
                    //Debug.Log("Agent Name: " + AgentName);
                        Debug.Log(AgentName + ": Total Time Taken: " + timer);
                        Debug.Log(AgentName+": Total Distance Covered: " + totalDistance);
                        Debug.Log(AgentName +": Total Deliveries Completed: " + DeliveryCount.ToString());
                        Debug.Log("********************************");
                        timer = 0;
                        totalDistance = 0;
                        outputTimeAndDistance = false;
                    }
                }
            }



        }
    }
