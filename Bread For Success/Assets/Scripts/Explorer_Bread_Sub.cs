using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
public class Explorer_Bread_Sub : Base_Bread_Class
{
    List<GameObject> Current_Path_Seek_Point_Storage = new List<GameObject>(0);



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assign_Components_And_GameObjects();
        Find_Map_Objects();
        Current_Behavior = "idle";
        Is_Explorer = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Dead)
        {
            Base_Bread_Operation_Function_Calls(); // includes die check, current target check, depositing check, collecting check, and retreat threshold check. is idle will be set to true if our current target is not active
            Explore_Behaviors();

        }
    }

    private void FixedUpdate()
    {
        if (!Dead && Current_Target != null && Current_Target.activeInHierarchy)
        {
            Move_Towards_Target();
        }

    }

    private void Explore_Behaviors()
    {
        if (Current_Behavior == "idle")
        {
            if (Discovered_Active_Map_Objects == null)
            {
                Search_For_Locations_Of_Interest();
            }
            else
            {
                int temp = Random.Range(1, 4);

                if (temp == 1)
                {
                    Optimize_Path();
                }
                else
                {
                  Search_For_Locations_Of_Interest();
                }
            }


        }
    }

    // my idea right now is our explorers do not know where these locations of interest are but they know where the fog of war is, so 
    //they will attempt to move towards the nearest fog of war and when they reach it they will look for the next nearest fog of war. along the way all the semi random but directionally targeted
    //pathing point game objects will not be destroyed. If our explorer finds and object of interest then we will save the pathing points as a path and store it in the global game object
    //as path_1 for example. each path will be a class with a length, destination, list of target points, and maybe some other stuff like average damage taken IDK. we will use these stats
    // later for other explorers attempting to optimize paths or other breads attempting to use paths, like they wont use paths with more than 10 length.
    public void Search_For_Locations_Of_Interest()
    {
        Current_Behavior = "searching";

        Collider2D[] Nearby_Locations_Of_Interest = Physics2D.OverlapCircleAll(transform.position, Sight_Range, (1 << 7)); // need to limit this to only the breads layer somehow

        //need to check if object in sight has been discovered yet or not, if it has not then we need to placenextseekpointtowardstargetand feed it the reference to the undiscovered object
        //if we are touching the undiscovered object then the onTriggerEnter2D function in the base bread class will set our current_Behavior to "is touching object of interest"
        //also if this finds an undiscovered object we dont want to run the rest of this function so just return after seting our current target.
        //we will also add a function in update for when we run into an object of interest that has not yet been discovered that will save our path GameObjects in the 
        //game manager singleton before having our current behavior be to "follow return path" which will proc another if inside of update that will look at the last object
        //we were targeting or something and look for its path object list and seek them out in reverse, idk how yet

        GameObject[] Squares_To_Explore = GameObject.FindGameObjectsWithTag("Fog_Of_War");
        List<GameObject> Fog_Tiles = new List<GameObject>(0);

        foreach(GameObject Fog_Tile in Squares_To_Explore)
        {
            Fog_Tiles.Add(Fog_Tile);
        }

        Current_Target = Place_Next_Seek_Point_Towards_Target(Fog_Tiles[1]);
    }

    public void Optimize_Path()
    {
        Current_Behavior = "idle"; // for now, will add actual optimization stuff later

    }

    public GameObject Place_Next_Seek_Point_Towards_Target(GameObject Target_Fog_Tile, float Step_Distance = 1, float Step_Rotation_Variance = 1) 
    {

        Vector2 Target_Directional_Vector = (gameObject.transform.position - Target_Fog_Tile.transform.position).normalized;

        // Calculate where to put the object
        Vector2 Next_Seek_Point_Position = (Vector2) gameObject.transform.position + (Target_Directional_Vector * Step_Distance);

        GameObject Next_Seek_Point_Ref = Instantiate(Seek_Point_Prefab, Next_Seek_Point_Position, gameObject.transform.rotation);

        Current_Path_Seek_Point_Storage.Add(Next_Seek_Point_Ref);

        return Next_Seek_Point_Ref;

    }



}
