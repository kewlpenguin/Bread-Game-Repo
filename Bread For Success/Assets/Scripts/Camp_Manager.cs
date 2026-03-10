using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Camp_Manager : MonoBehaviour
{
    public float Health = 100;
    public List<GameObject> Enemies_To_Instantiate = new List<GameObject>(0);

    // for initialization
    public GameObject Enemy_To_Spawn_1 = null;
    public GameObject Enemy_To_Spawn_2 = null;
    public GameObject Enemy_To_Spawn_3 = null;

    public int Amount_Enemy_1 = 0;
    public int Amount_Enemy_2 = 0;
    public int Amount_Enemy_3 = 0;

    public float Leash_Distance = 0;

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0) { gameObject.SetActive(false); }
    }

    private void Awake()
    {
        Initialize_Camp_Values(Health, Leash_Distance, Enemy_To_Spawn_1, Amount_Enemy_1, Enemy_To_Spawn_2, Amount_Enemy_2, Enemy_To_Spawn_3, Amount_Enemy_3);
    }


    public void Camp_Take_Hit(GameObject Source_Of_Attack, float Attack_Damage, GameObject AOE_To_Instantiate)
    {
        Health -= Attack_Damage;
    }


    //ts will be called after we instantiate the camp using the camps object reference 
    public void Initialize_Camp_Values(float Camp_Health, float Leash_Dist = 0,  GameObject Enemy_1 = null, int Ammount_1 = 0, GameObject Enemy_2 = null, int Ammount_2 = 0, GameObject Enemy_3 = null, int Ammount_3 = 0) // a camp can have up to 3 different enemies
    {
        Health = Camp_Health;

        if(Ammount_1 > 0)
        {
            for (int i = 0; i < Ammount_1; i++)
            {
                Enemies_To_Instantiate.Add(Enemy_1);
            }
        }

        if (Ammount_2 > 0)
        {
            for (int o = 0; o < Ammount_1; o++)
            {
                Enemies_To_Instantiate.Add(Enemy_2);
            }
        }

        if (Ammount_3 > 0)
        {
            for (int p = 0; p < Ammount_3; p++)
            {
                Enemies_To_Instantiate.Add(Enemy_3);
            }
        }

        for(int l = 0; l < Enemies_To_Instantiate.Count; l++)
        {
           GameObject Enemy_Just_Spawned =  Instantiate(Enemies_To_Instantiate[l],gameObject.transform.position,gameObject.transform.rotation);

            Enemy_Just_Spawned.GetComponent<Base_Enemy_Behavior>().Home_Camp = gameObject;
            Enemy_Just_Spawned.GetComponent<Base_Enemy_Behavior>().Camp_Leash_Distance = Leash_Dist;
        }

    }





}
