// Project:         Daggerfall Unity -- A game built with Daggerfall Tools For Unity
// Description:     This is a modified version of a script provided by Daggerfall Tools for Unity
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Project Page:    https://github.com/EBFEh/DFUnity -- https://code.google.com/p/daggerfall-unity/

using UnityEngine;
using System.Collections;
using DaggerfallWorkshop;
using Daggerfall.Game;

namespace Daggerfall.Gameplay {
    // 
    // Thanks to FatiguedArtist in forum thread below for this code.
    // http://forum.unity3d.com/threads/a-free-simple-smooth-mouselook.73117/
    //
    public class PlayerMouseLook : MonoBehaviour {
        Vector2 _mouseAbsolute;
        Vector2 _smoothMouse;

        public bool invertMouseY = false;
        public Vector2 clampInDegrees = new Vector2(360, 180);
        public bool lockCursor;
        public Vector2 sensitivity = new Vector2(2, 2);
        public Vector2 smoothing = new Vector2(3, 3);
        public Vector2 targetDirection;
        public Vector2 targetCharacterDirection;

        // Assign this if there's a parent object controlling motion, such as a Character Controller.
        // Yaw rotation will affect this object instead of the camera if set.
        public GameObject characterBody;
        public bool enableMouseLook = true;
        public GameObject uiOwner;

        void Start()
        {
            Init();
        }

        void Update()
        {
            // Ensure the cursor is always locked when set
#if UNITY_5_0
			if (lockCursor)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
#else
            Screen.lockCursor = lockCursor;
#endif

            if (uiOwner.GetComponent<UIManager>().isUIOpen) {
                enableMouseLook = false;
            } else { 
                enableMouseLook = true;
            }

            // Suppress mouse look if fire2 is down
            // This means the player is swinging weapon
            if (!enableMouseLook || Input.GetButton("Fire2")) {
                this.lockCursor = false;
                return;
            } else {
                this.lockCursor = true;
            }


            // Allow the script to clamp based on a desired target value.
            var targetOrientation = Quaternion.Euler(targetDirection);
            var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

            // Get raw mouse input for a cleaner reading on more sensitive mice.
            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Invert mouse Y
            if (invertMouseY)
                mouseDelta.y = -mouseDelta.y;

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

            // Interpolate mouse movement over time to apply smoothing delta.
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

            // Find the absolute mouse movement value from point zero.
            _mouseAbsolute += _smoothMouse;

            // Clamp and apply the local x value first, so as not to be affected by world transforms.
            if (clampInDegrees.x < 360)
                _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

            var xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
            transform.localRotation = xRotation;

            // Then clamp and apply the global y value.
            if (clampInDegrees.y < 360)
                _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

            transform.localRotation *= targetOrientation;

            // If there's a character body that acts as a parent to the camera
            if (characterBody)
            {
                var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, characterBody.transform.up);
                characterBody.transform.localRotation = yRotation;
                characterBody.transform.localRotation *= targetCharacterOrientation;
            }
            else
            {
                var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
                transform.localRotation *= yRotation;
            }
        }

        public void Init()
        {
            // Set target direction to the camera's initial orientation.
            targetDirection = transform.localRotation.eulerAngles;

            // Set target direction for the character body to its inital state.
            if (characterBody) targetCharacterDirection = characterBody.transform.localRotation.eulerAngles;

            // Reset smoothing
            _mouseAbsolute = Vector2.zero;
            _smoothMouse = Vector2.zero;
        }

        public void SetFacing(Vector3 forward)
        {
            if (characterBody) characterBody.transform.rotation = Quaternion.LookRotation(forward);
            Init();
        }
    }
}