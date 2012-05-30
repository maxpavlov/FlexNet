using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

namespace SenseNet.ContentRepository.Storage.Security
{
    public enum PermittedLevel { None, HeadOnly, PublicOnly, All }

    public class PermissionBits
    {
        public int AllowBits { get; protected set; }
        public int DenyBits { get; protected set; }
        public PermissionValue[] PermissionValues { get { return GetPermissionValues(); } }

        public PermissionBits(int allowBits, int denyBits)
        {
            AllowBits = allowBits;
            DenyBits = denyBits;
        }
        public PermissionBits(PermissionValue[] values)
        {
            SetPermissionValues(values);
        }

        protected string AllowBitsString
        {
            get { return Convert.ToString(AllowBits, 2); }
        }
        protected string DenyBitsString
        {
            get { return Convert.ToString(DenyBits, 2); }
        }
        protected string ValuesString
        {
            get
            {
                var values = GetPermissionValues();
                var chars = new char[ActiveSchema.PermissionTypes.Count];
                for (int i = 0; i < values.Length; i++)
                {
                    switch (values[values.Length - i - 1])
                    {
                        case PermissionValue.NonDefined: chars[i] = '_'; break;
                        case PermissionValue.Allow: chars[i] = '+'; break;
                        case PermissionValue.Deny: chars[i] = '-'; break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                return new String(chars);
            }
        }

        private PermissionValue[] GetPermissionValues()
        {
            var result = new PermissionValue[ActiveSchema.PermissionTypes.Count];
            var allow = AllowBits;
            var deny = DenyBits;
            for (int i = 0; i < result.Length; i++)
            {
                if ((deny & 1) == 1)
                    result[i] = PermissionValue.Deny;
                else if ((allow & 1) == 1)
                    result[i] = PermissionValue.Allow;
                else
                    result[i] = PermissionValue.NonDefined;
                allow = allow >> 1;
                deny = deny >> 1;
            }
            return result;
        }
        private void SetPermissionValues(PermissionValue[] values)
        {
            int allow = 0;
            int deny = 0;
            //foreach (var value in values)
            //{
            //    allow = allow << 1;
            //    deny = deny << 1;
            //    if (value == PermissionValue.Allow)
            //        allow++;
            //    else if (value == PermissionValue.Deny)
            //        deny++;
            //}
            for (int i = values.Length - 1; i >= 0; i--)
            {
                allow = allow << 1;
                deny = deny << 1;
                if (values[i] == PermissionValue.Allow)
                    allow++;
                else if (values[i] == PermissionValue.Deny)
                    deny++;
            }
            AllowBits = allow;
            DenyBits = deny;
        }
    }

    [DebuggerDisplay("Principal:{PrincipalId}; Propagates:{Propagates}; Values:{ValuesString}")]
    public class PermissionSet : PermissionBits
    {
        public int PrincipalId { get; private set; }
        public bool Propagates { get; private set; }

        public PermissionSet(int principalId, bool propagates, int allowBits, int denyBits) : base(allowBits, denyBits)
        {
            PrincipalId = principalId;
            Propagates = propagates;
        }
        public PermissionSet(int principalId, bool propagates, PermissionValue[] values) : base(values)
        {
            PrincipalId = principalId;
            Propagates = propagates;
        }

        /// <summary>
        /// Format: [inheritbit] principalid permissionflags
        /// inheritbit: '+' (inherit) or '-' (not inherit).
        /// principalid: max 10 number chars (e.g. 0000000647)
        /// permissionflags: '_' (not defined), '+' (allow) or '-' (deny)
        /// The permissionflags will be aligned right.
        /// For example: "_-_+" == "_____________________________-_+" and it means: 
        /// OpenMinor deny, See allow, other permissions are not defined.
        /// </summary>
        /// <param name="src">Source string with the defined format.</param>
        /// <returns>Parsed instance of PermissionSet.</returns>
        public static PermissionSet Parse(string src)
        {
            var s = src.Trim();
            var pmax = s.Length;
            var p = 0;

            var isInheritable = !(s[p] == '-');
            if (s[p] == '+' || s[p] == '-')
                p++;

            var p0 = p;
            while (p < pmax && Char.IsDigit(s[p]))
                p++;

            var s1 = s.Substring(p0, p - 1);
            if (s1.Length == 0)
                s1 = "0";
            var principal = Int32.Parse(s1);

            int allow = 0;
            int deny = 0;
            while (p < pmax)
            {
                var c = s[p];
                allow = allow << 1;
                deny = deny << 1;
                if (c == '+')
                    allow++;
                else if (c == '-')
                    deny++;
                p++;
            }

            return new PermissionSet(principal, isInheritable, allow, deny);
        }

        internal SecurityEntry ToEntry(int nodeId)
        {
            return new SecurityEntry(nodeId, PrincipalId, Propagates, this.PermissionValues);
        }
    }

    //[DebuggerDisplay("DefinedOn={DefinedOnNodeId}, Principal={Principal}, Propagates={Propagates}, Values={ValuesString}")]
    [DebuggerDisplay("{ToString()}")]
    public sealed class SecurityEntry : PermissionSet
    {
        internal SecurityEntry(int definedOnNodeId, int principalId, bool isInheritable, PermissionValue[] permissionValues)
            : base(principalId, isInheritable, permissionValues)
        {
            DefinedOnNodeId = definedOnNodeId;
            //_principalId = principalId;
            //_isInheritable = isInheritable;
            //_permissionValues = permissionValues;
        }

        public int DefinedOnNodeId { get; private set; }

        public override string ToString()
        {
            return String.Format("DefinedOn={0}, Principal={1}, Propagates={2}, Values={3}",
                DefinedOnNodeId, PrincipalId, Propagates.ToString().ToLower(), ValuesString);
        }
        public string ValuesToString()
        {
            return ValuesString;
        }
        public void Export(XmlWriter writer)
        {
            //TODO: ? write propagates ?
            writer.WriteStartElement("Identity");
            writer.WriteAttributeString("path", NodeHead.Get(PrincipalId).Path);
            foreach (var permType in ActiveSchema.PermissionTypes)
            {
                var value = this.PermissionValues[permType.Id - 1];
                if (value == PermissionValue.NonDefined)
                    continue;
                writer.WriteElementString(permType.Name, value.ToString());
            }
            writer.WriteEndElement();
        }
        public void Export1(XmlWriter writer)
        {
            //TODO: ? write propagates ?
            writer.WriteStartElement("Identity");
            writer.WriteAttributeString("values", ValuesString);
            writer.WriteAttributeString("path", NodeHead.Get(PrincipalId).Path);
            writer.WriteEndElement();
        }

        internal void Combine(PermissionSet permissionSet)
        {
            AllowBits |= permissionSet.AllowBits;
            DenyBits |= permissionSet.DenyBits;
        }
    }

    [DebuggerDisplay("Id:{Id}; Path:{Path}; Inherits:{Inherits};")]
    internal class PermissionInfo
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public int Creator { get; set; }
        public int LastModifier { get; set; }
        public bool Inherits { get; set; }
        public PermissionInfo Parent { get; set; }
        public List<PermissionInfo> Children { get; set; }
        public List<PermissionSet> PermissionSets { get; set; }

        public PermissionInfo()
        {
            Children = new List<PermissionInfo>();
            PermissionSets = new List<PermissionSet>();
        }

        internal IEnumerable<SecurityEntry> GetAllEntries()
        {
            IEnumerable<SecurityEntry> aggregatedEntries = null;
            var info = this;
            while (info != null)
            {
                var entriesOnLevel = info.GetExplicitEntries();
                if (aggregatedEntries == null)
                    aggregatedEntries = entriesOnLevel;
                else
                    aggregatedEntries = aggregatedEntries.Union(entriesOnLevel);
                if (!info.Inherits)
                    break;
                info = info.Parent;
            }
            if (aggregatedEntries == null)
                return new SecurityEntry[0];
            return aggregatedEntries;
        }
        internal IEnumerable<SecurityEntry> GetExplicitEntries()
        {
            var x = (from set in PermissionSets select set.ToEntry(this.Id));
            return x;
        }
        internal SecurityEntry GetExplicitEntry(int identity)
        {
            var permSet = PermissionSets.Where(x => x.PrincipalId == identity).FirstOrDefault();
            if (permSet == null)
                return null;
            return permSet.ToEntry(this.Id);
        }
        internal IEnumerable<SecurityEntry> GetEffectiveEntries(bool withLevelOnly)
        {
            var entryIndex = new List<int>();
            var entries = new List<SecurityEntry>();
            var principals = GetEffectedPrincipals();
            foreach (var principal in principals)
            {
                int allow = 0;
                int deny = 0;
                for (var permInfo = this; permInfo != null; permInfo = permInfo.Inherits ? permInfo.Parent : null)
                    permInfo.AggregateEffectiveValues(new List<int>(new int[] { principal }), ref allow, ref deny);
                if (allow + deny > 0)
                {
                    entries.Add(new PermissionSet(principal, true, allow, deny).ToEntry(this.Id));
                    entryIndex.Add(principal);
                }
            }
            if (!withLevelOnly)
                return entries;
            foreach (var principal in principals)
            {
                int allow = 0;
                int deny = 0;
                AggregateLevelOnlyValues(new List<int>(new int[] { principal }), ref allow, ref deny);
                if (allow + deny > 0)
                {
                    var index = entryIndex.IndexOf(principal);
                    if (index < 0)
                        entries.Add(new PermissionSet(principal, false, allow, deny).ToEntry(this.Id));
                    else
                        entries[index].Combine(new PermissionSet(principal, false, allow, deny));
                }
            }
            return entries;
        }
        internal List<int> GetEffectedPrincipals()
        {
            var principals = new List<int>();
            var info = this;
            while (info != null)
            {
                foreach (var set in info.PermissionSets)
                    if (!principals.Contains(set.PrincipalId))
                        principals.Add(set.PrincipalId);
                if (!info.Inherits)
                    break;
                info = info.Parent;
            }
            return principals;
        }

        internal SnAccessControlList BuildAcl(SnAccessControlList acl)
        {
            //var principals = GetEffectedPrincipals();
            var aces = new Dictionary<int, SnAccessControlEntry>();
            for (var permInfo = this; permInfo != null; permInfo = permInfo.Inherits ? permInfo.Parent : null)
            {
                foreach (var permSet in permInfo.PermissionSets)
                {
                    // get ace by princ
                    var princ = permSet.PrincipalId;
                    SnAccessControlEntry ace;
                    if (!aces.TryGetValue(princ, out ace))
                    {
                        ace = SnAccessControlEntry.CreateEmpty(princ, permSet.Propagates);
                        aces.Add(princ, ace);
                    }

                    // get permissions and paths
                    int mask = 1;
                    for (int i = 0; i < ActiveSchema.PermissionTypes.Count; i++)
                    {
                        var permission = ace.Permissions.ElementAt(i);
                        if (!permission.Deny)
                        {
                            if ((permSet.DenyBits & mask) != 0)
                            {
                                permission.Deny = true;
                                permission.DenyFrom = SearchFirstPath(acl.Path, permInfo, permSet, mask, true);
                            }
                        }
                        if (!permission.Allow)
                        {
                            var allow = (permSet.AllowBits & mask) != 0;
                            if ((permSet.AllowBits & mask) != 0)
                            {
                                permission.Allow = true;
                                permission.AllowFrom = SearchFirstPath(acl.Path, permInfo, permSet, mask, false);
                            }
                        }
                        mask = mask << 1;
                    }

                }
            }

            acl.Inherits = acl.Path == this.Path ? this.Inherits : true;
            acl.Entries = aces.Values.ToArray();
            return acl;
        }
        private string SearchFirstPath(string aclPath, PermissionInfo basePermInfo, PermissionSet permSet, int mask, bool deny)
        {
            string lastPath = basePermInfo.Path;
            for (var permInfo = basePermInfo; permInfo != null; permInfo = permInfo.Inherits ? permInfo.Parent : null)
            {
                var entry = permInfo.GetExplicitEntry(permSet.PrincipalId);
                if (entry != null)
                {
                    var bit = mask & (deny ? entry.DenyBits : entry.AllowBits);
                    if (bit == 0)
                        break;
                    lastPath = permInfo.Path;
                }
            }
            return aclPath == lastPath ? null : lastPath;
        }

        public void AggregateEffectiveValues(List<int> principals, ref int allow, ref int deny)
        {
            foreach (var permSet in this.PermissionSets)
            {
                if (!permSet.Propagates)
                    continue;
                if (!principals.Contains(permSet.PrincipalId))
                    continue;
                allow |= permSet.AllowBits;
                deny |= permSet.DenyBits;
            }
        }
        public void AggregateLevelOnlyValues(List<int> principals, ref int allow, ref int deny)
        {
            foreach (var permSet in this.PermissionSets)
            {
                if (permSet.Propagates)
                    continue;
                if (!principals.Contains(permSet.PrincipalId))
                    continue;
                allow |= permSet.AllowBits;
                deny |= permSet.DenyBits;
            }
        }

        /// <summary>
        /// Format: [inheritedbit] path space* (| permSet)*
        /// inheritedbit: '+' (inherited) or '-' (breaked).
        /// path: lowercase string
        /// permSet: see PermissionSet.Parse
        /// Head info and PermissionSets are separated by '|'
        /// For example: "+/root/folder|+1345__+__|+0450__+__"
        /// </summary>
        /// <param name="src">Source string with the defined format.</param>
        /// <returns>Parsed instance of Entry.</returns>
        internal static PermissionInfo Parse(string src)
        {
            var sa = src.Trim().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var s = sa[0];
            var pmax = s.Length;
            var p = 0;

            var inherited = !(s[p] == '-');
            if (s[p] == '+' || s[p] == '-')
                p++;

            var id = 0;
            var path = s.Substring(p).Trim();
            p = 0;
            while (Char.IsDigit(path[p]))
                p++;
            if (p > 0)
            {
                id = Int32.Parse(path.Substring(0, p));
                path = path.Substring(p);
            }

            var permInfo = new PermissionInfo { Inherits = inherited, Path = path, Id = id };
            for (var i = 1; i < sa.Length; i++)
                permInfo.PermissionSets.Add(PermissionSet.Parse(sa[i]));

            return permInfo;
        }
    }

    //============================================================================== classes for editing


}