namespace IFPV.States
{
    using NetScriptFramework.SkyrimSE;

    internal class MountedProcess : CameraState
    {
        internal override int Priority => (int)Priorities.MountedProcess;

        internal override bool Check(CameraUpdate update)
        {
            if (!update.CameraMain.IsEnabled)
            {
                return false;
            }

            if (!update.CachedMounted)
            {
                return false;
            }

            var actor = update.Target.Actor;
            if (actor == null)
            {
                return false;
            }

            switch (actor.SitState)
            {
                case ActorActionStates.NotAction:
                case ActorActionStates.InProgress: return false;
            }

            return true;
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            update.Values.RestrictLeft.AddModifier(this,
                CameraValueModifier.ModifierTypes.SetIfPreviousIsHigherThanThis, 10.0);
        }
    }
}