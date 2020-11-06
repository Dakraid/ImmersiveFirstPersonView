using NetScriptFramework.SkyrimSE;

namespace IFPV.Values
{
    internal abstract class IniValue : CameraValueBase
    {
        private readonly string _name;
        private          double _def;

        private Setting _setting;
        private bool    _tried;

        internal IniValue(string name)
        {
            _name =  name;
            Flags |= CameraValueFlags.NoTween;
        }

        internal override double ChangeSpeed => 1;

        internal override double CurrentValue
        {
            get
            {
                init();
                if (_setting == null)
                    return 0;

                switch (_setting.SettingType)
                {
                    case SettingTypes.Float: return _setting.GetFloat();
                    case SettingTypes.Int:
                    case SettingTypes.UInt: return _setting.GetInt();
                }

                return 0;
            }

            set
            {
                init();
                if (_setting == null)
                    return;

                switch (_setting.SettingType)
                {
                    case SettingTypes.Float:
                        _setting.SetFloat((float) value);
                        break;
                    case SettingTypes.Int:
                    case SettingTypes.UInt:
                        _setting.SetInt((int) value);
                        break;
                }
            }
        }

        internal override double DefaultValue => _def;

        internal override string Name => _name ?? "unk_ini_value";

        private void init()
        {
            if (_tried)
                return;

            _tried   = true;
            _setting = Setting.FindSettingByName(Name, true, true);

            if (_setting != null)
                _def = CurrentValue;
        }
    }
}