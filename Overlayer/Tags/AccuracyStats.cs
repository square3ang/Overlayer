using Overlayer.Tags.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overlayer.Tags {
    public static class AccuracyStats {
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double Accuracy;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double XAccuracy;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double AbsXAccuracy;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double MaxAccuracy;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double MaxXAccuracy;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double AbsMaxXAccuracy;

        public static void Reset() {
            Accuracy = XAccuracy = MaxAccuracy = AbsXAccuracy = MaxXAccuracy = AbsMaxXAccuracy = double.NaN;
        }
    }
}
