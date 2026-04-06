using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace RapidGUI;

/// <summary>
///     FindObjectOfTypeを呼びまくるのは重いので適度に散らす
/// </summary>
public class LazyFindObject {
    protected Object _obj;
    protected Type _type;
    protected int _delayCount;
    private const int _delayCountMax = 60;

    public LazyFindObject(Type type) {
        _type = type;
    }

    public Object GetObject() {
        if (Event.current.type == EventType.Layout && _obj == null)
            if (--_delayCount <= 0) {
                _obj = Object.FindObjectOfType(_type);
                _delayCount = Random.Range(0, _delayCountMax);
            }

        return _obj;
    }
}