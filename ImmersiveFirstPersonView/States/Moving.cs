using System;
using NetScriptFramework;

namespace IFPV.States
{
    internal abstract class Moving : CameraState
    {
        private static long _move_update = -1;

        internal static int _move_dir { get; private set; } = -1;

        internal override int Priority => (int) Priorities.Moving;

        protected internal abstract int IsDirection { get; }

        internal override bool Check(CameraUpdate update)
        {
            if (!update.CameraMain.IsEnabled)
                return false;

            if (update.CachedMounted)
                return false;

            _update_check(update);
            return _move_dir == IsDirection;
        }

        protected void Enter_Backwards(CameraUpdate update, bool diagonal)
        {
            if (Settings.Instance.Enable360WalkAnimationCompatibilityMode)
            {
                var ok = true;
                if (!Settings.Instance.My360WalkAnimationActivatesWithSneakToo)
                {
                    var actor = update.Target.Actor;
                    if (actor == null || actor.IsSneaking)
                        ok = false;
                }

                if (ok && !Settings.Instance.My360WalkAnimationActivatesWithSwimToo)
                {
                    var actor = update.Target.Actor;
                    if (actor == null) { ok = false; }
                    else
                    {
                        var flags = Memory.ReadUInt32(actor.Address + 0xC0);
                        if ((flags & 0x400) != 0)
                            ok = false;
                    }
                }

                if (ok)
                    //update.Values.Offset1PositionY.AddModifier(this, CameraValueModifier.ModifierTypes.Multiply, -1.0);
                    update.Values.StabilizeIgnoreOffsetX.AddModifier(this, CameraValueModifier.ModifierTypes.Force,
                                                                     360.0);
            }
        }

        protected void Enter_Forwards(CameraUpdate update, bool diagonal) { }

        protected void Enter_Left(CameraUpdate update, bool diagonal) { }

        protected void Enter_Right(CameraUpdate update, bool diagonal) { }

        private static void _update_check(CameraUpdate update)
        {
            var now = update.CameraMain.Plugin.Time;
            if (now == _move_update)
                return;
            _move_update = now;

            var actor = update.Target.Actor;
            if (actor == null)
            {
                _move_dir = -1;
                return;
            }

            var  moveFlags = Memory.ReadUInt32(actor.Address + 0xC0) & 0x3FFF;
            uint mask      = 0xCF;

            if ((moveFlags & mask) == 0)
            {
                _move_dir = -1;
                return;
            }

            double dir = Memory.InvokeCdeclF(update.CameraMain.Plugin.Actor_GetMoveDirection, actor.Address);
            var    pi  = Math.PI;
            dir =  dir + pi;
            dir %= pi * 2.0;

            dir =  Utility.RadToDeg(dir);
            dir -= 180.0;

            if (dir >= -22.5 && dir < 22.5)
            {
                _move_dir = 0;
                return;
            }

            if (dir >= 22.5 && dir < 67.5)
            {
                _move_dir = 1;
                return;
            }

            if (dir >= 67.5 && dir < 112.5)
            {
                _move_dir = 2;
                return;
            }

            if (dir >= 112.5 && dir < 157.5)
            {
                _move_dir = 3;
                return;
            }

            if (dir >= 157.5 || dir < -157.5)
            {
                _move_dir = 4;
                return;
            }

            if (dir >= -157.5 && dir < -112.5)
            {
                _move_dir = 5;
                return;
            }

            if (dir >= -112.5 && dir < -67.5)
            {
                _move_dir = 6;
                return;
            }

            if (dir >= -67.5 && dir < -22.5)
            {
                _move_dir = 7;
                return;
            }

            // Some fraction error?
            _move_dir = 0;

            /*
            -0.79   0   0.79

            -1.57       1.57

            -2.36   pi  2.36
            */
        }
    }

    internal class Moving_Forward : Moving
    {
        protected internal override int IsDirection => 0;

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            Enter_Forwards(update, false);
        }
    }

    internal class Moving_ForwardRight : Moving
    {
        protected internal override int IsDirection => 1;

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            Enter_Forwards(update, true);
            Enter_Right(update, true);
        }
    }

    internal class Moving_Right : Moving
    {
        protected internal override int IsDirection => 2;

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            Enter_Right(update, false);
        }
    }

    internal class Moving_BackwardRight : Moving
    {
        protected internal override int IsDirection => 3;

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            Enter_Backwards(update, true);
            Enter_Right(update, true);
        }
    }

    internal class Moving_Backward : Moving
    {
        protected internal override int IsDirection => 4;

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            Enter_Backwards(update, false);
        }
    }

    internal class Moving_BackwardLeft : Moving
    {
        protected internal override int IsDirection => 5;

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            Enter_Backwards(update, true);
            Enter_Left(update, true);
        }
    }

    internal class Moving_Left : Moving
    {
        protected internal override int IsDirection => 6;

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            Enter_Left(update, false);
        }
    }

    internal class Moving_ForwardLeft : Moving
    {
        protected internal override int IsDirection => 7;

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            Enter_Forwards(update, true);
            Enter_Left(update, true);
        }
    }
}