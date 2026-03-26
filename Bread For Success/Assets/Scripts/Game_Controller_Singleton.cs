using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Game_Controller_Singleton : MonoBehaviour
{

    //used for testing and keeping track of global variables such as enemy ID's and Xp and everything else not tied to an object


    // GameObject references
    public GameObject Camp_Prefab;
    public GameObject Enemy_Prefab;
    public int Seek_Empty_Gameobject_ID;
    public int Number_Of_Paths;
    // GameObject lists
    public List<Path_Class> Bread_Paths = new List<Path_Class>(0);

    public List<GameObject> Discovered_Active_Map_Objects; // all map objects that the breads know about and have a path to



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
