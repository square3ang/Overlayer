﻿using ADOFAI;
using Overlayer.Tags.Attributes;

namespace Overlayer.Tags
{
    public static class ADOFAI
    {
        //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
        public static LevelData LevelData => scnGame.instance?.levelData ?? scnEditor.instance?.levelData;
        //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
        public static RDConstants RDC => RDConstants.data;
        //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
        public static scrController Controller => scrController.instance;
        //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
        public static scrConductor Conductor => scrConductor.instance;
        //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
        public static scrLevelMaker LevelMaker => scrLevelMaker.instance;
        //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
        public static scrPlanet ChosenPlanet => Controller?.chosenplanet;
        //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
        public static scrPlanet RedPlanet => Controller?.redPlanet;
        //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
        public static scrPlanet BluePlanet => Controller?.bluePlanet;
        //[Tag(NotPlaying = true)]
        public static scrPlanet OtherPlanet(int index) => Controller?.allPlanets[index];
        //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
        public static scnCLS CLS => scnCLS.instance;
        //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
        public static scnEditor Editor => scnEditor.instance;
        //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
        public static scnGame CustomLevel => scnGame.instance;
        //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
        public static scrFloor CurrentFloor => Controller?.currFloor;
        //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
        public static scrMistakesManager JudgementManager => Controller?.mistakesManager;
    }
}
