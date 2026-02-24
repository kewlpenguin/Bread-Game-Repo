using UnityEngine;

public class Recource_Management_Instance : MonoBehaviour
{

    public float inventory = 1;//initialize at 1 so we dont immediately destroy it



    void Update()
    {
        if (inventory <= 0) { gameObject.SetActive(false); } // when we run out, just dissapear
    }

    private void Awake()
    {
        Set_Inventory();
    }

    private void Set_Inventory()
    {
        string temp = gameObject.tag;

        switch(temp)
        {
            case "Flour_Pile":
                inventory = 100;
                    break;

            case "Wheat_Patch":
                inventory = 10;
                break;
        }
    }








}
