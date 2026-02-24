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
    public float Health;
    public float Mass;
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




    // enemy Components
    private Rigidbody2D My_Rigidbody;
    public GameObject Current_Target = null;

    //Enemy Conditionals
    public bool Is_Inside_Patrol_Radius;
    public bool Dead;

    // time variables 
    public float checkInterval;
    private float Next_Enemy_In_Sight_Check_CheckTime = 0f;
    private float Next_Aggro_CheckTime = 0f;
    private float Last_Enemy_Sighted_Time = 0f;
    public float Aggro_Time;





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assign_Objects_And_Components();
        Enemy_Is_Aggressive = true; //TEMPORARY REMOVE LATER, BEHAVIOR WILL BE SET THROUGH INSTANTIATION

    }

    // Update is called once per frame
    void Update()
    {
        if (!Dead)
        {
            if (Health <= 0)
            {
                Die(1, null, null);
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

    public virtual void Enemy_Take_Hit(GameObject Source_Of_Attack, float Attack_Damage, float Attack_Knockback, int Status_Effect_Num_Applied, int Second_Status_Effect_Num_Applied, GameObject AOE_To_Instantiate) 
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
        Destroy(gameObject, Death_Animation_Length);
    }




}
