using System;
using IFPV.Values;
using NetScriptFramework.SkyrimSE;

namespace IFPV.Values
{
    internal sealed class NearClip : CameraValueBase
    {
        private static Setting _setting;

        private double? _defaultValue;

        internal NearClip() { Flags |= CameraValueFlags.NoTween; }

        internal override double ChangeSpeed => 1.0;

        internal override double CurrentValue
        {
            get
            {
                UpdateDefaultValue();
                return _setting.GetFloat();
            }

            set
            {
                UpdateDefaultValue();
                _setting.SetFloat((float) value);
            }
        }

        internal override double DefaultValue
        {
            get
            {
                UpdateDefaultValue();
                return _defaultValue.Value;
            }
        }

        internal override string Name => "Near clip";

        private void UpdateDefaultValue()
        {
            if (_defaultValue.HasValue)
                return;

            _setting = Setting.FindSettingByName("fNearDistance:Display", true, true);
            if (_setting == null)
                throw new InvalidOperationException("Failed to find fNearDistance setting!");

            _defaultValue = _setting.GetFloat();
        }
    }
}

namespace IFPV
{
    internal partial class CameraValueMap
    {
        internal readonly NearClip NearClip = new NearClip();
    }
}