using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
public class Gatherer_Bread_Sub : Base_Bread_Class
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assign_Components_And_GameObjects();
        Find_Map_Objects();
        Is_Idle = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Dead)
        {
            Base_Bread_Operation_Function_Calls(); // includes die check, current target check, depositing check, collecting check, and retreat threshold check
            Collector_Behavior();
        }
    }

    private void FixedUpdate()
    {
        if (!Dead && Current_Target != null && Current_Target.activeInHierarchy)
        {
            Move_Towards_Target();
        }

    }

    private void Collector_Behavior()
    {
        if (Is_Idle)
        {
            if(inventory_Space == 0)
            {
                Current_Target = Oven;
                Is_Idle = false;
            }
            
            else if (inventory_Space > 0)
            {
                Current_Target = Find_Next_Target(All_Map_Object_Lists[4]); // temp, will be random or contolled by traits later
                if (Current_Target.activeInHierarchy)
                {
                    Is_Idle = false;
                }

                else if (!(Current_Target.activeInHierarchy)) // shit to to so we are not idle
                {
                    Current_Target = Oven;
                    Is_Idle = false;
                }
            }
        }

    }




}
