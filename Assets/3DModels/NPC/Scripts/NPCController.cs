using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class NPCController : MonoBehaviour
{

   
    private GameObject playerObject;
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private float horizontal, vertical;
    private float roamTimer;

    public float maxRoamDist;
    public int delayChangeDest;
    public int characterState;
    public Slider healthBar;
    public float health;

    private bool stateChangeFlag = false;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        healthBar.maxValue = health;
    }

    // Update is called once per frame
    void Update()
    {
        Roam();
        healthBar.value = health;
    }

    void Animations()
    {
        _animator.SetFloat("Vertical", vertical);
        _animator.SetFloat("Horizontal", horizontal);
        _animator.SetFloat("State", characterState);
    }

    void Roam()
    {
        if (characterState != 0) return;

        if (Time.time > roamTimer)
        {
            float a = Random.Range(0, 2);
            roamTimer = Time.time + delayChangeDest;
            _navMeshAgent.SetDestination(new Vector3(transform.position.x +
                                                     Random.Range(maxRoamDist / 2, maxRoamDist) * (a == 1 ? 1 : -1), 0,
                transform.position.x +
                Random.Range(maxRoamDist / 2, maxRoamDist) * (a == 1 ? 1 : -1)));
            Animations();
        }
    }
    
    private string GetCharState()
    {
        switch (characterState)
        {
            case 0:
                return "Peaceful";
            case 1:
                return "Combat";
        }

        return "Out of range";
    }

    public void ReceiveDamage(float value)
    {
        _animator.SetTrigger("TakeDamage");
        health -= value;
    }
}
