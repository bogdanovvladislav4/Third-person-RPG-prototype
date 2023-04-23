using UnityEngine;

namespace LateExe
{
    public interface IOneHandedCombat
    {
        
        static void OneHandedWeaponEquip(GameObject weapon, GameObject unequippedPos, GameObject handPos, float delay, 
            PlayerStates playerStates, int equipAnimKey, int unEquipAnimKey, Animator animator, int characterState, 
            Player player, MeleState meleState)
        {

            if (playerStates == PlayerStates.OneHandedSwordCombat)
            {
                OneHandedWeaponEquipAnimation(animator, playerStates, characterState, equipAnimKey);
                Executer exe = new Executer(player);
                exe.DelayExecute(delay, x => SetPosWeaponEquip(weapon, handPos, meleState));
                Debug.Log("Equip one-handed weapon");
            }
            else if (playerStates == PlayerStates.Peaceful)
            {
                OneHandedWeaponUnEquipAnimation(animator, playerStates, characterState, unEquipAnimKey);
                Executer exe = new Executer(player);
                exe.DelayExecute(delay, x => SetPosWeaponUnEquip(weapon, unequippedPos, meleState));
                Debug.Log("Unequip one-handed weapon");
            }
        }

        static void SetPosWeaponEquip(GameObject weapon, GameObject handPos, MeleState meleState)
        {
            weapon.transform.parent = handPos.transform;
            weapon.transform.localPosition = meleState.equipmentPos;
            weapon.transform.localEulerAngles = meleState.equipmentRot;
            weapon.transform.localScale = new Vector3(1,1,1);
        }
        
        static void SetPosWeaponUnEquip(GameObject weapon, GameObject unequippedPos, MeleState meleState)
        {
            weapon.transform.parent = unequippedPos.transform;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localEulerAngles = meleState.unEquipmentRot;
            weapon.transform.localScale = new Vector3(1,1,1);
        }

       static void OneHandedLightAttack(Animator animator, int animationKey,
           float lightAttackTimer, Player player, MeleState meleState, ref float currentTimeAttack)
       {
           if (currentTimeAttack + lightAttackTimer > Time.time) return;

            bool attack = false;
            animator.SetTrigger(animationKey);
            meleState.hasHit = false;
            player.playerState = PlayerStates.LightAttack;
            Executer aa = new Executer(player);
            aa.DelayExecute(lightAttackTimer, x => player.playerState = PlayerStates.Battle);
            currentTimeAttack = Time.time;
            Debug.Log("Light Attack");
       }

       static void OneHandedHeavyAttack(Animator animator, int animationKey,
           float heavyAttackTimer, Player player, MeleState meleState, ref float currentTimeAttack)
       {
           if (currentTimeAttack + heavyAttackTimer > Time.time) return;
           bool attack = false;
           animator.SetTrigger(animationKey);
           meleState.hasHit = false;
           player.playerState = PlayerStates.HeavyAttack;
           Executer bb = new Executer(player);
           bb.DelayExecute(heavyAttackTimer , x=> player.playerState = PlayerStates.Battle);
           currentTimeAttack = Time.time;
           Debug.Log("Heavy Attack");
       }

       static void OneHandedHeavyBlockStatic(Animator animator, int animationKey, Player player, float blockTimer, ref float timeBlock, float delayBlock)
       {
           if (timeBlock + delayBlock > Time.time) return;
           timeBlock = Time.time;
           animator.SetTrigger(animationKey);
           player.playerState = PlayerStates.Block;
           Executer bb = new Executer(player);
           bb.DelayExecute(blockTimer , x=> player.playerState = PlayerStates.Battle);
       }
       
       static void OneHandedHeavyBlockingAttacks(Animator animator, int animationKey, Player player, float blockTimer, ref float timeBlock, float delayBlock)
       {
           if (timeBlock + delayBlock > Time.time) return;
           timeBlock = Time.time;
           animator.SetTrigger(animationKey);
           player.playerState = PlayerStates.BlockingAttack;
           Executer bb = new Executer(player);
           bb.DelayExecute(blockTimer , x=> player.playerState = PlayerStates.Battle);
       }

           static void OneHandedMove(Vector3 motionVector, Transform transform, CharacterController characterController, 
           float combatSpeed, InputManager inputManager, GameObject focusPoint, float turnDirection, float turnMultiplier)
       {
           motionVector = transform.right * inputManager.rawHorizontal + transform.forward * (inputManager.rawVertical * 
               (1 + (inputManager.rawVertical > 0 ? inputManager.shift : 0)));
           characterController.Move(motionVector * (combatSpeed * Time.deltaTime));

           if (inputManager.rawHorizontal != 0 || inputManager.rawVertical != 0)
           {
               focusPoint.transform.parent.Rotate(transform.up * (-turnDirection * turnMultiplier * Time.deltaTime));
               transform.Rotate(transform.up * (turnDirection * turnMultiplier * Time.deltaTime));
           }
       }

        static void OneHandedWeaponEquipAnimation(Animator animator, PlayerStates playerStates, int characterState, int key)
        {
            if (playerStates == PlayerStates.OneHandedSwordCombat)
            {
                animator.SetInteger(key,characterState);
            }
            else if (playerStates == PlayerStates.Peaceful)
            {
                animator.SetInteger(key, characterState);
            }
        }
        
        static void OneHandedWeaponUnEquipAnimation(Animator animator, PlayerStates playerStates, int characterState, int key)
        {
            if (playerStates == PlayerStates.OneHandedSwordCombat)
            {
                animator.SetInteger(key,characterState);
            }
            else if (playerStates == PlayerStates.Peaceful)
            {
                animator.SetInteger(key, characterState);
            }
        }

        static void Rolling(Vector3 motionVector, Transform transform, CharacterController characterController,
            InputManager inputManager, float rollSpeed, float rollDistance, float attackCooldown, Animator animator,
            int animationKey, Player player, float rollTimer, ref float timeRolling)
        {

            if (timeRolling + rollTimer > Time.time) return;

            motionVector = transform.right * inputManager.rawHorizontal + transform.forward * rollDistance;
            characterController.Move(motionVector * (rollSpeed * Time.deltaTime));
                if (inputManager.rawVertical != 0 || inputManager.rawHorizontal != 0 && Time.time > attackCooldown)
                {
                    player.playerState = PlayerStates.Roll;
                    animator.SetTrigger(animationKey);
                    transform.Rotate(transform.up * (inputManager.horizontal * 90));
                    Executer exe = new Executer(player);
                    exe.DelayExecute(rollTimer, x => player.playerState = PlayerStates.Battle);
                    timeRolling = Time.time;
                }
        }
    }
}