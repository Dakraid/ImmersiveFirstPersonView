namespace IFPV
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using NetScriptFramework;
    using States;

    internal sealed class CameraStack
    {
        private readonly List<CameraState> _states = new List<CameraState>();
        private readonly CameraState[] _temp;
        internal readonly CameraMain CameraMain;
        private bool Warned;

        internal CameraStack(CameraMain cameraMain)
        {
            if (cameraMain == null)
            {
                throw new ArgumentNullException("cameraMain");
            }

            this.CameraMain = cameraMain;

            var types = Assembly.GetExecutingAssembly().GetTypes();

            foreach (var t in types)
            {
                if (t.IsAbstract || !t.IsSubclassOf(typeof(CameraState)))
                {
                    continue;
                }

                if (t == typeof(Custom) || t.IsSubclassOf(typeof(Custom)))
                {
                    continue;
                }

                var cis = t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                ConstructorInfo ci = null;
                foreach (var c in cis)
                {
                    if (c.GetParameters().Length ==
                        0)
                    {
                        ci = c;
                    }
                }

                if (ci != null)
                {
                    var state = (CameraState)ci.Invoke(new object[0]);
                    if (state != null)
                    {
                        state._init(this);
                        this._states.Add(state);

                        var grp = state.Group;
                        if (grp > this.MaxGroup)
                        {
                            this.MaxGroup = grp;
                        }
                    }
                }
            }

            this.LoadCustomProfiles();

            if (this._states.Count > 1)
            {
                this._states.Sort((u, v) => u.Priority.CompareTo(v.Priority));
            }

            this.MaxGroup = this.MaxGroup + 1;
            this._temp = new CameraState[this.MaxGroup];

            foreach (var s in this._states)
            {
                s.Initialize();
            }
        }

        internal int MaxGroup { get; private set; }

        internal IReadOnlyList<CameraState> States => this._states;

        internal void Check(CameraUpdate update)
        {
            foreach (var s in this.States)
            {
                var grp = s.Group;
                if (grp < 0 || grp >= this.MaxGroup)
                {
                    if (!this.Warned)
                    {
                        this.Warned = true;
                        Main.Log.AppendLine("IFPV: State " +
                                            s.GetType().Name +
                                            " has invalid group " +
                                            grp +
                                            "!");
                    }

                    continue;
                }

                var isActive = s.Check(update);
                var wasActive = s.IsActive;
                s.__wActivate = isActive;
                if (!isActive || grp == 0)
                {
                    continue;
                }

                var p = this._temp[grp];
                this._temp[grp] = s;

                if (p != null)
                {
                    p.__wActivate = false;
                }
            }

            for (var i = 0; i < this.MaxGroup; i++)
            {
                this._temp[i] = null;
            }

            foreach (var s in this.States)
            {
                var a = s.__wActivate;
                if (a != s.IsActive)
                {
                    s._set(a);
                    if (a)
                    {
                        s.OnEntering(update);
                    }
                    else
                    {
                        s.OnLeaving(update);
                    }
                }
            }
        }

        internal void DisableAll(CameraUpdate update)
        {
            foreach (var s in this.States)
            {
                if (s.IsActive)
                {
                    s._set(false);
                    s.OnLeaving(update);
                }
            }
        }

        internal void Update(CameraUpdate update)
        {
            foreach (var s in this.States)
            {
                if (s.IsActive)
                {
                    s.Update(update);
                }
            }
        }

        private void LoadCustomProfiles()
        {
            var dir = new DirectoryInfo("Data/NetScriptFramework/Plugins");
            if (!dir.Exists)
            {
                return;
            }

            var files = dir.GetFiles();
            var prefix = Custom.Prefix + ".";
            var suffix = ".config.txt";
            foreach (var f in files)
            {
                if (!f.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!f.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var n = f.Name;
                n = n.Substring(prefix.Length);
                n = n.Substring(0, n.Length - suffix.Length);

                if (n.Length == 0)
                {
                    continue;
                }

                var state = Custom.LoadFrom(n);
                if (state == null)
                {
                    continue;
                }

                state._init(this);
                this._states.Add(state);

                var grp = state.Group;
                if (grp > this.MaxGroup)
                {
                    this.MaxGroup = grp;
                }
            }
        }
    }
}