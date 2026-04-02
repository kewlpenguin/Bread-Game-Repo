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

        if (Dead)
        {
            foreach (GameObject Seek_Point in Current_Path_Seek_Point_Storage)
            {
                Destroy(Seek_Point, 1);
            }
        }
    }

    private void Explore_Behaviors()
    {
        if (Current_Behavior == "idle")
        {
            if (Game_Controller_Singleton.GetComponent<Game_Controller_Singleton>().Discovered_Active_Map_Objects == null)
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

        if (Current_Behavior == "waiting for next seek point")
        {
            Search_For_Locations_Of_Interest();
        }

        if (Current_Behavior == "found new map object following path back to oven") // at this point we are still targeting the object of interest so we can iterate through all stored bread paths checking the target object 
        {
            foreach (Path_Class Path in Game_Controller_Singleton.GetComponent<Game_Controller_Singleton>().Bread_Paths)
            {
                if (Path.Target_GameObject == Current_Target) // look for the stored path that we just created based on our current target
                {
                    Path_Following = Path;
                    Current_Target = Path.Path_Points[Path.Path_Points.Count - 1];

                    Current_Behavior = "going to next path point back";
                }
            }
        }

        if (Current_Behavior == "following path back to oven") // at this point we are still targeting the object of interest so we can iterate through all stored bread paths checking the target object 
        {
            Current_Target = Current_Path_Seek_Point_Storage[Current_Path_Seek_Point_Storage.Count - 1];
            Current_Behavior = "going to next path point back";
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

        if (Nearby_Locations_Of_Interest != null)
        {
            foreach (Collider2D Location_Nearby in Nearby_Locations_Of_Interest)
            {
                if (!Game_Controller_Singleton.GetComponent<Game_Controller_Singleton>().Discovered_Active_Map_Objects.Contains(Location_Nearby.gameObject)) // if we do not have the location in the known locations list then add it and save the route
                {
                    Game_Controller_Singleton.GetComponent<Game_Controller_Singleton>().Discovered_Active_Map_Objects.Add(Location_Nearby.gameObject);

                    List<GameObject> Temp_Path_Storage = new List<GameObject>(0);

                    Current_Target = Place_Next_Seek_Point_Towards_Target(Location_Nearby.gameObject, true);

                    foreach (GameObject Seek_Point_To_Reach_Destination in Current_Path_Seek_Point_Storage) // assign all stored stepping stone pathing targets as a temp path
                    {
                        Temp_Path_Storage.Add(Seek_Point_To_Reach_Destination);
                    }

                    Path_Class Path_To_Store = new Path_Class(); // create the path class aka the storage mechanism to put the path into game_controller

                    Path_To_Store.Target_GameObject = Location_Nearby.gameObject;
                    Path_To_Store.Number_Of_Points = Temp_Path_Storage.Count;
                    Path_To_Store.Path_Points = Temp_Path_Storage;

                    Vector2 Previous_Point_Location = new Vector2();
                    float Running_Total = 0;

                    foreach (GameObject Path_Point in Temp_Path_Storage)
                    {
                        if (Previous_Point_Location != null) // if we have a recorded previous point then use the distance formula to calc distance between it and the current point then
                                                             // save the current point as the previous point and so on
                        {
                            Running_Total += Mathf.Sqrt(Mathf.Pow((Path_Point.transform.position.x - Previous_Point_Location.x), 2) + Mathf.Pow((Path_Point.transform.position.y - Previous_Point_Location.y), 2));
                            Previous_Point_Location = Path_Point.transform.position;
                        }
                        else // if we are looking at the first point we have no other point to compare it to so just record it 
                        {
                            Previous_Point_Location = Path_Point.transform.position;
                        }

                    }

                    Path_To_Store.Path_Length = Running_Total; // assign the calculated total distance of the path

                    Path_To_Store.Average_Damage_Taken = Max_Health - Health;

                    Game_Controller_Singleton.GetComponent<Game_Controller_Singleton>().Bread_Paths.Add(Path_To_Store); // actually store the path in game controllers memory
                    Game_Controller_Singleton.GetComponent<Game_Controller_Singleton>().Number_Of_Paths++;

                    foreach(GameObject Path_Point in Current_Path_Seek_Point_Storage)
                    {
                        switch (Game_Controller_Singleton.GetComponent<Game_Controller_Singleton>().Number_Of_Paths)
                        {
                            case 0:
                                Path_Point.GetComponent<Seek_Point_Info>().Change_Path_Point_Color(Color.beige);
                                break;

                            case 1:
                                Path_Point.GetComponent<Seek_Point_Info>().Change_Path_Point_Color(Color.azure);
                                break;

                            case 2:
                                Path_Point.GetComponent<Seek_Point_Info>().Change_Path_Point_Color(Color.chocolate);
                                break;

                            case 3:
                                Path_Point.GetComponent<Seek_Point_Info>().Change_Path_Point_Color(Color.limeGreen);
                                break;

                            case 4:
                                Path_Point.GetComponent<Seek_Point_Info>().Change_Path_Point_Color(Color.magenta);
                                break;

                            case 5:
                                Path_Point.GetComponent<Seek_Point_Info>().Change_Path_Point_Color(Color.softRed);
                                break;

                            case 6:
                                Path_Point.GetComponent<Seek_Point_Info>().Change_Path_Point_Color(Color.tan);
                                break;

                            case 7:
                                Path_Point.GetComponent<Seek_Point_Info>().Change_Path_Point_Color(Color.softYellow);
                                break;

                            case 8:
                                Path_Point.GetComponent<Seek_Point_Info>().Change_Path_Point_Color(Color.peachPuff);
                                break;

                        }

                    }

                    Current_Path_Seek_Point_Storage = new List<GameObject>(0); // after we assign the path to permanent memory, delete our temporary memory and start over


                    Current_Behavior = "found new map object following path back to oven";

                    return;
                }

                else if (Game_Controller_Singleton.GetComponent<Game_Controller_Singleton>().Discovered_Active_Map_Objects.Contains(Location_Nearby.gameObject))
                {
                    Current_Target = Place_Next_Seek_Point_Towards_Target(Location_Nearby.gameObject, false);

                }
            }
        }

        // if we get through all the loops above then either we did not find any nearby objects of interest or we did but it has already been discovered

        //need to check if object in sight has been discovered yet or not, if it has not then we need to placenextseekpointtowardstargetand feed it the reference to the undiscovered object
        //if we are touching the undiscovered object then the onTriggerEnter2D function in the base bread class will set our current_Behavior to "is touching object of interest"
        //also if this finds an undiscovered object we dont want to run the rest of this function so just return after seting our current target.
        //we will also add a function in update for when we run into an object of interest that has not yet been discovered that will save our path GameObjects in the 
        //game manager singleton before having our current behavior be to "follow return path" which will proc another if inside of update that will look at the last object
        //we were targeting or something and look for its path object list and seek them out in reverse, idk how yet

        if (Fog_Tile_Target == null || !Fog_Tile_Target.activeInHierarchy)
        {
            GameObject[] Squares_To_Explore = GameObject.FindGameObjectsWithTag("Fog_Of_War"); //find all fog tiles
            List<GameObject> Fog_Tiles = new List<GameObject>(0);

            foreach (GameObject Fog_Tile in Squares_To_Explore) // for each we will add to a list
            {
                Fog_Tiles.Add(Fog_Tile);
            }

            GameObject Closest_Fog_Tile = Find_Next_Target(Fog_Tiles);

            Current_Target = Place_Next_Seek_Point_Towards_Target(Closest_Fog_Tile, false, Step_Distance, Step_Rotation_Variance); // the first tile in the list we will seek point towards until we reach it then we will target another one 
                                                                                                                                   //will need some kind of distance check in the future to try to go to the closest one
            Fog_Tile_Target = Fog_Tiles[1];
        }
        else
        {
            Current_Target = Place_Next_Seek_Point_Towards_Target(Fog_Tile_Target, false, Step_Distance, Step_Rotation_Variance);
        }

    }

    public void Optimize_Path()
    {
        Current_Behavior = "idle"; // for now, will add actual optimization stuff later

    }


    public GameObject Place_Next_Seek_Point_Towards_Target(GameObject Target, bool Is_In_Range_Of_Object_Of_Interest = false, float Step_Distance = 10, float Step_Rotation_Variance = 5)
    {
        if (Is_In_Range_Of_Object_Of_Interest)
        {
            Current_Path_Seek_Point_Storage.Add(Target); // the last stop on the path will be the object we found
            return Target;
        }

        Vector2 Target_Directional_Vector = (Target.transform.position - gameObject.transform.position).normalized;

        // Calculate where to put the object
        Vector2 Next_Seek_Point_Position = (Vector2)gameObject.transform.position + (Target_Directional_Vector * Step_Distance);

        GameObject Next_Seek_Point_Ref = Instantiate(Seek_Point_Prefab, Next_Seek_Point_Position, gameObject.transform.rotation);
        
        Seek_Point_Registry.Register(Next_Seek_Point_Ref, gameObject);


        if(Current_Path_Seek_Point_Storage.Count == 0)
        {
            Seek_Point_Registry.Register_Previous_Seek_Point(Oven, Next_Seek_Point_Ref); // if this is the first point then make the previous of this the oven
        }

        else if(Current_Path_Seek_Point_Storage.Count > 0)
        {
            Seek_Point_Registry.Register_Previous_Seek_Point(Current_Path_Seek_Point_Storage[Current_Path_Seek_Point_Storage.Count - 1], Next_Seek_Point_Ref);
        }
        // the plan here is to feed the most recent point into the next point as the previous point


        Seek_Point_Registry.DebugLogAll();

        Game_Controller_Singleton.GetComponent<Game_Controller_Singleton>().Seek_Empty_Gameobject_ID++; // we do not need to give the new seek point its information because it has its own methods
                                                                                                        //for this inside seek_Point_Info

        Current_Path_Seek_Point_Storage.Add(Next_Seek_Point_Ref);

        return Next_Seek_Point_Ref;
    }



}
