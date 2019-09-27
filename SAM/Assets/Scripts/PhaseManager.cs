using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// PhaseManager is the place to keep a succession of events or "phases" when building 
/// a multi-step AI demo. This is essentially a state variable for the map (aka level)
/// itself, not the state of any particular NPC.
/// 
/// Map state changes could happen for one of two reasons:
///     when the user has pressed a number key 0..9, desiring a new phase
///     when something happening in the game forces a transition to the next phase
/// 
/// One use will be for AI demos that are switched up based on keyboard input. For that, 
/// the number keys 0..9 will be used to dial in whichever phase the user wants to see.
/// </summary>
public class PhaseManager : MonoBehaviour {
    // Set prefabs
    public GameObject PlayerPrefab; // You, the player
    public GameObject HunterPrefab; // Agent doing chasing
    public GameObject WolfPrefab; // Agent getting chased

    public GameObject RedPrefab; // reserved for future use
    // public GameObject BluePrefab;    // reserved for future use

    public NPCController house; // THis goes away

    // Set up to use spawn points. Can add more here, and also add them to the 
    // Unity project. This won't be a good idea later on when you want to spawn
    // a lot of agents dynamically, as with Flocking and Formation movement.

    public GameObject spawner1;
    public Text SpawnText1;
    public GameObject spawner2;
    public Text SpawnText2;
    public GameObject spawner3;
    public Text SpawnText3;

    private List<GameObject> spawnedNPCs; // When you need to iterate over a number of agents.

    private int currentMapState = 0; // This stores which state the map or level is in.
    private int previousMapState = 0; // The map state we were just in

    public int MapState => currentMapState;

    LineRenderer line; // GOING AWAY

    public GameObject[] Path;

    public bool keyClicked = false;

    public Text narrator; // 


    // Use this for initialization. Create any initial NPCs here and store them in the 
    // spawnedNPCs list. You can always add/remove NPCs later on.

    void Start() {
        narrator.text = @"Press 1-7: {1: Seek/Flee, 
        3: Pursue/Evade, 5: Face/ Wander, 6: Align/Wander, 7: Wander, 0: Restart}";
        spawnedNPCs = new List<GameObject>();
    }

    /// <summary>
    /// This is where you put the code that places the level in a particular state.
    /// Unhide or spawn NPCs (agents) as needed, and give them things (like movements)
    /// to do. For each case you may well have more than one thing to do.
    /// </summary>
    private void Update() {
        string inputstring = Input.inputString;
        int num;
        // Look for a number key click
        if (inputstring.Length > 0) {
            Debug.Log(inputstring);
            if (Int32.TryParse(inputstring, out num)) {
                if (num == 0) {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }

                if (num == 1) {
                    EnterMapStateOne();
                }

                if (num == 2) {
                    EnterMapStateTwo();
                }

                if (num == 3) {
                    EnterMapStateThree();
                }

                if (num == 4) {
                    EnterMapStateFour();
                }

                if (num == 5) {
                    EnterMapStateFive();
                }

                if (num == 6) {
                    EnterMapStateSix();
                }

                if (num == 7) {
                    EnterMapStateSeven();
                }
            }
        }

        // Check if a game event had caused a change of state in the level.
        if (currentMapState == previousMapState)
            return;
    }

    private void EnterMapStateOne() {
        // Clear the list of NPCs
        //spawnedNPCs = new List<GameObject>();
        narrator.text = "In MapState Zero, the hunter will seek the fleeing wolf o.o\nSEEKing you out";
        // Spawn a Hunter with a wolf as the target and a map state of 1 which will call the seek function
        NPCController target = WolfPrefab.GetComponent<NPCController>();
        spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText1, 1));


        // Spawn a fleeing wolf
        SpawnWolf(2);

        // Set the targets to each other just to make sure
        setTarget();
        //Invoke("setTarget", 0);

        // Clear the NPC list again 
        Invoke("Farewell", 2);
    }


    private void EnterMapStateTwo() {
        // Clear list and set text
        spawnedNPCs = new List<GameObject>();
        narrator.text = "In MapState Zero, the wolf will flee the hunter 0.0";

        // Same as function above
        // NPCController target = WolfPrefab.GetComponent<NPCController>();
        spawnedNPCs.Add(SpawnItem(spawner1, WolfPrefab, null, SpawnText1, 1));

        // Invoke("SpawnHunterFlee", 0);

        SpawnHunter(2);
        // Set the targets to each other just to make sure
        setTarget();
        //Invoke("SeekFarewell", 15);
    }

    private void EnterMapStateThree() {
        spawnedNPCs = new List<GameObject>();


        narrator.text = "In MapState Two, we're going to Pursue >:) !";
        spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText1, 3));

        //Invoke("SpawnWolfEvade", 0);
        SpawnWolf(4); // Spawn a wolf with a map state of 4 (evading)
        setTarget(); // set targets to each other
        //Invoke("PursueMeeting", 2);
    }

    private void EnterMapStateFour() {
        spawnedNPCs = new List<GameObject>();
        narrator.text = "In MapSate Three, we're going to Evade Dx";
        spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText1, 4));

        Invoke("EvadeMeeting", 0);
    }

    private void EnterMapStateFive() // Face
    {
        spawnedNPCs.Clear(); //= new List<GameObject>();
        narrator.text = "In MapSate Four, we're going to Face with the Player";
        spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText1, 5));

        SpawnWolf(7); // Spawn a wolf with a map state of 7 (wondering)
        // set the target of align to the wondering NPC
        spawnedNPCs[0].GetComponent<SteeringBehavior>().target = spawnedNPCs[1].GetComponent<NPCController>();

        //spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText1, 6));
        //Invoke("FaceMeeting", 3);
    }

    private void EnterMapStateSix() // Align
    {
        /* 
        spawnedNPCs = new List<GameObject>();
        narrator.text = "Entering MapState Five...aligning";
        spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, 6));
        spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText2, 6));
        Invoke("FaceMeeting", 3);*/

        narrator.text = "Align with Wander";

        spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText1, 5));

        SpawnWolf(7);
        spawnedNPCs[0].GetComponent<SteeringBehavior>().target = spawnedNPCs[1].GetComponent<NPCController>();
        //Invoke("SpawnWolfWander", 0);
        //Invoke("SeekMeeting", 0);
        //Invoke("SeekFarewell", 15);
    }

    private void EnterMapStateSeven() // Wander
    {
        spawnedNPCs = new List<GameObject>();

        narrator.text = "Let's both Wander!";
        spawnedNPCs.Add(SpawnItem(spawner2, HunterPrefab, null, SpawnText2, 7));
        spawnedNPCs.Add(SpawnItem(spawner1, WolfPrefab, null, SpawnText2, 7));
        //Invoke("WanderMeeting", 4);
    }

    /// <summary>
    /// SpawnItem placess an NPC of the desired type into the game and sets up the neighboring 
    /// floating text items nearby (diegetic UI elements), which will follow the movement of the NPC.
    /// </summary>
    /// <param name="spawner"></param>
    /// <param name="spawnPrefab"></param>
    /// <param name="target"></param>
    /// <param name="spawnText"></param>
    /// <param name="mapState"></param>
    /// <returns></returns>
    private GameObject SpawnItem(GameObject spawner, GameObject spawnPrefab, NPCController target, Text spawnText,
        int mapState) {
        Vector3 size = spawner.transform.localScale;
        Vector3 position = spawner.transform.position + new Vector3(UnityEngine.Random.Range(-size.x / 2, size.x / 2),
                               0, UnityEngine.Random.Range(-size.z / 2, size.z / 2));
        GameObject temp = Instantiate(spawnPrefab, position, Quaternion.identity);
        if (target) {
            temp.GetComponent<SteeringBehavior>().target = target;
        }

        temp.GetComponent<NPCController>().label = spawnText;
        temp.GetComponent<NPCController>().mapState = mapState; // This is separate from the NPC's internal state
        Camera.main.GetComponent<CameraController>().player = temp;
        return temp;
    }

    // These next two methods show spawning an agent might look.
    // You make them happen when you want to by using the Invoke() method.
    // These aren't needed for the first assignment.

    private void SpawnWolf(int num) {
        spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, num));
    }

    private void SpawnHunter(int num) {
        spawnedNPCs.Add(SpawnItem(spawner2, HunterPrefab, null, SpawnText2, num));
    }

    private void SpawnHunterFlee() {
        narrator.text = "The Hunter appears, Fleeing";
        spawnedNPCs.Add(SpawnItem(spawner2, HunterPrefab, null, SpawnText2, 2));
    }

    private void SpawnWolfEvade() {
        narrator.text = "The Wolf appears, Evading";
        spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, 4));
    }

    private void SpawnWolfWander() {
        narrator.text = "The Wolf appears, Wandering";
        spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, 7));
    }

    private void Meeting1() {
        narrator.text = "The Wolf and Hunter have meet.";
        if (currentMapState == 0) {
            spawnedNPCs[0].GetComponent<SteeringBehavior>().target = spawnedNPCs[1].GetComponent<NPCController>();
            spawnedNPCs[1].GetComponent<SteeringBehavior>().target = spawnedNPCs[0].GetComponent<NPCController>();
        }

        SetArrive(spawnedNPCs[0]);
        SetArrive(spawnedNPCs[1]);
    }

    private void SeekMeeting() {
        /// narrator.text = "SEEKing you out";
        // put more actions in here

        if (currentMapState == 0) {
            spawnedNPCs[0].GetComponent<SteeringBehavior>().target = spawnedNPCs[1].GetComponent<NPCController>();
            spawnedNPCs[1].GetComponent<SteeringBehavior>().target = spawnedNPCs[0].GetComponent<NPCController>();
        }

        //SetArrive(spawnedNPCs[0]);
        //SetArrive(spawnedNPCs[1]);
    }

    private void Farewell() {
        spawnedNPCs.Clear(); // = new List<GameObject>();
    }

    private void setTarget() {
        spawnedNPCs[0].GetComponent<SteeringBehavior>().target = spawnedNPCs[1].GetComponent<NPCController>();
        spawnedNPCs[1].GetComponent<SteeringBehavior>().target = spawnedNPCs[0].GetComponent<NPCController>();
    }


    // Here is an example of a method you might want for when an arrival actually happens.
    private void SetArrive(GameObject character) {
        character.GetComponent<NPCController>().mapState = 0; // Whatever the new map state is after arrival
        character.GetComponent<NPCController>()
            .DrawConcentricCircle(character.GetComponent<SteeringBehavior>().slowRadiusL);
    }

    // Following the above examples, write whatever methods you need that you can bolt together to 
    // create more complex movement behaviors.

    // YOUR CODE HERE

    // Vestigial. Maybe you'll find it useful.
    void OnDrawGizmosSelected() {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(spawner1.transform.position, spawner1.transform.localScale);
        Gizmos.DrawCube(spawner2.transform.position, spawner2.transform.localScale);
        Gizmos.DrawCube(spawner3.transform.position, spawner3.transform.localScale);
    }
}