using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Oven_Manager : MonoBehaviour
{
    public float Flour_Inventory = 0;
    public float Health = 100;











    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Oven_Take_Hit(GameObject Source_Of_Attack, float Attack_Damage, GameObject AOE_To_Instantiate)
    {
        Health -= Attack_Damage;


    }







}
