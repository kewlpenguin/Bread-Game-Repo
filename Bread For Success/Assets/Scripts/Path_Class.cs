using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
public class Path_Class : MonoBehaviour
{
    public GameObject Target_GameObject;
    public List<GameObject> Path_Points = new List<GameObject>(0);
    public float Path_Length;
    public float Average_Damage_Taken; // IDK if will use this yet
    public int Number_Of_Points; 

}
