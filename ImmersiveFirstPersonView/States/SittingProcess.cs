using NetScriptFramework.SkyrimSE;

namespace IFPV.States
{
    internal class SittingProcess : CameraState
    {
        internal override int Priority => (int) Priorities.SittingProcess;

        internal override bool Check(CameraUpdate update)
        {
            if (!update.CameraMain.IsEnabled)
                return false;

            if (update.CachedMounted)
                return false;

            // Also triggers on some crafting benches but it should be fine

            var actor = update.Target.Actor;
            if (actor == null)
                return false;

            switch (actor.SitState)
            {
                case ActorActionStates.NotAction:
                case ActorActionStates.InProgress:
                    return false;
            }

            return true;
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            update.Values.RotationFromHead.AddModifier(
                this, CameraValueModifier.ModifierTypes.SetIfPreviousIsLowerThanThis, 0.2);
        }
    }
}