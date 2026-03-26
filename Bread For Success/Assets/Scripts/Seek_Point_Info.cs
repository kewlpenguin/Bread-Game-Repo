using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
public class Seek_Point_Info : MonoBehaviour
{
    public int Point_ID;
    public GameObject Object_Of_Origin; // the bread that created it 
    public GameObject Game_Controller;
    public GameObject Object_We_Are_Leading_To; // the bread that created it 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    private void Awake()
    {
        Game_Controller = GameObject.Find("Game_Controller (and Memory)");

        Point_ID = Game_Controller.GetComponent<Game_Controller_Singleton>().Seek_Empty_Gameobject_ID;
    }




    public void Insert_Point_Data(GameObject Parent, GameObject Destination = null)
    {
        Object_Of_Origin = Parent;
        Object_We_Are_Leading_To = Destination;
    }
}
