using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Stores an array of static doors.
    /// Exposes helpers to check doors in correct world space.
    /// </summary>
    [Serializable]
    public class DaggerfallStaticDoors : MonoBehaviour
    {
        public StaticDoor[] Doors;                  // Array of doors attached this building or group of buildings

        [Serializable]
        public struct StaticDoor
        {
            public Matrix4x4 buildingMatrix;        // Matrix of individual building owning this door
            public DoorTypes doorType;              // Type of door
            public int blockIndex;                  // Block index in BLOCKS.BSA
            public int recordIndex;                 // Record index of interior
            public int doorIndex;                   // Door index for individual building/record (most buildings have only 1-2 doors)
            public Vector3 centre;                  // Door centre in model space
            public Vector3 size;                    // Door size in model space
            public Vector3 normal;                  // Normal pointing away from door
        }

        /// <summary>
        /// Check for a door hit in world space.
        /// </summary>
        /// <param name="point">Hit point from ray test in world space.</param>
        /// <param name="doorOut">StaticDoor out if hit found.</param>
        /// <returns>True if point hits a static door.</returns>
        public bool HasHit(Vector3 point, out StaticDoor doorOut)
        {
            doorOut = new StaticDoor();
            if (Doors == null)
                return false;

            // Using a single hidden trigger created when testing door positions
            // This avoids problems with AABBs as trigger rotates nicely with model transform
            // A trigger is also more useful for debugging as its drawn by editor
            GameObject go = new GameObject();
            go.hideFlags = HideFlags.HideAndDontSave;
            go.transform.parent = transform;
            BoxCollider c = go.AddComponent<BoxCollider>();
            c.isTrigger = true;

            // Test each door in array
            bool found = false;
            for (int i = 0; i < Doors.Length; i++)
            {
                // Setup trigger position and size over this door
                go.transform.position = transform.rotation * Doors[i].buildingMatrix.MultiplyPoint3x4(Doors[i].centre);
                c.size = GameObjectHelper.QuaternionFromMatrix(Doors[i].buildingMatrix) * Doors[i].size;
                go.transform.position += transform.position;
                go.transform.rotation = transform.rotation;

                // Deprecated: Bounds checking method.
                //Vector3 centre = transform.rotation * Doors[i].buildingMatrix.MultiplyPoint3x4(Doors[i].centre) + transform.position;
                //Vector3 size = new Vector3(50, 90, 50) * MeshReader.GlobalScale; // Native door fit
                //Bounds bounds = new Bounds(centre, size);

                // Check if hit was inside trigger
                if (c.bounds.Contains(point))
                {
                    found = true;
                    doorOut = Doors[i];
                    break;
                }
            }

            // Remove temp trigger
            if (go)
                Destroy(go);

            return found;
        }

        /// <summary>
        /// Find closest door to player position in world space.
        /// </summary>
        /// <param name="playerPos">Player position in world space.</param>
        /// <param name="record">Door record index.</param>
        /// <param name="doorPosOut">Position of closest door in world space.</param>
        /// <param name="doorIndexOut">Door index in Doors array of closest door.</param>
        /// <returns></returns>
        public bool FindClosestDoorToPlayer(Vector3 playerPos, int record, out Vector3 doorPosOut, out int doorIndexOut)
        {
            // Init output
            doorPosOut = playerPos;
            doorIndexOut = -1;

            // Must have door array
            if (Doors == null)
                return false;

            // Find closest door to player position
            float minDistance = float.MaxValue;
            for (int i = 0; i < Doors.Length; i++)
            {
                // Get this door centre in world space
                Vector3 centre = transform.rotation * Doors[i].buildingMatrix.MultiplyPoint3x4(Doors[i].centre) + transform.position;

                // Check if door belongs to same building record
                if (Doors[i].recordIndex == record)
                {
                    // Check distance and save closest
                    float distance = Vector3.Distance(playerPos, centre);
                    if (distance < minDistance)
                    {
                        doorPosOut = centre;
                        doorIndexOut = i;
                        minDistance = distance;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Gets world transformed normal of door at index.
        /// </summary>
        /// <param name="index">Door index.</param>
        /// <returns>Normal pointing away from door in world.</returns>
        public Vector3 GetDoorNormal(int index)
        {
            return Vector3.Normalize(transform.rotation * Doors[index].buildingMatrix.MultiplyVector(Doors[index].normal));
        }
    }
}