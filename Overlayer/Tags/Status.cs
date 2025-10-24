using Overlayer.Tags.Attributes;
using System;
using UnityEngine;

namespace Overlayer.Tags
{
    public static class Status
    {
        [Tag]
        public static bool IsAutoEnabled => ADOFAI.RDC?.auto ?? false;
        [Tag]
        public static bool IsPracticeModeEnabled => ADOFAI.RDC?.practice ?? false;
        [Tag]
        public static bool IsOldAutoEnabled => ADOFAI.RDC?.useOldAuto ?? false;
        [Tag]
        public static bool IsNoFailEnabled => ADOFAI.Controller?.noFail ?? GCS.useNoFail;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double Progress() => scrController.instance?.percentComplete * 100 ?? 0;
        [Tag(NotPlaying = true)]
        public static int Deaths() => scrController.deaths;
        [Tag]
        public static int Attempts;

        public static void Attempts_Update() {
            if(scnGame.instance == null) {
                if(scrController.instance != null && scrConductor.instance != null) {
                    if(ADOBase.sceneName.Contains("-") && !scrController.instance.noFail && scrConductor.instance.isGameWorld) {
                        Attempts = Persistence.GetWorldAttempts(scrController.currentWorld);
                    } else {
                        Attempts = 0;
                    }
                } else {
                    Attempts = 0;
                }
            } else {
                var level = ADOFAI.LevelData;
                if(level != null) {
                    Attempts = Persistence.GetCustomWorldAttempts(
                        MD5Hash.GetHash(level.author + level.artist + level.song)
                    );
                } else {
                    Attempts = 0;
                }
            }
        }
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double ActualProgress()
        {
            var listFloors = scrLevelMaker.instance?.listFloors;
            if(listFloors == null || listFloors.Count == 0) {
                return 0;
            }
            var firstFloorTime = listFloors[1].entryTime;
            var lastFloorTime = listFloors[listFloors.Count - 1].entryTime;
            var actualProgress = (scrController.instance?.currFloor.entryTime - firstFloorTime) / (lastFloorTime - firstFloorTime) * 100;
            if(actualProgress == null) {
                return 0;
            }
            return Mathf.Clamp((float)actualProgress, 0, 100);
        }

        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double Pitch() => GCS.currentSpeedTrial;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double EditorPitch() => ((ADOFAI.LevelData?.pitch ?? 0) / 100.0);
        [Tag]
        public static double BestProgress;

        public static void BestProgress_Reset() {
            BestProgress = 0;
        }

        public static void BestProgress_Update() {
            if(scrLevelMaker.instance == null) {
                return;
            }
            BestProgress = Math.Max(BestProgress, scrController.instance.percentComplete * 100);
        }

        public static void BestProgress_Fix() {
            if(scrController.instance.gameworld) {
                BestProgress = 100;
            }
        }
    }
}
