using HarmonyLib;
using UnityEngine;

namespace PsychoMenuCU.Patches
{
    public static class EmergencyMeetingPatches
    {
        [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Begin))]
        public static class UnlimitedMeetings
        {
            public static bool enabled = false;

            static void Prefix()
            {
                if (enabled) PlayerControl.LocalPlayer.RemainingEmergencies = 999999;
            }
        }
    }
}
