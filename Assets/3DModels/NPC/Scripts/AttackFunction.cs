using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LateExe;
using UnityEngine.Serialization;


public class AttackFunction : MonoBehaviour{

    [SerializeField]private float damage;
    [SerializeField] private NpcAi npcAi;
    public TrailRenderer[] trail;

    [SerializeField][Range(0,3)]internal float maxDistance;
    [SerializeField]private bool hasHit = true;

    [SerializeField]private Vector3 attackDirection;


    string _tt;

    void Update()
    {
        AttackNpc();
    }

    void AttackNpc()
    {
        Debug.Log(npcAi.npcState);
        if (npcAi.player.playerState == PlayerStates.Death)
        {
            npcAi.playerObject = null;
            return;
        }
        if (npcAi.npcState != NpcAi.NPCState.Attack) return;
            RaycastHit hit;
        if (Physics.Raycast(transform.position, attackDirection, out hit, maxDistance))
        {
            if (hit.transform.root.GetComponent<Player>() != null && !hasHit)
            {
                Player _player = hit.transform.root.GetComponent<Player>();
                _player.ReceiveDamage(damage);
                _player._combatState = Player.CombatState.TakingDamage;
                hasHit = true;
                Executer exe = new Executer(this);
                exe.DelayExecute(1.5f, x => hasHit = false);
            }
        }
    }

    public void SetTrailEmission(bool val){
        if(trail != null ){
            foreach (var item in trail){
                item.emitting = val;
            }
        }
    }

    void OnDrawGizmos(){
        Gizmos.DrawRay(transform.position, attackDirection * maxDistance);
    }

    void OnGUI(){
        GUI.Label(new Rect(180, 50,200,20 ), _tt);

    }
}
