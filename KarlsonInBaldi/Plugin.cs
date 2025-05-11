using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Rewired;
using Steamworks;
using UnityEngine;
using UnityEngine.UIElements;
namespace KarlsonInBaldi
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        bool IsPlus = true;
        public void Start()
        {
            // Plugin startup logic
            var harmony = new Harmony("com.64BitDev.KarlsonMod");
            harmony.PatchAll();

            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Logger.LogInfo($"Appid:{SteamUtils.GetAppID().m_AppId}");
            DontDestroyOnLoad(new GameObject("InputAPI").AddComponent<InputApi>().gameObject);
        }


        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                InputApi.Inst.StartCoroutine(InputApi.Inst.SpawnPlayerInTenSecs());
            }
        }


    }

}
