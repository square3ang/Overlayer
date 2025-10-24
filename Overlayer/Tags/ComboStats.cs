using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overlayer.Tags {
    public static class ComboStats {
        [Tag]
        public static int Combo;
        [Tag]
        public static int MaxCombo;

        #region MarginCombo
        [Tag]
        public static int LMarginCombo(HitMargin margin) => Combos[(int)Difficulty.Lenient][(int)margin];
        [Tag]
        public static int NMarginCombo(HitMargin margin) => Combos[(int)Difficulty.Normal][(int)margin];
        [Tag]
        public static int SMarginCombo(HitMargin margin) => Combos[(int)Difficulty.Strict][(int)margin];
        [Tag]
        public static int MarginCombo(HitMargin margin) => Combos[(int)GCS.difficulty][(int)margin];
        #endregion

        #region MarginMaxCombo
        [Tag]
        public static int LMarginMaxCombo(HitMargin margin) => MaxCombos[(int)Difficulty.Lenient][(int)margin];
        [Tag]
        public static int NMarginMaxCombo(HitMargin margin) => MaxCombos[(int)Difficulty.Normal][(int)margin];
        [Tag]
        public static int SMarginMaxCombo(HitMargin margin) => MaxCombos[(int)Difficulty.Strict][(int)margin];
        [Tag]
        public static int MarginMaxCombo(HitMargin margin) => MaxCombos[(int)GCS.difficulty][(int)margin];
        #endregion

        public static int[][] Combos = new int[EnumHelper<Difficulty>.GetValues().Length][];
        public static int[][] MaxCombos = new int[EnumHelper<Difficulty>.GetValues().Length][];

        public static void Combos_Set(Difficulty diff, HitMargin hit) {
            int iHit = (int)hit;
            int[] combos = Combos[(int)diff];
            int[] maxCombos = MaxCombos[(int)diff];
            combos[iHit]++;
            for(int i = 0; i < combos.Length; i++) {
                if(i != iHit) {
                    combos[i] = 0;
                }
            }
            for(int i = 0; i < maxCombos.Length; i++) {
                maxCombos[i] = Math.Max(maxCombos[i], combos[i]);
            }
        }

        public static void Reset() {
            Combo = MaxCombo = 0;

            int margins = EnumHelper<HitMargin>.GetValues().Length;
            for(int i = 0; i < Combos.Length; i++) {
                Combos[i] = new int[margins];
            }
            for(int i = 0; i < MaxCombos.Length; i++) {
                MaxCombos[i] = new int[margins];
            }
        }
    }
}
