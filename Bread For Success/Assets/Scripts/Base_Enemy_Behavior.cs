using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Base_Enemy_Behavior : MonoBehaviour
{
    // enemy personality conditionals
    public bool Enemy_Is_Aggressive = false;
    public bool Enemy_Is_Neutral = false;
    public bool Enemy_Is_Passive = false;
    public bool Enemy_Is_Raider = false;
    public bool Enemy_Is_Camp_Mob = false;
    public bool Enemy_Is_Wandering_Mob = false;

    //enemy base stats
    public float Health = 1;
    public float Max_Speed;
    public float Acceleration;
    public float Sight_Range;

    //enemy Combat Stats
    public float Damage;
    public float Attack_Frequency;
    private float Next_Attack_Time = 0;
    private float Next_AttackCheck_Time = 0;
    public float Attack_Range;
    public float Knockback;
    public float Death_Animation_Time;



    // enemy Components
    private Rigidbody2D My_Rigidbody;
    public GameObject Current_Target = null;
    public GameObject Home_Camp = null;
    public GameObject Seek_Point_Prefab;
    public GameObject Current_Seek_Point;
    private GameObject Game_Manager;
    public float Camp_Leash_Distance;
    public float In_Camp_Offset;
    private List<GameObject> All_Previous_seek_Points = new List<GameObject>(0) ;


    //Enemy Conditionals
    public bool Is_Inside_Patrol_Radius;
    public bool Dead;
    public bool Is_Targeting_Bread_Or_Oven;

    // time variables 
    public float checkInterval;
    private float Next_Aggro_CheckTime = 0f;
    private float Last_Enemy_Sighted_Time = 0f;
    public float Aggro_Time;





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assign_Objects_And_Components();

    }

    // Update is called once per frame
    void Update()
    {
        if (!Dead)
        {
            Base_Enemy_Function_Calls();
        }

    }

    private void FixedUpdate()
    {
        if (!Dead)
        {
            if (Current_Target != null)
            {
                Move_Enemy_Towards_Target();
            }
        }
    }
        

    private void Base_Enemy_Function_Calls()
    {
        if (Health <= 0)
        {
            Die(Death_Animation_Time, null, null);
        }

        if (Time.time >= Last_Enemy_Sighted_Time && Current_Target == null)
        {
            Check_For_Breads_In_Sight();
            Last_Enemy_Sighted_Time = Time.time + checkInterval;
        }


        if (Time.time >= Next_Aggro_CheckTime && Current_Target != null) // once we are locked on, delay the checks by the aggro time instead
        {
            Check_For_Breads_In_Sight();
            Next_Aggro_CheckTime = Time.time + Aggro_Time;
            // Debug.Log("Aggro_Check");
        }

        if (Time.time >= Next_Attack_Time && Time.time >= Next_AttackCheck_Time && Current_Target != null)
        {
            if (Current_Target.CompareTag("Breads") || Current_Target.CompareTag("Oven"))
            {
                //Debug.Log("Attempt_Attack");
                Attack_Bread_Or_Oven();
                Next_AttackCheck_Time = Time.time + .2f; // so we can only attack check 5 times per second, if we somehow end up with attacking more than 5 times per second lower this
            }
        }

        if (Current_Target != null && Current_Target.CompareTag("Breads"))
        {
            Is_Targeting_Bread_Or_Oven = true;
        }

        else if(Current_Target == null || !(Current_Target.CompareTag("Breads")))
        {
            Is_Targeting_Bread_Or_Oven = false;
        }


        if (Home_Camp != null && Home_Camp.activeInHierarchy && !Is_Targeting_Bread_Or_Oven) //if camp is destroyed then become normal wandering enemy
        {
            Camp_Leash_Behavior();
        }

    }


    protected virtual void Move_Enemy_Towards_Target() // if we want different movement overwrite this function
    {
        Vector2 direction = (Current_Target.transform.position - transform.position).normalized;

        if (Mathf.Abs(My_Rigidbody.linearVelocity.x) < Max_Speed && Mathf.Abs(My_Rigidbody.linearVelocity.y) < Max_Speed)
        {
            My_Rigidbody.AddForce(direction * Acceleration, ForceMode2D.Force);
        }

    }


    private void Assign_Objects_And_Components()
    {
        My_Rigidbody = gameObject.GetComponent<Rigidbody2D>();
        Game_Manager = GameObject.Find("Game_Controller (and Memory)");
    }


    protected void Check_For_Breads_In_Sight() // also looks for the oven
    {
        Collider2D[] NearbyBreads = Physics2D.OverlapCircleAll(transform.position, Sight_Range, (1 << 6) | (1 << 9)); // need to limit this to only the breads layer somehow

        // Then loop through what you found
        foreach (Collider2D obj in NearbyBreads)
        {
            if (obj.gameObject.CompareTag("Breads") || obj.gameObject.CompareTag("Oven"))
            {
               // Debug.Log("Found: " + obj.gameObject.name);
                Current_Target = obj.gameObject;
                Last_Enemy_Sighted_Time = Time.time;
                return;
            }
        }


        Current_Target = null; // will only run if above loop never hits
    }


    private void Camp_Leash_Behavior()
    {
            if (Current_Target == null && Current_Seek_Point == null)
            {
                Current_Target = Home_Camp;
            }

            else if (!(Current_Seek_Point == null))
            {
                Current_Target = Current_Seek_Point;
            }


            if (Is_Inside_Patrol_Radius && Current_Seek_Point == null) // current seek point will be null once we walk into and destroy is
            {

                // Get random angle
                float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

                // Get direction (these are just instructions!)
                Vector2 direction = new Vector2(
                    Mathf.Cos(randomAngle),  // Tells us how much to move horizontally
                    Mathf.Sin(randomAngle)   // Tells us how much to move vertically
                );

                // Calculate where to put the object
                Vector2 worldPosition = (Vector2)gameObject.transform.position + (direction * Camp_Leash_Distance);

                GameObject Patrol_Seek_Point = Instantiate(Seek_Point_Prefab, worldPosition, gameObject.transform.rotation);

            All_Previous_seek_Points.Add(Patrol_Seek_Point);

                Patrol_Seek_Point.GetComponent<Seek_Point_Info>().Point_ID = Game_Manager.GetComponent<Game_Controller_Singleton>().Seek_Empty_Gameobject_ID;

                Game_Manager.GetComponent<Game_Controller_Singleton>().Seek_Empty_Gameobject_ID++; //increment this to the next number so any future points created will have a unique ID

                Patrol_Seek_Point.name = "Seek Point Number: " + Patrol_Seek_Point.GetComponent<Seek_Point_Info>().Point_ID;

                Current_Seek_Point = Patrol_Seek_Point;
                Current_Target = Patrol_Seek_Point;
            }


            else if (!Is_Inside_Patrol_Radius)
            {
                Current_Target = Home_Camp;
            }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == Home_Camp)  //use is inside patrol radius as a bool that flips upon touching the camp or patrol_seek_point that is targeted
        {
            Is_Inside_Patrol_Radius = true;
        }
       
        if (Current_Seek_Point != null && collision.gameObject == Current_Seek_Point)
        {
            Is_Inside_Patrol_Radius = false;
            Destroy(Current_Seek_Point);
        }
    }



    #region Combat_Functions
    protected virtual void Attack_Bread_Or_Oven() // can be overwritten because yea, basic enemies should only be able to attack one bread at a time tho so just attack the current target
    {
        Collider2D[] Breads_In_Attack_Range_Now = Physics2D.OverlapCircleAll(transform.position, Attack_Range, (1 << 6) | (1 << 9)); // range check

        foreach (Collider2D Bread in Breads_In_Attack_Range_Now)
        {
            //Debug.Log("Foreach_Check ran " + Bread.gameObject.name + " In Range" );
            if (Bread.gameObject == Current_Target.gameObject && Current_Target.CompareTag("Breads")) // Target Check
            {
                //Debug.Log("Hit_Bread" + Current_Target.name);
                Current_Target.gameObject.GetComponent<Base_Bread_Class>().Bread_Take_Hit(gameObject, Damage, Knockback, 0, 0, null); // perform hit
                Next_Attack_Time = Time.time + Attack_Frequency;
                return;
            }
            else if (Bread.gameObject == Current_Target.gameObject && Current_Target.CompareTag("Oven")) // Target Check
            {
                //Debug.Log("Hit_Oven" + Current_Target.name);
                Current_Target.gameObject.GetComponent<Oven_Manager>().Oven_Take_Hit(gameObject, Damage, null); // perform hit
                Next_Attack_Time = Time.time + Attack_Frequency;
                return;
            }
        }

    }

    public virtual void Enemy_Take_Hit(GameObject Source_Of_Attack, float Attack_Damage, float Attack_Knockback, int Status_Effect_Num_Applied = 0, int Second_Status_Effect_Num_Applied = 0, GameObject AOE_To_Instantiate = null) 
    {
        // aggro if attacked
        if (!Enemy_Is_Passive) // if is a puss, dont fight back
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

        foreach(GameObject Seek_Point in All_Previous_seek_Points) // clean up last seek point before death or others if there are any
        {
            if(Seek_Point != null )
            {
                Destroy(Seek_Point);
            }
        }

        Destroy(gameObject, Death_Animation_Length);
    }

    #endregion


}
