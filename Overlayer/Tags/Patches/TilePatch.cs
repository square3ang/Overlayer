using MonsterLove.StateMachine;
using Overlayer.Core.Patches;
using SA.GoogleDoc;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Overlayer.Tags.Patches
{
    public class TilePatch : PatchBase<TilePatch>
    {
        [LazyPatch("Tags.Tile.StartTile&ProgressPatch", "MonsterLove.StateMachine.StateBehaviour", "ChangeState", new string[] { "System.Enum" }, Triggers = new string[]
        {
            nameof(Tile.StartTile), nameof(Tile.StartProgress), nameof(Tile.IsStarted),
            nameof(Status.MaxXAccuracy), nameof(Status.AbsMaxXAccuracy),
        })]
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
        [LazyPatch("Tags.Tile.StartedResetter_scnEditor", "scnEditor", "ResetScene", Triggers = new string[]
        {
            nameof(Tile.StartTile), nameof(Tile.StartProgress)
        })]
        [LazyPatch("Tags.Tile.StartedResetter_scnGame", "scnGame", "ResetScene", Triggers = new string[]
        {
            nameof(Tile.StartTile), nameof(Tile.StartProgress)
        })]
        public static class StartedResetter
        {
            public static void Postfix()
            {
                Tile.IsStarted = false;
            }
        }
    }
}
