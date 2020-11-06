using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using IFPV.States;
using NetScriptFramework;

namespace IFPV
{
    internal sealed class CameraStack
    {
        internal readonly CameraMain        CameraMain;
        private readonly  List<CameraState> _states = new List<CameraState>();
        private readonly  CameraState[]     _temp;
        private           bool              Warned;

        internal CameraStack(CameraMain cameraMain)
        {
            if (cameraMain == null)
                throw new ArgumentNullException("cameraMain");

            CameraMain = cameraMain;

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var t in types)
            {
                if (t.IsAbstract || !t.IsSubclassOf(typeof(CameraState)))
                    continue;

                if (t == typeof(Custom) || t.IsSubclassOf(typeof(Custom)))
                    continue;

                var             cis = t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                ConstructorInfo ci  = null;
                foreach (var c in cis)
                    if (c.GetParameters().Length == 0)
                        ci = c;

                if (ci != null)
                {
                    var state = (CameraState) ci.Invoke(new object[0]);
                    if (state != null)
                    {
                        state._init(this);
                        _states.Add(state);

                        var grp = state.Group;
                        if (grp > MaxGroup)
                            MaxGroup = grp;
                    }
                }
            }

            LoadCustomProfiles();

            if (_states.Count > 1)
                _states.Sort((u, v) => u.Priority.CompareTo(v.Priority));

            MaxGroup = MaxGroup + 1;
            _temp    = new CameraState[MaxGroup];

            foreach (var s in _states)
                s.Initialize();
        }

        internal int MaxGroup { get; private set; }

        internal IReadOnlyList<CameraState> States => _states;

        internal void Check(CameraUpdate update)
        {
            foreach (var s in States)
            {
                var grp = s.Group;
                if (grp < 0 || grp >= MaxGroup)
                {
                    if (!Warned)
                    {
                        Warned = true;
                        Main.Log.AppendLine("IFPV: State " + s.GetType().Name + " has invalid group " + grp + "!");
                    }

                    continue;
                }

                var isActive  = s.Check(update);
                var wasActive = s.IsActive;
                s.__wActivate = isActive;
                if (!isActive || grp == 0)
                    continue;

                var p = _temp[grp];
                _temp[grp] = s;

                if (p != null)
                    p.__wActivate = false;
            }

            for (var i = 0; i < MaxGroup; i++)
                _temp[i] = null;

            foreach (var s in States)
            {
                var a = s.__wActivate;
                if (a != s.IsActive)
                {
                    s._set(a);
                    if (a)
                        s.OnEntering(update);
                    else
                        s.OnLeaving(update);
                }
            }
        }

        internal void DisableAll(CameraUpdate update)
        {
            foreach (var s in States)
                if (s.IsActive)
                {
                    s._set(false);
                    s.OnLeaving(update);
                }
        }

        internal void Update(CameraUpdate update)
        {
            foreach (var s in States)
                if (s.IsActive)
                    s.Update(update);
        }

        private void LoadCustomProfiles()
        {
            var dir = new DirectoryInfo("Data/NetScriptFramework/Plugins");
            if (!dir.Exists)
                return;

            var files  = dir.GetFiles();
            var prefix = Custom.Prefix + ".";
            var suffix = ".config.txt";
            foreach (var f in files)
            {
                if (!f.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!f.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    continue;

                var n = f.Name;
                n = n.Substring(prefix.Length);
                n = n.Substring(0, n.Length - suffix.Length);

                if (n.Length == 0)
                    continue;

                var state = Custom.LoadFrom(n);
                if (state == null)
                    continue;

                state._init(this);
                _states.Add(state);

                var grp = state.Group;
                if (grp > MaxGroup)
                    MaxGroup = grp;
            }
        }
    }
}