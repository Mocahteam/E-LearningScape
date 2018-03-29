using UnityEngine;
using System.Collections.Generic;

public class Lock : MonoBehaviour
{
    public int id;                  //id of the lock, needs to be the same as the key to be opened
    public List<GameObject> disappearOnOpen;
    public List<GameObject> appearOnOpen;
}