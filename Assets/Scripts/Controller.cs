using System;
using System.Collections;
using System.Collections.Generic;
using LateExe;
using UnityEditor.UIElements;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class Controller : MonoBehaviour, IOneHandedCombat
{

    [Header("Character components")]
    public GameObject focusPoint;

    public GameObject oneHandedSwordUnequippedPos;
    public GameObject twoHandedSwordUnequippedPos;
    public GameObject bowUnequippedPos;
    public GameObject crossbowUnequippedPos;

    [Header("Character weapon")] 
    public GameObject handPos;
    public GameObject backPos;
    private GameObject mele;    
    private GameManager _gameManager;
    [HideInInspector] public InputManager inputManager;
    [HideInInspector] public CharacterController characterController;
    [HideInInspector] public Animator animator;

    [Header("Character value")]
    public int characterState = 0;
    public float health = 100;
    [Range(1,4)]public float movementSpeed = 2;
    [Range(0, 0.5f)]public float groundClearance;
    [Range(0, 1)] public float groundDistance;
    public float rollDistance;

    [Header("Combat value")] 
    [Range(1,4)]public float combatSpeed;
    [Range(0, 1)] public float lightAttackTimer;
    [Range(0, 1)] public float heavyAttackTimer;
    public bool lightAttack;
    public bool heavyAttack;
    public bool takingDamage;
    [Range(0, 1)] public float takingDamageTimer;
    [Range(0, 1)] public float rollTimer;
    [Range(0, 4)] public float rollSpeed;
    
    [Header("Debug value")]
    public float jumpValue = -9.8f;
    
    [Header("Private value")]
    [HideInInspector] public Vector3 motionVector, gravityVector;
    private Vector3 relaviveVector;
    public float gravityPower = -9.8f;
    private bool cursorLoced;
    private float gravityForce = -9.18f;
    private float jumpTimer;
    private float attackCooldown;
    public bool rolling;
    private float turnDirection;

    
    public float turnMultiplier;
    private float turnTimer = 0.5f;
    private PlayerStates _playerState;
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int State = Animator.StringToHash("State");
    private static readonly int TakeDamage = Animator.StringToHash("TakeDamage");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Jump1 = Animator.StringToHash("Jump");
    private static readonly int AttackHeavy = Animator.StringToHash("AttackHeavy");
    private static readonly int Roll = Animator.StringToHash("Roll");
    private static readonly int Die = Animator.StringToHash("Die");



    private void Start()
    {
        inputManager = GetComponent<InputManager>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        _gameManager = FindObjectOfType<GameManager>();
        if(_gameManager == null) print("No manager in scene");
    }

    private void Update()
    {
        SpawnWeapon();
        PlayerStates();
        MouseLook();
    }

    public void EquipMele(GameObject currentWeapon)
    {
        if (characterState == 0)
        {
            mele.transform.parent = backPos.transform;
            mele.transform.localPosition = Vector3.zero;
            mele.transform.localEulerAngles = currentWeapon.transform.eulerAngles;
            mele.transform.localScale = currentWeapon.transform.localScale;
        }
        else
        {
            mele.transform.parent = handPos.transform;
            mele.transform.localPosition = Vector3.zero;
            mele.transform.localEulerAngles = currentWeapon.transform.eulerAngles;
            mele.transform.localScale = currentWeapon.transform.localScale;
        }
    }
    
    /*public void EquipMele()
    {
        if (characterState == 0)
        {
            mele.transform.parent = backPos.transform;
            mele.transform.localPosition = Vector3.zero;
        }
        else
        {
            mele.transform.parent = handPos.transform;
            mele.transform.localPosition = Vector3.zero;
        }
    }*/
    
    private void SpawnWeapon()
    {
        if (characterState > 0)
        {
            GameObject currentWeapon = _gameManager.meleWeapons.GetComponent<MeleWeapons>()
                .weapons[PlayerPrefs.GetInt("CurrentWeaponIndex")];
            mele = Instantiate(currentWeapon);
            EquipMele(currentWeapon);
        }
    }
    void MouseLook()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) cursorLoced = !cursorLoced;

        Cursor.lockState = cursorLoced ? CursorLockMode.Locked : CursorLockMode.None;
        if (cursorLoced)
        {
            relaviveVector = transform.InverseTransformPoint(focusPoint.transform.position);
            relaviveVector /= relaviveVector.magnitude;
            turnDirection = relaviveVector.x / relaviveVector.magnitude;

            //Vertical
            focusPoint.transform.eulerAngles =
                new Vector3(focusPoint.transform.eulerAngles.x + Input.GetAxis("Mouse Y"), focusPoint.transform.eulerAngles.y, 0);
            //Horizontal
            focusPoint.transform.parent.Rotate(transform.up * (Input.GetAxis("Mouse X") * 100 * Time.deltaTime));
        }
    }

    void CombatMovement()
    {
        if (isGrounded() && !lightAttack && !heavyAttack && !rolling)
        {
            motionVector = transform.right * inputManager.rawHorizontal + transform.forward * (inputManager.rawVertical * (1 + (inputManager.rawVertical > 0 ? inputManager.shift : 0)));
            characterController.Move(motionVector * (combatSpeed * Time.deltaTime));

            if (inputManager.rawHorizontal != 0 || inputManager.rawVertical != 0)
            {
                focusPoint.transform.parent.Rotate(transform.up * (-turnDirection * turnMultiplier * Time.deltaTime));
                transform.Rotate(transform.up * (turnDirection * turnMultiplier * Time.deltaTime));
            }
            
        }
        if(rolling)
        {
            motionVector = transform.right * inputManager.rawHorizontal + transform.forward * rollDistance;
            characterController.Move(motionVector * (rollSpeed * Time.deltaTime));
        }
        
        if (inputManager.jump == 1 && !rolling && !lightAttack && !heavyAttack)
            if (inputManager.rawVertical != 0 || inputManager.rawHorizontal != 0 && Time.time > attackCooldown)
            {
                rolling = true;
                animator.SetTrigger(Roll);
                print("Roll");
                transform.Rotate(transform.up * (inputManager.horizontal * 90));
                Executer exe = new Executer(this);
                exe.DelayExecute(rollTimer, x => rolling = false);
            }

        if (!lightAttack && !rolling && inputManager.fire == 1)
        {
            animator.SetTrigger(Attack);
            mele.GetComponent<MeleState>().hasHit = false;
            lightAttack = true;
            Executer aa = new Executer(this);
            aa.DelayExecute(lightAttackTimer, x => lightAttack = false);
        }
        
        if(!heavyAttack && !rolling && inputManager.fire1 == 1){
            animator.SetTrigger(AttackHeavy);
            mele.GetComponent<MeleState>().hasHit = false;
            heavyAttack = true;
            Executer bb = new Executer(this);
            bb.DelayExecute(heavyAttackTimer , x=> heavyAttack = false );
        }
        
        SetAnimatorCombatValue();

        if (isGrounded() && gravityVector.y < 0)
        {
            gravityVector.y = -2;
        }

        gravityVector.y += gravityPower * Time.deltaTime;
        characterController.Move(gravityVector * Time.deltaTime);
    }

    void SetAnimatorCombatValue()
    {
        animator.SetBool(Grounded, isGrounded());
        animator.SetFloat(Horizontal, inputManager.horizontal);
        animator.SetFloat(Vertical, inputManager.rawVertical * (1 + (inputManager.rawVertical > 0 ? inputManager.shift : 0)));
        animator.SetInteger(State, characterState);
    }

    public void ReceiveDamage(float value)
    {
        if (!takingDamage)
        {
            health -= value;
            animator.SetTrigger(TakeDamage);
            Executer bb = new Executer(this);
            bb.DelayExecute(takingDamageTimer, x => takingDamage = false);
        }
    }

    void PeacefulMovement()
    {
        if (isGrounded() && gravityVector.y < 0)
        {
            gravityVector.y = -2;
        }

        gravityVector.y += gravityPower * Time.deltaTime;
        characterController.Move(gravityVector * Time.deltaTime);

        if (inputManager.jump != 0 && isGrounded())
        {
            if (Time.time > jumpTimer)
            {
                animator.SetTrigger(Jump1);
                Executer exe = new Executer(this);
                exe.DelayExecute(0.1f, x => Jump());
                jumpTimer = Time.time + 1.1f;
                inputManager.vertical = 1;
            }
        }

        motionVector = transform.right *
                       (inputManager.vertical > 1 ? inputManager.horizontal * 2 : inputManager.horizontal)
                       + transform.forward * (inputManager.vertical * (1 + inputManager.shift));
        if (isGrounded())
        {
            if (inputManager.vertical > 0)
            {
                transform.Rotate(transform.up * (turnDirection * turnMultiplier * Time.deltaTime));
                focusPoint.transform.parent.Rotate(transform.up * (-turnDirection * turnMultiplier * Time.deltaTime));
            }

            if (inputManager.rawHorizontal != 0 && inputManager.rawVertical == 0)
            {
                focusPoint.transform.parent.Rotate(transform.up * inputManager.horizontal / 2 * (100 * Time.deltaTime));
            }
        }

        characterController.Move(motionVector * (movementSpeed * Time.deltaTime));
        
        SetAnimatorPeacefulValue();
        
        if (inputManager.fire != 0 && Time.time >  attackCooldown && characterState > 0)
        {
            animator.SetTrigger(Attack);
            attackCooldown = Time.time + 1.4f;
            mele.GetComponent<MeleState>().hasHit = false;
        }
    }

    void SetAnimatorPeacefulValue()
    {
        animator.SetBool(Grounded, isGrounded());
        animator.SetFloat(Horizontal, inputManager.rawVertical != 0 ? inputManager.horizontal / 2 : inputManager.horizontal);
        animator.SetFloat(Vertical, inputManager.vertical);
        animator.SetInteger(State, characterState);
    }

    

    void PlayerDeath()
    {
        if (health <= 0)
        {
            animator.SetTrigger(Die);
        }
    }

    /*void Animations()
    {
        animator.SetFloat("Vertical", inputManager.vertical);
        animator.SetFloat("Horizontal", inputManager.vertical != 0 ? inputManager.horizontal / 2 : inputManager.horizontal);
        animator.SetBool("Grounded", isGrounded());
        animator.SetInteger("Fire", characterState);
        animator.SetFloat("Jump", inputManager.jump);
        if (inputManager.fire != 0 && Time.time > attackCooldown && characterState == 1)
        {
            animator.SetTrigger("Attack");
            attackCooldown = Time.time + 1.4f;
            mele.GetComponent<MeleState>().hasHit = false;
        }
        animator.SetFloat("Fire", inputManager.fire);
    }*/

    /*void Movement()
    {
        if (isGrounded() && gravityVector.y < 0)
            gravityVector.y = -2;
        
        gravityVector.y += gravityPower * Time.deltaTime;
        characterController.Move(gravityVector * Time.deltaTime);

        if (isGrounded())
        {
            motionVector = transform.right * inputManager.horizontal + transform.forward * inputManager.vertical;
            if (inputManager.vertical > 0)
            {
                characterController.Move(motionVector * (movementSpeed * Time.deltaTime));
                transform.Rotate(transform.up * (turnDirection * turnMultiplier * Time.deltaTime));
                focusPoint.transform.parent.Rotate(transform.up * (-turnDirection * turnMultiplier * Time.deltaTime));
                _playerState = PlayerState.Walking;
            }
        }
        else
        {
            _playerState = PlayerState.Stay;
        }

        if (inputManager.jump != 0)
        {
            Jump();
        }
    }*/

    private void Jump()
    {
        /*if (isGrounded())
        {
            characterController.Move(transform.up * (jumpValue * -2 * gravityForce) * Time.deltaTime);
            _playerState = PlayerState.Jump;
            jumpTimer = Time.time + 4;
        }
        else
        {
            _playerState = PlayerState.Stay;
        }*/
        characterController.Move(transform.up * (jumpValue * -2 * gravityForce * Time.deltaTime));
    }

    bool isGrounded()
    {
        return Physics.CheckSphere(new Vector3(transform.position.x, 
            transform.position.y - groundDistance, transform.position.z), groundClearance);
    }

    void OnGUI()
    {
        float rectPos = 50;
        GUI.Label(new Rect(20, rectPos, 200, 20), "Is Grounded: " + isGrounded());
        rectPos += 30f;
        GUI.Label(new Rect(20, rectPos, 200, 20), "Player state: " + characterState);
        rectPos += 30f;
        GUI.Label(new Rect(20, rectPos, 200, 20), "CursorLocked: " + cursorLoced);
        rectPos += 30f;
        GUI.Label(new Rect(20, rectPos, 200, 20), "TurnDirection: " + turnDirection.ToString("0.00"));
        rectPos += 30f;
    }

    /*private string GetCharState()
    {
        switch (characterState)
        {
            case 0:
                return "Peaceful";
            case 1:
                return "Combat";
        }

        return "Out of range";
    }*/

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(new Vector3(transform.position.x,
            transform.position.y - groundDistance, transform.position.z), groundClearance);
    }

    void PlayerStates()
    {
        if (characterState == 0 && !takingDamage)
        {
            PeacefulMovement();
        }
        else
        {
            CombatMovement();
        }
    }
}
