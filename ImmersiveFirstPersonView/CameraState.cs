namespace IFPV
{
    using System.Collections.Generic;

    internal abstract class CameraState
    {
        internal readonly List<CameraValueModifier> RemoveModifiersOnLeave = new List<CameraValueModifier>();

        internal bool __wActivate { get; set; }

        internal virtual int Group => 0;

        internal bool IsActive { get; private set; }

        internal virtual int Priority => 0;

        internal CameraStack Stack { get; private set; }

        internal void _init(CameraStack s) => this.Stack = s;

        internal void _set(bool a)
        {
            this.IsActive = a;

            //NetScriptFramework.Debug.GUI.WriteLine((a ? ">>> " : "<<< ") + this.GetType().Name);

            if (!a)
            {
                foreach (var m in this.RemoveModifiersOnLeave)
                {
                    var time = m.AutoRemoveDelay;
                    if (time > 0)
                    {
                        m.RemoveDelayed(time);
                    }
                    else
                    {
                        m.Remove();
                    }
                }

                this.RemoveModifiersOnLeave.Clear();
            }
        }

        internal virtual bool Check(CameraUpdate update) => true;

        internal virtual void Initialize() { }

        internal virtual void OnEntering(CameraUpdate update) { }

        internal virtual void OnLeaving(CameraUpdate update) { }

        internal virtual void Update(CameraUpdate update) { }

        protected void AddHeadBobModifier(CameraUpdate update,
            bool forceHeadBob = false,
            bool forceReducedStabilizeHistory = false,
            double multiplier = 1.0,
            long extraDuration = 0)
        {
            var headBob = forceHeadBob || Settings.Instance.HeadBob;
            if (headBob)
            {
                var value = 0.5;
                var amount = (forceHeadBob ? 1.0 : Settings.Instance.HeadBobAmount) * multiplier;
                if (amount > 0.01)
                {
                    value /= amount;
                    update.Values.StabilizeIgnorePositionY.AddModifier(this,
                        CameraValueModifier.ModifierTypes.SetIfPreviousIsHigherThanThis,
                        value,
                        true,
                        extraDuration);
                }
            }

            if (headBob || forceReducedStabilizeHistory)
            {
                update.Values.StabilizeHistoryDuration.AddModifier(this,
                    CameraValueModifier.ModifierTypes.SetIfPreviousIsHigherThanThis, 100.0, true, extraDuration);
            }
        }
    }
}