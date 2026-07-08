using HarmonyLib;
using UnityEngine;

namespace PsychoMenuCU.Patches
{
    public static class ChatPatches
    {
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
        public static class OnChat
        {
            public static bool ShowMessagesByGhosts { get; set; } = false;

            static void Postfix(ChatController __instance, PlayerControl sourcePlayer, string chatText)
            {
                if (sourcePlayer == null) return;

                if (ShowMessagesByGhosts && !PlayerControl.LocalPlayer.Data.IsDead && sourcePlayer.Data.IsDead)
                {
                    __instance.AddChatWarning($"{sourcePlayer.Data.PlayerName}\n{chatText}");
                }
            }
        }
    }
}
