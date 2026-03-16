using Overlayer.Models;
using UnityEngine;

namespace Overlayer.Unity;

public abstract class OverlayerObject : MonoBehaviour {
    public OverlayerProfile Parent;
    public abstract ObjectConfig Config { get; }
    public abstract void ApplyConfig();
}