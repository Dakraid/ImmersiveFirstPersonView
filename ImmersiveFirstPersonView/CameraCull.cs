using System;
using System.Collections.Generic;
using NetScriptFramework;
using NetScriptFramework.SkyrimSE;

namespace IFPV
{
    internal sealed class CameraCull
    {
        internal readonly CameraMain      CameraMain;
        private readonly  HashSet<IntPtr> _put_back = new HashSet<IntPtr>();

        private readonly List<Tuple<NiAVObject, int>> Disabled = new List<Tuple<NiAVObject, int>>();

        private readonly object Locker = new object();

        private readonly List<Tuple<NiAVObject, float>> Unscaled = new List<Tuple<NiAVObject, float>>();

        private int _state_cull;
        private int _state_update;

        internal CameraCull(CameraMain cameraMain)
        {
            if (cameraMain == null)
                throw new ArgumentNullException("cameraMain");

            CameraMain = cameraMain;
        }

        internal static float UnscaleAmount { get; set; } = 0.00087f;

        private bool ShouldObjectBeDisabled => _state_cull <= 0;

        private bool ShouldObjectBeUnscaled => _state_cull <= 0 && _state_update <= 0;

        internal void AddDisable(NiAVObject obj)
        {
            if (obj == null)
                return;

            obj.IncRef();
            var reset = 0;
            if (ShouldObjectBeDisabled)
            {
                reset = IsEnabled(obj) ? 1 : -1;
                if (reset > 0)
                    SetEnabled(obj, false);
            }

            lock (Locker) { Disabled.Add(new Tuple<NiAVObject, int>(obj, reset)); }
        }

        internal void AddUnscale(NiAVObject obj)
        {
            if (obj == null)
                return;

            obj.IncRef();
            var orig = obj.LocalTransform.Scale;
            if (orig == UnscaleAmount)
                orig = 1.0f;
            if (ShouldObjectBeUnscaled)
                SetScale(obj, UnscaleAmount, true);

            lock (Locker) { Unscaled.Add(new Tuple<NiAVObject, float>(obj, orig)); }
        }

        internal void Clear()
        {
            lock (Locker)
            {
                foreach (var s in Disabled)
                {
                    if (s.Item2 > 0)
                        SetEnabled(s.Item1, true);
                    s.Item1.DecRef();
                }

                Disabled.Clear();

                foreach (var t in Unscaled)
                {
                    SetScale(t.Item1, t.Item2, true);
                    t.Item1.DecRef();
                }

                Unscaled.Clear();
            }
        }

        internal void OnShadowCulling(int index)
        {
            lock (Locker)
            {
                if (index == 0)
                    IncCull();
                else if (index == 1)
                    DecCull();
            }
        }

        internal void OnUpdating(int index)
        {
            lock (Locker)
            {
                if (index == 0)
                    IncUpdate();
                else if (index == 1)
                    DecUpdate();
            }
        }

        internal void RemoveDisable(NiAVObject obj)
        {
            if (obj == null)
                return;

            var had   = false;
            var reset = 0;
            lock (Locker)
            {
                var addr = obj.Address;
                for (var i = 0; i < Disabled.Count; i++)
                    if (Disabled[i].Item1.Address == addr)
                    {
                        reset = Disabled[i].Item2;
                        Disabled.RemoveAt(i);
                        had = true;
                        break;
                    }
            }

            if (!had)
                return;

            if (reset > 0)
                SetEnabled(obj, true);
            obj.DecRef();
        }

        private void DecCull()
        {
            if (--_state_cull != 0)
                return;

            foreach (var s in Disabled)
                if (_put_back.Contains(s.Item1.Address))
                    SetEnabled(s.Item1, false);

            _put_back.Clear();

            if (_state_update <= 0)
                foreach (var t in Unscaled)
                    SetScale(t.Item1, UnscaleAmount, true);
        }

        private void DecUpdate()
        {
            if (--_state_update != 0)
                return;

            if (_state_cull <= 0)
                foreach (var t in Unscaled)
                    SetScale(t.Item1, UnscaleAmount, false);
        }

        private void IncCull()
        {
            if (++_state_cull != 1)
                return;

            for (var i = 0; i < Disabled.Count; i++)
            {
                var s     = Disabled[i];
                var reset = s.Item2;
                if (reset == 0)
                {
                    reset       = IsEnabled(s.Item1) ? 1 : -1;
                    Disabled[i] = new Tuple<NiAVObject, int>(s.Item1, reset);
                }

                if (!IsEnabled(s.Item1) && reset > 0)
                {
                    SetEnabled(s.Item1, true);
                    _put_back.Add(s.Item1.Address);
                }
            }

            if (_state_update <= 0)
                foreach (var t in Unscaled)
                    SetScale(t.Item1, t.Item2, true);
        }

        private void IncUpdate()
        {
            if (++_state_update != 1)
                return;

            if (_state_cull <= 0)
                foreach (var t in Unscaled)
                    SetScale(t.Item1, t.Item2, false);
        }

        private bool IsEnabled(NiAVObject obj)
        {
            var fl         = Memory.ReadUInt32(obj.Address + 0xF4);
            var hadEnabled = (fl & 1) == 0;
            return hadEnabled;
        }

        private void SetEnabled(NiAVObject obj, bool enabled)
        {
            if (obj.Parent == null)
                return;

            var fl         = Memory.ReadUInt32(obj.Address + 0xF4);
            var hadEnabled = (fl & 1) == 0;
            if (hadEnabled == enabled)
                return;

            if (enabled)
                fl &= ~(uint) 1;
            else
                fl |= 1;

            Memory.WriteUInt32(obj.Address + 0xF4, fl);
        }

        private void SetScale(NiAVObject obj, float scale, bool cull)
        {
            if (obj.Parent == null)
                return;

            obj.LocalTransform.Scale = scale;
            obj.Update(0.0f);
        }
    }
}