namespace IFPV.Values
{
    using System;
    using NetScriptFramework.SkyrimSE;

    internal sealed class NearClip : CameraValueBase
    {
        private static Setting _setting;

        private double? _defaultValue;

        internal NearClip() => this.Flags |= CameraValueFlags.NoTween;

        internal override double ChangeSpeed => 1.0;

        internal override double CurrentValue
        {
            get
            {
                this.UpdateDefaultValue();
                return _setting.GetFloat();
            }

            set
            {
                this.UpdateDefaultValue();
                _setting.SetFloat((float)value);
            }
        }

        internal override double DefaultValue
        {
            get
            {
                this.UpdateDefaultValue();
                return this._defaultValue.Value;
            }
        }

        internal override string Name => "Near clip";

        private void UpdateDefaultValue()
        {
            if (this._defaultValue.HasValue)
            {
                return;
            }

            _setting = Setting.FindSettingByName("fNearDistance:Display", true, true);
            if (_setting == null)
            {
                throw new InvalidOperationException("Failed to find fNearDistance setting!");
            }

            this._defaultValue = _setting.GetFloat();
        }
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly NearClip NearClip = new NearClip();
    }
}
