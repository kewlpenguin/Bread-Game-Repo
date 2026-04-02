using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
public class Seek_Point_Info : MonoBehaviour
{
    public int Point_ID;
    public GameObject Game_Controller;


   // public GameObject Object_We_Are_Leading_To; // the bread that created it 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GetCreator() // returns the creator of this seek point
    {
        return Seek_Point_Registry.GetCreator(gameObject);
    }

    private void OnDestroy()
    {
        // Clean up the registry when this seek point is destroyed
        Seek_Point_Registry.Unregister(gameObject);
    }

    private void Awake()
    {
        Game_Controller = GameObject.Find("Game_Controller (and Memory)");

        Point_ID = Game_Controller.GetComponent<Game_Controller_Singleton>().Seek_Empty_Gameobject_ID;
    }

    public void Change_Path_Point_Color(Color newColor)
    {
        // Get all SpriteRenderers in children (active or inactive)
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);

        foreach (SpriteRenderer renderer in childRenderers)
        {
            renderer.color = newColor;
        }
    }


}
