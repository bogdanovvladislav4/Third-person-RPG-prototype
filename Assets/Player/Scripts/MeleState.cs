using System;
using System.Collections;
using System.Collections.Generic;
using LateExe;
using UnityEngine;

public class MeleState : MonoBehaviour
{
    [SerializeField] private float damage;

    [SerializeField] private TrailRenderer[] trail;

    [SerializeField] private Player player;

    [SerializeField] [Range(0, 1)] private float maxDistance;

    [SerializeField] internal bool hasHit = true;
    [SerializeField] private Vector3 damageDirection;
    [SerializeField] internal Vector3 equipmentPos;
    [SerializeField] internal Vector3 equipmentRot;
    [SerializeField] internal Vector3 unEquipmentRot;

    private string tt;

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, damageDirection, out hit,  maxDistance))
        {
            print("hit: " + hit.transform.name);
            if (hit.transform.root.GetComponent<NpcAi>() != null && !hasHit)
            {
                if (player.playerState == PlayerStates.LightAttack || player.playerState == PlayerStates.HeavyAttack)
                {
                    hit.transform.root.GetComponent<NpcAi>().ReceiveDamage(damage);
                    hasHit = true;
                }
            }
        }
    }

    public void SetTrailEmission(bool val)
    {
        if (trail != null)
        {
            foreach (var item in trail)
            {
                item.emitting = val;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, damageDirection * maxDistance);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(180, 50, 200, 20), tt);
    }
}
