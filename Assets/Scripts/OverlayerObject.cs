using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Overlayer
{
    public class OverlayerObject : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private GameObject outline;

        [NonSerialized] public string Id;
        
        public string Name;

        public bool DragLock = false;

        private float xoff;

        private float yoff;

        [NonSerialized] public bool Configuring;
        [NonSerialized] public RectTransform _rectTransform;

        internal void Awake()
        {
            Id = Guid.NewGuid().ToString();
            _rectTransform ??= GetComponent<RectTransform>();
        }

        public virtual JObject ToJson()
        {
            var jo = new JObject();
            jo["Name"] = Name;
            jo["Type"] = "undefined";
            jo["Locked"] = DragLock;
            jo["X"] = transform.localPosition.x;
            jo["Y"] = transform.localPosition.y;
            jo["Rotation"] = transform.eulerAngles.z;
            jo["PivotX"] = _rectTransform.pivot.x;
            jo["PivotY"] = _rectTransform.pivot.y;
            return jo;
        }
        
        public virtual void FromJson(JObject jo)
        {
            Name = jo["Name"].Value<string>();
            DragLock = jo["Locked"].Value<bool>();
            transform.localPosition = new Vector3(jo["X"].Value<float>(), jo["Y"].Value<float>(), 0);
            transform.eulerAngles = new Vector3(0, 0, jo["Rotation"].Value<float>());
            _rectTransform.pivot = new Vector2(jo["PivotX"].Value<float>(), jo["PivotY"].Value<float>());
        }

        // Update is called once per frame
        internal void Update()
        {
            outline.SetActive(OverlayerSettings.open);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!OverlayerSettings.open || DragLock) return;
            var canvpos = eventData.position / Screen.height * 1080;
            xoff = transform.localPosition.x - canvpos.x;
            yoff = transform.localPosition.y - canvpos.y;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!OverlayerSettings.open || DragLock) return;
            var canvpos = eventData.position / Screen.height * 1080;
            transform.localPosition = new Vector3(canvpos.x + xoff, canvpos.y + yoff, 0);
        }
    }
}
