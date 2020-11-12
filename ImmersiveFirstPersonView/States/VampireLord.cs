namespace IFPV.States
{
    using System;
    using NetScriptFramework.SkyrimSE;

    internal class VampireLord : CameraState
    {
        internal override int Group => (int)Groups.Beast;

        internal override int Priority => (int)Priorities.VampireLord;

        internal override bool Check(CameraUpdate update)
        {
            if (!update.CameraMain.IsEnabled)
            {
                return false;
            }

            var actor = update.Target.Actor;
            if (actor == null)
            {
                return false;
            }

            var want = Settings.Instance.VampireLordRaceName;
            if (string.IsNullOrEmpty(want))
            {
                return false;
            }

            var race = actor.Race;
            if (race == null)
            {
                return false;
            }

            var name = race.Name;
            var id = race.EditorId;

            return (!string.IsNullOrEmpty(name) && name.IndexOf(want, StringComparison.OrdinalIgnoreCase) >= 0) ||
                   (!string.IsNullOrEmpty(id) && id.IndexOf(want, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            if (Settings.Instance.HideHead)
            {
                update.Values.HideHead2.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
            }

            update.Values.HideHead.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 0.0);
            update.Values.HideArms.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 0.0);
            update.Values.Show1stPersonArms.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 0.0);
        }
    }

    internal class VampireLordTransform : CameraState
    {
        private EffectSetting _effect;

        private bool _t_init;

        internal override int Group => (int)Groups.Beast;

        internal override int Priority => (int)Priorities.VampireLordTransform;

        internal override bool Check(CameraUpdate update)
        {
            this.init();

            if (this._effect == null)
            {
                return false;
            }

            var actor = update.Target.Actor;
            if (actor == null)
            {
                return false;
            }

            return actor.HasMagicEffect(this._effect);
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            update.Values.Offset1PositionY.AddModifier(this, CameraValueModifier.ModifierTypes.Add, 5.0);
            update.Values.Offset1PositionZ.AddModifier(this, CameraValueModifier.ModifierTypes.Add, 2.0);
        }

        private void init()
        {
            if (this._t_init)
            {
                return;
            }

            this._t_init = true;

            var id = Settings.Instance.VampireLordTransformationEffectId;
            var file = Settings.Instance.VampireLordTransformationEffectFile;

            if (id == 0 || string.IsNullOrEmpty(file))
            {
                return;
            }

            this._effect = TESForm.LookupFormFromFile(id, file) as EffectSetting;
        }
    }
}