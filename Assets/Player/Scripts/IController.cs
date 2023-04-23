using UnityEngine;

namespace LateExe
{
    public interface IController

    {
        void MouseLook(bool cursorLoced, Vector3 relaviveVector, float turnDirection,
            GameObject focusPoint, Transform transform)
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
                    new Vector3(focusPoint.transform.eulerAngles.x + Input.GetAxis("Mouse Y"),
                        focusPoint.transform.eulerAngles.y, 0);
                //Horizontal
                focusPoint.transform.parent.Rotate(transform.up * (Input.GetAxis("Mouse X") * 100 * Time.deltaTime));
            }
        }
    }
}