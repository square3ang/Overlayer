using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RapidGUI;

public static partial class RGUI {
    private static bool CheckIgnoreField(string label) {
        return ignoreFieldStack.Any(set => set.Contains(label));
    }

    private static readonly Stack<HashSet<string>> ignoreFieldStack = new();

    public static void BeginIgnoreField(params string[] fieldNames) {
        ignoreFieldStack.Push([.. fieldNames]);
    }

    public static void EndIgnoreField() {
        ignoreFieldStack.Pop();
    }

    public class IgnoreFieldScope : GUI.Scope {
        public IgnoreFieldScope(params string[] fieldNames) {
            BeginIgnoreField(fieldNames);
        }

        protected override void CloseScope() {
            EndIgnoreField();
        }
    }
}