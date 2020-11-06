using System;
using System.Collections.Generic;
using NetScriptFramework;
using NetScriptFramework.SkyrimSE;

namespace IFPV
{
    internal sealed class CameraStabilize
    {
        private static MemoryAllocation Allocation;

        internal readonly CameraMain CameraMain;

        private readonly TargetChangeCheck ForTarget;

        private readonly LinkedList<CameraStabilizeHistoryEntry>
            History = new LinkedList<CameraStabilizeHistoryEntry>();

        private readonly NiPoint3 TempPoint;

        private readonly NiTransform TempTransform;

        private readonly NiPoint3 TweenPoint;

        private CameraStabilizeHistoryEntry LastCalculated;

        private bool NeedRecalculate;

        private long TweenBegin;

        private long TweenEnd;

        internal CameraStabilize(CameraMain cameraMain, CameraTarget target)
        {
            if (cameraMain == null)
                throw new ArgumentNullException("cameraMain");

            if (target == null)
                throw new ArgumentNullException("target");

            CameraMain = cameraMain;

            if (Allocation == null)
                Allocation = Memory.Allocate(0x60);

            TempPoint           = MemoryObject.FromAddress<NiPoint3>(Allocation.Address);
            TempTransform       = MemoryObject.FromAddress<NiTransform>(Allocation.Address + 0x10);
            TempTransform.Scale = 1.0f;
            TweenPoint          = MemoryObject.FromAddress<NiPoint3>(Allocation.Address + 0x50);

            ForTarget = GetFromTarget(target);
        }

        internal double IgnoreOffsetX { get; private set; }

        internal double IgnoreOffsetY { get; private set; }

        internal double IgnorePositionX { get; private set; }

        internal double IgnorePositionY { get; private set; }

        internal double IgnorePositionZ { get; private set; }

        internal double IgnoreRotationX { get; private set; }

        internal double IgnoreRotationY { get; private set; }

        internal long MaxHistoryDuration { get; private set; }

        internal void AddTweenFrom(long duration, NiPoint3 cur)
        {
            if (LastCalculated == null || duration <= 0 || cur == null)
                return;

            var now = IFPVPlugin.Instance.Time;
            TweenPoint.CopyFrom(cur);
            TweenBegin = now;
            TweenEnd   = now + duration;
        }

        internal void ApplyTween(NiPoint3 target, long time)
        {
            if (time >= TweenEnd || time < TweenBegin || TweenPoint == null)
                return;

            var sx    = TweenPoint.X;
            var sy    = TweenPoint.Y;
            var sz    = TweenPoint.Z;
            var tx    = target.X;
            var ty    = target.Y;
            var tz    = target.Z;
            var ratio = (time - TweenBegin) / (float) (TweenEnd - TweenBegin);
            ratio = (float) Utility.ApplyFormula(ratio, TValue.TweenTypes.Linear);

            target.X = (tx - sx) * ratio + sx;
            target.Y = (ty - sy) * ratio + sy;
            target.Z = (tz - sz) * ratio + sz;
        }

        internal void ClearTweenFrom()
        {
            TweenBegin = 0;
            TweenEnd   = 0;
        }

        internal bool Get(NiAVObject root, NiTransform result)
        {
            if (NeedRecalculate)
            {
                Recalculate();
                NeedRecalculate = false;
            }

            if (LastCalculated == null)
                return false;

            root.WorldTransform.Rotation.GetEulerAngles(TempPoint);
            var angle = TempPoint.Z;

            TempPoint.X = LastCalculated.Position[0];
            TempPoint.Y = LastCalculated.Position[1];
            TempPoint.Z = LastCalculated.Position[2];
            var len = TempPoint.Length;
            if (len > 0.0f)
                TempPoint.Normalize(TempPoint);

            var tpos = TempTransform.Position;
            tpos.X = 0.0f;
            tpos.Y = 0.0f;
            tpos.Z = 0.0f;
            TempTransform.LookAt(TempPoint);

            TempTransform.Rotation.RotateZ(-angle, TempTransform.Rotation);

            TempPoint.X = 0.0f;
            TempPoint.Y = len;
            TempPoint.Z = 0.0f;

            TempTransform.Translate(TempPoint, TempPoint);

            var pos  = result.Position;
            var spos = root.WorldTransform.Position;
            pos.X = spos.X + TempPoint.X;
            pos.Y = spos.Y + TempPoint.Y;
            pos.Z = spos.Z + TempPoint.Z;

            root.WorldTransform.Rotation.GetEulerAngles(TempPoint);

            var y = (float) Utility.ClampToPi(TempPoint.X + LastCalculated.OffsetY);
            var x = (float) Utility.ClampToPi(TempPoint.Z + LastCalculated.OffsetX);

            var rot = result.Rotation;
            rot.Identity(1.0f);

            if (y != 0.0f)
                rot.RotateX(y, rot);

            if (x != 0.0f)
                rot.RotateZ(-x, rot);

            return true;
        }

        internal bool ShouldRecreate(CameraTarget current)
        {
            var other = GetFromTarget(current);
            if (ReferenceEquals(other, null) != ReferenceEquals(ForTarget, null))
                return true;
            if (ForTarget == null)
                return false;
            return !ForTarget.IsEqual(other);
        }

        internal void Update(NiAVObject root, NiAVObject head, CameraUpdate update)
        {
            UpdateValues(update);

            var tpos = head.WorldTransform.Position;
            var spos = root.WorldTransform.Position;

            var x = tpos.X - spos.X;
            var y = tpos.Y - spos.Y;
            var z = tpos.Z - spos.Z;

            root.WorldTransform.Rotation.GetEulerAngles(TempPoint);
            var angle = TempPoint.Z;

            TempPoint.X = x;
            TempPoint.Y = y;
            TempPoint.Z = z;
            var len = TempPoint.Length;
            if (len > 0.0f)
                TempPoint.Normalize(TempPoint);

            tpos   = TempTransform.Position;
            tpos.X = 0.0f;
            tpos.Y = 0.0f;
            tpos.Z = 0.0f;

            TempTransform.LookAt(TempPoint);
            TempTransform.Rotation.RotateZ(angle, TempTransform.Rotation);

            TempPoint.X = 0.0f;
            TempPoint.Y = len;
            TempPoint.Z = 0.0f;
            TempTransform.Translate(TempPoint, TempPoint);

            x = TempPoint.X;
            y = TempPoint.Y;
            z = TempPoint.Z;

            root.WorldTransform.Rotation.GetEulerAngles(TempPoint);
            head.WorldTransform.Rotation.GetEulerAngles(TempTransform.Position);

            var ofx = Utility.ClampToPi(TempTransform.Position.Z - TempPoint.Z);
            var ofy = Utility.ClampToPi(TempTransform.Position.X - TempPoint.X);

            ApplyIgnoreOffset(ref ofx, ref ofy);

            var now = CameraMain.Plugin.Time;

            var e = new CameraStabilizeHistoryEntry();
            e.Time     = now;
            e.OffsetX  = (float) ofx;
            e.OffsetY  = (float) ofy;
            e.Position = new[] {x, y, z};
            History.AddLast(e);

            NeedRecalculate = true;
        }

        private void ApplyIgnoreOffset(ref double x, ref double y)
        {
            var ofx  = x;
            var ofy  = y;
            var ofxi = IgnoreOffsetX;
            var ofyi = IgnoreOffsetY;
            if (ofxi > 0.0)
            {
                if (ofx >= 0.0)
                {
                    ofx -= ofxi;
                    if (ofx < 0.0)
                        ofx = 0.0;
                }
                else
                {
                    ofx += ofxi;
                    if (ofx > 0.0)
                        ofx = 0.0;
                }
            }

            if (ofyi > 0.0)
            {
                if (ofy >= 0.0)
                {
                    ofy -= ofyi;
                    if (ofy < 0.0)
                        ofy = 0.0;
                }
                else
                {
                    ofy += ofyi;
                    if (ofy > 0.0)
                        ofy = 0.0;
                }
            }

            x = ofx;
            y = ofy;
        }

        private TargetChangeCheck GetFromTarget(CameraTarget target)
        {
            if (target == null)
                return new TargetChangeCheck();

            var obj  = target.Object;
            var root = target.StabilizeRootNode;
            var head = target.HeadNode;

            var c = new TargetChangeCheck();
            if (obj != null)
                c.FormId = obj.FormId;
            if (root != null)
                c.RootName = (root.Name.Text ?? string.Empty).ToLowerInvariant() + "_" + root.Address.ToHexString();
            if (head != null)
                c.HeadName = (head.Name.Text ?? string.Empty).ToLowerInvariant() + "_" + head.Address.ToHexString();
            return c;
        }

        private void Recalculate()
        {
            var now = CameraMain.Plugin.Time;
            {
                var remove = now - MaxHistoryDuration;
                while (History.Count != 0)
                {
                    var n = History.First;
                    if (n.Value.Time <= remove)
                        History.RemoveFirst();
                    else
                        break;
                }
            }

            if (History.Count == 0)
                return;

            // Calculate weighted average.
            var totalWeight = 0.0;

            var totalPosition = new double[3];
            var totalOffsetX  = 0.0;
            var totalOffsetY  = 0.0;

            {
                var n = History.Last;
                while (n != null)
                {
                    var cur = n.Value;
                    n = n.Previous;

                    var diff   = MaxHistoryDuration - (now - cur.Time);
                    var ratio  = diff  / (double) MaxHistoryDuration;
                    var weight = ratio * ratio;
                    totalWeight      += weight;
                    totalPosition[0] += cur.Position[0] * weight;
                    totalPosition[1] += cur.Position[1] * weight;
                    totalPosition[2] += cur.Position[2] * weight;
                    double ofx = cur.OffsetX;
                    double ofy = cur.OffsetY;
                    //this.ApplyIgnoreOffset(ref ofx, ref ofy);
                    totalOffsetX += ofx * weight;
                    totalOffsetY += ofy * weight;
                }
            }

            if (totalWeight <= 0.0)
                return;

            totalPosition[0] /= totalWeight;
            totalPosition[1] /= totalWeight;
            totalPosition[2] /= totalWeight;
            totalOffsetX     /= totalWeight;
            totalOffsetY     /= totalWeight;

            if (LastCalculated == null)
            {
                LastCalculated             = new CameraStabilizeHistoryEntry();
                LastCalculated.Time        = now;
                LastCalculated.Position    = new float[3];
                LastCalculated.Position[0] = (float) totalPosition[0];
                LastCalculated.Position[1] = (float) totalPosition[1];
                LastCalculated.Position[2] = (float) totalPosition[2];
                LastCalculated.OffsetX     = (float) totalOffsetX;
                LastCalculated.OffsetY     = (float) totalOffsetY;
                return;
            }

            var changed = false;

            {
                var    value  = totalPosition[0];
                double old    = LastCalculated.Position[0];
                var    ignore = IgnorePositionX;
                var    diff   = value - old;
                if (Math.Abs(diff) > ignore)
                {
                    if (diff >= 0.0)
                        diff -= ignore;
                    else
                        diff += ignore;
                    LastCalculated.Position[0] += (float) diff;
                    changed                    =  true;
                }
            }

            {
                var    value  = totalPosition[1];
                double old    = LastCalculated.Position[1];
                var    ignore = IgnorePositionY;
                var    diff   = value - old;
                if (Math.Abs(diff) > ignore)
                {
                    if (diff >= 0.0)
                        diff -= ignore;
                    else
                        diff += ignore;
                    LastCalculated.Position[1] += (float) diff;
                    changed                    =  true;
                }
            }

            {
                var    value  = totalPosition[2];
                double old    = LastCalculated.Position[2];
                var    ignore = IgnorePositionZ;
                var    diff   = value - old;
                if (Math.Abs(diff) > ignore)
                {
                    if (diff >= 0.0)
                        diff -= ignore;
                    else
                        diff += ignore;
                    LastCalculated.Position[2] += (float) diff;
                    changed                    =  true;
                }
            }

            {
                var    value  = totalOffsetX;
                double old    = LastCalculated.OffsetX;
                var    ignore = IgnoreRotationX;
                var    diff   = value - old;
                if (Math.Abs(diff) > ignore)
                {
                    if (diff >= 0.0)
                        diff -= ignore;
                    else
                        diff += ignore;
                    LastCalculated.OffsetX += (float) diff;
                    changed                =  true;
                }
            }

            {
                var    value  = totalOffsetY;
                double old    = LastCalculated.OffsetY;
                var    ignore = IgnoreRotationY;
                var    diff   = value - old;
                if (Math.Abs(diff) > ignore)
                {
                    if (diff >= 0.0)
                        diff -= ignore;
                    else
                        diff += ignore;
                    LastCalculated.OffsetY += (float) diff;
                    changed                =  true;
                }
            }

            if (changed)
                LastCalculated.Time = now;
        }

        private void UpdateValues(CameraUpdate update)
        {
            MaxHistoryDuration = (long) update.Values.StabilizeHistoryDuration.CurrentValue;
            IgnorePositionX    = update.Values.StabilizeIgnorePositionX.CurrentValue;
            IgnorePositionY    = update.Values.StabilizeIgnorePositionY.CurrentValue;
            IgnorePositionZ    = update.Values.StabilizeIgnorePositionZ.CurrentValue;
            IgnoreRotationX    = Utility.DegToRad(update.Values.StabilizeIgnoreRotationX.CurrentValue);
            IgnoreRotationY    = Utility.DegToRad(update.Values.StabilizeIgnoreRotationY.CurrentValue);
            IgnoreOffsetX      = Utility.DegToRad(update.Values.StabilizeIgnoreOffsetX.CurrentValue);
            IgnoreOffsetY      = Utility.DegToRad(update.Values.StabilizeIgnoreOffsetY.CurrentValue);
        }

        private sealed class CameraStabilizeHistoryEntry
        {
            internal float   OffsetX;
            internal float   OffsetY;
            internal float[] Position;
            internal long    Time;
        }

        private sealed class TargetChangeCheck
        {
            internal uint   FormId;
            internal string HeadName;
            internal string RootName;

            internal bool IsEqual(TargetChangeCheck other)
            {
                if (other == null)
                    return false;

                return FormId == other.FormId && RootName == other.RootName && HeadName == other.HeadName;
            }
        }
    }
}