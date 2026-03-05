using System.Collections.Generic;
using UnityEngine;

namespace RapidGUI;

public class IMColorPreset : ScriptableObject {

    [field: SerializeField]
    public List<Color> Colors { get; } = new();

    public void Save(Color color) => Colors.Add(color);

    public void Remove(int index) => Colors.RemoveAt(index);

}

