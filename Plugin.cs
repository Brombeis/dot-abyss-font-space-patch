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

// Both Initialize and SetParam need to be patched because the game calls
// SetParam after Initialize and would otherwise reset our values.
// Only sizes 30, 36, 40, 48 are valid — anything else crashes GetFontSize().

[HarmonyPatch(typeof(NovelText), nameof(NovelText.Initialize))]
static class NovelText_Initialize_Patch
{
    [HarmonyPostfix]
    static void Postfix(NovelText __instance)
    {
        __instance.SetFontSpace(2f);          // slight gap so proportional letters don't squash together
        __instance.SetFontSize(30f);          // smallest valid size (enum: Small=30, R18Mid=36, Mid=40, Big=48)
        __instance.SetUseProportional(true);  // variable-width positioning instead of monospaced grid
        __instance.SetMaxLength(200);         // raised from 37 because each space is 7 chars (see below)
    }
}

[HarmonyPatch(typeof(NovelText), nameof(NovelText.SetParam))]
static class NovelText_SetParam_Patch
{
    [HarmonyPostfix]
    static void Postfix(NovelText __instance)
    {
        __instance.SetFontSpace(2f);
        __instance.SetFontSize(30f);
        __instance.SetUseProportional(true);
        __instance.SetMaxLength(200);
    }
}

// Proportional mode has a bug where spaces render ~1/7th their normal width.
// This can't be fixed in the layout code (it's native IL2CPP inside Show()).
// Expanding each space to 7 spaces at Parse time (not in translation files)
// keeps the text log clean — AddLog receives the original single-space string.

[HarmonyPatch(typeof(NovelText), nameof(NovelText.Parse))]
static class NovelText_Parse_Patch
{
    [HarmonyPrefix]
    static void Prefix(ref string message)
    {
        message = message.Replace(" ", "       ");
    }
}

// The 7× space expansion creates a new problem: the typing animation pauses
// on each space, so a single word gap causes 7× the normal delay.
// This patch skips the text counter past consecutive spaces in one frame.
//
// Important: the typing animation lives in NovelViewMessageWindow, NOT in
// NovelMessageTextComponent (which handles center/UI text only).

[HarmonyPatch(typeof(NovelViewMessageWindow), nameof(NovelViewMessageWindow.OnViewUpdate))]
static class NovelViewMessageWindow_OnViewUpdate_Patch
{
    [HarmonyPostfix]
    static void Postfix(NovelViewMessageWindow __instance)
    {
        if (!__instance._isPlay) return;
        var letters = __instance._letters;
        if (letters == null) return;

        // _textCount is a float that the game increments each frame by speed*deltaTime.
        // When it crosses an integer boundary, the next letter is revealed.
        // We jump it forward past any run of spaces so they all appear at once.
        var idx = (int)__instance._textCount;
        var startIdx = idx;
        while (idx < letters.Count && !letters[idx].isNewLineCode && letters[idx].rawText == " ")
        {
            idx++;
        }
        if (idx != startIdx)
        {
            __instance._textCount = idx;
            __instance._prevCount = idx;
        }
    }
}
