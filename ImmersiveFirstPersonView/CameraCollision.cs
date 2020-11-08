namespace IFPV
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetScriptFramework;
    using NetScriptFramework.SkyrimSE;

    internal static class CameraCollision
    {
        private static MemoryAllocation Allocation;
        private static ulong RaycastMask;
        private static NiPoint3 TempNormal;
        private static NiPoint3 TempPoint1;
        private static NiPoint3 TempPoint2;
        private static NiPoint3 TempSafety;
        private static NiTransform TempTransform;

        internal static bool Apply(CameraUpdate update, NiTransform transform, NiPoint3 result)
        {
            init();

            if (update == null || transform == null || result == null)
            {
                return false;
            }

            if (update.Values.CollisionEnabled.CurrentValue < 0.5)
            {
                return false;
            }

            var actor = update.Target.Actor;

            var cell = actor?.ParentCell;
            if (cell == null)
            {
                return false;
            }

            var safety = (float)(update.Values.NearClip.CurrentValue + 1.0);
            if (safety < 1.0f)
            {
                safety = 1.0f;
            }

            var safety2 = Math.Max(0.0f, Settings.Instance.CameraCollisionSafety);

            var tpos = transform.Position;

            TempPoint1.CopyFrom(actor.Position);
            TempPoint1.Z = tpos.Z;

            if (safety2 > 0.0f)
            {
                TempSafety.Y = -safety2 * 0.5f;
                TempTransform.CopyFrom(transform);
                TempTransform.Position.CopyFrom(TempPoint1);
                TempTransform.Translate(TempSafety, TempPoint1);
            }

            TempNormal.X = tpos.X - TempPoint1.X;
            TempNormal.Y = tpos.Y - TempPoint1.Y;
            TempNormal.Z = tpos.Z - TempPoint1.Z;

            var len = TempNormal.Length;
            if (len <= 0.0f)
            {
                return false;
            }

            TempNormal.Normalize(TempNormal);
            TempNormal.Multiply(len + safety + safety2, TempNormal);

            TempPoint2.X = TempPoint1.X + TempNormal.X;
            TempPoint2.Y = TempPoint1.Y + TempNormal.Y;
            TempPoint2.Z = TempPoint1.Z + TempNormal.Z;

            var ls = TESObjectCELL.RayCast(new RayCastParameters
            {
                Cell = cell,
                Begin = new[] {TempPoint1.X, TempPoint1.Y, TempPoint1.Z},
                End = new[] {TempPoint2.X, TempPoint2.Y, TempPoint2.Z}
            });

            if (ls == null || ls.Count == 0)
            {
                return false;
            }

            RayCastResult best = null;
            var bestDist = 0.0f;
            var ignore = new List<NiAVObject>(3);
            {
                var sk = actor.GetSkeletonNode(true);
                if (sk != null)
                {
                    ignore.Add(sk);
                }
            }
            {
                var sk = actor.GetSkeletonNode(false);
                if (sk != null)
                {
                    ignore.Add(sk);
                }
            }
            if (update.CachedMounted)
            {
                var mount = actor.GetMount();
                var sk = mount?.GetSkeletonNode(false);
                if (sk != null)
                {
                    ignore.Add(sk);
                }
            }

            foreach (var r in ls)
            {
                if (!IsValid(r, ignore))
                {
                    continue;
                }

                var dist = r.Fraction;
                if (best == null)
                {
                    best = r;
                    bestDist = dist;
                }
                else if (dist < bestDist)
                {
                    best = r;
                    bestDist = dist;
                }
            }

            if (best == null)
            {
                return false;
            }

            bestDist *= len + safety + safety2;
            bestDist -= safety + safety2;
            bestDist /= len + safety + safety2;

            // Negative is ok!

            result.X = ((TempPoint2.X - TempPoint1.X) * bestDist) + TempPoint1.X;
            result.Y = ((TempPoint2.Y - TempPoint1.Y) * bestDist) + TempPoint1.Y;
            result.Z = ((TempPoint2.Z - TempPoint1.Z) * bestDist) + TempPoint1.Z;

            return true;
        }

        private static void init()
        {
            if (Allocation != null)
            {
                return;
            }

            Allocation = Memory.Allocate(0x90);
            TempPoint1 = MemoryObject.FromAddress<NiPoint3>(Allocation.Address);
            TempPoint2 = MemoryObject.FromAddress<NiPoint3>(Allocation.Address + 0x10);
            TempNormal = MemoryObject.FromAddress<NiPoint3>(Allocation.Address + 0x20);
            TempSafety = MemoryObject.FromAddress<NiPoint3>(Allocation.Address + 0x30);
            TempTransform = MemoryObject.FromAddress<NiTransform>(Allocation.Address + 0x40);
            TempTransform.Scale = 1.0f;
            TempSafety.X = 0.0f;
            TempSafety.Y = 0.0f;
            TempSafety.Z = 0.0f;

            SetupRaycastMask(new[]
            {
                CollisionLayers.AnimStatic, CollisionLayers.Biped, CollisionLayers.CharController,
                //CollisionLayers.Clutter,
                CollisionLayers.DebrisLarge, CollisionLayers.Ground,
                //CollisionLayers.Props,
                CollisionLayers.Static, CollisionLayers.Terrain, CollisionLayers.Trap, CollisionLayers.Trees,
                CollisionLayers.Unidentified
            });
        }

        private static bool IsValid(RayCastResult r, List<NiAVObject> ignore)
        {
            var havokObj = r.HavokObject;
            if (havokObj != IntPtr.Zero)
            {
                var flags = Memory.ReadUInt32(havokObj + 0x2C) & 0x7F;
                var mask = (ulong)1 << (int)flags;
                if ((RaycastMask & mask) == 0)
                {
                    return false;
                }
            }

            if (ignore == null || ignore.Count == 0)
            {
                return true;
            }

            var obj = r.Object;

            return obj == null || ignore.All(o => o == null || !o.Equals(obj));
        }

        private static void SetupRaycastMask(CollisionLayers[] layers)
        {
            var m = layers.Select(l => (ulong)1 << (int)l).Aggregate<ulong, ulong>(0, (current, fl) => current | fl);

            RaycastMask = m;
        }
    }
}
