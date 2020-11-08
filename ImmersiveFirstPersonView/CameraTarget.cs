﻿namespace IFPV
{
    using NetScriptFramework.SkyrimSE;

    internal sealed class CameraTarget
    {
        private static readonly string[] EyeNodeNames =
        {
            "NPCEyeBone", "NPC Head [Head]", "NPC Head", "Head [Head]", "HEAD", "Scull", "FireAtronach_Head [Head]",
            "ElkScull", "Canine_Head", "DragPriestNPC Head [Head]", "DwarvenSpiderHead_XYZ", "Goat_Head",
            "ChaurusFlyerHead", "Boar_Reikling_Head", "NPC_mainbody_bone", "RabbitHead", "Horker_Head01",
            "HorseScull", "IW Head", "Mammoth Head", "MagicEffectsNode", "Sabrecat_Head [Head]",
            "SlaughterfishHead", "Wisp Head", "Witchlight Body", "NPC Spine2 [Spn2]", "NPC Root [Root]"
        };

        private static readonly string[] RootNodeNames =
        {
            // Don't use this!
            //"NPC COM [COM ]",
        };

        private CameraTarget() { }

        internal Actor Actor { get; private set; }

        internal NiAVObject HeadNode { get; private set; }

        internal TESObjectREFR Object { get; private set; }

        internal Actor OriginalActor { get; private set; }

        internal TESObjectREFR OriginalObject { get; private set; }

        internal NiAVObject RootNode { get; private set; }

        internal NiAVObject StabilizeRootNode { get; private set; }

        internal static CameraTarget Create(TESObjectREFR obj)
        {
            if (obj == null)
            {
                return null;
            }

            var originalObj = obj;
            var originalActor = obj as Actor;

            var isMountChange = false;
            {
                var horse = obj as Actor;
                if (horse != null && horse.IsBeingRidden)
                {
                    var rider = horse.GetMountedBy();
                    if (rider != null)
                    {
                        obj = rider;
                        isMountChange = true;
                    }
                }
            }

            var t = new CameraTarget();
            t.Object = obj;
            t.Actor = obj as Actor;
            t.OriginalObject = originalObj;
            t.OriginalActor = originalActor;

            var node = t.Actor != null && t.Actor.IsPlayer ? t.Actor.GetSkeletonNode(false) : obj.Node;
            if (node == null)
            {
                return null;
            }

            for (var i = 0; i < EyeNodeNames.Length; i++)
            {
                var name = EyeNodeNames[i];
                var n = node.LookupNodeByName(name);
                if (n != null)
                {
                    t.HeadNode = n;
                    break;
                }
            }

            if (t.HeadNode == null)
            {
                t.HeadNode = node;
            }

            for (var i = 0; i < RootNodeNames.Length; i++)
            {
                var name = RootNodeNames[i];
                var n = node.LookupNodeByName(name);
                if (n != null)
                {
                    t.RootNode = n;
                    break;
                }
            }

            if (t.RootNode == null)
            {
                t.RootNode = node;
            }

            t.StabilizeRootNode = t.RootNode;
            if (isMountChange && t.Actor != null && t.OriginalActor != null && !t.Actor.Equals(t.OriginalActor))
            {
                var stabilize = t.OriginalActor.IsPlayer
                    ? t.OriginalActor.GetSkeletonNode(false)
                    : t.OriginalActor.Node;
                if (stabilize != null)
                {
                    t.StabilizeRootNode = stabilize;
                }
            }

            return t;
        }
    }
}
