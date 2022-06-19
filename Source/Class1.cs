using System;
using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;


namespace ViewInfoHotkey
{

    // reference the XML setting
    [DefOf]
    public static class VIHKeyBingings
    {
        public static KeyBindingDef ViewInfo;
    }


    // Closing InfoCard is handled in the class that inherits GameComponent
    // Opening InfoCard is handled in the patch (with CD from the above class)


    [StaticConstructorOnStartup]
    static class InfoCardButtonPatcher
    {
        static InfoCardButtonPatcher()
        {
            var harmony = new Harmony("ViewInfoCardHotkey.elfxiong");
            harmony.PatchAll();

            // some relavant methods (probably only meed to patch one of them):
            // Find.WindowStack.Add(new Dialog_InfoCard(...))
            // Widget.InfoCardButton
            // Widget.InfoCardButtonWorker

        }

    }

    [HarmonyPatch(typeof(Widgets))]
    class Patch
    {

        //private static int cd = 0;

        [HarmonyPostfix]
        [HarmonyPatch("InfoCardButtonWorker")]
        [HarmonyPatch(new Type[] { typeof(Rect) })]
        static void workerPostfix(ref bool __result)
        {
            if (!InfoCardManager.ready()) { return; }
            /**
#if DEBUG

            if (Event.current.isKey || VIHKeyBingings.ViewInfo.JustPressed || VIHKeyBingings.ViewInfo.KeyDownEvent || VIHKeyBingings.ViewInfo.IsDown || VIHKeyBingings.ViewInfo.IsDownEvent)
            {
                Log.Message($"isKey {Event.current.isKey} \tjustPressed {VIHKeyBingings.ViewInfo.JustPressed} \tkeyDown {VIHKeyBingings.ViewInfo.KeyDownEvent} \tisDown {VIHKeyBingings.ViewInfo.IsDown} \tisDownEvent {VIHKeyBingings.ViewInfo.IsDownEvent} \tisDown {VIHKeyBingings.ViewInfo.IsDown} \teventType {Event.current.type} \t");
            }
#endif
            **/
            if (VIHKeyBingings.ViewInfo.JustPressed)
            {
#if DEBUG
                Log.Message("Opening InfoCard");
                //Log.Message("Setting Worker return type to True");
#endif
                __result = true;
                InfoCardManager.resetCD();
            }
        }

    }
    class InfoCardManager : GameComponent
    {

        public static int cooldown = 0;
        public const int COOLDOWN = 10;
        public InfoCardManager(Game g) : base() { }

        public static void decrementCD() { if (cooldown > 0) { cooldown -= 1; } }
        public static bool ready() { return cooldown <= 0; }
        public static void resetCD() { cooldown = COOLDOWN; }

        public override void GameComponentOnGUI()
        {
            decrementCD();
            // If JustPressed has be handled recently, do nothing
            if (!ready()) { return; }
            // If the hotkey is not pressed, do nothing
            if (!VIHKeyBingings.ViewInfo.JustPressed) { return; }


            // If an InfoCard window is already open, close it instead
            Dialog_InfoCard currentInfoCard = Find.WindowStack.WindowOfType<Dialog_InfoCard>();
            if (currentInfoCard != null)
            {
                currentInfoCard.Close();
                resetCD();
#if DEBUG
                Log.Message("Closing InfoCard");
#endif
                return;
            }
        }
    }

}
