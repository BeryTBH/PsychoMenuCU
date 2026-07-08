using System;
using UnityEngine;
using InnerNet;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using System.IO;
using Hazel;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using Il2CppInterop.Runtime.Injection;

namespace PsychoMenuCU.Utilities
{
    public static class Utils
    {

        public static void CopyPlayer(PlayerControl player)
        {
            var local = PlayerControl.LocalPlayer;
            var outfit = player.Data;

            if (local == null || outfit == null) return;

            if (!AmongUsClient.Instance.AmHost)
            {
                NotificationUtils.Show("This requires host to function correctly.");
                return;
            }
            
            local.RpcSetName(outfit.PlayerName);
            local.RpcSetColor((byte)outfit.ColorId);

            local.RpcSetHat(outfit.HatId);
            local.RpcSetSkin(outfit.SkinId);
            local.RpcSetPet(outfit.PetId);
        }

        public static void Overload(int targetId, int strength)
        {
            if (strength < 1) return;

            int maxRpc = 400;

            uint netId = PlayerControl.LocalPlayer.MyPhysics.NetId;
            byte rpcCall = (byte)PlayerControl.RpcCalls.SetPet;

            if (strength <= maxRpc)
            {
                // SendOption.None has no flow control, allowing for flooding without limits

                var messageWriter = MessageWriter.Get(SendOption.None);

                if (targetId < 0) // -1 = Broadcast
                {
                    messageWriter.StartMessage((byte)GameData.Instance.NetId);
                    messageWriter.Write(AmongUsClient.Instance.GameId);
                }
                else
                {
                    messageWriter.StartMessage((byte)GameData.Instance.NetId);
                    messageWriter.Write(AmongUsClient.Instance.GameId);
                    messageWriter.WritePacked(targetId);
                }

                for (var msg = 0; msg < strength; msg++)
                {
                    messageWriter.StartMessage((byte)PlayerControl.RpcCalls.SetPet);
                    messageWriter.WritePacked(netId);
                    messageWriter.Write(rpcCall);

                    // Use LocalPlayer.GetTruePosition() as the petting position
                    // to minimize WalkPlayerTo delay and start the hand-petting animation immediately

                    NetHelpers.WriteVector2(PlayerControl.LocalPlayer.GetTruePosition(), messageWriter);

                    // Pet position is decoded as (-50, -50) on target clients
                    // This keeps the hand-petting animation out of normal view

                    messageWriter.Write((ushort)0);

                    messageWriter.Write((ushort)0);

                    messageWriter.EndMessage();
                }

                messageWriter.EndMessage();

                AmongUsClient.Instance.connection.Send(messageWriter);

                messageWriter.Recycle();
            }
            else
            {
                int strengthGroups = strength / maxRpc;
                int remainder = strength % maxRpc;

                for (int group = 0; group < strengthGroups; group++)
                {
                    Overload(targetId, maxRpc);
                }

                Overload(targetId, remainder);
            }
        }
    }
}
