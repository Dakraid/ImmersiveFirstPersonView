namespace IFPV.States
{
    using NetScriptFramework.SkyrimSE;

    internal class CustomPassenger : Passenger
    {
        internal override int Priority => (int)Priorities.CustomPassenger;

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

            var kw = Settings.Instance.CustomPassengerKeyword;
            if (!string.IsNullOrEmpty(kw))
            {
                if (actor.HasKeywordText(kw))
                {
                    return true;
                }
            }

            kw = Settings.Instance.CustomPassengerMagicKeyword;
            if (!string.IsNullOrEmpty(kw))
            {
                MagicItem item = null;
                if (actor.HasMagicEffectWithKeywordText(kw, ref item))
                {
                    return true;
                }
            }

            return false;
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            update.Values.NearClip.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
            update.Values.InputRotationXMultiplier.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
            update.Values.InputRotationYMultiplier.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);

            update.Values.PositionFromHead.AddModifier(this,
                CameraValueModifier.ModifierTypes.Set,
                Settings.Instance.CustomPassengerPositionFromHead);

            update.Values.RotationFromHead.AddModifier(this,
                CameraValueModifier.ModifierTypes.Set,
                Settings.Instance.CustomPassengerRotationFromHead);

            update.Values.RestrictDown.AddModifier(this, CameraValueModifier.ModifierTypes.Add, 50.0);
            update.Values.Offset1PositionY.AddModifier(this, CameraValueModifier.ModifierTypes.Multiply, 0.5);
            update.Values.CollisionEnabled.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 0.0);

            update.Values.StabilizeHistoryDuration.AddModifier(this,
                CameraValueModifier.ModifierTypes.Set,
                Settings.Instance.CustomPassengerStabilizeHistoryDuration *
                1000.0f);

            update.Values.StabilizeIgnorePositionX.AddModifier(this,
                CameraValueModifier.ModifierTypes.Set,
                Settings.Instance.CustomPassengerStabilizeIgnorePositionX);

            update.Values.StabilizeIgnorePositionY.AddModifier(this,
                CameraValueModifier.ModifierTypes.Set,
                Settings.Instance.CustomPassengerStabilizeIgnorePositionY);

            update.Values.StabilizeIgnorePositionZ.AddModifier(this,
                CameraValueModifier.ModifierTypes.Set,
                Settings.Instance.CustomPassengerStabilizeIgnorePositionZ);

            update.Values.StabilizeIgnoreRotationX.AddModifier(this,
                CameraValueModifier.ModifierTypes.Set,
                Settings.Instance.CustomPassengerStabilizeIgnoreRotationX);

            update.Values.StabilizeIgnoreRotationY.AddModifier(this,
                CameraValueModifier.ModifierTypes.Set,
                Settings.Instance.CustomPassengerStabilizeIgnoreRotationY);

            update.Values.StabilizeIgnoreOffsetX.AddModifier(this,
                CameraValueModifier.ModifierTypes.Set,
                Settings.Instance.CustomPassengerStabilizeIgnoreOffsetX);

            update.Values.StabilizeIgnoreOffsetY.AddModifier(this,
                CameraValueModifier.ModifierTypes.Set,
                Settings.Instance.CustomPassengerStabilizeIgnoreOffsetY);
        }
    }
}