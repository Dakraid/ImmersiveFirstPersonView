using NetScriptFramework.SkyrimSE;

namespace IFPV.States
{
    internal class Sitting : CameraState
    {
        internal override int Priority => (int) Priorities.Sitting;

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

            if (actor.SitState == ActorActionStates.NotAction)
                return false;

            return true;
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            update.Values.FaceCamera.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 0);
            Default.CantAutoTurnCounter++;
        }

        internal override void OnLeaving(CameraUpdate update)
        {
            base.OnLeaving(update);

            Default.CantAutoTurnCounter--;
        }
    }
}