using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
public class Explorer_Bread_Sub : Base_Bread_Class
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assign_Components_And_GameObjects();
        Find_Map_Objects();
        Is_Idle = true;
        Is_Explorer = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Dead)
        {
            Base_Bread_Operation_Function_Calls(); // includes die check, current target check, depositing check, collecting check, and retreat threshold check. is idle will be set to true if our current target is not active
            Support_Behavior();
          
            if (Current_Target != null && Is_Targeting_Enemy && (Time.time - Last_Enemy_Check_Time) > Enemy_In_Sight_Range_Check_Delay) // if we are not targeting an enemy, check if there is one in range then target it
            {
                Last_Enemy_Check_Time = Time.time;
                Check_For_Enemies_In_Sight();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!Dead && Current_Target != null && Current_Target.activeInHierarchy)
        {
            Move_Towards_Target();
        }

    }

    private void Support_Behavior()
    {
        if (Is_Idle)
        {
         
        }
    }



}
