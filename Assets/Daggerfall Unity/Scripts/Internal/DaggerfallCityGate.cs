using UnityEngine;
using System.Collections;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshop
{
    public class DaggerfallCityGate : MonoBehaviour
    {
        bool isOpen;

        public void SetOpen(bool open)
        {
            // Do nothing if no change
            if (open == isOpen)
                return;

            // TODO: Change model

            // Save new state
            isOpen = open;
        }
    }
}