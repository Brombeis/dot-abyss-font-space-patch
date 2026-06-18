using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Project.Novel;

namespace FontSpacePatch;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public override void Load()
    {
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
        Log.LogInfo("FontSpacePatch loaded");
    }
}

[HarmonyPatch(typeof(NovelText), nameof(NovelText.Initialize))]
static class NovelText_Initialize_Patch
{
    [HarmonyPostfix]
    static void Postfix(NovelText __instance)
    {
        __instance.SetFontSpace(-6f);
    }
}

[HarmonyPatch(typeof(NovelText), nameof(NovelText.SetParam))]
static class NovelText_SetParam_Patch
{
    [HarmonyPostfix]
    static void Postfix(NovelText __instance)
    {
        __instance.SetFontSpace(-6f);
    }
}
