using UnityEngine;

namespace LateExe
{
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField] private Vector3 relativeVector;
        [SerializeField] private float turnDirection;
        [SerializeField] private GameObject focusPoint;
        [SerializeField] private bool cursorLoced;

        void MouseLook()
        {
            if (Input.GetKeyDown(KeyCode.Tab)) cursorLoced = !cursorLoced;

            Cursor.lockState = cursorLoced ? CursorLockMode.Locked : CursorLockMode.None;
            if (cursorLoced)
            {
                relativeVector = transform.InverseTransformPoint(focusPoint.transform.position);
                relativeVector /= relativeVector.magnitude;
                turnDirection = relativeVector.x / relativeVector.magnitude;

                //Vertical
                focusPoint.transform.eulerAngles =
                    new Vector3(focusPoint.transform.eulerAngles.x + Input.GetAxis("Mouse Y"),
                        focusPoint.transform.eulerAngles.y, 0);
                //Horizontal
                focusPoint.transform.parent.Rotate(transform.up * (Input.GetAxis("Mouse X") * 100 * Time.deltaTime));
            }
        }
    }
}