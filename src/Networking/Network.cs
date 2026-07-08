using Hazel;
using InnerNet;
using UnityEngine;

namespace PsychoMenuCU.Networking
{
    public static class Network
    {
        public static void SendSetScanner(bool scanning)
        {
            byte scanCount = ++PlayerControl.LocalPlayer.scannerCount;

            PlayerControl.LocalPlayer.SetScanner(scanning, scanCount);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)PlayerControl.RpcCalls.SetScanner,
                SendOption.Reliable,
                -1
            );

            writer.Write(scanning);
            writer.Write(scanCount);

            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}
