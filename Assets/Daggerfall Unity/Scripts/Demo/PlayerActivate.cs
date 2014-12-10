using UnityEngine;
using System.Collections;

namespace DaggerfallWorkshop.Demo
{
    /// <summary>
    /// Example class to handle activation of doors, switches, etc. from Fire1 input.
    /// </summary>
    public class PlayerActivate : MonoBehaviour
    {
        PlayerEnterExit playerEnterExit;           // Example component to enter/exit buildings
        GameObject mainCamera;

        public float RayDistance = 2.5f;        // Distance of ray check, tune this to your scale and preference

        void Start()
        {
            playerEnterExit = GetComponent<PlayerEnterExit>();
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        void Update()
        {
            if (mainCamera == null)
                return;

            // Fire ray into scene
            if (Input.GetButtonDown("Fire1"))
            {
                // Using RaycastAll as sometime hits are blocked by decorations
                Ray ray = new Ray(mainCamera.transform.position + mainCamera.transform.forward * 0.2f, mainCamera.transform.forward);
                RaycastHit[] hits;
                hits = Physics.RaycastAll(ray, RayDistance);
                if (hits != null)
                {
                    // Check each hit in range for action, exit on first valid action processed
                    for (int i = 0; i < hits.Length; i++)
                    {
                        // Check for a static door hit
                        DaggerfallStaticDoors doors = hits[i].transform.GetComponent<DaggerfallStaticDoors>();
                        if (doors && playerEnterExit)
                        {
                            DaggerfallStaticDoors.StaticDoor door;
                            if (doors.HasHit(hits[i].point, out door))
                            {
                                if (door.doorType == DoorTypes.Building && !playerEnterExit.PlayerInside)
                                {
                                    // Hit door while outside, transition inside
                                    playerEnterExit.TransitionInterior(hits[i].transform.gameObject, door);
                                }
                                else if (door.doorType == DoorTypes.Building && playerEnterExit.PlayerInside)
                                {
                                    // Hit door while inside, transition outside
                                    playerEnterExit.TransitionExterior();
                                }
                            }

                            return;
                        }

                        // Check for an action door hit
                        DaggerfallActionDoor actionDoor;
                        if (ActionDoorCheck(hits[i], out actionDoor))
                        {
                            actionDoor.ToggleDoor();
                            return;
                        }

                        // Check for action record hit
                        DaggerfallAction action;
                        if (ActionCheck(hits[i], out action))
                        {
                            action.Play();
                            return;
                        }
                    }
                }
            }
        }

        // Check if raycast hit a static door
        private bool StaticDoorCheck(RaycastHit hitInfo, out DaggerfallStaticDoors door)
        {
            door = hitInfo.transform.GetComponent<DaggerfallStaticDoors>();
            if (door == null)
                return false;

            return true;
        }

        // Check if raycast hit an action door
        private bool ActionDoorCheck(RaycastHit hitInfo, out DaggerfallActionDoor door)
        {
            door = hitInfo.transform.GetComponent<DaggerfallActionDoor>();
            if (door == null)
                return false;

            return true;
        }

        // Check if raycast hit a generic action component
        private bool ActionCheck(RaycastHit hitInfo, out DaggerfallAction action)
        {
            // Look for action
            action = hitInfo.transform.GetComponent<DaggerfallAction>();
            if (action == null)
                return false;

            // Must be root action of chain (no parent)
            if (action.PreviousObject != null)
            {
                action = null;
                return false;
            }

            return true;
        }
    }
}