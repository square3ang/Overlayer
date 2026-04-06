using System;
using System.Collections.Generic;
using DG.Tweening;

namespace Overlayer.Core;

public class EventEase {
    private static readonly Dictionary<string, double> valueCache = [];
    private static readonly Dictionary<string, double> pvalueCache = [];
    private static readonly Dictionary<string, long> startTimeCache = [];
    public Func<double> Getter { get; }
    public Ease Ease { get; set; }
    public double Speed { get; set; }
    public bool Invert { get; set; }
    public double Value => Getter();

    public EventEase(Func<double> getter, Ease ease = Ease.Linear, double speed = 500, bool invert = false) {
        Getter = getter;
        Ease = ease;
        Speed = speed;
        Invert = invert;
    }

    /// <summary>
    ///     Compute Ease Of Value
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public double Compute(string id) {
        var val = Value;
        valueCache.TryGetValue(id, out var vCache);
        startTimeCache.TryGetValue(id, out var stCache);
        var mills = FastDateTime.Now.Ticks / 10000;
        if (val != vCache) {
            startTimeCache[id] = stCache = mills;
            pvalueCache[id] = vCache;
            valueCache[id] = val;
        }

        var elapsed = mills - stCache;
        if (elapsed < Speed) {
            var eased = DOVirtual.EasedValue(0, 1, (float)(elapsed / Speed), Ease);
            return Invert ? 1 - eased : eased;
        }

        return Invert ? 0 : 1;
    }

    public double GetPrevValue(string id) {
        return pvalueCache.TryGetValue(id, out var vCache) ? vCache : Value;
    }
}