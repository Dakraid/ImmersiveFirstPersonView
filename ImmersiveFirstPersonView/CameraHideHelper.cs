using System;
using System.Collections.Generic;
using NetScriptFramework;
using NetScriptFramework.SkyrimSE;

namespace IFPV
{
    internal sealed class CameraHideHelper
    {
        internal readonly CameraMain       CameraMain;
        private readonly  biped_mask       IsHelmetBipedMask;
        private readonly  List<NiAVObject> LastHelmet = new List<NiAVObject>();
        private readonly  biped_mask       NotHelmetBipedMask;
        private           NiAVObject       FirstPersonSkeleton;
        private           IntPtr           LastAddress = IntPtr.Zero;
        private           HideFlags        LastFlags   = HideFlags.None;
        private           uint             LastFormId;

        private uint   LastRaceId;
        private IntPtr LastSkeleton = IntPtr.Zero;

        internal CameraHideHelper(CameraMain cameraMain)
        {
            if (cameraMain == null)
                throw new ArgumentNullException("cameraMain");

            CameraMain = cameraMain;

            // 30 = head
            // 31 = hair
            // 32 = body
            // 42 = circlet
            // 43 = ears
            // 130 = head
            // 131 = hair
            // 142 = circlet
            // 143 = ears
            // 230 = head?

            if (Settings.Instance.HideHelmet)
                IsHelmetBipedMask = GenerateBipedMask(30, 31, 42, 43, 130, 131, 142, 143, 230);
            else if (Settings.Instance.HideHead)
                IsHelmetBipedMask = GenerateBipedMask(43);
            else
                IsHelmetBipedMask = new biped_mask();
            NotHelmetBipedMask = GenerateBipedMask(32);
        }

        [Flags]
        private enum HideFlags : uint
        {
            None = 0,

            Head    = 1,
            Helmet  = 2,
            Arms    = 4,
            Show1st = 8,
            Has1st  = 0x10,
            Head2   = 0x20,

            NeedUpdate = Head | Helmet | Arms | Head2
        }

        internal void Clear(CameraUpdate update)
        {
            if (LastAddress == IntPtr.Zero)
                return;

            LastAddress  = IntPtr.Zero;
            LastSkeleton = IntPtr.Zero;
            LastFormId   = 0;
            LastFlags    = HideFlags.None;
            LastRaceId   = 0;

            CameraMain.Cull.Clear();
            SetFirstPersonSkeleton(null, update.GameCameraState.Id == TESCameraStates.FirstPerson, false);
            ClearHelmet();
        }

        internal void Update(CameraUpdate update)
        {
            var actor = update.Target.Actor;
            if (actor == null || update.Target.RootNode == null)
            {
                Clear(update);
                return;
            }

            {
                var addr   = actor.Address;
                var formId = actor.FormId;
                var race   = actor.Race;
                var raceId = race != null ? race.FormId : 0;
                var node   = update.Target.RootNode;
                var addr2  = node != null ? node.Address : IntPtr.Zero;

                if (addr != LastAddress || formId != LastFormId || raceId != LastRaceId || LastSkeleton != addr2)
                {
                    Clear(update);

                    LastAddress  = addr;
                    LastSkeleton = addr2;
                    LastFormId   = formId;
                    LastRaceId   = raceId;
                }
            }

            var wantFlags = HideFlags.None;
            if (update.Values.HideHead.CurrentValue >= 0.5)
                wantFlags |= HideFlags.Head | HideFlags.Helmet;
            if (update.Values.HideHead2.CurrentValue >= 0.5)
                wantFlags |= HideFlags.Head2;
            if (update.Values.HideArms.CurrentValue >= 0.5)
                wantFlags |= HideFlags.Arms;
            if (update.Values.Show1stPersonArms.CurrentValue >= 0.5)
                wantFlags |= HideFlags.Show1st;
            switch (update.GameCameraState.Id)
            {
                case TESCameraStates.FirstPerson:
                case TESCameraStates.Free:
                    break;

                default:
                    wantFlags |= HideFlags.Has1st;
                    break;
            }

            if (wantFlags == LastFlags)
            {
                if ((wantFlags & HideFlags.Helmet) != HideFlags.None)
                    UpdateHelmet(update.Target.RootNode);
                return;
            }

            if ((wantFlags & HideFlags.NeedUpdate) != (LastFlags & HideFlags.NeedUpdate))
            {
                CameraMain.Cull.Clear();
                ClearHelmet();
                LastFlags &= ~HideFlags.NeedUpdate;
                UpdateHideWithCull(actor, wantFlags, update.Target.RootNode as NiNode);
            }

            if ((wantFlags & HideFlags.Has1st) != (LastFlags & HideFlags.Has1st))
            {
                if ((wantFlags & HideFlags.Has1st) != HideFlags.None)
                {
                    var skeleton = actor.GetSkeletonNode(true);
                    if (skeleton != null)
                    {
                        SetFirstPersonSkeleton(skeleton, update.GameCameraState.Id == TESCameraStates.FirstPerson,
                                               (wantFlags & HideFlags.Show1st)     != HideFlags.None);
                        LastFlags |= HideFlags.Has1st;
                        if ((wantFlags & HideFlags.Show1st) != HideFlags.None)
                            LastFlags |= HideFlags.Show1st;
                    }
                }
                else
                {
                    LastFlags &= ~(HideFlags.Has1st | HideFlags.Show1st);
                    SetFirstPersonSkeleton(null, update.GameCameraState.Id == TESCameraStates.FirstPerson, false);
                }
            }
        }

        internal void UpdateFirstPersonSkeletonRotation(CameraUpdate update)
        {
            if (FirstPersonSkeleton == null)
                return;

            var transform = FirstPersonSkeleton.LocalTransform;
            var rot       = transform.Rotation;

            var result = update.Result.Transform.Rotation;
            rot.CopyFrom(result);

            var ymult = update.Values.FirstPersonSkeletonRotateYMultiplier.CurrentValue;
            if (ymult != 1.0)
            {
                var temp = CameraMain.TempResult.Transform.Rotation;
                temp.Identity(1.0f);

                var y = (float) (update.Values.InputRotationY.CurrentValue           *
                                 update.Values.InputRotationYMultiplier.CurrentValue * (ymult - 1.0));
                if (y != 0.0f)
                    temp.RotateX(y, temp);

                rot.Multiply(temp, rot);
            }

            FirstPersonSkeleton.Update(0.0f);
        }

        private static biped_mask GenerateBipedMask(params int[] slots)
        {
            var mask = new biped_mask();
            foreach (var s in slots)
                mask[s] = true;
            return mask;
        }

        private void ClearHelmet()
        {
            LastFlags &= ~HideFlags.Helmet;

            foreach (var x in LastHelmet)
                CameraMain.Cull.RemoveDisable(x);
            LastHelmet.Clear();
        }

        private void ExploreEquipment(NiNode root, NiAVObject current, List<NiAVObject> ls)
        {
            // Special case, we should ignore and stop looking here, head is handled elsewhere.
            if (current != null)
            {
                var nm = current.Name.Text ?? "";
                if (nm.Equals("BSFaceGenNiNodeSkinned", StringComparison.OrdinalIgnoreCase))
                    return;
            }

            if (current is BSGeometry)
            {
                var g    = (BSGeometry) current;
                var skin = g.Skin.Value;
                if (skin != null && skin is BSDismemberSkinInstance)
                {
                    var count = Memory.ReadInt32(skin.Address + 0x88);
                    if (count > 0)
                    {
                        var pbuf = Memory.ReadPointer(skin.Address + 0x90);
                        if (pbuf != IntPtr.Zero)
                        {
                            var isHelm = false;
                            for (var i = 0; i < count; i++)
                            {
                                int m = Memory.ReadUInt16(pbuf + (4 * i + 2));
                                if (NotHelmetBipedMask[m])
                                {
                                    isHelm = false;
                                    break;
                                }

                                if (IsHelmetBipedMask[m]) isHelm = true;
                            }

                            if (isHelm)
                                ls.Add(current);
                        }
                    }
                }
            }

            if (current is NiNode)
            {
                var n = current as NiNode;
                foreach (var ch in n.Children)
                    ExploreEquipment(root, ch, ls);
            }
        }

        private void FillValidBipedObjects(NiNode root, List<NiAVObject> ls) { ExploreEquipment(root, root, ls); }

        private List<NiAVObject> GetHelmetNodes(NiNode root)
        {
            var ls = new List<NiAVObject>();

            if (root != null)
                FillValidBipedObjects(root, ls);

            return ls;
        }

        private void InitializeHelmet(NiAVObject root, List<NiAVObject> calculated)
        {
            LastFlags |= HideFlags.Helmet;

            var rootNode = root as NiNode;
            if (rootNode == null)
                return;

            var ls = calculated ?? GetHelmetNodes(rootNode);
            foreach (var x in ls)
            {
                CameraMain.Cull.AddDisable(x);
                LastHelmet.Add(x);
            }
        }

        private void SetFirstPersonSkeleton(NiAVObject node, bool is1stPersonCam, bool shouldShow)
        {
            if (FirstPersonSkeleton != null)
                FirstPersonSkeleton.DecRef();

            FirstPersonSkeleton = node;

            if (FirstPersonSkeleton != null)
                FirstPersonSkeleton.IncRef();
        }

        private void UpdateHelmet(NiAVObject root)
        {
            if (root == null)
                return;

            var rootNode = root as NiNode;
            if (rootNode == null)
                return;

            var ls      = GetHelmetNodes(rootNode);
            var changed = ls.Count != LastHelmet.Count;
            if (!changed)
                for (var i = 0; i < ls.Count; i++)
                    if (!ls[i].Equals(LastHelmet[i]))
                    {
                        changed = true;
                        break;
                    }

            if (changed)
            {
                ClearHelmet();
                InitializeHelmet(root, ls);
            }
        }

        private void UpdateHideWithCull(Actor actor, HideFlags want, NiNode root)
        {
            if (root == null)
                return;

            if ((want & HideFlags.Head) != HideFlags.None)
            {
                var node = root.LookupNodeByName("BSFaceGenNiNodeSkinned");
                if (node != null)
                {
                    LastFlags |= HideFlags.Head;
                    CameraMain.Cull.AddDisable(node);
                }
            }

            if ((want & HideFlags.Head2) != HideFlags.None)
            {
                var node = root.LookupNodeByName("WereWolfLowHead01");
                if (node != null)
                {
                    LastFlags |= HideFlags.Head2;
                    CameraMain.Cull.AddDisable(node);

                    node = root.LookupNodeByName("WereWolfTeeth");
                    if (node != null)
                        CameraMain.Cull.AddDisable(node);

                    node = root.LookupNodeByName("EyesMaleWerewolfBeast");
                    if (node != null)
                        CameraMain.Cull.AddDisable(node);
                }
                else
                {
                    node = root.LookupNodeByName("NPC Head [Head]");
                    if (node != null)
                    {
                        LastFlags |= HideFlags.Head2;
                        CameraMain.Cull.AddUnscale(node);
                    }
                }
            }

            if ((want & HideFlags.Arms) != HideFlags.None)
            {
                var left  = root.LookupNodeByName("NPC L UpperArm [LUar]");
                var right = root.LookupNodeByName("NPC R UpperArm [RUar]");
                if (left != null && right != null)
                {
                    LastFlags |= HideFlags.Arms;
                    CameraMain.Cull.AddUnscale(left);
                    CameraMain.Cull.AddUnscale(right);
                }
            }

            if ((want & HideFlags.Helmet) != HideFlags.None)
                InitializeHelmet(root, null);
        }

        private sealed class biped_mask
        {
            private readonly ulong[] mask = new ulong[4];

            internal bool this[int index]
            {
                get
                {
                    if (index < 0 || index >= 256)
                        return false;

                    var i  = index / 64;
                    var j  = index % 64;
                    var fl = (ulong) 1 << j;
                    return (mask[i] & fl) != 0;
                }
                set
                {
                    if (index < 0 || index >= 256)
                        return;

                    var i  = index / 64;
                    var j  = index % 64;
                    var fl = (ulong) 1 << j;
                    if (value)
                        mask[i] |= fl;
                    else
                        mask[i] &= ~fl;
                }
            }
        }
    }
}