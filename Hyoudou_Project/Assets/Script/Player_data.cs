using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_data : MonoBehaviour
{
    int ID = 0;
    int MovePoint = 5;

    string TYPE = "";

    public Player_data(int id, string type)
    {
        ID = id;
        TYPE = type;
        GameSceneDirector.Players.Add(this);

    }
}
