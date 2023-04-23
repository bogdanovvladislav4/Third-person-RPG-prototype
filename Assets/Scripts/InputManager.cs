using System.Collections;
using System.Collections.Generic;
using LateExe;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    internal enum InputType
    {
        keyboard,
        mobile
    }
    

    [SerializeField] private InputType input;

    public float vertical, horizontal, jump, fire /*light attack*/, fire1 /*Heavy attack*/, shift;
    public float rawVertical, rawHorizontal;
    public Player player;

    private float _calculatedVertical;
    private int _pressCombatButton;
    internal float block;
    internal float sneakingUp;


    // Update is called once per frame
    void Update()
    {
        if (input == InputType.mobile)
        {

        }
        else
        {
            KeyboardInput();
        }
    }

    void KeyboardInput()
    {
        _calculatedVertical = Input.GetAxis("Horizontal") != 0 && Input.GetAxis("Vertical") == 0 ? horizontal :
            Input.GetAxis("Vertical") >= 0 ? Input.GetAxis("Vertical") : 0;
        /*vertical = Mathf.Abs(_calculatedVertical) * (1 + shift);*/
        vertical = Input.GetAxis("Vertical") * (1 + shift);
        /*horizontal = Mathf.Lerp(horizontal, Input.GetAxis("Horizontal") == 0 ?  Input.GetAxis("Horizontal") / 2 : Mathf.Abs(Input.GetAxis("Horizontal")  * (1 + shift)) , 15 * Time.deltaTime);*/
        horizontal = Input.GetAxis("Horizontal") / 2 * (1 + shift);
        fire = Input.GetAxis("Fire1");
        fire1 = Input.GetAxis("HeavyAttack");
        block = Input.GetAxis("Fire2");
        sneakingUp = Input.GetAxis("SneakingUp");
        jump = Input.GetAxis("Jump");
        rawVertical = Input.GetAxis("Vertical");
        rawHorizontal = Input.GetAxis("Horizontal");
        shift = Mathf.Lerp(shift, Input.GetAxis("Fire3"), 5 * Time.deltaTime);
        CombatButtonPressed();
    }
    
    void CombatButtonPressed()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            _pressCombatButton++;
        }
        CombatMode();
    }

    void CombatMode()
    {
        if (_pressCombatButton % 2 != 0 && _pressCombatButton != 0)
        {
            player.characterState = 2;
            player.playerState = PlayerStates.EquipWeapon;
        }
        else
        {
            player.characterState = 0;
            player.playerState = PlayerStates.UnequipWeapon;
        }
    }
}
