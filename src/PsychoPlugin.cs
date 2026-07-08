using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PsychoMenuCU
{
    [BepInPlugin("bery.psycho.menucu", "PsychoMenu-CU", "1.0.0")]
    [BepInProcess("Classic Us.exe")]
    public partial class PsychoPlugin : BasePlugin
    {
        public Harmony Harmony = new Harmony("bery.psycho.menu");
        public static List<string> supportedVersions = new List<string> { "2026.6.5", "2026.3.31" };

        public static String version = "1.0.0";
        public static String author = "https://github.com/BeryTBH";

        public override void Load()
        {
            Harmony.PatchAll();

            AddComponent<Menu>();

            //SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) =>
            //{
            //    if (scene.name == "MainMenu")
            //    {
            //        // Warn about unsupported versions
            //        if (!supportedVersions.Contains(Application.version))
            //        {
            //            Utils.ShowPopup("\nThis version of PsychoMenu and this version of Among Us are incompatible\ntogether\n\nInstall the newest version of Among Us to avoid problems using the menu.");
            //        }
            //    }
            //}));
        }
    }
}
