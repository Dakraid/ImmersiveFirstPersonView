using System;
using System.Collections.Generic;
using System.Reflection;

namespace IFPV
{
    internal sealed partial class CameraValueMap
    {
        internal readonly CameraMain CameraMain;

        private readonly List<CameraValueBase> values = new List<CameraValueBase>();

        internal CameraValueMap(CameraMain cameraMain)
        {
            if (cameraMain == null)
                throw new ArgumentNullException("cameraMain");

            CameraMain = cameraMain;

            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var f in fields)
            {
                var ft = f.FieldType;
                if (ft != typeof(CameraValueBase) && !ft.IsSubclassOf(typeof(CameraValueBase)))
                    continue;

                var val = f.GetValue(this) as CameraValueBase;
                if (val != null)
                    values.Add(val);
            }
        }

        internal void Reset()
        {
            foreach (var v in values)
                v.Reset();
        }

        internal void Update(long now, bool enabled)
        {
            foreach (var v in values)
                v.Update(now, enabled);
        }
    }
}