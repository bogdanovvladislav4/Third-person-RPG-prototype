using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Weapon : ScriptableObject
{
    public GameObject weapon;
    
    public float damage;
    
    [Range(0, 1)] public float maxDistance;
}
