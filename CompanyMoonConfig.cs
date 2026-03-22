using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;

namespace CompanyMoonConfig
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class CompanyMoonConfig : BaseUnityPlugin
    {
        public static CompanyMoonConfig Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;

        public static bool mapEnabled;
        public static bool localVolumetricFogEnabled;
        public static float localVolumetricFogThinness;
        public static bool sunWithShadowsEnabled;
        public static float sunBrightness;
        public static bool indirectEnabled;
        public static float indirectBrightness;
        public static bool skyAndFogGlobalVolumeEnabled;
        public static float fogGlobalVolumeWeight;
        public static bool sellDeskPlaneEnabled;


        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            mapEnabled = Config.Bind<bool>("General", "CompanyMapEnabled", true, "Should the comany moon map object and related props be enabled?").Value;
            sellDeskPlaneEnabled = Config.Bind<bool>("General", "SellDeskPlaneEnabled", true, "Should the plane behind the sell desk be enabled?").Value;
            localVolumetricFogEnabled = Config.Bind<bool>("Lighting", "LocalVolumetricFog", false, "Should local volumetric fog be enabled?").Value;
            localVolumetricFogThinness = Config.Bind<float>("Lighting", "LocalVolumetricFogThinness", 45f, "How thin should the fog be? Lower values make fog thicker.").Value;
            sunWithShadowsEnabled = Config.Bind<bool>("Lighting", "SunWithShadows", true, "Should the sun object be enabled?").Value;
            sunBrightness = Config.Bind<float>("Lighting", "SunBrightness", 1.972f, "How bright should the sunlight be?").Value;
            indirectEnabled = Config.Bind<bool>("Lighting", "Indirect", true, "Should indirect lighting be enabled?").Value;
            indirectBrightness = Config.Bind<float>("Lighting", "IndirectBrightness", 2f, "How bright should the indirect light be?").Value;
            skyAndFogGlobalVolumeEnabled = Config.Bind<bool>("Lighting", "SkyAndFogGlobalVolume", true, "Should sky and global fog be enabled?").Value;
            fogGlobalVolumeWeight = Config.Bind<float>("Lighting", "GlobalFogVolumeWeight", 1f, new ConfigDescription("How strong should the fog be? 0 disables the object.", new AcceptableValueRange<float>(0f, 1f))).Value;

            SceneManager.sceneLoaded += OnSceneLoaded;

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "CompanyBuilding")
            {
                return;
            }
            Transform environment = null!;
            foreach (GameObject g in scene.GetRootGameObjects())
            {
                if (g.name == "Environment")
                {
                    environment = g.transform;
                }
                if (g.name == "Plane")
                {
                    g.SetActive(sellDeskPlaneEnabled);
                }
            }
            if (environment == null)
            {
                Logger.LogError("Environment not found");
                return;
            }
            Transform? mapObject = environment.Find("Map");
            mapObject?.gameObject.SetActive(mapEnabled);
            environment.Find("LightsContainer")?.gameObject.SetActive(mapEnabled);
            environment.Find("Interactables")?.gameObject.SetActive(mapEnabled);
            Transform? volumetricFogObject = environment.Find("Lighting")?.Find("BrightDay")?.Find("Local Volumetric Fog");
            if (volumetricFogObject != null)
            {
                volumetricFogObject.gameObject.SetActive(localVolumetricFogEnabled);
                volumetricFogObject.GetComponent<LocalVolumetricFog>().parameters.meanFreePath = localVolumetricFogThinness;
            }
            Transform? sunAnimObject = environment.Find("Lighting")?.Find("BrightDay")?.Find("Sun").GetChild(0);
            Transform? sunObject = sunAnimObject?.Find("SunWithShadows");
            Transform? indirectObject = sunAnimObject?.Find("Indirect");
            Transform? skyAndFogGlobalVolumeObject = sunAnimObject?.Find("Sky and Fog Global Volume");
            if (sunObject != null)
            {
                sunObject.gameObject.SetActive(sunWithShadowsEnabled);
                sunObject.GetComponent<Light>().intensity = sunBrightness;
            }
            if (indirectObject != null)
            {
                indirectObject.gameObject.SetActive(indirectEnabled);
                indirectObject.GetComponent<Light>().intensity = indirectBrightness;
            }
            skyAndFogGlobalVolumeObject?.gameObject.SetActive(skyAndFogGlobalVolumeEnabled);
            Volume? globalVolume = sunAnimObject?.Find("Sky and Fog Global Volume").GetComponent<Volume>();
            if (globalVolume != null)
            {
                globalVolume.weight = fogGlobalVolumeWeight;
            }
        }
    }
}
