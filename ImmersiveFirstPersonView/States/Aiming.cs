namespace IFPV.States
{
    using NetScriptFramework;
    using NetScriptFramework.SkyrimSE;

    internal class Aiming : CameraState
    {
        internal override int Priority => (int)Priorities.Aiming;

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

            // Aiming bow or crossbow.
            var flags = Memory.ReadUInt32(actor.Address + 0xC0) >> 28;
            if (flags == 0xA)
            {
                return true;
            }

            for (var i = 0; i < 3; i++)
            {
                var caster = actor.GetMagicCaster((EquippedSpellSlots)i);
                if (caster == null)
                {
                    continue;
                }

                var state = caster.State;
                switch (state)
                {
                    case MagicCastingStates.Charged:
                    case MagicCastingStates.Charging:
                    case MagicCastingStates.Concentrating: return true;
                }
            }

            return false;
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            update.Values.FirstPersonSkeletonRotateYMultiplier.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
            update.Values.FaceCamera.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
            update.Values.RestrictDown.AddModifier(this, CameraValueModifier.ModifierTypes.SetIfPreviousIsHigherThanThis, 70.0);
            update.Values._HeadTrackEnabled.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 0.0);
        }
    }
}
