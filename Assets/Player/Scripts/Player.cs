using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LateExe
{
    [RequireComponent(typeof(InputManager))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour, IOneHandedCombat, IPeaceful
    {
        [Header("Character components")] public GameObject focusPoint;

        [SerializeField] private TerrainCollider terrainCollider;
        [SerializeField] private float delayEquip;

        private float timeLightAttack;
        private float timeHeavyAttack;
        private float timeRolling;
        private float timeTakeDamage;
        private float timeBlock;

        [SerializeField] private GameObject oneHandedSwordUnequippedPos;
        [SerializeField] private GameObject twoHandedSwordUnequippedPos;
        [SerializeField] private GameObject bowUnequippedPos;
        [SerializeField] private GameObject crossbowUnequippedPos;
        [SerializeField] private float blockTimer;
        [SerializeField] private float delayBlock;

        [Header("Character weapon")] public GameObject handPos;
        public GameObject backPos;
        private GameObject mele;
        private GameManager _gameManager;
        [HideInInspector] public InputManager inputManager;
        [HideInInspector] public CharacterController characterController;
        [HideInInspector] public Animator animator;

        [Header("Character value")] [SerializeField]
        internal int characterState = 0;

        public float health = 100;
        [SerializeField] [Range(1, 4)] private float movementSpeed = 2;
        [SerializeField] [Range(0, 1)] private float groundClearance;
        [SerializeField] [Range(-1, 1)] private float groundDistance;
        public float rollDistance;

        [Header("Combat value")] [SerializeField] [Range(1, 4)]
        private float combatSpeed;

        [SerializeField] [Range(0, 1)] private float lightAttackTimer;
        [SerializeField] [Range(0, 1)] private float heavyAttackTimer;

        internal enum CombatState
        {
            TakingDamage,
            NoDamage
        }

        internal CombatState _combatState;
        [SerializeField] [Range(0, 1)] private float takingDamageTimer;
        [SerializeField] [Range(0, 1)] private float rollTimer;
        [SerializeField] [Range(0, 4)] private float rollSpeed;

        [SerializeField] private Slider playerHealthBar;
        [Header("Debug value")] [SerializeField]
        private float jumpValue = -9.8f;

        [Header("Private value")] private Vector3 motionVector, gravityVector;
        private Vector3 _relativeVector;

        [SerializeField] private float gravityPower = -9.8f;
        private bool _cursorLoced;
        private readonly float gravityForce = -9.18f;
        private float _jumpTimer;
        private float _attackCooldown;
        private float _turnDirection;

        [SerializeField] private float turnMultiplier;
        internal PlayerStates playerState;

        //Animation keys
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int Horizontal = Animator.StringToHash("Horizontal");
        private static readonly int Vertical = Animator.StringToHash("Vertical");
        private static readonly int State = Animator.StringToHash("State");
        private static readonly int TakeDamage = Animator.StringToHash("TakeDamage");
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int AttackHeavy = Animator.StringToHash("AttackHeavy");
        private static readonly int Roll = Animator.StringToHash("Roll");
        private static readonly int Death = Animator.StringToHash("Death");
        private static readonly int OneHandedSwordEquip = Animator.StringToHash("OneHandedSwordEquip");
        private static readonly int OneHandedSwordUnEquip = Animator.StringToHash("OneHandedSwordUnEquip");
        private static readonly int GameOver = Animator.StringToHash("GameOver");
        private static readonly int StaticBlock = Animator.StringToHash("StaticBlock");
        private static readonly int BlockingAttack = Animator.StringToHash("BlockingAttack");
        private static readonly int SneakingUp = Animator.StringToHash("SneakingUp");


        private MeleState _meleState;
        [SerializeField] private GameObject _weapon;

        private bool _grounded;
        private static readonly int Sneaking = Animator.StringToHash("Sneaking");


        private void Start()
        {
            inputManager = GetComponent<InputManager>();
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            _gameManager = FindObjectOfType<GameManager>();
            if (_gameManager == null) print("No manager in scene");
            _meleState = _weapon.GetComponent<MeleState>();
            SetStartHealthBar();
        }

        private void Update()
        {
            PlayerStates();
            MouseLook();
            PlayerDeath();
            HealthCounter();
        }

        private void FixedUpdate()
        {
            _grounded = isGrounded();
        }

        void MouseLook()
        {
            if (inputManager.horizontal > 0 && inputManager.vertical == 0) return;

            if (Input.GetKeyDown(KeyCode.Tab)) _cursorLoced = !_cursorLoced;

            Cursor.lockState = _cursorLoced ? CursorLockMode.Locked : CursorLockMode.None;
            if (_cursorLoced)
            {
                _relativeVector = transform.InverseTransformPoint(focusPoint.transform.position);
                _relativeVector /= _relativeVector.magnitude;
                _turnDirection = _relativeVector.x / _relativeVector.magnitude;

                //Vertical
                var eulerAngles = focusPoint.transform.eulerAngles;
                eulerAngles =
                    new Vector3(eulerAngles.x + Input.GetAxis("Mouse Y"),
                        eulerAngles.y, 0);
                focusPoint.transform.eulerAngles = eulerAngles;
                //Horizontal
                focusPoint.transform.parent.Rotate(transform.up * (Input.GetAxis("Mouse X") * 100 * Time.deltaTime));
            }
        }

        void CombatMovement()
        {
            if (_grounded && playerState != LateExe.PlayerStates.LightAttack &&
                playerState != LateExe.PlayerStates.HeavyAttack && playerState != LateExe.PlayerStates.Roll)
            {
                IOneHandedCombat.OneHandedMove(motionVector, transform, characterController, combatSpeed, inputManager,
                    focusPoint, _turnDirection, turnMultiplier);
            }

            if (playerState != LateExe.PlayerStates.Roll && inputManager.jump == 1)
            {
                IOneHandedCombat.Rolling(motionVector, transform, characterController, inputManager, rollSpeed,
                    rollDistance, _attackCooldown, animator, Roll, this, rollTimer, ref timeRolling);
            }

            if (playerState != LateExe.PlayerStates.LightAttack && inputManager.fire == 1)
            {
                if (playerState != LateExe.PlayerStates.HeavyAttack && inputManager.fire1 == 1 && inputManager.fire == 1)
                {
                    IOneHandedCombat.OneHandedHeavyAttack(animator, AttackHeavy, heavyAttackTimer, this, _meleState,
                        ref timeHeavyAttack);
                }
                else
                {
                    IOneHandedCombat.OneHandedLightAttack(animator, Attack, lightAttackTimer, this, _meleState,
                        ref timeLightAttack);
                }
            }

            if (inputManager.block == 1)
            {
                IOneHandedCombat.OneHandedHeavyBlockStatic(animator, StaticBlock, this, blockTimer, ref timeBlock, delayBlock);
            }

            if (inputManager.block == 1 && playerState == LateExe.PlayerStates.TakingDamage)
            {
                IOneHandedCombat.OneHandedHeavyBlockingAttacks(animator, BlockingAttack, this, blockTimer, ref timeBlock, delayBlock);
            }

            if (_grounded && gravityVector.y < 0)
            {
                gravityVector.y = -2;
            }

            gravityVector.y += gravityPower * Time.deltaTime;
            characterController.Move(gravityVector * Time.deltaTime);
            SetAnimatorCombatValue();
        }

        void SetAnimatorCombatValue()
        {
            animator.SetBool(Grounded, _grounded);
            animator.SetFloat(Horizontal, inputManager.horizontal);
            animator.SetFloat(Vertical,
                inputManager.rawVertical * (1 + (inputManager.rawVertical > 0 ? inputManager.shift : 0)));
            animator.SetInteger(State, characterState);
        }

        public void ReceiveDamage(float value)
        {
            if (playerState == LateExe.PlayerStates.Death) return;

            if (_combatState == CombatState.TakingDamage)
            {
                if (timeTakeDamage + takingDamageTimer > Time.time)
                {
                    _combatState = CombatState.NoDamage;
                    return;
                }
                health -= value;
                animator.SetTrigger(TakeDamage);
                timeTakeDamage = Time.time;
                Debug.Log("Player taking damage");
            }
        }

        void PeacefulMovement()
        {
            if (_grounded && gravityVector.y < 0)
            {
                gravityVector.y = -2;
            }

            if (inputManager.sneakingUp == 1)
            {
                IPeaceful.SneakingUp(animator, SneakingUp, this);
                animator.SetBool(Sneaking, true);
            }
            else
            {
                animator.SetBool(Sneaking, false);
            }

            gravityVector.y += gravityPower * Time.deltaTime;
            characterController.Move(gravityVector * Time.deltaTime);

            if (inputManager.jump != 0 && _grounded)
            {
                IPeaceful.JumpMove(ref _jumpTimer, animator, Jump, this, inputManager, characterController, jumpValue,
                    gravityForce);
            }

            motionVector = transform.right *
                           (inputManager.vertical > 1 ? inputManager.horizontal * 2 : inputManager.horizontal)
                           + transform.forward * (inputManager.vertical * (1 + inputManager.shift));
            if (_grounded)
            {
                IPeaceful.CameraControl(inputManager, transform, _turnDirection, turnMultiplier, focusPoint);
            }

            characterController.Move(motionVector * (movementSpeed * Time.deltaTime));

            if (inputManager.fire != 0 && Time.time > _attackCooldown && characterState > 0)
            {
                animator.SetTrigger(Attack);
                _attackCooldown = Time.time + 1.4f;
                _meleState.hasHit = false;
            }

            SetAnimatorPeacefulValue();
        }

        void HealthCounter()
        {
            playerHealthBar.value = health;
        }
        
        void SetStartHealthBar()
        {
            playerHealthBar.maxValue = health;
        }

        void SetAnimatorPeacefulValue()
        {
            animator.SetBool(Grounded, _grounded);
            animator.SetFloat(Horizontal,
                inputManager.rawVertical != 0 ? inputManager.horizontal / 2 : inputManager.horizontal);
            animator.SetFloat(Vertical, inputManager.vertical);
            animator.SetInteger(State, characterState);
        }


        void PlayerDeath()
        {
            if (playerState == LateExe.PlayerStates.Death)
            {
                animator.SetTrigger(GameOver);
                return;
            }

            if (health <= 0)
            {
                animator.SetTrigger(Death);
                playerState = LateExe.PlayerStates.Death;
                Debug.Log("Death player");
            }
        }


        bool isGrounded()
        {
            List<Collider> colliders = new List<Collider>(Physics.OverlapSphere(transform.position, groundClearance));
            if (colliders.Contains(terrainCollider))
            {
                return true;
            }

            foreach (var collider1 in colliders)
            {
                if (collider1.gameObject.tag.Equals("Movable"))
                {
                    return true;
                }
            }

            return false;
            /*return Physics.CheckSphere(new Vector3(transform.position.x,
                transform.position.y - groundDistance, transform.position.z), groundClearance);*/
        }

        void OnGUI()
        {
            float rectPos = 50;
            GUI.Label(new Rect(20, rectPos, 200, 20), "Is Grounded: " + _grounded);
            rectPos += 30f;
            GUI.Label(new Rect(20, rectPos, 200, 20), "Character state: " + characterState);
            rectPos += 30f;
            GUI.Label(new Rect(20, rectPos, 200, 20), "CursorLocked: " + _cursorLoced);
            rectPos += 30f;
            GUI.Label(new Rect(20, rectPos, 200, 20), "TurnDirection: " + _turnDirection.ToString("0.00"));
            rectPos += 30f;
            GUI.Label(new Rect(20, rectPos, 200, 20), "Player state: " + playerState);
            rectPos += 30f;
        }

        private void OnDrawGizmosSelected()
        {
            var transform1 = transform;
            var position = transform1.position;
            Gizmos.DrawSphere(new Vector3(position.x, position.y - groundDistance, position.z), groundClearance);
        }

        void CheckPlayerState()
        {
            switch (playerState)
            {
                case LateExe.PlayerStates.UnequipWeapon:
                    playerState = LateExe.PlayerStates.Peaceful;
                    break;
                case LateExe.PlayerStates.EquipWeapon:
                    playerState = LateExe.PlayerStates.OneHandedSwordCombat;
                    break;
            }
        }

        void CheckWeaponType()
        {
        }

        // ReSharper disable Unity.PerformanceAnalysis
        void PlayerStates()
        {
            CheckPlayerState();

            IOneHandedCombat.OneHandedWeaponEquip(_weapon, oneHandedSwordUnequippedPos, handPos, delayEquip,
                playerState,
                OneHandedSwordEquip, OneHandedSwordUnEquip, animator, characterState, this, _meleState);
            if (playerState == LateExe.PlayerStates.Peaceful)
            {
                PeacefulMovement();
                Debug.Log("Peaceful");
            }
            else
            {
                CombatMovement();
                Debug.Log("Combat");
            }
        }
    }
}