using UnityEngine;

namespace LateExe
{
    public interface IPeaceful
    {

        static void JumpMove(ref float jumpTimer, Animator animator, int animationKey, Player player, InputManager inputManager, 
            CharacterController characterController,  float jumpValue, float gravityForce)
        {
            if (Time.time > jumpTimer)
            {
                animator.SetTrigger(animationKey);
                Executer exe = new Executer(player);
                exe.DelayExecute(0.1f, x => Jump(characterController, player.transform, jumpValue, gravityForce));
                jumpTimer = Time.time + 1.1f;
                inputManager.vertical = 1;
            }
        }

        static void Jump(CharacterController characterController, Transform transform, float jumpValue, float gravityForce)
        {
            characterController.Move(transform.up * (jumpValue * -2 * gravityForce * Time.deltaTime));
        }

        static void SneakingUp(Animator animator, int animationKey, Player player)
        {
            animator.SetTrigger(animationKey);
            player.playerState = PlayerStates.SneakingUp;
        }

        static void CameraControl(InputManager inputManager, Transform transform, float turnDirection, float turnMultiplier, GameObject focusPoint)
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
    }
}