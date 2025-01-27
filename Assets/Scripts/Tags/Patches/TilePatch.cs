using MonsterLove.StateMachine;
using SA.GoogleDoc;
using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Overlayer.Tags.Patches
{
    public class TilePatch
    {
        [HarmonyPatch(typeof(scnGame), "Update")]
        public static class TileCountPatch
        {
            public static void Postfix()
            {
                Tile.CurTile = scrController.instance.currentSeqID + 1;
                Tile.TotalTile = ADOBase.lm.listFloors.Count;
                Tile.LeftTile = Tile.TotalTile - Tile.CurTile;
            }
        }
        [HarmonyPatch(typeof(MonsterLove.StateMachine.StateBehaviour), "ChangeState", new Type[] { typeof(Enum) })]
        public static class StartTileAndProgressPatch
        {
            public static void Prefix(StateBehaviour __instance, Enum newState)
            {
                if (__instance is not scrController ctrl) return;
                var cur = __instance.stateMachine.GetState();
                if (cur != newState && (States)newState == States.PlayerControl)
                    SetStartTileProg(ctrl);
            }
            public static void SetStartTileProg(scrController ctrl)
            {
                if (!Tile.IsStarted)
                {
                    Tile.IsStarted = true;
                    if (ctrl.gameworld)
                    {
                        Tile.StartProgress = ctrl.percentComplete * 100;
                        Tile.StartTile = ctrl.currentSeqID + 1;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(scnEditor), "ResetScene")]
        public static class StartedResetter
        {
            public static void Postfix()
            {
                Tile.IsStarted = false;
            }
        }
        [HarmonyPatch(typeof(scnGame), "ResetScene")]
        public static class StartedResetter2
        {
            public static void Postfix()
            {
                Tile.IsStarted = false;
            }
        }
    }
}
