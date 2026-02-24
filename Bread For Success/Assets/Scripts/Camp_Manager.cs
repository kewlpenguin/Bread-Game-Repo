using UnityEngine;

public class Camp_Manager : MonoBehaviour
{
    public float Health = 100;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0) { gameObject.SetActive(false); }
    }


    public void Camp_Take_Hit(GameObject Source_Of_Attack, float Attack_Damage, GameObject AOE_To_Instantiate)
    {
        Health -= Attack_Damage;


    }



}
