using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using PsychoMenuCU.Networking;
using PsychoMenuCU.Patches;
using PsychoMenuCU.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PsychoMenuCU
{
    public class Menu : MonoBehaviour
    {
        private bool showMenu;
        private Rect windowRect = new Rect(50, 50, 650, 500);

        private bool stylesInitialized;
        private string lastScene;

        private GUIStyle windowStyle;
        private GUIStyle buttonStyle;
        private GUIStyle toggleStyle;
        private GUIStyle labelStyle;
        private GUIStyle scrollStyle;

        private bool exampleToggle;

        private bool noClipEnabled = false;

        private bool walkInVents = false;

        private bool alwaysShowChat = false;

        private Vector2 playerScroll;
        private PlayerControl selectedPlayer;

        private bool followPlayer = false;
        private PlayerControl followTarget;

        private List<string> logs = new List<string>();
        private Vector2 consoleScroll;

        private float keyDelay;

        private int selectedTab = 0;
        private string[] tabs;

        private void Log(string message)
        {
            logs.Add($"[{System.DateTime.Now:HH:mm:ss}] {message}");

            if (logs.Count > 200) logs.RemoveAt(0);

            Debug.Log(message);
        }

        private string[] GetTabs()
        {
            List<string> tabs = new List<string>();

            tabs.Add("About");
            tabs.Add("Player");
            tabs.Add("Utility");
            tabs.Add("Visuals");
            tabs.Add("Players");
            tabs.Add("Console");

            if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
            {
                tabs.Add("Host");
            }

            return tabs.ToArray();
        }

        void Start()
        {
            Log("Successfully started menu class");
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.Delete) && Time.time > keyDelay)
            {
                keyDelay = Time.time + 0.25f;
                showMenu = !showMenu;
            }

            if (exampleToggle)
            {
                Debug.Log("hi");
                exampleToggle = false;
            }

            var pc = PlayerControl.LocalPlayer;

            if (pc != null && pc.Collider != null)
            {
                pc.Collider.enabled = !(noClipEnabled || pc.onLadder);
            }

            if (walkInVents)
            {
                pc.inVent = false;
                pc.moveable = true;
            }

            if (alwaysShowChat)
            {
                var chat = HudManager.Instance?.Chat;
                if (chat != null)
                {
                    chat.gameObject.SetActive(true);
                }
            }

            if (followPlayer)
            {
                if (followTarget == null || followTarget.Data.Disconnected)
                {
                    followPlayer = false;
                }
                else
                {
                    PlayerControl.LocalPlayer.transform.position = followTarget.transform.position;
                }
            }
        }

        private Texture2D MakeTex(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            stylesInitialized = true;

            windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.normal.textColor = Color.white;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 13;

            buttonStyle.normal.background = MakeTex(new Color(0.10f, 0.20f, 0.45f));
            buttonStyle.hover.background = MakeTex(new Color(0.15f, 0.30f, 0.60f));
            buttonStyle.active.background = MakeTex(new Color(0.08f, 0.15f, 0.35f));
            buttonStyle.focused.background = MakeTex(new Color(0.10f, 0.20f, 0.45f));

            buttonStyle.normal.textColor = Color.white;
            buttonStyle.hover.textColor = Color.white;
            buttonStyle.active.textColor = Color.white;

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = Color.white;

            toggleStyle = new GUIStyle(GUI.skin.toggle);
            toggleStyle.fontSize = 13;
            toggleStyle.normal.textColor = Color.white;
            toggleStyle.hover.textColor = Color.white;
            toggleStyle.active.textColor = Color.white;
            toggleStyle.onNormal.textColor = Color.white;
            toggleStyle.onHover.textColor = Color.white;
            toggleStyle.onActive.textColor = Color.white;

            scrollStyle = new GUIStyle(GUI.skin.scrollView);
        }

        void OnGUI()
        {
            if (!showMenu) return;

            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene != lastScene)
            {
                lastScene = currentScene;
                stylesInitialized = false;
            }

            InitStyles();

            GUI.skin.window = windowStyle;
            GUI.skin.button = buttonStyle;
            GUI.skin.label = labelStyle;
            GUI.skin.toggle = toggleStyle;
            GUI.skin.scrollView = scrollStyle;

            GUI.backgroundColor = new Color(0.15f, 0.22f, 0.45f);
            GUI.contentColor = Color.white;
            GUI.color = Color.white;

            windowRect = GUI.Window(
                0,
                windowRect,
                (GUI.WindowFunction)DrawMenu,
                "PsychoMenuCU - " + PsychoPlugin.version
            );

            NotificationUtils.Draw();
        }

        void DrawMenu(int id)
        {
            tabs = GetTabs();

            if (selectedTab >= tabs.Length)
            {
                selectedTab = 0;
            }

            GUILayout.BeginHorizontal(); // Split to L/R

            // Tabs
            GUILayout.BeginVertical(GUILayout.Width(120));

            for (int i = 0; i < tabs.Length; i++)
            {
                if (GUILayout.Button(tabs[i]))
                {
                    selectedTab = i;
                }
            }

            GUILayout.EndVertical();

            // Content
            GUILayout.BeginVertical();

            // About Tab
            if (selectedTab == 0)
            {
                GUILayout.Label("PsychoMenu (for Classic Us)");
                GUILayout.Label($"Version: {PsychoPlugin.version}");

                GUILayout.Space(10);

                GUILayout.Label($"Made by context.smali ({PsychoPlugin.author})");

                GUILayout.Space(10);

                GUILayout.Label("Keybind: DEL");
            }

            // Player Tab
            if (selectedTab == 1)
            {
                // Use the example Toggle code for toggles
                // Toggle Code: exampleToggle = GUILayout.Toggle(exampleToggle, "Example Toggle");

                noClipEnabled = GUILayout.Toggle(noClipEnabled, "No Clip");

                GUILayout.Space(10);

                if (GUILayout.Button("Teleport to Random Player"))
                {
                    try
                    {
                        var list = PlayerControl.AllPlayerControls.ToArray();

                        var players = new List<PlayerControl>(list);
                        players.Remove(PlayerControl.LocalPlayer);

                        var target = players[UnityEngine.Random.Range(0, players.Count)];
                        PlayerControl.LocalPlayer.transform.position = target.transform.position;
                    }
                    catch
                    {
                        Debug.Log("TP failed");
                    }
                }

                walkInVents = GUILayout.Toggle(walkInVents, "Walk In Vents");

                GUILayout.Label("Visual Animations:");

                if (GUILayout.Button("Start Medbay Scan"))
                {
                    Network.SendSetScanner(true);
                }

                if (GUILayout.Button("Finish Medbay Scan"))
                {
                    Network.SendSetScanner(false);
                }
            }

            // Utility Tab
            if (selectedTab == 2)
            {
                /*if (GUILayout.Button("Test Button"))
                {
                    Debug.Log("Button clicked");
                }*/

                if (GUILayout.Button("Call Meeting"))
                {
                    if (PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        NotificationUtils.Show("You can't call a meeting while dead.", 4f);
                        return;
                    }

                    if (AmongUsClient.Instance.AmHost)
                    {
                        MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, null);
                        DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                        PlayerControl.LocalPlayer.RpcStartMeeting(null);
                    }
                    else
                    {
                        PlayerControl.LocalPlayer.CmdReportDeadBody(null);
                    }

                    NotificationUtils.Show("Meeting request sent.", 4f);
                }

                if (GUILayout.Button("Skip Meeting"))
                {
                    if (!AmongUsClient.Instance.AmHost)
                    {
                        NotificationUtils.Show("You are not the host", 4f);
                        return;
                    }

                    try
                    {
                        MeetingHud.Instance.RpcVotingComplete(new Il2CppStructArray<MeetingHud.VoterState>(0L), null, true);
                        NotificationUtils.Show("Successfully skipped the meeting", 4f);
                    }
                    catch (Exception e)
                    {
                        NotificationUtils.Show("Failed to skip the meeting", 4f);
                        Log($"Failed to skip the meeting, error: {e}");
                    }
                }

                if (GUILayout.Button("Complete All Tasks"))
                {
                    Il2CppSystem.Collections.Generic.List<PlayerTask> allTasks = PlayerControl.LocalPlayer.myTasks;

                    foreach (PlayerTask task in allTasks)
                    {
                        if (task.IsComplete)
                        {
                            Log($"Task {task.Id} has already been completed, skipping");
                            continue;
                        }

                        PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);
                        task.Complete();
                    }

                    NotificationUtils.Show("Completed all tasks successfully", 4f);
                    Log("Completed all tasks successfully!");
                }

                EmergencyMeetingPatches.UnlimitedMeetings.enabled = GUILayout.Toggle(EmergencyMeetingPatches.UnlimitedMeetings.enabled, "Unlimited Emergency Meetings");

                ChatPatches.OnChat.ShowMessagesByGhosts = GUILayout.Toggle(ChatPatches.OnChat.ShowMessagesByGhosts, "Show Messages by Ghosts");

                alwaysShowChat = GUILayout.Toggle(alwaysShowChat, "Always See Chat Button");
            }

            // Visuals Tab
            if (selectedTab == 3)
            {
                GUILayout.Label("There is no mods in this tab.");
            }

            // Players Tab
            if (selectedTab == 4)
            {
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical(GUILayout.Width(180));

                playerScroll = GUILayout.BeginScrollView(playerScroll);

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    string role = "??";

                    if (GUILayout.Button($"{player.Data.PlayerName}\n{role}", GUILayout.Height(45)))
                    {
                        selectedPlayer = player;
                    }
                }

                GUILayout.EndScrollView();

                GUILayout.EndVertical();

                GUILayout.BeginVertical();

                if (selectedPlayer != null)
                {
                    GUILayout.Label(selectedPlayer.Data.PlayerName);

                    GUILayout.Label(selectedPlayer.Data.IsDead ? "Dead" : "Alive");

                    GUILayout.Label(selectedPlayer.AmOwner ? "Local Player" : "");

                    GUILayout.Space(10);

                    if (GUILayout.Button("Teleport To"))
                    {
                        PlayerControl.LocalPlayer.transform.position = selectedPlayer.transform.position;
                    }

                    // Removed Overload cause it doesn't work to an error.
                    // if (GUILayout.Button("Overload"))
                    // {
                    //     Utils.Overload(selectedPlayer.PlayerId, 400);
                    // }

                    if (GUILayout.Button("Copy Player"))
                    {
                        Utils.CopyPlayer(selectedPlayer);
                    }

                    followTarget = selectedPlayer;
                    followPlayer = GUILayout.Toggle(followPlayer, "Follow Player");

                    if (AmongUsClient.Instance.AmHost)
                    {
                        if (GUILayout.Button("Kick"))
                        {
                            AmongUsClient.Instance.KickPlayer(selectedPlayer.PlayerId, false);
                        }

                        if (GUILayout.Button("Ban"))
                        {
                            AmongUsClient.Instance.KickPlayer(selectedPlayer.PlayerId, true);
                        }

                        if (GUILayout.Button("Revive"))
                        {
                            selectedPlayer.Revive();
                        }

                        if (GUILayout.Button("Kill"))
                        {
                            PlayerControl.LocalPlayer.RpcMurderPlayer(selectedPlayer, MurderResultFlags.Succeeded);
                        }
                    }
                }
                else
                {
                    GUILayout.Label("Select a player.");
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }

            // Console Tab
            if (selectedTab == 5)
            {
                consoleScroll = GUILayout.BeginScrollView(consoleScroll);

                foreach (string log in logs)
                {
                    GUILayout.Label(log);
                }

                GUILayout.EndScrollView();

                if (GUILayout.Button("Clear"))
                {
                    logs.Clear();
                }
            }

            // Host Tab
            if (selectedTab == 6)
            {
                if (GUILayout.Button("Force Start Game"))
                {
                    if (!AmongUsClient.Instance.AmHost)
                    {
                        NotificationUtils.Show("You are not the host", 4f);
                        return;
                    }

                    AmongUsClient.Instance.SendStartGame();
                }

                if (GUILayout.Button("Kick All"))
                {
                    if (!AmongUsClient.Instance.AmHost)
                    {
                        NotificationUtils.Show("You are not the host", 4f);
                        return;
                    }

                    foreach (var player in PlayerControl.AllPlayerControls.ToArray())
                    {
                        if (player == PlayerControl.LocalPlayer)
                        {
                            continue;
                        }

                        try
                        {
                            AmongUsClient.Instance.KickPlayer(player.PlayerId, false);
                        }
                        catch
                        {
                            NotificationUtils.Show("Failed to Kick All", 4f);
                        }
                    }
                }

                if (GUILayout.Button("Ban All"))
                {
                    if (!AmongUsClient.Instance.AmHost)
                    {
                        NotificationUtils.Show("You are not the host", 4f);
                        return;
                    }

                    foreach (var player in PlayerControl.AllPlayerControls.ToArray())
                    {
                        if (player == PlayerControl.LocalPlayer)
                        {
                            continue;
                        }

                        try
                        {
                            AmongUsClient.Instance.KickPlayer(player.PlayerId, true);
                        }
                        catch
                        {
                            NotificationUtils.Show("Failed to Ban All", 4f);
                        }
                    }
                }

                if (GUILayout.Button("Revive Self"))
                {
                    if (!AmongUsClient.Instance.AmHost)
                    {
                        NotificationUtils.Show("You are not the host", 4f);
                        return;
                    }

                    try
                    {
                        PlayerControl.LocalPlayer.Revive();
                    }
                    catch
                    {
                        NotificationUtils.Show("Failed to Revive Self", 4f);
                    }
                }

                if (GUILayout.Button("Kill All"))
                {
                    if (!AmongUsClient.Instance.AmHost)
                    {
                        NotificationUtils.Show("You are not the host", 4f);
                        return;
                    }

                    if (AmongUsClient.Instance && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Joined && AmongUsClient.Instance.AmHost)
                    {
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            if (player != PlayerControl.LocalPlayer)
                            {
                                PlayerControl.LocalPlayer.RpcMurderPlayer(player, MurderResultFlags.Succeeded);

                                foreach (var item in PlayerControl.AllPlayerControls)
                                {
                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)PlayerControl.RpcCalls.MurderPlayer, SendOption.None, AmongUsClient.Instance.GetClientIdFromCharacter(item));
                                    writer.WriteNetObject(player);
                                    writer.Write((int)MurderResultFlags.Succeeded);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                }
                            }
                        }
                    }
                }

                if (GUILayout.Button("Kill All Crewmates"))
                {
                    if (!AmongUsClient.Instance.AmHost)
                    {
                        NotificationUtils.Show("You are not the host", 4f);
                        return;
                    }

                    if (AmongUsClient.Instance && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Joined && AmongUsClient.Instance.AmHost)
                    {
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            if (player != PlayerControl.LocalPlayer)
                            {
                                if (player.Data.myRole.RoleTeamType == RoleTeamTypes.Crewmate)
                                {
                                    PlayerControl.LocalPlayer.RpcMurderPlayer(player, MurderResultFlags.Succeeded);

                                    foreach (var item in PlayerControl.AllPlayerControls)
                                    {
                                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)PlayerControl.RpcCalls.MurderPlayer, SendOption.None, AmongUsClient.Instance.GetClientIdFromCharacter(item));
                                        writer.WriteNetObject(player);
                                        writer.Write((int)MurderResultFlags.Succeeded);
                                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    }
                                }
                            }
                        }
                    }
                }

                if (GUILayout.Button("Kill All Imposters"))
                {
                    if (!AmongUsClient.Instance.AmHost)
                    {
                        NotificationUtils.Show("You are not the host", 4f);
                        return;
                    }

                    if (AmongUsClient.Instance && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Joined && AmongUsClient.Instance.AmHost)
                    {
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            if (player != PlayerControl.LocalPlayer)
                            {
                                if (player.Data.myRole.RoleTeamType == RoleTeamTypes.Impostor)
                                {
                                    PlayerControl.LocalPlayer.RpcMurderPlayer(player, MurderResultFlags.Succeeded);

                                    foreach (var item in PlayerControl.AllPlayerControls)
                                    {
                                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)PlayerControl.RpcCalls.MurderPlayer, SendOption.None, AmongUsClient.Instance.GetClientIdFromCharacter(item));
                                        writer.WriteNetObject(player);
                                        writer.Write((int)MurderResultFlags.Succeeded);
                                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }
    }
}