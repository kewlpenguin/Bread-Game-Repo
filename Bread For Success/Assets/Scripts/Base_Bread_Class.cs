using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;



public class Base_Bread_Class : MonoBehaviour
{
    // stats will be overwritten upon bread instantiation, not all breads will use all base class variables but all base class variables will be used

    //whole bejhavior system centered arround the breads current target. the bread will approach their target until trigger enter or other condition is met, upon which the conditional
    //for the corresponding behavior will flip allowing the function call inside update to run until a contition is met or we get a new target


    //GameObject References
    protected GameObject Oven;
    protected GameObject Patrol_Point_Group_0_Parent;
    protected GameObject Game_Controller_Singleton;

    //Components
    protected Rigidbody2D My_Rigidbody;


    //Pathfinding stats
    public float Sight_Range;
    public int Patrol_Type;
    public int Patrol_Speed;
    public float Step_Distance;
    public float Step_Rotation_Variance;
    public GameObject Fog_Tile_Target;
    public Path_Class Path_Following = null;
    public int Current_Path_Point_Number;
    public int Next_Path_Point_Number;

    //Pathfinding Variables
    public GameObject Current_Target; // make sure to reset when idle because some functions rely on this being null, the basis of all bread control
    public GameObject Seek_Point_Prefab;

    //speed stats
    public float Acceleration;
    public float Max_Speed;  // speed cap via linear velocity
    public float Retreat_Speed_Mult;
    public float Lunge_Force;

    //Gatherer Stats
    public float Ingrediant_Gather_Speed_Mult;
    public float Ingrediant_Gather_Speed;
    public float Ingrediant_Gather_Quantity;
    public float Material_Gather_Speed;
    public float Material_Gather_Quantity;
    GameObject Recource_Reference;


    //inventory stats
    public float Max_inventory_Space; // for like 2 checks
    public float inventory_Space; // works like a ballance, you have a start amount and you spend it picking things up while gaining it depositing. cannot go below 0
    public float Flour_In_Inventory;
    protected float Collection_Progress;
    public float Deposit_Speed;
    public float Deposit_Quantity;


    //Combat Stats
    public float Health = 1; //so dont immediately die
    public float Max_Health = 1;
    public float Damage;
    public float Knockback;
    public float Attack_Speed;
    protected float Next_Attack_Time = 0;
    protected float Next_AttackCheck_Time = 0; // so we dont spam the circle range check too much
    public float Attack_Range;
    protected float Enemy_In_Sight_Range_Check_Delay = .5f;
    protected float Last_Enemy_Check_Time = 0;
    public float Death_Anim_Length;


    //GameObjects Arrays For all Map Objects and enemies
    public List<GameObject> Map_Objects; // all objects on map
    public List<GameObject> Discovered_Active_Map_Objects; // all map objects that the breads know about and have a path to


    // Conditionals
    public bool Is_Touching_Flour_Pile;
    public bool Collect_On_Cooldown;
    public bool Deposit_On_Cooldown;
    public bool Is_Path_Optimizing;
    public bool Is_A_Puss;
    public bool Enemy_In_Sight;
    public bool Dead;
    public bool Looking_For_Enemies_In_Sight;
    public bool Is_Explorer;
    public bool Is_Attacker;
    public bool Is_Gatherer;
    public bool Is_Home;
    public string Current_Behavior;

    void Start()
    {
        Assign_Components_And_GameObjects();
        Find_Map_Objects();

    }


    #region Universal_Functions
    protected void Assign_Components_And_GameObjects()
    {
        My_Rigidbody = gameObject.GetComponent<Rigidbody2D>();
        Oven = GameObject.Find("The_Oven");
        Game_Controller_Singleton = GameObject.Find("Game_Controller (and Memory)");
    }


    protected void Find_Map_Objects()// needs to be much more robust, will change in the near future 
    {

    }


    // will be added to all bread sub types
    void Update()
    {
        if (!Dead)
        {
            Base_Bread_Operation_Function_Calls();
        }
    }

    //controls what sets our bread to idle, basic condition checks like isdead?, attack cooldowns and attack calls, depositing on trigger enter, collecting on trigger enter, and retreat conditional check set

    protected virtual void Base_Bread_Operation_Function_Calls()
    {
        if (Current_Target == null || !(Current_Target.activeInHierarchy))
        {
            Current_Behavior = "idle";
        }

        if (Current_Target != null && Current_Target.CompareTag("Enemy")) 
        {
            Current_Behavior = "targeting enemy";
        }
       

        if (Current_Target != null && Current_Target.CompareTag("Camp_(Small)"))
        {
            Current_Behavior = "targeting camp";
        }
        

        if (Health <= 0)
        {
            Die(Death_Anim_Length, null, null);
        }


        if (Time.time >= Next_Attack_Time  && Current_Behavior == "targeting enemy") // if our target is an enemy then we start trying to attack it
        {
            Attack_Enemy();
        }

        if (Time.time >= Next_Attack_Time && Current_Behavior == "targeting camp") // if our target is an camp then we start trying to attack it
        {
            Attack_Camp();
        }

        if (Current_Behavior == "depositing")
        {
            Deposit_Inventory(); // will be moved elsewhere
        }

        if (Current_Behavior == "collecting")  //this allows the collection coroutine to run, later collect_Ingredients checks if touching X pile so we do not need an additional safeguard
        {
            Collect_Indgredients();
        }

        if ((Health / Max_Health) <= .25f && Current_Behavior != "retreating") // will need other checks to make sure bread is healed b4 leaving oven
        {
            Current_Behavior = "retreating";
        }

        if (Current_Behavior == "retreating")
        {
            Retreat_Behavior(); //need to add later
        }


    }


    // checks in a circle size attack range for our current targhet, then attacks them if attacks them, only call-able if our target is an enemy
    protected virtual void Attack_Enemy()
    {
        Collider2D[] Enemies_In_Attack_Range_Now = Physics2D.OverlapCircleAll(transform.position, Attack_Range, (1 << 3)); // range check

        foreach (Collider2D Enemy in Enemies_In_Attack_Range_Now)
        {
            if (Enemy.gameObject == Current_Target) // Target Check
            {
                Vector2 Normalized_Vect = (Current_Target.transform.position - gameObject.transform.position).normalized;
                My_Rigidbody.AddForce(Normalized_Vect * Lunge_Force, ForceMode2D.Impulse);

                Current_Target.gameObject.GetComponent<Base_Enemy_Behavior>().Enemy_Take_Hit(gameObject, Damage, Knockback, 0, 0, null); // perform hit
                Next_Attack_Time = Time.time + Attack_Speed;
                return;
            }
        }
    }


    // checks in a circle size attack range for our current targhet, then attacks them if attacks them, only call-able if our target is an camp
    protected virtual void Attack_Camp()
    {
        Collider2D[] Camp_In_Attack_Range_Now = Physics2D.OverlapCircleAll(transform.position, Attack_Range, (1 << 10)); // range check

        foreach (Collider2D Camp in Camp_In_Attack_Range_Now)
        {
            if (Camp.gameObject == Current_Target) // Target Check
            {
                Vector2 Normalized_Vect = (Current_Target.transform.position - gameObject.transform.position).normalized;
                My_Rigidbody.AddForce(Normalized_Vect * Lunge_Force, ForceMode2D.Impulse);

                Current_Target.gameObject.GetComponent<Camp_Manager>().Camp_Take_Hit(gameObject, Damage, null); // perform hit
                                                                                                                Debug.Log("Hit_Enemy" + Current_Target.name);
                Next_Attack_Time = Time.time + Attack_Speed;
                return;
            }
        }
    }


    protected virtual void Retreat_Behavior()
    {

    }

    // will be added to all breads, moves the bread towards the current target which could be anything, assuming it exists and we are not dead
    private void FixedUpdate()
    {
        if (!Dead && Current_Target != null)
        {
            Move_Towards_Target();
        }
    }


    //general movement, bread will go to the target then depending on what it comes into cantact with it will flip a conditional that will run a coroutine until the conditional is false or another condition is met
    protected virtual void Move_Towards_Target()
    {
        if (Mathf.Abs(My_Rigidbody.linearVelocity.x) < Max_Speed && Mathf.Abs(My_Rigidbody.linearVelocity.y) < Max_Speed && Current_Behavior != "retreating")
        {
            Vector2 Normalized_Direction = (Current_Target.transform.position - gameObject.transform.position).normalized;
            My_Rigidbody.AddForce(Normalized_Direction * Acceleration, ForceMode2D.Force);
        }

        else if (My_Rigidbody.linearVelocity.x < (Max_Speed * Retreat_Speed_Mult) && My_Rigidbody.linearVelocity.y < (Max_Speed * Retreat_Speed_Mult) && Current_Behavior == "retreating") //if we are retreating ise different formula to go faster
        {
            Vector2 Normalized_Direction = (Current_Target.transform.position - gameObject.transform.position).normalized;
            My_Rigidbody.AddForce(Normalized_Direction * Acceleration * Retreat_Speed_Mult, ForceMode2D.Force);
        }

    }


    // right now only used by collectors but eventually may be used by surrort and attackers, is called on trigger enter through the is_Depositing conditional
    protected void Deposit_Inventory()
    {
        if (!Deposit_On_Cooldown) // if we do not have our full inventory space, deposit
        {
            StartCoroutine(Deposit_Routine());
        }
    }


    protected virtual IEnumerator Deposit_Routine() // 
    {
        Deposit_On_Cooldown = true;
        yield return new WaitForSeconds(Deposit_Speed);

        for (int i = 0; i < Deposit_Quantity; i++)
        {
            if (inventory_Space < Max_inventory_Space)
            {
                if (Flour_In_Inventory > 0)
                {
                    Oven.GetComponent<Oven_Manager>().Flour_Inventory++;
                    inventory_Space++;
                    Flour_In_Inventory--;
                }
                // add else ifs for other materials and ingredients and shit
            }

            else if (inventory_Space == Max_inventory_Space) // we are done depositing
            {
                Current_Behavior = "idle";
                break;
            }
        }

        Deposit_On_Cooldown = false;
    }



    public virtual GameObject Find_Next_Target(List<GameObject> List_To_Search) // virtual because target to go to is not always the closest. but the base case will be the closest
    {
        GameObject Closest_Target = List_To_Search[0]; // to initialize

        float Distance_To_Potential_Target;
        float Shortest_Distance = 99999999999999; //default value, any dist will be shorter

        for (int i = 0; i < List_To_Search.Count; i++)
        {
            Vector2 Temp = gameObject.transform.position - List_To_Search[i].transform.position;

            Distance_To_Potential_Target = Mathf.Sqrt((Temp.x * Temp.x) + (Temp.y * Temp.y)); // calc straight line length 

            if (List_To_Search != null && Distance_To_Potential_Target < Shortest_Distance && List_To_Search[i].gameObject.activeInHierarchy)
            {
                Closest_Target = List_To_Search[i];
                Shortest_Distance = Distance_To_Potential_Target;
            }

        }

        return Closest_Target;

    }


    public virtual void Bread_Take_Hit(GameObject Source_Of_Attack, float Attack_Damage, float Attack_Knockback, int Status_Effect_Num_Applied, int Second_Status_Effect_Num_Applied, GameObject AOE_To_Instantiate) // when attacking other enemy, give myself as source of attacxk
    {
        if (!Is_A_Puss) // if is a puss, dont fight back
        {
            Current_Target = Source_Of_Attack;
        }

        //take damage
        Health -= Attack_Damage;

        //take Knockback
        Vector2 Knockback_Direction = (gameObject.transform.position - Source_Of_Attack.gameObject.transform.position).normalized;
        My_Rigidbody.AddForce(Knockback_Direction * Attack_Knockback, ForceMode2D.Impulse);

    }

    public virtual void Die(float Death_Animation_Length, GameObject Death_Animation, GameObject Post_Death_Effect)  // post death effect is like a suicide bomb or healing projectiles
    {
        Dead = true;
        Destroy(gameObject, Death_Animation_Length);
    }


    #endregion

    #region Collector_Functions
    //Collector Functions:
    // will be called after a short delay while idle. bread will collect until inventory gets full, it is nightime, or its health goes below the retreat threshold
    protected virtual void Collect_Indgredients()
    {
        if (Is_Touching_Flour_Pile && !Collect_On_Cooldown && inventory_Space > 0) // if we are collecting, touching the flour pile, and collect is not on cd
        {
            StartCoroutine(Flour_Collection(Ingrediant_Gather_Speed)); // incremenet collection bar
        }

        // add other recources here


        else if (inventory_Space == 0 || Current_Target == null ||!Current_Target.activeInHierarchy) // return to oven and begin depositing, this will be specified through idle behavior and condition checks in the subclass
        {
            Current_Behavior = "idle";
        }
    }


    protected IEnumerator Flour_Collection(float Collection_Time)
    {
        Collect_On_Cooldown = true;
        Recource_Reference = Current_Target;
        yield return new WaitForSeconds((Collection_Time / Ingrediant_Gather_Speed_Mult)); // progress will be split into 10 segments

        if (Collection_Progress < 10)
        {
            Collection_Progress++;
        }

        else if (Collection_Progress == 10) // when progress bar is full, pickup based on quantity pickup stat
        {
            Collection_Progress = 0;

            for (int i = 0; i < Ingrediant_Gather_Quantity; i++)
            {
                if (inventory_Space > 0 && Recource_Reference.activeInHierarchy && Recource_Reference.GetComponent<Recource_Management_Instance>().inventory > 0)
                {
                    Recource_Reference.GetComponent<Recource_Management_Instance>().inventory--;
                    inventory_Space--;
                    Flour_In_Inventory++;
                }

            }

        }

        Collect_On_Cooldown = false;
    }

    #endregion;

    #region Support_Functions
    protected virtual void Patrol_Oven(int My_Patrol_type, float Patrol_Speed) // needs reworked, just copy the enemy patrolling logic
    {
        Current_Behavior = "patrolling";

    }


    #endregion

    #region Attacker_Functions
    //also used for supports sometimes
    protected void Check_For_Enemies_In_Sight()
    {
        Collider2D[] NearbyEnemies = Physics2D.OverlapCircleAll(transform.position, Sight_Range, (1 << 3)); // layer and range check

        // Then loop through what you found
        foreach (Collider2D Collider in NearbyEnemies)
        {
            if (Collider.gameObject.CompareTag("Enemy"))
            {
                // Debug.Log("Found: " + obj.gameObject.name);
                if (!Is_Explorer)
                {
                    Current_Target = Collider.gameObject;
                }

                return;
            }
        }
    }

    #endregion

    #region OnTriggerEnters
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Oven"))
        {
            Is_Home = true;

            Deposit_On_Cooldown = false;

            if (inventory_Space < Max_inventory_Space)
            {
                Current_Behavior = "depositing";
            }
            if(Current_Behavior == "going to next path point back" && Is_Explorer)
            {
                Current_Behavior = "idle";
            }

        }

        else if (collision.CompareTag("Fog_Of_War"))
        {
            collision.gameObject.SetActive(false);
        }

        else if (collision.CompareTag("Seek_Point_Bread"))
        {
            if (Current_Behavior == "searching")
            {
                Current_Behavior = "waiting for next seek point";
            }   

            if (Current_Behavior == "going to next path point back")
            {
                if (Next_Path_Point_Number >= 0)
                {
                    Current_Target = Path_Following.Path_Points[Next_Path_Point_Number];
                    Current_Path_Point_Number--;
                    Next_Path_Point_Number--;
                }
                else { Current_Target = Oven; }
            }
        }

        else if (collision.CompareTag("Flour_Pile"))
        {
            if (Is_Gatherer)
            {
                Collect_On_Cooldown = false;
                Is_Touching_Flour_Pile = true;
                Current_Behavior = "is gathering";
            }
            if (Is_Explorer && Current_Behavior == "going to next path point back")
            {
                Current_Target = Path_Following.Path_Points[Next_Path_Point_Number];
                Current_Path_Point_Number--;
                Next_Path_Point_Number--;
            }
        }


    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Flour_Pile"))
        {
            Collect_On_Cooldown = false;
            Is_Touching_Flour_Pile = false;
        }
        else if (collision.CompareTag("Oven"))
        {
            Is_Home = false;
        }

    }

    #endregion






}
