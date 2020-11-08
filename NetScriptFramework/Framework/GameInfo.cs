using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScriptFramework
{
    /// <summary>
    /// Contains custom information about the application types and addresses.
    /// </summary>
    public sealed class GameInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameInfo" /> class.
        /// </summary>
        /// <param name="baseOffset">The base offset.</param>
        /// <param name="is64Bit">if set to <c>true</c> then library is 64 bit.</param>
        internal GameInfo(ulong baseOffset, bool is64Bit)
        {
            BaseOffset = baseOffset;
            Is64Bit    = is64Bit;
        }

        /// <summary>
        /// The library is 64-bit?
        /// </summary>
        private readonly bool Is64Bit;

        /// <summary>
        /// The registration list.
        /// </summary>
        internal readonly List<GameTypeRegistration> registrationList = new List<GameTypeRegistration>();

        /// <summary>
        /// The cached values.
        /// </summary>
        internal readonly List<int?> cachedValues = new List<int?>();

        /// <summary>
        /// The functions.
        /// </summary>
        private readonly List<GameFunctionInfo> functionsList = new List<GameFunctionInfo>();

        /// <summary>
        /// The types.
        /// </summary>
        private readonly List<GameTypeInfo> typesList = new List<GameTypeInfo>();

        /// <summary>
        /// The globals.
        /// </summary>
        private readonly List<GameGlobalInfo> globalsList = new List<GameGlobalInfo>();

        /// <summary>
        /// The vid address map.
        /// </summary>
        private readonly Dictionary<ulong, ulong> vidAddrMap = new Dictionary<ulong, ulong>();

        /// <summary>
        /// The vftable type map.
        /// </summary>
        private readonly Dictionary<ulong, GameTypeInfo> vtTpMap = new Dictionary<ulong, GameTypeInfo>();

        /// <summary>
        /// The unique id type map.
        /// </summary>
        private readonly Dictionary<ulong, GameTypeInfo> uqTpMap = new Dictionary<ulong, GameTypeInfo>();

        /// <summary>
        /// The vid function map.
        /// </summary>
        private readonly Dictionary<ulong, GameFunctionInfo> vidFnMap = new Dictionary<ulong, GameFunctionInfo>();

        /// <summary>
        /// The vid global map.
        /// </summary>
        private readonly Dictionary<ulong, GameGlobalInfo> vidGbMap = new Dictionary<ulong, GameGlobalInfo>();

        /// <summary>
        /// The type instance info map.
        /// </summary>
        private readonly Dictionary<uint, List<GameTypeInstanceInfo>> tiiMap = new Dictionary<uint, List<GameTypeInstanceInfo>>();

        /// <summary>
        /// The file version.
        /// </summary>
        internal int[] FileVersion = null;

        /// <summary>
        /// The alias file version.
        /// </summary>
        internal int[] AliasFileVersion = null;

        /// <summary>
        /// The library version.
        /// </summary>
        internal int LibraryVersion;

        /// <summary>
        /// The hash version.
        /// </summary>
        internal ulong HashVersion;

        /// <summary>
        /// The base offset.
        /// </summary>
        public readonly ulong BaseOffset;

        /// <summary>
        /// Gets the library base offset.
        /// </summary>
        /// <value>
        /// The library base offset.
        /// </value>
        public ulong LibraryBaseOffset { get; internal set; }

        /// <summary>
        /// Gets the types.
        /// </summary>
        /// <value>
        /// The types.
        /// </value>
        public IReadOnlyList<GameTypeInfo> Types => typesList;

        /// <summary>
        /// Gets the globals.
        /// </summary>
        /// <value>
        /// The globals.
        /// </value>
        public IReadOnlyList<GameGlobalInfo> Globals => globalsList;

        /// <summary>
        /// Gets the functions.
        /// </summary>
        /// <value>
        /// The functions.
        /// </value>
        public IReadOnlyList<GameFunctionInfo> Functions => functionsList;

        /// <summary>
        /// Gets the cached values.
        /// </summary>
        /// <value>
        /// The cached values.
        /// </value>
        public IReadOnlyList<int?> CachedValues => cachedValues;

        /// <summary>
        /// Dumps the version independent identifiers to specified file. Format will be "id tab offset" on each line.
        /// </summary>
        /// <param name="targetFileInfo">The target file information.</param>
        public void DumpVids(System.IO.FileInfo targetFileInfo)
        {
            using (var sw = targetFileInfo.CreateText())
            {
                foreach (var x in vidAddrMap)
                {
                    sw.Write(x.Key);
                    sw.Write("\t0x");
                    sw.Write(x.Value.ToString("X"));
                    sw.WriteLine();
                }
            }
        }

    #if NETSCRIPTFRAMEWORK
        /// <summary>
        /// Gets the type from vtable address.
        /// </summary>
        /// <param name="vtable">The vtable address.</param>
        /// <param name="withBaseOffset">Does the address include base offset of module?</param>
        /// <returns></returns>
        public GameTypeInfo GetTypeInfo(IntPtr vtable, bool withBaseOffset)
        {
            var v = Is64Bit ? vtable.ToUInt64() : vtable.ToUInt32();
            if (withBaseOffset)
                v = unchecked(v - BaseOffset);

            GameTypeInfo result = null;
            if (vtTpMap.TryGetValue(v, out result))
                return result;
            return null;
        }

        /// <summary>
        /// Comparer for function search.
        /// </summary>
        private sealed class FunctionSearcher : IComparer<GameFunctionInfo>
        {
            /// <summary>
            /// Gets or sets the address.
            /// </summary>
            /// <value>
            /// The address.
            /// </value>
            internal ulong Address { get; set; }

            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
            /// </returns>
            public int Compare(GameFunctionInfo x, GameFunctionInfo y)
            {
                var v = Address;
                var a = x;
                var m = 1;
                if (a == null)
                {
                    a = y;
                    m = -1;
                }

                if (a.End <= v)
                    return -m;
                if (a.Begin > v)
                    return m;
                return 0;
            }
        }

        /// <summary>
        /// Gets the function information.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="withBaseOffset">Does the address include base offset of module?</param>
        /// <returns></returns>
        public GameFunctionInfo GetFunctionInfo(IntPtr address, bool withBaseOffset)
        {
            var v = Is64Bit ? address.ToUInt64() : address.ToUInt32();
            if (withBaseOffset)
                v = unchecked(v - BaseOffset);

            /*foreach (var f in this.functionsList)
            {
                if (v >= f.Begin && v < f.End)
                    return f;
            }*/

            var searcher = new FunctionSearcher() {Address = v};
            var result   = functionsList.BinarySearch(null, searcher);
            if (result < 0)
                return null;
            return functionsList[result];
        }
    #endif

        /// <summary>
        /// Gets the function information.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public GameFunctionInfo GetFunctionInfo(ulong id)
        {
            if (id != 0)
            {
                GameFunctionInfo fi = null;
                if (vidFnMap.TryGetValue(id, out fi))
                    return fi;
            }

            return null;
        }

        /// <summary>
        /// Gets the global information.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public GameGlobalInfo GetGlobalInfo(ulong id)
        {
            if (id != 0)
            {
                GameGlobalInfo fi = null;
                if (vidGbMap.TryGetValue(id, out fi))
                    return fi;
            }

            return null;
        }

        /// <summary>
        /// Gets the type information.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public GameTypeInfo GetTypeInfo(ulong id)
        {
            if (id != 0)
            {
                GameTypeInfo ti = null;
                if (uqTpMap.TryGetValue(id, out ti))
                    return ti;
            }

            return null;
        }

        /// <summary>
        /// Gets the type instance infos.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public IReadOnlyList<GameTypeInstanceInfo> GetTypeInstanceInfos(uint id)
        {
            if (id != 0)
            {
                List<GameTypeInstanceInfo> ls = null;
                if (tiiMap.TryGetValue(id, out ls))
                    return ls;
            }

            return EmptyTypeInstanceInfos;
        }

        /// <summary>
        /// The empty type instance infos.
        /// </summary>
        private static readonly GameTypeInstanceInfo[] EmptyTypeInstanceInfos = new GameTypeInstanceInfo[0];

    #if NETSCRIPTFRAMEWORK
        /// <summary>
        /// Gets the address of the specified object by its version independent identifier. This will throw an exception if the address was not found.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="extraOffset">Extra offset to add to the address.</param>
        /// <param name="patternOffset">Offset of pattern at target address (id + extraOffset).</param>
        /// <param name="pattern">Must have this byte pattern at target location (id + extraOffset + patternOffset) (optional).</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
        public IntPtr GetAddressOf(ulong id, int extraOffset = 0, int patternOffset = 0, string pattern = null)
        {
            ulong offset = 0;
            if (id != 0 && vidAddrMap.TryGetValue(id, out offset))
            {
                var    full = BaseOffset + offset;
                IntPtr result;
                if (Is64Bit)
                    result = new IntPtr(unchecked((long) full));
                else
                    result = new IntPtr(unchecked((int) full));
                if (extraOffset != 0)
                    result = result + extraOffset;
                if (!string.IsNullOrEmpty(pattern))
                {
                    var target = result + patternOffset;
                    while (pattern.Length >= 2 && pattern[0] == '[' && pattern[pattern.Length - 1] == ']')
                    {
                        pattern = pattern.Substring(1, pattern.Length - 2);
                        target  = Memory.ReadPointer(target);
                    }

                    if (!Memory.VerifyBytes(target, pattern))
                        throw new ArgumentException("Object with version independent id `" + id +
                                                    "` did not match specified byte pattern! This usually means plugin must be updated by author.");
                }

                return result;
            }

            throw new KeyNotFoundException("Object with version independent id `" + id +
                                           "` was not found in version library! This usually means plugin must be updated by author.");
        }

        /// <summary>
        /// Tries to get the address of the specified object by its version independent identifier. This will return null if the address was not found.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="extraOffset">Extra offset to add to the address.</param>
        /// <param name="patternOffset">Offset of pattern at target address (id + extraOffset).</param>
        /// <param name="pattern">Must have this byte pattern at target location (optional).</param>
        /// <returns></returns>
        public IntPtr? TryGetAddressOf(ulong id, int extraOffset = 0, int patternOffset = 0, string pattern = null)
        {
            ulong offset = 0;
            if (id != 0 && vidAddrMap.TryGetValue(id, out offset))
            {
                var    full = BaseOffset + offset;
                IntPtr result;
                if (Is64Bit)
                    result = new IntPtr(unchecked((long) full));
                else
                    result = new IntPtr(unchecked((int) full));
                if (extraOffset != 0)
                    result = result + extraOffset;
                if (!string.IsNullOrEmpty(pattern))
                {
                    var target = result + patternOffset;
                    while (pattern.Length >= 2 && pattern[0] == '[' && pattern[pattern.Length - 1] == ']')
                    {
                        pattern = pattern.Substring(1, pattern.Length - 2);
                        if (!Memory.TryReadPointer(target, ref target))
                            return null;
                    }

                    try
                    {
                        if (!Memory.VerifyBytes(target, pattern))
                            return null;
                    }
                    catch { return null; }
                }

                return result;
            }

            return null;
        }
    #endif

        /// <summary>
        /// Adds the type information.
        /// </summary>
        /// <param name="dt">The type info.</param>
        internal void AddTypeInfo(GameTypeInfo dt)
        {
            if (dt.Id != 0)
            {
                if (uqTpMap.ContainsKey(dt.Id))
                    throw new ArgumentException("A type with this unique identifier (" + dt.Id + ") is already added!");

                uqTpMap[dt.Id] = dt;
            }

            if (dt.VTable != 0)
                if (!vtTpMap.ContainsKey(dt.VTable))
                    vtTpMap[dt.VTable] = dt;
            typesList.Add(dt);
        }

        /// <summary>
        /// Adds the function information.
        /// </summary>
        /// <param name="fi">The function info.</param>
        internal void AddFunctionInfo(GameFunctionInfo fi)
        {
            if (fi.Id != 0)
            {
                if (vidAddrMap.ContainsKey(fi.Id))
                    throw new ArgumentException("An object with specified version independent identifier (" + fi.Id + ") was already registered!");

                vidAddrMap[fi.Id] = fi.Begin;
                vidFnMap[fi.Id]   = fi;
            }

            functionsList.Add(fi);
        }

        /// <summary>
        /// Adds the function information.
        /// </summary>
        /// <param name="gb">The global info.</param>
        internal void AddGlobalInfo(GameGlobalInfo gb)
        {
            if (gb.Id != 0)
            {
                if (vidAddrMap.ContainsKey(gb.Id))
                    throw new ArgumentException("An object with specified version independent identifier (" + gb.Id + ") was already registered!");

                vidAddrMap[gb.Id] = gb.Begin;
                vidGbMap[gb.Id]   = gb;
            }

            globalsList.Add(gb);
        }

        /// <summary>
        /// Adds the type instance infos.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ls">The list.</param>
        internal void AddTypeInstanceInfos(uint id, List<GameTypeInstanceInfo> ls)
        {
            if (id != 0)
                tiiMap[id] = ls;
        }

        /// <summary>
        /// Reads from specified file. This will clear previous info.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="isLoadingAlias">Is loading alias already?</param>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        internal void ReadFromFile(System.IO.FileInfo file, int isLoadingAlias)
        {
            Clear();

            if (!file.Exists)
                throw new System.IO.FileNotFoundException(file.FullName);

            using (var stream = file.OpenRead())
            {
                using (var comp = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
                {
                    using (var reader = new System.IO.BinaryReader(comp))
                    {
                        ReadFromStream(reader, file, isLoadingAlias);

                        if (isLoadingAlias == 0 && AliasFileVersion != null)
                        {
                            FileVersion      = AliasFileVersion;
                            AliasFileVersion = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes to specified file. This will replace the file if it already exists.
        /// </summary>
        /// <param name="file">The file.</param>
        internal void WriteToFile(System.IO.FileInfo file)
        {
            functionsList.Sort((u, v) => u.Begin.CompareTo(v.Begin));

            using (var stream = file.Create())
            {
                using (var comp = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Compress))
                {
                    using (var writer = new System.IO.BinaryWriter(comp)) { WriteToStream(writer); }
                }
            }
        }

        /// <summary>
        /// Clears this instance from all data.
        /// </summary>
        internal void Clear()
        {
            cachedValues.Clear();
            registrationList.Clear();
            typesList.Clear();
            globalsList.Clear();
            functionsList.Clear();
            vidAddrMap.Clear();
            vidFnMap.Clear();
            vidGbMap.Clear();
            vtTpMap.Clear();
            uqTpMap.Clear();
            tiiMap.Clear();
        }

        /// <summary>
        /// The stream version.
        /// </summary>
        private const int StreamVersion = 2;

        /// <summary>
        /// Reads from stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="info">The file.</param>
        /// <param name="isLoadingAlias">Counter for loading alias file.</param>
        /// <exception cref="System.IO.InvalidDataException">Version of object is not supported!</exception>
        private void ReadFromStream(System.IO.BinaryReader stream, System.IO.FileInfo info, int isLoadingAlias)
        {
            var version = stream.ReadInt32();
            if (version < 2 || version > StreamVersion)
                throw new System.IO.InvalidDataException("Version of library is not supported!");

            LibraryVersion = stream.ReadInt32();
            FileVersion    = new int[4];
            for (var i = 0; i < 4; i++)
                FileVersion[i] = stream.ReadInt32();

            if (stream.ReadByte() != 0)
            {
                if (isLoadingAlias >= 10)
                    throw new ArgumentException("Version library failed to load because alias loading depth was exceeded (" + isLoadingAlias +
                                                ")! This could indicate infinite alias file recursion.");

                var alias = new int[4];
                for (var i = 0; i < 4; i++)
                    alias[i] = stream.ReadInt32();

                if (isLoadingAlias == 0)
                    AliasFileVersion = FileVersion;

                var oldPart = string.Join("_", FileVersion);
                var newPart = string.Join("_", alias);

                int replaceIndex;
                if ((replaceIndex = info.FullName.LastIndexOf(oldPart)) < 0)
                    throw new ArgumentException("Unable to solve library file alias because old file name did not contain `" + oldPart + "`!");

                var newName = info.FullName.Remove(replaceIndex, oldPart.Length);
                newName = newName.Insert(replaceIndex, newPart);

                if (newName == info.FullName)
                    throw new ArgumentException("Unable to load library due to alias pointing to same file!");

                var newFile = new System.IO.FileInfo(newName);
                ReadFromFile(newFile, isLoadingAlias + 1);
                return;
            }

            LibraryBaseOffset = stream.ReadUInt64();
            HashVersion       = stream.ReadUInt64();

            {
                var count = stream.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var dt = new GameTypeInfo();
                    dt.ReadFromStream(stream, version);
                    AddTypeInfo(dt);
                }
            }

            {
                var count = stream.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var fi = new GameFunctionInfo();
                    fi.ReadFromStream(stream, version);
                    AddFunctionInfo(fi);
                }
            }

            {
                var count = stream.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var fi = new GameGlobalInfo();
                    fi.ReadFromStream(stream, version);
                    AddGlobalInfo(fi);
                }
            }

            {
                var count = stream.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var ri = new GameTypeRegistration();
                    ri.ReadFromStream(stream, version);
                    registrationList.Add(ri);
                }
            }

            {
                var count = stream.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    int  lcount;
                    uint lid;
                    {
                        var ltype = stream.ReadByte();

                        if ((ltype & 1) == 0)
                            lcount = stream.ReadByte();
                        else
                            lcount = stream.ReadInt32();

                        if ((ltype & 2) != 0)
                            lid = stream.ReadUInt16();
                        else if ((ltype & 4) != 0)
                            lid = stream.ReadByte();
                        else
                            lid = stream.ReadUInt32();
                    }

                    var ls = new List<GameTypeInstanceInfo>(Math.Max(0, Math.Min(lcount, 64)));
                    for (var j = 0; j < lcount; j++)
                    {
                        int?  begin = null;
                        int?  end   = null;
                        ulong id    = 0;

                        var jtype = stream.ReadByte();
                        if ((jtype & 1) != 0)
                        {
                            if ((jtype & 2) != 0)
                                begin = stream.ReadByte();
                            else if ((jtype & 4) != 0)
                                begin = stream.ReadUInt16();
                            else
                                begin = stream.ReadInt32();
                        }

                        if ((jtype & 8) != 0)
                        {
                            if ((jtype & 0x10) != 0)
                                end = stream.ReadByte();
                            else if ((jtype & 0x20) != 0)
                                end = stream.ReadUInt16();
                            else
                                end = stream.ReadInt32();
                        }

                        if ((jtype & 0x40) != 0)
                            id = stream.ReadUInt16();
                        else if ((jtype & 0x80) != 0)
                            id = stream.ReadUInt32();
                        else
                            id = stream.ReadUInt64();

                        var inf = GetTypeInfo(id);
                        var ti  = new GameTypeInstanceInfo(begin, end, inf);
                        ls.Add(ti);
                    }

                    tiiMap[lid] = ls;
                }
            }

            {
                var count = stream.ReadInt32();
                var did   = 0;
                while (did < count)
                {
                    var nx = stream.ReadInt32();
                    if (nx > 0)
                    {
                        did += nx;
                        for (var i = 0; i < nx; i++)
                        {
                            var val = stream.ReadInt32();
                            cachedValues.Add(val);
                        }
                    }
                    else
                    {
                        nx  =  -nx;
                        did += nx;
                        for (var i = 0; i < nx; i++)
                            cachedValues.Add(null);
                    }
                }
            }
        }

        /// <summary>
        /// Writes to stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        private void WriteToStream(System.IO.BinaryWriter stream)
        {
            stream.Write(StreamVersion);

            stream.Write(LibraryVersion);

            for (var i = 0; i < 4; i++)
                stream.Write(FileVersion[i]);

            if (AliasFileVersion != null)
            {
                stream.Write((byte) 1);
                for (var i = 0; i < 4; i++)
                    stream.Write(AliasFileVersion[i]);
                return;
            }
            else { stream.Write((byte) 0); }

            stream.Write(LibraryBaseOffset);
            stream.Write(HashVersion);

            {
                stream.Write(typesList.Count);
                foreach (var x in typesList)
                    x.WriteToStream(stream);
            }

            {
                stream.Write(functionsList.Count);
                foreach (var x in functionsList)
                    x.WriteToStream(stream);
            }

            {
                stream.Write(globalsList.Count);
                foreach (var x in globalsList)
                    x.WriteToStream(stream);
            }

            {
                stream.Write(registrationList.Count);
                foreach (var x in registrationList)
                    x.WriteToStream(stream);
            }

            {
                stream.Write(tiiMap.Count);
                foreach (var pair in tiiMap)
                {
                    {
                        var lid    = pair.Key;
                        var ls     = pair.Value;
                        var lcount = ls.Count;

                        byte ltype = 0;
                        if (lcount < 0 || lcount > byte.MaxValue)
                            ltype |= 1;

                        if (lid <= byte.MaxValue)
                            ltype |= 4;
                        else if (lid <= ushort.MaxValue)
                            ltype |= 2;

                        stream.Write(ltype);

                        if ((ltype & 1) == 0)
                            stream.Write((byte) lcount);
                        else
                            stream.Write(lcount);

                        if ((ltype & 2) != 0)
                            stream.Write((ushort) lid);
                        else if ((ltype & 4) != 0)
                            stream.Write((byte) lid);
                        else
                            stream.Write(lid);
                    }

                    foreach (var x in pair.Value)
                    {
                        byte jtype = 0;
                        if (x.BeginOffset.HasValue)
                        {
                            jtype |= 1;
                            if (x.BeginOffset.Value >= 0 && x.BeginOffset.Value <= byte.MaxValue)
                                jtype |= 2;
                            else if (x.BeginOffset.Value >= 0 && x.BeginOffset.Value <= ushort.MaxValue)
                                jtype |= 4;
                        }

                        if (x.EndOffset.HasValue)
                        {
                            jtype |= 8;
                            if (x.EndOffset.Value >= 0 && x.EndOffset.Value <= byte.MaxValue)
                                jtype |= 0x10;
                            else if (x.EndOffset.Value >= 0 && x.EndOffset.Value <= ushort.MaxValue)
                                jtype |= 0x20;
                        }

                        var id = x.Info != null ? x.Info.Id : 0;
                        if (id <= ushort.MaxValue)
                            jtype |= 0x40;
                        else if (id <= uint.MaxValue)
                            jtype |= 0x80;

                        stream.Write(jtype);

                        if ((jtype & 1) != 0)
                        {
                            if ((jtype & 2) != 0)
                                stream.Write((byte) x.BeginOffset.Value);
                            else if ((jtype & 4) != 0)
                                stream.Write((ushort) x.BeginOffset.Value);
                            else
                                stream.Write(x.BeginOffset.Value);
                        }

                        if ((jtype & 8) != 0)
                        {
                            if ((jtype & 0x10) != 0)
                                stream.Write((byte) x.EndOffset.Value);
                            else if ((jtype & 0x20) != 0)
                                stream.Write((ushort) x.EndOffset.Value);
                            else
                                stream.Write(x.EndOffset.Value);
                        }

                        if ((jtype & 0x40) != 0)
                            stream.Write((ushort) id);
                        else if ((jtype & 0x80) != 0)
                            stream.Write((uint) id);
                        else
                            stream.Write(id);
                    }
                }
            }

            {
                var cvt = cachedValues.Count;
                stream.Write(cvt);
                var lastWrite = 0;
                while (lastWrite < cvt)
                {
                    var cnt = 1;
                    if (cachedValues[lastWrite].HasValue)
                    {
                        for (var i = lastWrite + 1; i < cvt; i++)
                        {
                            var v = cachedValues[i];
                            if (!v.HasValue)
                                break;

                            cnt++;
                        }

                        stream.Write(cnt);

                        for (var i = 0; i < cnt; i++)
                            stream.Write(cachedValues[i + lastWrite].Value);
                        lastWrite += cnt;
                    }
                    else
                    {
                        for (var i = lastWrite + 1; i < cvt; i++)
                        {
                            var v = cachedValues[i];
                            if (v.HasValue)
                                break;

                            cnt++;
                        }

                        stream.Write(-cnt);

                        lastWrite += cnt;
                    }
                }
            }
        }

        /// <summary>
        /// Information about an instance of a type.
        /// </summary>
        public sealed class GameTypeInstanceInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GameTypeInstanceInfo"/> class.
            /// </summary>
            /// <param name="begin">The begin.</param>
            /// <param name="end">The end.</param>
            /// <param name="info">The type information.</param>
            public GameTypeInstanceInfo(int? begin, int? end, GameTypeInfo info)
            {
                BeginOffset = begin;
                EndOffset   = end;
                Info        = info;
            }

            /// <summary>
            /// The begin offset. This may be null if unknown.
            /// </summary>
            public readonly int? BeginOffset;

            /// <summary>
            /// The end offset. This may be null if unknown.
            /// </summary>
            public readonly int? EndOffset;

            /// <summary>
            /// The information of the type from version library.
            /// </summary>
            public readonly GameTypeInfo Info;
        }

        /// <summary>
        /// Debug info for a type.
        /// </summary>
        public sealed class GameTypeInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GameTypeInfo"/> class.
            /// </summary>
            internal GameTypeInfo() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="GameTypeInfo"/> class.
            /// </summary>
            /// <param name="guid">The unique id.</param>
            /// <param name="vtable">The vtable.</param>
            /// <param name="name">The name.</param>
            /// <param name="size">The size.</param>
            /// <param name="fields">The fields.</param>
            public GameTypeInfo(ulong guid, ulong vtable, string name, int? size, IReadOnlyList<GameFieldInfo> fields)
            {
                Id     = guid;
                VTable = vtable;
                Name   = name;
                Size   = size;
                Fields = fields;
            }

            /// <summary>
            /// Gets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public ulong Id { get; private set; }

            /// <summary>
            /// The virtual function table address offset.
            /// </summary>
            public ulong VTable { get; private set; }

            /// <summary>
            /// The name of type to display.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Gets the size of type. This is null if unknown.
            /// </summary>
            /// <value>
            /// The size.
            /// </value>
            public int? Size { get; private set; }

            /// <summary>
            /// Gets or sets the known fields list. This may be null if unknown or missing.
            /// </summary>
            /// <value>
            /// The fields.
            /// </value>
            public IReadOnlyList<GameFieldInfo> Fields { get; private set; }

            /// <summary>
            /// Reads from stream.
            /// </summary>
            /// <param name="stream">The stream.</param>
            /// <param name="version">The version.</param>
            /// <exception cref="System.IO.InvalidDataException">Version of object is not supported!</exception>
            internal void ReadFromStream(System.IO.BinaryReader stream, int version)
            {
                var ltype = stream.ReadByte();

                if ((ltype & 1) != 0)
                {
                    if ((ltype & 2) != 0)
                        VTable = stream.ReadUInt64();
                    else
                        VTable = stream.ReadUInt32();
                }

                Name = stream.ReadString();
                if ((ltype & 4) != 0)
                    Size = stream.ReadInt32();

                if ((ltype & 8) != 0)
                    Id = stream.ReadUInt64();
                else
                    Id = stream.ReadUInt32();

                if ((ltype & 0x10) != 0)
                {
                    var cn = stream.ReadInt32();
                    var ls = new List<GameFieldInfo>(Math.Min(256, cn));
                    for (var i = 0; i < cn; i++)
                    {
                        var fi = new GameFieldInfo();
                        fi.ReadFromStream(stream, version);
                        ls.Add(fi);
                    }

                    Fields = ls;
                }
            }

            /// <summary>
            /// Writes to stream.
            /// </summary>
            /// <param name="stream">The stream.</param>
            internal void WriteToStream(System.IO.BinaryWriter stream)
            {
                byte ltype = 0;
                if (VTable != 0)
                {
                    ltype |= 1;
                    if (VTable > uint.MaxValue)
                        ltype |= 2;
                }

                if (Size.HasValue)
                    ltype |= 4;
                if (Id > uint.MaxValue)
                    ltype |= 8;
                if (Fields != null && Fields.Count != 0)
                    ltype |= 0x10;

                stream.Write(ltype);

                if ((ltype & 1) != 0)
                {
                    if ((ltype & 2) != 0)
                        stream.Write(VTable);
                    else
                        stream.Write((uint) VTable);
                }

                stream.Write(Name);
                if ((ltype & 4) != 0)
                    stream.Write(Size.Value);
                if ((ltype & 8) != 0)
                    stream.Write(Id);
                else
                    stream.Write((uint) Id);
                if ((ltype & 0x10) != 0)
                {
                    stream.Write(Fields.Count);
                    foreach (var x in Fields)
                        x.WriteToStream(stream);
                }
            }
        }

        /// <summary>
        /// Debug info for a field of a type.
        /// </summary>
        public sealed class GameFieldInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GameFieldInfo"/> class.
            /// </summary>
            internal GameFieldInfo() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="GameFieldInfo"/> class.
            /// </summary>
            /// <param name="fieldId">The field identifier.</param>
            /// <param name="begin">The begin.</param>
            /// <param name="shortname">The shortname.</param>
            /// <param name="typename">The typename.</param>
            public GameFieldInfo(uint fieldId, int? begin, string shortname, string typename)
            {
                FieldId   = fieldId;
                Begin     = begin;
                ShortName = shortname;
                TypeName  = typename;
            }

            /// <summary>
            /// Gets the field identifier. This is not the same as global unique identifier. Fields in different types can have the same identifier.
            /// </summary>
            /// <value>
            /// The field identifier.
            /// </value>
            public uint FieldId { get; private set; }

            /// <summary>
            /// Gets the begin offset in complete type. This is null if unknown.
            /// </summary>
            /// <value>
            /// The begin.
            /// </value>
            public int? Begin { get; private set; }

            /// <summary>
            /// Gets the name of field. This is null if unknown.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string ShortName { get; private set; }

            /// <summary>
            /// Gets the value type name of the field. This is null if unknown.
            /// </summary>
            /// <value>
            /// The name of the type.
            /// </value>
            public string TypeName { get; private set; }

            /// <summary>
            /// Reads from stream.
            /// </summary>
            /// <param name="stream">The stream.</param>
            /// <param name="version">The version.</param>
            /// <exception cref="System.IO.InvalidDataException">Version of object is not supported!</exception>
            internal void ReadFromStream(System.IO.BinaryReader stream, int version)
            {
                var ltype = stream.ReadByte();
                if ((ltype & 1) != 0)
                {
                    if ((ltype & 2) != 0)
                        Begin = stream.ReadInt32();
                    else
                        Begin = stream.ReadUInt16();
                }

                if ((ltype & 4) != 0)
                    ShortName = stream.ReadString();
                if ((ltype & 8) != 0)
                    TypeName = stream.ReadString();
                if ((ltype & 0x10) != 0)
                {
                    if ((ltype & 0x20) != 0)
                        FieldId = stream.ReadByte();
                    else
                        FieldId = stream.ReadUInt16();
                }
                else { FieldId = stream.ReadUInt32(); }
            }

            /// <summary>
            /// Writes to stream.
            /// </summary>
            /// <param name="stream">The stream.</param>
            internal void WriteToStream(System.IO.BinaryWriter stream)
            {
                byte ltype = 0;
                if (Begin.HasValue)
                {
                    ltype |= 1;
                    if (Begin.Value < 0 || Begin.Value > ushort.MaxValue)
                        ltype |= 2;
                }

                if (!string.IsNullOrEmpty(ShortName))
                    ltype |= 4;
                if (!string.IsNullOrEmpty(TypeName))
                    ltype |= 8;
                if (FieldId <= byte.MaxValue)
                    ltype |= 0x30;
                else if (FieldId <= ushort.MaxValue)
                    ltype |= 0x10;

                stream.Write(ltype);

                if ((ltype & 1) != 0)
                {
                    if ((ltype & 2) != 0)
                        stream.Write(Begin.Value);
                    else
                        stream.Write((ushort) Begin.Value);
                }

                if ((ltype & 4) != 0)
                    stream.Write(ShortName);
                if ((ltype & 8) != 0)
                    stream.Write(TypeName);
                if ((ltype & 0x10) != 0)
                {
                    if ((ltype & 0x20) != 0)
                        stream.Write((byte) FieldId);
                    else
                        stream.Write((ushort) FieldId);
                }
                else { stream.Write(FieldId); }
            }
        }

        /// <summary>
        /// Debug info for a global variable.
        /// </summary>
        public sealed class GameGlobalInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GameGlobalInfo"/> class.
            /// </summary>
            internal GameGlobalInfo() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="GameGlobalInfo"/> class.
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <param name="begin">The begin.</param>
            /// <param name="shortname">The shortname.</param>
            /// <param name="typename">The typename.</param>
            public GameGlobalInfo(ulong id, ulong begin, string shortname, string typename)
            {
                Id        = id;
                Begin     = begin;
                ShortName = shortname;
                TypeName  = typename;
            }

            /// <summary>
            /// The identifier of the gobal variable. This is version independent.
            /// </summary>
            public ulong Id { get; private set; }

            /// <summary>
            /// The begin offset.
            /// </summary>
            public ulong Begin { get; private set; }

            /// <summary>
            /// The short name of function.
            /// </summary>
            public string ShortName { get; private set; }

            /// <summary>
            /// Gets or sets the value type name of the global variable.
            /// </summary>
            /// <value>
            /// The name of the type.
            /// </value>
            public string TypeName { get; private set; }

            /// <summary>
            /// Reads from stream.
            /// </summary>
            /// <param name="stream">The stream.</param>
            /// <param name="version">The version.</param>
            /// <exception cref="System.IO.InvalidDataException">Version of object is not supported!</exception>
            internal void ReadFromStream(System.IO.BinaryReader stream, int version)
            {
                var ltype = stream.ReadByte();
                if ((ltype & 1) != 0)
                    Begin = stream.ReadUInt64();
                else
                    Begin = stream.ReadUInt32();

                if ((ltype & 2) != 0)
                    ShortName = stream.ReadString();

                if ((ltype & 4) != 0)
                    TypeName = stream.ReadString();

                if ((ltype & 8) != 0)
                    Id = stream.ReadUInt64();
                else
                    Id = stream.ReadUInt32();
            }

            /// <summary>
            /// Writes to stream.
            /// </summary>
            /// <param name="stream">The stream.</param>
            internal void WriteToStream(System.IO.BinaryWriter stream)
            {
                byte ltype = 0;
                if (Begin > uint.MaxValue)
                    ltype |= 1;
                if (!string.IsNullOrEmpty(ShortName))
                    ltype |= 2;
                if (!string.IsNullOrEmpty(TypeName))
                    ltype |= 4;
                if (Id > uint.MaxValue)
                    ltype |= 8;

                stream.Write(ltype);

                if ((ltype & 1) != 0)
                    stream.Write(Begin);
                else
                    stream.Write((uint) Begin);

                if ((ltype & 2) != 0)
                    stream.Write(ShortName);

                if ((ltype & 4) != 0)
                    stream.Write(TypeName);

                if ((ltype & 8) != 0)
                    stream.Write(Id);
                else
                    stream.Write((uint) Id);
            }
        }

        /// <summary>
        /// Debug info for a function.
        /// </summary>
        public sealed class GameFunctionInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GameFunctionInfo"/> class.
            /// </summary>
            internal GameFunctionInfo() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="GameFunctionInfo" /> class.
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <param name="begin">The begin.</param>
            /// <param name="end">The end.</param>
            /// <param name="shortname">The shortname.</param>
            /// <param name="fullname">The fullname.</param>
            public GameFunctionInfo(ulong id, ulong begin, ulong end, string shortname, string fullname)
            {
                Id        = id;
                Begin     = begin;
                End       = end;
                ShortName = shortname;
                FullName  = fullname;
            }

            /// <summary>
            /// The identifier of the function. This is version independent.
            /// </summary>
            public ulong Id { get; private set; }

            /// <summary>
            /// The begin offset.
            /// </summary>
            public ulong Begin { get; private set; }

            /// <summary>
            /// The end offset.
            /// </summary>
            public ulong End { get; private set; }

            /// <summary>
            /// The short name of function.
            /// </summary>
            public string ShortName { get; private set; }

            /// <summary>
            /// The full name of function.
            /// </summary>
            public string FullName { get; private set; }

            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <param name="includeOffset">Include the offset of function?</param>
            /// <returns></returns>
            public string GetName(bool includeOffset)
            {
                var bld = new StringBuilder(32);
                bld.Append(!string.IsNullOrEmpty(ShortName) ? ShortName : "unk");
                if (includeOffset)
                {
                    bld.Append('_');
                    bld.Append(Begin.ToString("X"));
                }

                return bld.ToString();
            }

            /// <summary>
            /// Reads from stream.
            /// </summary>
            /// <param name="stream">The stream.</param>
            /// <param name="version">The version.</param>
            /// <exception cref="System.IO.InvalidDataException">Version of object is not supported!</exception>
            internal void ReadFromStream(System.IO.BinaryReader stream, int version)
            {
                var ltype = stream.ReadByte();

                if ((ltype & 1) != 0)
                    Begin = stream.ReadUInt64();
                else
                    Begin = stream.ReadUInt32();

                if ((ltype & 2) != 0)
                {
                    if ((ltype & 4) != 0)
                        End = stream.ReadUInt64();
                    else
                        End = stream.ReadUInt32();
                }
                else { End = Begin + stream.ReadUInt16(); }

                if ((ltype & 8) != 0)
                    ShortName = stream.ReadString();

                if ((ltype & 0x10) != 0)
                    FullName = stream.ReadString();

                if ((ltype & 0x20) != 0)
                    Id = stream.ReadUInt64();
                else
                    Id = stream.ReadUInt32();
            }

            /// <summary>
            /// Writes to stream.
            /// </summary>
            /// <param name="stream">The stream.</param>
            internal void WriteToStream(System.IO.BinaryWriter stream)
            {
                byte ltype = 0;
                if (Begin > uint.MaxValue)
                    ltype |= 1;
                if (End < Begin || End - Begin > ushort.MaxValue)
                {
                    ltype |= 2;
                    if (End > uint.MaxValue)
                        ltype |= 4;
                }

                if (!string.IsNullOrEmpty(ShortName))
                    ltype |= 8;
                if (!string.IsNullOrEmpty(FullName))
                    ltype |= 0x10;
                if (Id > uint.MaxValue)
                    ltype |= 0x20;

                stream.Write(ltype);

                if ((ltype & 1) != 0)
                    stream.Write(Begin);
                else
                    stream.Write((uint) Begin);

                if ((ltype & 2) != 0)
                {
                    if ((ltype & 4) != 0)
                        stream.Write(End);
                    else
                        stream.Write((uint) End);
                }
                else { stream.Write((ushort) (End - Begin)); }

                if ((ltype & 8) != 0)
                    stream.Write(ShortName);

                if ((ltype & 0x10) != 0)
                    stream.Write(FullName);

                if ((ltype & 0x20) != 0)
                    stream.Write(Id);
                else
                    stream.Write((uint) Id);
            }
        }

        /// <summary>
        /// The game type registration.
        /// </summary>
        internal sealed class GameTypeRegistration
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GameTypeRegistration"/> class.
            /// </summary>
            internal GameTypeRegistration() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="GameTypeRegistration"/> class.
            /// </summary>
            /// <param name="interfaceId">The interface identifier.</param>
            /// <param name="implementationId">The implementation identifier.</param>
            /// <param name="vtableOffset">The vtable offset.</param>
            /// <param name="offsetInType">Type of the offset in.</param>
            internal GameTypeRegistration(uint interfaceId, uint implementationId, int vtableOffset, int offsetInType)
            {
                InterfaceId      = interfaceId;
                ImplementationId = implementationId;
                VTableOffset     = vtableOffset;
                OffsetInType     = offsetInType;
            }

            /// <summary>
            /// Gets the interface identifier.
            /// </summary>
            /// <value>
            /// The interface identifier.
            /// </value>
            public uint InterfaceId { get; private set; }

            /// <summary>
            /// Gets the implementation identifier.
            /// </summary>
            /// <value>
            /// The implementation identifier.
            /// </value>
            public uint ImplementationId { get; private set; }

            /// <summary>
            /// Gets the virtual function table offset. This is negative if not available.
            /// </summary>
            /// <value>
            /// The v table offset.
            /// </value>
            public int VTableOffset { get; private set; }

            /// <summary>
            /// Gets the offset in complete type.
            /// </summary>
            /// <value>
            /// The type of the offset in.
            /// </value>
            public int OffsetInType { get; private set; }

            /// <summary>
            /// Reads from stream.
            /// </summary>
            /// <param name="stream">The stream.</param>
            /// <param name="version">The version.</param>
            /// <exception cref="System.IO.InvalidDataException">Version of object is not supported!</exception>
            internal void ReadFromStream(System.IO.BinaryReader stream, int version)
            {
                var ltype = stream.ReadByte();

                if ((ltype & 1) != 0)
                    InterfaceId = stream.ReadUInt16();
                else
                    InterfaceId = stream.ReadUInt32();

                if ((ltype & 2) != 0)
                    ImplementationId = stream.ReadUInt16();
                else
                    ImplementationId = stream.ReadUInt32();

                if ((ltype & 4) != 0)
                    VTableOffset = stream.ReadInt32();
                else
                    VTableOffset = -1;

                if ((ltype & 8) != 0)
                {
                    if ((ltype & 0x10) != 0)
                        OffsetInType = stream.ReadByte();
                    else if ((ltype & 0x20) != 0)
                        OffsetInType = stream.ReadUInt16();
                    else
                        OffsetInType = stream.ReadInt32();
                }
            }

            /// <summary>
            /// Writes to stream.
            /// </summary>
            /// <param name="stream">The stream.</param>
            internal void WriteToStream(System.IO.BinaryWriter stream)
            {
                byte ltype = 0;

                if (InterfaceId <= ushort.MaxValue)
                    ltype |= 1;
                if (ImplementationId <= ushort.MaxValue)
                    ltype |= 2;
                if (VTableOffset >= 0)
                    ltype |= 4;
                if (OffsetInType != 0)
                {
                    ltype |= 8;
                    if (OffsetInType >= 0)
                    {
                        if (OffsetInType <= byte.MaxValue)
                            ltype |= 0x10;
                        else if (OffsetInType <= ushort.MaxValue)
                            ltype |= 0x20;
                    }
                }

                stream.Write(ltype);

                if ((ltype & 1) != 0)
                    stream.Write((ushort) InterfaceId);
                else
                    stream.Write(InterfaceId);

                if ((ltype & 2) != 0)
                    stream.Write((ushort) ImplementationId);
                else
                    stream.Write(ImplementationId);

                if ((ltype & 4) != 0)
                    stream.Write(VTableOffset);

                if ((ltype & 8) != 0)
                {
                    if ((ltype & 0x10) != 0)
                        stream.Write((byte) OffsetInType);
                    else if ((ltype & 0x20) != 0)
                        stream.Write((ushort) OffsetInType);
                    else
                        stream.Write(OffsetInType);
                }
            }
        }
    }
}