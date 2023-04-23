using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LateExe;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Serialization;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class NpcAi : MonoBehaviour
{
    [Header("components")]
    public Slider healthSlider;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    internal GameObject playerObject;
    internal Player player;

    [Header("values")]
    public float roamCooldownTime = 10;
    public float maxRoamDistance;
    public float health;
    public float attackCooldown;
    public float attackTimer;

    internal enum NPCState
    {
        Attack,
        Ide,
        TakingDamage,
        Roam,
        Combat,
        GetCloserToPlayer,
        Death
    }

    internal NPCState npcState;

    [Header("debug")]

    [Header("private")]
    [HideInInspector]public int characterState;
    private float horizontal, vertical;
    private float RoamTimer;
    private float Velocity = 0;
    private bool stateChangedFlag = false;//used once to change state of player
    private bool attacking;
    private bool hasDied;

    [SerializeField] private AttackFunction attackFunction;
    //Animation keys
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int State = Animator.StringToHash("State");
    private static readonly int TakeDamage = Animator.StringToHash("TakeDamage");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Die = Animator.StringToHash("Die");


    void Start(){
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject.GetComponent<Player>();
        if(playerObject == null) print("no object with player tag");

        healthSlider.maxValue = health;
    }

    void Update()
    {
        Death();
        
        healthSlider.value = health;
        
        if(GetPlayerDistance() < 5)
        {
            Combat();
        }
        else
        {
            Roam();
        }
    }

    void Death()
    {
        if(npcState != NPCState.Death) return;

        if(health < 1){
            hasDied = true;
            animator.SetTrigger(Die);
            healthSlider.transform.parent.gameObject.SetActive(false);
            npcState = NPCState.Death;
        }
    }

    void Combat(){
        if(GetPlayerDistance() < attackFunction.maxDistance){
            AttackNpc();
        }else{
            GetCloserToPlayer();
        }
    }

    void GetCloserToPlayer(){
        if (!attacking)
        {
            navMeshAgent.SetDestination(playerObject.transform.position);
            npcState = NPCState.GetCloserToPlayer;
        }
    }

    void AttackNpc(){
        if(!attacking){
            animator.SetTrigger(Attack);
            transform.LookAt(playerObject.transform);
            Executer exe = new Executer(this);
            npcState = NPCState.Attack;
            exe.DelayExecute(attackCooldown , x=> attacking = false);
        }
    }

    void FixedUpdate(){
        Animations();
        CheckNpc();    
    }

    void CheckNpc(){
        if(GetPlayerDistance() < 20 && !stateChangedFlag){
            stateChangedFlag = true ;
            /*SetState(1);*/
        }
        if(GetPlayerDistance() > 20 && stateChangedFlag){
            stateChangedFlag = false;
            /*SetState(0);*/
        }
    }

    void SetState(int value){
        characterState = value;
        playerObject.GetComponent<Player>().characterState = value;
    }

    void Roam(){
        if(characterState != 0 && GetPlayerDistance() < 5)return;
        
        if(Time.time > RoamTimer){
            float a = Random.Range(0,2);
            RoamTimer = Time.time + roamCooldownTime;
            navMeshAgent.SetDestination(new Vector3(transform.position.x +  Random.Range(0 , maxRoamDistance) * (a == 1 ?  1 : -1),0
            ,transform.position.z +  Random.Range(0 , maxRoamDistance) * (a == 1 ?  1 : -1)));
        }
    }

    void Animations(){
        vertical = Mathf.Lerp(vertical , navMeshAgent.remainingDistance > 0 ? 1 : 0 , 5 * Time.deltaTime);
        animator.SetFloat(Vertical, vertical);
        animator.SetFloat(Horizontal, horizontal);
    }

    string GetCharState(){
        switch(characterState){
            case 0:
                return "peaceful";
            case 1:
                return "Combat";
        }
        return"out of range";
        
    }
    
    float GetPlayerDistance(){
        if(playerObject != null){
            return Vector3.Distance(transform.position , playerObject.transform.position);
        }
        return 0;
    }

    public void ReceiveDamage(float value){
        animator.SetTrigger(TakeDamage);
        health -= value;
    }

    void OnGUI(){
        GUI.Label(new Rect(20, PlayerPrefs.GetInt("rectPos"),200,20 ),"NPC acceleration: " +  navMeshAgent.remainingDistance);
        PlayerPrefs.SetInt("rectPos",PlayerPrefs.GetInt("rectPos") + 30);

        GUI.Label(new Rect(20, PlayerPrefs.GetInt("rectPos"),200,20 ),"NPC state: " +  GetCharState());
        PlayerPrefs.SetInt("rectPos",PlayerPrefs.GetInt("rectPos") + 30);
      
        GUI.Label(new Rect(20, PlayerPrefs.GetInt("rectPos"),200,20 ),"NPC state: " +  GetPlayerDistance().ToString("0.00"));
        PlayerPrefs.SetInt("rectPos",PlayerPrefs.GetInt("rectPos") + 30);
    }
}
    