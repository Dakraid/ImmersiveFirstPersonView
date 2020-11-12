namespace IFPV.Values
{
    using NetScriptFramework.SkyrimSE;

    internal abstract class IniValue : CameraValueBase
    {
        private readonly string _name;
        private double _def;

        private Setting _setting;
        private bool _tried;

        internal IniValue(string name)
        {
            this._name = name;
            this.Flags |= CameraValueFlags.NoTween;
        }

        internal override double ChangeSpeed => 1;

        internal override double CurrentValue
        {
            get
            {
                this.init();
                if (this._setting == null)
                {
                    return 0;
                }

                switch (this._setting.SettingType)
                {
                    case SettingTypes.Float: return this._setting.GetFloat();
                    case SettingTypes.Int:
                    case SettingTypes.UInt: return this._setting.GetInt();
                }

                return 0;
            }

            set
            {
                this.init();
                if (this._setting == null)
                {
                    return;
                }

                switch (this._setting.SettingType)
                {
                    case SettingTypes.Float:
                        this._setting.SetFloat((float)value);
                        break;
                    case SettingTypes.Int:
                    case SettingTypes.UInt:
                        this._setting.SetInt((int)value);
                        break;
                }
            }
        }

        internal override double DefaultValue => this._def;

        internal override string Name => this._name ?? "unk_ini_value";

        private void init()
        {
            if (this._tried)
            {
                return;
            }

            this._tried = true;
            this._setting = Setting.FindSettingByName(this.Name, true, true);

            if (this._setting != null)
            {
                this._def = this.CurrentValue;
            }
        }
    }
}