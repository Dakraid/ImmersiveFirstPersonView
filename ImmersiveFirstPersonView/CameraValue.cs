using System;
using System.Collections.Generic;
using System.Text;

namespace IFPV
{
    internal abstract class CameraValueBase
    {
        private readonly List<CameraValueModifier> Modifiers = new List<CameraValueModifier>();

        [Flags]
        internal enum CameraValueFlags : uint
        {
            None = 0,

            NoTween = 1,

            NoModifiers = 2,

            IncreaseInstantly = 4,

            DecreaseInstantly = 8,

            DontUpdateIfDisabled = 0x10
        }

        internal abstract double ChangeSpeed { get; }

        internal abstract double CurrentValue { get; set; }

        internal abstract double DefaultValue { get; }

        internal CameraValueFlags Flags { get; set; }

        internal TValue.TweenTypes Formula { get; set; }

        internal abstract string Name { get; }

        private TValue InternalValue { get; set; }

        private double LastValue { get; set; }

        private double TargetValue { get; set; }

        private int UpdatedCountWhenDisabled { get; set; }

        internal CameraValueModifier AddModifier(CameraState                       fromState,
                                                 CameraValueModifier.ModifierTypes type,
                                                 double                            amount,
                                                 bool                              autoRemoveOnLeaveState = true,
                                                 long                              autoRemoveDelay        = 0)
        {
            if ((Flags & CameraValueFlags.NoModifiers) != CameraValueFlags.None)
                return null;

            var mod   = new CameraValueModifier(this, fromState, type, amount, autoRemoveOnLeaveState, autoRemoveDelay);
            var added = false;
            for (var i = 0; i < Modifiers.Count; i++)
            {
                var m = Modifiers[i];
                if (m.Priority > mod.Priority)
                {
                    Modifiers.Insert(i, mod);
                    added = true;
                    break;
                }
            }

            if (!added)
                Modifiers.Add(mod);

            if (mod.AutoRemove && fromState != null)
                fromState.RemoveModifiersOnLeave.Add(mod);

            UpdatedCountWhenDisabled = 0;

            return mod;
        }

        internal void RemoveModifier(CameraValueModifier mod)
        {
            if (mod.Owner == this)
                if (Modifiers.Remove(mod))
                    UpdatedCountWhenDisabled = 0;
        }

        internal void Reset()
        {
            if ((Flags & CameraValueFlags.NoModifiers) != CameraValueFlags.None)
                return;

            Modifiers.Clear();

            var value = DefaultValue;
            LastValue     = value;
            TargetValue   = value;
            InternalValue = null;
            CurrentValue  = value;
        }

        internal void Update(long now, bool enabled)
        {
            if ((Flags & CameraValueFlags.NoModifiers) != CameraValueFlags.None)
                return;

            if ((Flags & CameraValueFlags.DontUpdateIfDisabled) != CameraValueFlags.None)
            {
                if (enabled) { UpdatedCountWhenDisabled = 0; }
                else
                {
                    if (UpdatedCountWhenDisabled > 0)
                        return;
                    UpdatedCountWhenDisabled++;
                }
            }

            for (var i = Modifiers.Count - 1; i >= 0; i--)
            {
                var m = Modifiers[i];
                if (m.RemoveTimer.HasValue)
                {
                    var timer = m.RemoveTimer.Value;
                    if (timer < 0)
                        m.RemoveTimer = now - m.RemoveTimer.Value;
                    else if (now >= m.RemoveTimer.Value)
                        m.Remove();
                }
            }

            var     wantValue  = DefaultValue;
            double? forceValue = null;
            foreach (var x in Modifiers)
                switch (x.Type)
                {
                    case CameraValueModifier.ModifierTypes.Set:
                        wantValue  = x.Amount;
                        forceValue = null;
                        break;

                    case CameraValueModifier.ModifierTypes.SetIfPreviousIsLowerThanThis:
                        if (x.Amount > wantValue)
                        {
                            wantValue  = x.Amount;
                            forceValue = null;
                        }

                        break;

                    case CameraValueModifier.ModifierTypes.SetIfPreviousIsHigherThanThis:
                        if (x.Amount < wantValue)
                        {
                            wantValue  = x.Amount;
                            forceValue = null;
                        }

                        break;

                    case CameraValueModifier.ModifierTypes.Add:
                        wantValue  += x.Amount;
                        forceValue =  null;
                        break;

                    case CameraValueModifier.ModifierTypes.Multiply:
                        wantValue  *= x.Amount;
                        forceValue =  null;
                        break;

                    case CameraValueModifier.ModifierTypes.Force:
                        forceValue = x.Amount;
                        wantValue  = x.Amount;
                        break;

                    default:
                        throw new NotImplementedException();
                }

            if (wantValue != TargetValue)
            {
                TargetValue = wantValue;

                var shouldTween = !forceValue.HasValue && (Flags & CameraValueFlags.NoTween) == CameraValueFlags.None;
                if (shouldTween && wantValue                     > LastValue &&
                    (Flags & CameraValueFlags.IncreaseInstantly) != CameraValueFlags.None)
                    shouldTween = false;
                if (shouldTween && wantValue                     < LastValue &&
                    (Flags & CameraValueFlags.DecreaseInstantly) != CameraValueFlags.None)
                    shouldTween = false;

                if (shouldTween)
                {
                    InternalValue = new TValue(LastValue, double.MinValue, double.MaxValue);
                    InternalValue.TweenTo(wantValue, ChangeSpeed, Formula, true);
                }
                else
                {
                    CurrentValue = wantValue;
                    LastValue    = wantValue;
                }
            }
            else if (InternalValue != null)
            {
                InternalValue.Update(now);
                LastValue    = InternalValue.CurrentAmount;
                CurrentValue = LastValue;
                if (LastValue == wantValue)
                    InternalValue = null;
            }
            else
            {
                var hasNow = CurrentValue;
                if (hasNow != wantValue)
                {
                    CurrentValue = wantValue;
                    LastValue    = wantValue;
                }
            }
        }
    }

    internal sealed class CameraValueModifier
    {
        internal readonly double Amount;

        internal readonly bool AutoRemove;

        internal readonly long AutoRemoveDelay;

        internal readonly CameraValueBase Owner;

        internal readonly int Priority;

        internal readonly CameraState State;

        internal readonly ModifierTypes Type;

        internal long? RemoveTimer;

        internal CameraValueModifier(CameraValueBase owner,
                                     CameraState     state,
                                     ModifierTypes   type,
                                     double          amount,
                                     bool            autoRemove,
                                     long            autoRemoveDelay)
        {
            Owner           = owner;
            State           = state;
            Type            = type;
            Amount          = amount;
            AutoRemove      = autoRemove;
            AutoRemoveDelay = autoRemoveDelay;

            if (State != null)
                Priority = State.Priority;
            else
                Priority = -1000000;
        }

        internal enum ModifierTypes
        {
            Set,
            SetIfPreviousIsHigherThanThis,
            SetIfPreviousIsLowerThanThis,
            Add,
            Multiply,
            Force
        }

        internal void Remove() { Owner.RemoveModifier(this); }

        internal void RemoveDelayed(long time) { RemoveTimer = -time; }
    }

    internal class CameraValueSimple : CameraValueBase
    {
        private double _cur_value;

        internal CameraValueSimple(string name, double defaultValue, double changeSpeed)
        {
            Name         = name;
            DefaultValue = defaultValue;
            _cur_value   = defaultValue;
            ChangeSpeed  = changeSpeed;

            if (name == null)
            {
                var t     = GetType().Name;
                var words = new List<string>();
                var cur   = new StringBuilder(32);
                foreach (var c in t)
                {
                    if (char.IsUpper(c) && cur.Length != 0)
                    {
                        words.Add(cur.ToString().ToLowerInvariant());
                        cur.Clear();
                    }

                    cur.Append(c);
                }

                if (cur.Length != 0)
                    words.Add(cur.ToString().ToLowerInvariant());

                Name = string.Join(" ", words);
            }
        }

        internal override double ChangeSpeed { get; }

        internal override double CurrentValue
        {
            get => _cur_value;

            set => _cur_value = value;
        }

        internal override double DefaultValue { get; }

        internal override string Name { get; }
    }
}