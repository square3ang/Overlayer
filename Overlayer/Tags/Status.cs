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
                if(scnEditor.instance == null) {
                    var level = ADOFAI.LevelData;
                    if(level != null) {
                        Attempts = Persistence.GetCustomWorldAttempts(
                            MD5Hash.GetHash(level.author + level.artist + level.song)
                        );
                    } else {
                        Attempts = 0;
                    }
                } else {
                    Attempts = 0;
                }
            }
        }

        public static void Attempts_UpdateGame() {
            var level = ADOFAI.LevelData;
            Attempts = Persistence.GetCustomWorldAttempts(
                MD5Hash.GetHash(level.author + level.artist + level.song)
            );
        }

        public static void Attempts_UpdateOfficial() {
            if(ADOBase.sceneName.Contains("-") && !scrController.instance.noFail && scrConductor.instance.isGameWorld) {
                Attempts = Persistence.GetWorldAttempts(scrController.currentWorld);
            } else {
                Attempts = 0;
            }
        }

        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double Pitch() => GCS.currentSpeedTrial;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double EditorPitch() => ((ADOFAI.LevelData?.pitch ?? 0) / 100.0);
    }
}
