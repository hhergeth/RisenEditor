using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using GameLibrary.Objekte;
using SlimDX;
using GameLibrary.IO;
using GameLibrary;
using System.Threading;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;
using System.Xml.Linq;

namespace RisenEditor.Code
{
    public class InfoWrapper
    {
        public class c0
        {
            internal InfoWrapper A;
        }

        gCInfo nat;
        List<InfoCommandWrapper> m_pCommands;
        List<InfoCommandWrapper> m_pConditions;

        public InfoWrapper(gCInfo I)
        {
            nat = I;
            Build();
        }

        private void Build()
        {
            m_pCommands = new List<InfoCommandWrapper>();
            foreach (bCAccessorPropertyObject o in nat.getCommands())
            {
                InfoCommandWrapper w = InfoCommandWrapper.getWrapper(o, this);
                if (w != null)
                {
                    m_pCommands.Add(w);
                    w.Modified += commandModified;
                }
            }
            m_pConditions = new List<InfoCommandWrapper>();
            foreach (bCAccessorPropertyObject o in nat.getConditions())
            {
                InfoCommandWrapper w = InfoCommandWrapper.getWrapper(o, this);
                if (w != null)
                {
                    m_pConditions.Add(w);
                    w.Modified += commandModified;
                }
            }
        }

        void commandModified(InfoCommandWrapper obj)
        {
            if (Modified != null)
                Modified(this, obj);
        }

        public InfoWrapper(string a_Name, string a_Owner, gEInfoType a_Type, gEInfoCondType a_CndType)
        {
            bCAccessorPropertyObject Native = new bCAccessorPropertyObject(new gCInfo());
            nat = Native.Class as gCInfo;
            nat.Container.Properties.addProperty("Name", "bCString", new bCString(a_Name));
            nat.Container.Properties.addProperty("MainSortID", "long", 0);
            nat.Container.Properties.addProperty("SortID", "long", 0);
            nat.Container.Properties.addProperty("Owner", "bCString", new bCString(a_Owner));
            nat.Container.Properties.addProperty("InfoGiven", "bool", false);
            nat.Container.Properties.addProperty("Parent", "bCString", new bCString());
            nat.Container.Properties.addProperty("Quest", "bCString", new bCString());
            nat.Container.Properties.addProperty("ConditionType", "bTPropertyContainer<enum gEInfoCondType>", a_CndType);
            nat.Container.Properties.addProperty("Type", "bTPropertyContainer<enum gEInfoType>", a_Type);
            nat.Container.Properties.addProperty("GoldCost", "long", 0);
            nat.Container.Properties.addProperty("LearnPointCost", "long", 0);
            nat.Container.Properties.addProperty("ClearChildren", "bool", false);
            nat.Container.Properties.addProperty("Permanent", "bool", false);
            nat.Container.Properties.addProperty("Folder", "bCString", new bCString());
            nat.Container.Properties.addProperty("CurrentInfoCommandIndex", "int", -1);
            nat.Container.Properties.addProperty("InfoIsRunnig", "bool", false);
            nat.Container.Properties.addProperty("StartChapter", "long", -1);
            nat.Container.Properties.addProperty("EndChapter", "long", -1);
            m_pCommands = new List<InfoCommandWrapper>();
            m_pConditions = new List<InfoCommandWrapper>();
        }

        public static object FromString(bCProperty A, string val)
        {
            Func<string, string> F0 = (x) => string.IsNullOrEmpty(x) ? "0" : x;
            Func<string, string> F1 = (x) => string.IsNullOrEmpty(x) ? "false" : x;
            switch (A.PropertyType)
            {
                case "int":
                    return int.Parse(F0(val));
                case "bool":
                    return bool.Parse(F1(val));
                case "short":
                    return short.Parse(F0(val));
                case "float":
                    return float.Parse(F0(val));
                case "long":
                    return int.Parse(F0(val));
                case "char":
                    return string.IsNullOrEmpty(val) ? (char)0 : char.Parse(val);
                case "eCScriptProxyScript":
                    return new eCScriptProxyScript(val);
                case "eCEntityStringProxy":
                    return new eCEntityStringProxy(val);
            }
            if (A.PropertyType.EndsWith("String"))
            {
                bCString a = (bCString)Activator.CreateInstance(System.Type.GetType(typeof(bCString).FullName.Replace("bCString", A.PropertyType)));
                a.pString = val;
                return a;
            }
            else if (A.PropertyType.StartsWith("bTPropertyContainer"))
            {
                object a = bCProperty.getObject(A.PropertyType, new TempStream(6, null), true);
                return val.Length > 0 ? Enum.Parse(a.GetType(), val) : a;
            }
            else throw new Exception();
        }

        public static string ToString(bCProperty A, object val)
        {
            switch (A.PropertyType)
            {
                case "int":
                case "bool":
                case "short":
                case "float":
                case "long":
                case "char":
                    return val.ToString();
                case "eCScriptProxyScript":
                    return (val as eCScriptProxyScript).Object;
                case "eCEntityStringProxy":
                    return (val as eCEntityStringProxy).Object;
            }
            if (A.PropertyType.EndsWith("String"))
            {
                return val.ToString();
            }
            else if (A.PropertyType.StartsWith("bTPropertyContainer"))
            {
                return val.ToString();
            }
            else throw new Exception();
        }

        public static XElement ToXml(InfoWrapper i)
        {
            Action<bCAccessorPropertyObject, XElement> A = (x,n) =>
            {
                n.SetAttributeValue("ClassName", x.ClassName);
                foreach (bCProperty p in x.Properties)
                {
                    switch (p.PropertyType)
                    {
                        case "eCTemplateEntityProxy":
                            eCTemplateEntityProxy t = (eCTemplateEntityProxy)p.Object;
                            XElement n2 = new XElement(p.PropertyName);
                            n2.SetAttributeValue("Name", t.ObjectName);
                            n2.SetAttributeValue("ID", t.Guid.Value.ToString());
                            n2.SetAttributeValue("ClassName", "eCTemplateEntityProxy");
                            break;
                        case "gCQuestActor":
                            gCQuestActor q = p.Object as gCQuestActor;
                            n2 = new XElement(p.PropertyName);
                            n2.SetAttributeValue("Actor", q.Actor.Object);
                            n2.SetAttributeValue("ActorType", q.ActorType.ToString());
                            n2.SetAttributeValue("ClassName", "gCQuestActor");
                            break;
                        case "gCSkillValue":
                            gCSkillValue s = (gCSkillValue)p.Object;
                            n2 = new XElement(p.PropertyName);
                            n2.SetAttributeValue("Amount", s.Amount.ToString());
                            n2.SetAttributeValue("Skill", s.Skill.ToString());
                            n2.SetAttributeValue("ClassName", "gCSkillValue");
                            break;
                        default:
                            n.SetAttributeValue(p.PropertyName, ToString(p, p.Object));
                            break;
                    }
                }
            };
            Action<List<InfoCommandWrapper>, string, XElement> B = (a, b, p) =>
            {
                foreach (InfoCommandWrapper i2 in a)
                {
                    XElement c = new XElement(b);
                    A(i2.Object, c);
                    p.Add(c);
                }
            };
            XElement d = new XElement("Document");
            A(i.nat.Container as bCAccessorPropertyObject, d);
            B(i.m_pCommands, "Command", d);
            B(i.m_pConditions, "Condition", d);
            return d;
        }

        public static InfoWrapper FromXml(XElement d)
        {
            Action<bCAccessorPropertyObject, XElement, bool> A = (a, b, c) =>
            {
                foreach (XAttribute a2 in b.Attributes())
                    if (a2.Name.LocalName != "ClassName")
                        a.Properties[a2.Name.LocalName].Object = FromString(a.Properties[a2.Name.LocalName], a2.Value);
                if(c)
                    foreach (XElement e in b.Elements())
                    {
                        bCProperty p = a.Properties[e.Name.LocalName];
                        switch (p.PropertyType)
                        {
                            case "eCTemplateEntityProxy":
                                (p.Object as eCTemplateEntityProxy).Guid = new bCGuid(new Guid(e.Attribute("ID").Value));
                                break;
                            case "gCQuestActor":
                                (p.Object as gCQuestActor).ActorType = (gEQuestActor)Enum.Parse(typeof(gEQuestActor), e.Attribute("ActorType").Value);
                                (p.Object as gCQuestActor).Actor = new eCEntityStringProxy(e.Attribute("Actor").Value);
                                break;
                            case "gCSkillValue":
                                (p.Object as gCSkillValue).Amount = int.Parse(e.Attribute("Amount").Value);
                                (p.Object as gCSkillValue).Skill = (gESkill)Enum.Parse(typeof(gESkill), e.Attribute("Skill").Value);
                                break;
                            default:
                                throw new Exception();
                        }
                    }
            };
            Action<string, XElement, InfoWrapper> B = (a, b, w) => 
            {
                foreach (XElement e in b.Elements())
                    if (e.Name == a)
                    {
                        string c = e.Attribute("ClassName").Value;
                        InfoCommandWrapper v = InfoCommandWrapper.getWrapper(c + "_Wrapper", w);
                        A(v.Object, e, true);
                        typeof(InfoWrapper).GetMethod("Add" + a).Invoke(w, new object[]{v});
                    }
            };
            InfoWrapper i = new InfoWrapper("", "", gEInfoType.gEInfoType_Comment, gEInfoCondType.gEInfoCondType_Activator);
            if (d.Attribute("ClassName").Value != "gCInfo")
                throw new Exception();
            A(i.nat.Container as bCAccessorPropertyObject, d, false);
            B("Command", d, i);
            B("Condition", d, i);
            i.Build();
            return i;
        }

        public event Action<InfoWrapper, InfoCommandWrapper> Modified;

        [Browsable(false)]
        public c0 abs
        {
            get
            {
                 return new c0() { A = this };
            }
        }

        public void AddCommand(InfoCommandWrapper a)
        {
            m_pCommands.Add(a);
            nat.AddCommand(a.Object);
            a.Modified += commandModified;
        }

        public void InsertCommand(InfoCommandWrapper a, int i)
        {
            m_pCommands.Insert(i, a);
            nat.InsertCommand(a.Object, i);
            a.Modified += commandModified;
        }

        public void RemoveCommand(InfoCommandWrapper a)
        {
            int i = m_pCommands.IndexOf(a);
            m_pCommands.RemoveAt(i);
            nat.RemoveCommand(i);
            a.Modified -= commandModified;
        }

        public void AddCondition(InfoCommandWrapper a)
        {
            m_pConditions.Add(a);
            nat.AddCondition(a.Object);
            a.Modified += commandModified;
        }

        public void InsertCondition(InfoCommandWrapper a, int i)
        {
            m_pConditions.Insert(i, a);
            nat.InsertCondition(a.Object, i);
            a.Modified += commandModified;
        }

        public void RemoveCondition(InfoCommandWrapper a)
        {
            int i = m_pConditions.IndexOf(a);
            m_pConditions.RemoveAt(i);
            nat.RemoveCondition(i);
            a.Modified -= commandModified;
        }

        [Browsable(false)]
        public InfoCommandWrapper[] Commands
        {
            get
            {
                return m_pCommands.ToArray();
            }
        }

        [Browsable(false)]
        public InfoCommandWrapper[] Conditions
        {
            get
            {
                return m_pConditions.ToArray();
            }
        }

        public bCString Name
        {
            get
            {
                return (bCString)nat.Container.Properties["Name"].Object;
            }
            set
            {
                nat.Container.Properties["Name"].Object = value;
            }
        }

        public int MainSortID
        {
            get
            {
                return (int)nat.Container.Properties["MainSortID"].Object;
            }
            set
            {
                nat.Container.Properties["MainSortID"].Object = value;
            }
        }

        public int SortID
        {
            get
            {
                return (int)nat.Container.Properties["SortID"].Object;
            }
            set
            {
                nat.Container.Properties["SortID"].Object = value;
            }
        }

        public bCString Owner
        {
            get
            {
                return (bCString)nat.Container.Properties["Owner"].Object;
            }
            set
            {
                nat.Container.Properties["Owner"].Object = value;
            }
        }

        public bool InfoGiven
        {
            get
            {
                return (bool)nat.Container.Properties["InfoGiven"].Object;
            }
            set
            {
                nat.Container.Properties["InfoGiven"].Object = value;
            }
        }

        public bCString Parent
        {
            get
            {
                return (bCString)nat.Container.Properties["Parent"].Object;
            }
            set
            {
                nat.Container.Properties["Parent"].Object = value;
            }
        }

        public bCString Quest
        {
            get
            {
                return (bCString)nat.Container.Properties["Quest"].Object;
            }
            set
            {
                nat.Container.Properties["Quest"].Object = value;
            }
        }

        public gEInfoCondType ConditionType
        {
            get
            {
                return (gEInfoCondType)nat.Container.Properties["ConditionType"].Object;
            }
            set
            {
                nat.Container.Properties["ConditionType"].Object = value;
            }
        }

        public gEInfoType Type
        {
            get
            {
                return (gEInfoType)nat.Container.Properties["Type"].Object;
            }
            set
            {
                nat.Container.Properties["Type"].Object = value;
            }
        }

        public int GoldCost
        {
            get
            {
                return (int)nat.Container.Properties["GoldCost"].Object;
            }
            set
            {
                nat.Container.Properties["GoldCost"].Object = value;
            }
        }

        public int LearnPointCost
        {
            get
            {
                return (int)nat.Container.Properties["LearnPointCost"].Object;
            }
            set
            {
                nat.Container.Properties["LearnPointCost"].Object = value;
            }
        }

        public bool ClearChildren
        {
            get
            {
                return (bool)nat.Container.Properties["ClearChildren"].Object;
            }
            set
            {
                nat.Container.Properties["ClearChildren"].Object = value;
            }
        }

        public bool Permanent
        {
            get
            {
                return (bool)nat.Container.Properties["Permanent"].Object;
            }
            set
            {
                nat.Container.Properties["Permanent"].Object = value;
            }
        }

        public bCString Folder
        {
            get
            {
                return (bCString)nat.Container.Properties["Folder"].Object;
            }
            set
            {
                nat.Container.Properties["Folder"].Object = value;
            }
        }

        public int CurrentInfoCommandIndex
        {
            get
            {
                return (int)nat.Container.Properties["CurrentInfoCommandIndex"].Object;
            }
            set
            {
                nat.Container.Properties["CurrentInfoCommandIndex"].Object = value;
            }
        }

        public bool InfoIsRunnig
        {
            get
            {
                return (bool)nat.Container.Properties["InfoIsRunnig"].Object;
            }
            set
            {
                nat.Container.Properties["InfoIsRunnig"].Object = value;
            }
        }

        public int StartChapter
        {
            get
            {
                return (int)nat.Container.Properties["StartChapter"].Object;
            }
            set
            {
                nat.Container.Properties["StartChapter"].Object = value;
            }
        }

        public int EndChapter
        {
            get
            {
                return (int)nat.Container.Properties["EndChapter"].Object;
            }
            set
            {
                nat.Container.Properties["EndChapter"].Object = value;
            }
        }

        public override string ToString()
        {
            return Name.pString;
        }
    }

    public abstract class InfoCommandWrapper
    {
        public class c0
        {
            internal InfoCommandWrapper A;
        }

        protected bCAccessorPropertyObject Native;
        [Browsable(false)]
        public InfoWrapper Parent { get; private set; }

        public InfoCommandWrapper(bCAccessorPropertyObject A, InfoWrapper a_Parent)
        {
            Parent = a_Parent;
            Native = A;
        }

        protected InfoCommandWrapper(InfoWrapper a_Parent)
        {
            Parent = a_Parent;
        }

        protected void OnChange(string a_Property)
        {
            if (Modified != null)
                Modified(this);
        }

        public event Action<InfoCommandWrapper> Modified;

        [Browsable(false)]
        public string CommandName
        {
            get
            {
                return Native.ClassName;
            }
        }

        [Browsable(false)]
        public bCAccessorPropertyObject Object
        {
            get
            {
                return Native;
            }
        }

        [Browsable(false)]
        public c0 abs
        {
            get
            {
                return new c0() { A = this };
            }
        }

        static List<string> done = new List<string>();
        public static string BuildClassFromAccessor(string NAME, string BaseClass, bCAccessorPropertyObject a)
        {
            StringBuilder SB = new StringBuilder();
            SB.Append("public class " + NAME + "_Wrapper : " + BaseClass + Environment.NewLine + "{" + Environment.NewLine);
            SB.Append("public " + NAME + "_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }");
            SB.Append(Environment.NewLine + "public " + NAME + "_Wrapper(InfoWrapper P) : base(P)" + Environment.NewLine + "{" + Environment.NewLine);
            SB.AppendLine("Native = new bCAccessorPropertyObject(new " + NAME + "(), " + (char)34 + NAME + (char)34 + ");");
            foreach (bCProperty p in a.Properties)
            {
                string X = (char)34 + p.PropertyName + (char)34;
                Type T0 = p.Object.GetType();
                string Y = T0.Name;
                if (T0.IsGenericType)
                    Y = TypeCache.GetFriendlyTypeName(T0);
                SB.AppendLine("Native.Properties.addProperty(" + (char)34 + p.PropertyName + (char)34 + ", " + (char)34 + p.PropertyType + (char)34 + ", new " + Y + "());");
            }
            SB.Append("}");
            foreach (bCProperty p in a.Properties)
            {
                string X = (char)34 + p.PropertyName + (char)34;
                Type T0 = p.Object.GetType();
                string Y = T0.Name;
                if (T0.IsGenericType)
                    Y = TypeCache.GetFriendlyTypeName(T0);
                SB.Append(Environment.NewLine + Environment.NewLine);
                SB.Append("public " + Y + " " + p.PropertyName + Environment.NewLine + "{" + Environment.NewLine + "get" + Environment.NewLine + "{");
                SB.Append("return (" + Y + ")Native.Properties[" + X + "].Object;");
                SB.Append(Environment.NewLine + "}" + Environment.NewLine + "set" + Environment.NewLine + "{");
                SB.Append("Native.Properties[" + X + "].Object = value;");
                SB.Append(Environment.NewLine + "OnChange(" + (char)34 + p.PropertyName + (char)34 + ");");
                SB.Append(Environment.NewLine + "}" + Environment.NewLine + "}");
            }
            SB.Append(Environment.NewLine + "}");
            return SB.ToString();
        }
        public static InfoCommandWrapper getWrapper(bCAccessorPropertyObject a, InfoWrapper P)
        {
            string NAME = a.ClassName;
            Type T = Assembly.GetCallingAssembly().GetType(typeof(PropertySetWrapper).Namespace + "." + NAME + "_Wrapper");
            if (T == null)
            {
                if (!done.Contains(NAME))
                {
                    done.Add(NAME);
                    const string n = "gInfoWrapper.cs";
                    string b = BuildClassFromAccessor(a.ClassName, "InfoCommandWrapper", a);
                    string c = File.Exists(n) ? File.ReadAllText(n) : "";
                    File.WriteAllText(n, c + b);

                    string n2 = "gInfo.cs", nl = Environment.NewLine;
                    string b2 = "public class " + NAME + " : classData" + nl + "{" + nl + "short Version;" + nl + nl + "public override void deSerialize(IFile a_File)" + nl +
                        "{" + nl + "Version = a_File.Read<short>();" + nl + "}" + nl + nl + "public override void Serialize(IFile a_File)" + nl + "{" + nl + "a_File.Write<short>(Version);"
                        + nl + "}" + nl + nl + "public override int Size" + nl + "{" + nl + "get" + nl + "{" + nl + "return 2;" + nl + "}" + nl + "}" + nl + "}";
                    string c2 = File.Exists(n2) ? File.ReadAllText(n2) : "";
                    File.WriteAllText(n2, c2 + b2);
                }
                return null;
            }
            else
            {
                object o2 = Activator.CreateInstance(T, a, P);
                return o2 as InfoCommandWrapper;
            }
        }
        public static InfoCommandWrapper getWrapper(Type T, InfoWrapper P)
        {
            InfoCommandWrapper v = Activator.CreateInstance(T, P) as InfoCommandWrapper;
            return v;
        }
        public static InfoCommandWrapper getWrapper(string N, InfoWrapper P)
        {
            return getWrapper(Type.GetType("RisenEditor.Code." + N), P);
        }

        public override string ToString()
        {
            return CommandName;
        }
    }

#region Commands
    public class gCInfoCommandSay_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandSay_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandSay_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandSay(), "gCInfoCommandSay");
            Native.Properties.addProperty("Speaker", "bCString", new bCString());
            Native.Properties.addProperty("Listener", "bCString", new bCString());
            Native.Properties.addProperty("Text", "gCInfoLocString", new gCInfoLocString());
            Native.Properties.addProperty("Gesture", "bTPropertyContainer<enum gEInfoGesture>", new gEInfoGesture());
        }

        public bCString Speaker
        {
            get
            {
                return (bCString)Native.Properties["Speaker"].Object;
            }
            set
            {
                Native.Properties["Speaker"].Object = value;
                OnChange("Speaker");
            }
        }

        public bCString Listener
        {
            get
            {
                return (bCString)Native.Properties["Listener"].Object;
            }
            set
            {
                Native.Properties["Listener"].Object = value;
                OnChange("Listener");
            }
        }

        public gCInfoLocString Text
        {
            get
            {
                return (gCInfoLocString)Native.Properties["Text"].Object;
            }
            set
            {
                Native.Properties["Text"].Object = value;
                OnChange("Text");
            }
        }

        public gEInfoGesture Gesture
        {
            get
            {
                return (gEInfoGesture)Native.Properties["Gesture"].Object;
            }
            set
            {
                Native.Properties["Gesture"].Object = value;
                OnChange("Gesture");
            }
        }
    }
    public class gCInfoCommandSetGameEvent_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandSetGameEvent_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandSetGameEvent_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandSetGameEvent(), "gCInfoCommandSetGameEvent");
            Native.Properties.addProperty("GameEvent", "bCString", new bCString());
        }

        public bCString GameEvent
        {
            get
            {
                return (bCString)Native.Properties["GameEvent"].Object;
            }
            set
            {
                Native.Properties["GameEvent"].Object = value;
                OnChange("GameEvent");
            }
        }
    }
    public class gCInfoCommandRunScript_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandRunScript_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandRunScript_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandRunScript(), "gCInfoCommandRunScript");
            Native.Properties.addProperty("Script", "eCScriptProxyScript", new eCScriptProxyScript());
            Native.Properties.addProperty("Self", "bCString", new bCString());
            Native.Properties.addProperty("Other", "bCString", new bCString());
            Native.Properties.addProperty("OtherType", "bTPropertyContainer<enum gEOtherType>", new gEOtherType());
            Native.Properties.addProperty("Param", "int", new Int32());
        }

        public eCScriptProxyScript Script
        {
            get
            {
                return (eCScriptProxyScript)Native.Properties["Script"].Object;
            }
            set
            {
                Native.Properties["Script"].Object = value;
                OnChange("Script");
            }
        }

        public bCString Self
        {
            get
            {
                return (bCString)Native.Properties["Self"].Object;
            }
            set
            {
                Native.Properties["Self"].Object = value;
                OnChange("Self");
            }
        }

        public bCString Other
        {
            get
            {
                return (bCString)Native.Properties["Other"].Object;
            }
            set
            {
                Native.Properties["Other"].Object = value;
                OnChange("Other");
            }
        }

        public gEOtherType OtherType
        {
            get
            {
                return (gEOtherType)Native.Properties["OtherType"].Object;
            }
            set
            {
                Native.Properties["OtherType"].Object = value;
                OnChange("OtherType");
            }
        }

        public Int32 Param
        {
            get
            {
                return (Int32)Native.Properties["Param"].Object;
            }
            set
            {
                Native.Properties["Param"].Object = value;
                OnChange("Param");
            }
        }
    }
    public class gCInfoCommandEnd_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandEnd_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandEnd_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandEnd(), "gCInfoCommandEnd");
        }
    }
    public class gCInfoCommandRunQuest_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandRunQuest_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandRunQuest_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandRunQuest(), "gCInfoCommandRunQuest");
            Native.Properties.addProperty("QuestName", "bCString", new bCString());
        }

        public bCString QuestName
        {
            get
            {
                return (bCString)Native.Properties["QuestName"].Object;
            }
            set
            {
                Native.Properties["QuestName"].Object = value;
                OnChange("QuestName");
            }
        }
    }
    public class gCInfoCommandAddLogText_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandAddLogText_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandAddLogText_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandAddLogText(), "gCInfoCommandAddLogText");
            Native.Properties.addProperty("Text", "gCInfoLocString", new gCInfoLocString());
            Native.Properties.addProperty("QuestName", "bCString", new bCString());
        }

        public gCInfoLocString Text
        {
            get
            {
                return (gCInfoLocString)Native.Properties["Text"].Object;
            }
            set
            {
                Native.Properties["Text"].Object = value;
                OnChange("Text");
            }
        }

        public bCString QuestName
        {
            get
            {
                return (bCString)Native.Properties["QuestName"].Object;
            }
            set
            {
                Native.Properties["QuestName"].Object = value;
                OnChange("QuestName");
            }
        }
    }
    public class gCInfoCommandAddInfoSystemEndScript_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandAddInfoSystemEndScript_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandAddInfoSystemEndScript_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandAddInfoSystemEndScript(), "gCInfoCommandAddInfoSystemEndScript");
            Native.Properties.addProperty("Script", "eCScriptProxyScript", new eCScriptProxyScript());
            Native.Properties.addProperty("Self", "bCString", new bCString());
            Native.Properties.addProperty("Other", "bCString", new bCString());
            Native.Properties.addProperty("Param", "int", new Int32());
        }

        public eCScriptProxyScript Script
        {
            get
            {
                return (eCScriptProxyScript)Native.Properties["Script"].Object;
            }
            set
            {
                Native.Properties["Script"].Object = value;
                OnChange("Script");
            }
        }

        public bCString Self
        {
            get
            {
                return (bCString)Native.Properties["Self"].Object;
            }
            set
            {
                Native.Properties["Self"].Object = value;
                OnChange("Self");
            }
        }

        public bCString Other
        {
            get
            {
                return (bCString)Native.Properties["Other"].Object;
            }
            set
            {
                Native.Properties["Other"].Object = value;
                OnChange("Other");
            }
        }

        public Int32 Param
        {
            get
            {
                return (Int32)Native.Properties["Param"].Object;
            }
            set
            {
                Native.Properties["Param"].Object = value;
                OnChange("Param");
            }
        }
    }
    public class gCInfoConditionPlayerKnows_Wrapper : InfoCommandWrapper
    {
        public gCInfoConditionPlayerKnows_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoConditionPlayerKnows_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoConditionPlayerKnows(), "gCInfoConditionPlayerKnows");
            Native.Properties.addProperty("PlayerKnows", "bCString", new bCString());
        }

        public bCString PlayerKnows
        {
            get
            {
                return (bCString)Native.Properties["PlayerKnows"].Object;
            }
            set
            {
                Native.Properties["PlayerKnows"].Object = value;
                OnChange("PlayerKnows");
            }
        }
    }
    public class gCInfoCommandDescription_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandDescription_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandDescription_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandDescription(), "gCInfoCommandDescription");
            Native.Properties.addProperty("Text", "gCInfoLocString", new gCInfoLocString());
        }

        public gCInfoLocString Text
        {
            get
            {
                return (gCInfoLocString)Native.Properties["Text"].Object;
            }
            set
            {
                Native.Properties["Text"].Object = value;
                OnChange("Text");
            }
        }
    }
    public class gCInfoCommandPickPocket_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandPickPocket_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandPickPocket_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandPickPocket(), "gCInfoCommandPickPocket");
            Native.Properties.addProperty("Difficulty", "long", new Int32());
            Native.Properties.addProperty("Speaker", "bCString", new bCString());
            Native.Properties.addProperty("Listener", "bCString", new bCString());
            Native.Properties.addProperty("Text", "gCInfoLocString", new gCInfoLocString());
            Native.Properties.addProperty("SVMID", "bCString", new bCString());
            Native.Properties.addProperty("SVMText", "gCInfoLocString", new gCInfoLocString());
            Native.Properties.addProperty("Gesture", "bTPropertyContainer<enum gEInfoGesture>", new gEInfoGesture());
        }

        public Int32 Difficulty
        {
            get
            {
                return (Int32)Native.Properties["Difficulty"].Object;
            }
            set
            {
                Native.Properties["Difficulty"].Object = value;
                OnChange("Difficulty");
            }
        }

        public bCString Speaker
        {
            get
            {
                return (bCString)Native.Properties["Speaker"].Object;
            }
            set
            {
                Native.Properties["Speaker"].Object = value;
                OnChange("Speaker");
            }
        }

        public bCString Listener
        {
            get
            {
                return (bCString)Native.Properties["Listener"].Object;
            }
            set
            {
                Native.Properties["Listener"].Object = value;
                OnChange("Listener");
            }
        }

        public gCInfoLocString Text
        {
            get
            {
                return (gCInfoLocString)Native.Properties["Text"].Object;
            }
            set
            {
                Native.Properties["Text"].Object = value;
                OnChange("Text");
            }
        }

        public bCString SVMID
        {
            get
            {
                return (bCString)Native.Properties["SVMID"].Object;
            }
            set
            {
                Native.Properties["SVMID"].Object = value;
                OnChange("SVMID");
            }
        }

        public gCInfoLocString SVMText
        {
            get
            {
                return (gCInfoLocString)Native.Properties["SVMText"].Object;
            }
            set
            {
                Native.Properties["SVMText"].Object = value;
                OnChange("SVMText");
            }
        }

        public gEInfoGesture Gesture
        {
            get
            {
                return (gEInfoGesture)Native.Properties["Gesture"].Object;
            }
            set
            {
                Native.Properties["Gesture"].Object = value;
                OnChange("Gesture");
            }
        }
    }
    public class gCInfoCommandTeleportNPC_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandTeleportNPC_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandTeleportNPC_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandTeleportNPC(), "gCInfoCommandTeleportNPC");
            Native.Properties.addProperty("NPC", "bCString", new bCString());
            Native.Properties.addProperty("Target", "bCString", new bCString());
        }

        public bCString NPC
        {
            get
            {
                return (bCString)Native.Properties["NPC"].Object;
            }
            set
            {
                Native.Properties["NPC"].Object = value;
                OnChange("NPC");
            }
        }

        public bCString Target
        {
            get
            {
                return (bCString)Native.Properties["Target"].Object;
            }
            set
            {
                Native.Properties["Target"].Object = value;
                OnChange("Target");
            }
        }
    }
    public class gCInfoCommandSetRoutine_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandSetRoutine_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandSetRoutine_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandSetRoutine(), "gCInfoCommandSetRoutine");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("RoutineName", "bCString", new bCString());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public bCString RoutineName
        {
            get
            {
                return (bCString)Native.Properties["RoutineName"].Object;
            }
            set
            {
                Native.Properties["RoutineName"].Object = value;
                OnChange("RoutineName");
            }
        }
    }
    public class gCInfoConditionNPCStatus_Wrapper : InfoCommandWrapper
    {
        public gCInfoConditionNPCStatus_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoConditionNPCStatus_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoConditionNPCStatus(), "gCInfoConditionNPCStatus");
            Native.Properties.addProperty("SecondaryNPC", "eCEntityStringProxy", new eCEntityStringProxy());
            Native.Properties.addProperty("SecondaryNPCStatus", "bTPropertyContainer<enum gEInfoNPCStatus>", new gEInfoNPCStatus());
        }

        public eCEntityStringProxy SecondaryNPC
        {
            get
            {
                return (eCEntityStringProxy)Native.Properties["SecondaryNPC"].Object;
            }
            set
            {
                Native.Properties["SecondaryNPC"].Object = value;
                OnChange("SecondaryNPC");
            }
        }

        public gEInfoNPCStatus SecondaryNPCStatus
        {
            get
            {
                return (gEInfoNPCStatus)Native.Properties["SecondaryNPCStatus"].Object;
            }
            set
            {
                Native.Properties["SecondaryNPCStatus"].Object = value;
                OnChange("SecondaryNPCStatus");
            }
        }
    }
    public class gCInfoConditionPlayerNotKnows_Wrapper : InfoCommandWrapper
    {
        public gCInfoConditionPlayerNotKnows_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoConditionPlayerNotKnows_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoConditionPlayerNotKnows(), "gCInfoConditionPlayerNotKnows");
            Native.Properties.addProperty("PlayerNotKnows", "bCString", new bCString());
        }

        public bCString PlayerNotKnows
        {
            get
            {
                return (bCString)Native.Properties["PlayerNotKnows"].Object;
            }
            set
            {
                Native.Properties["PlayerNotKnows"].Object = value;
                OnChange("PlayerNotKnows");
            }
        }
    }
    public class gCInfoCommandGiveXP_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandGiveXP_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandGiveXP_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandGiveXP(), "gCInfoCommandGiveXP");
            Native.Properties.addProperty("Amount", "long", new Int32());
        }

        public Int32 Amount
        {
            get
            {
                return (Int32)Native.Properties["Amount"].Object;
            }
            set
            {
                Native.Properties["Amount"].Object = value;
                OnChange("Amount");
            }
        }
    }
    public class gCInfoCommandCreateItem_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandCreateItem_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandCreateItem_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandCreateItem(), "gCInfoCommandCreateItem");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("Item", "eCTemplateEntityProxy", new eCTemplateEntityProxy());
            Native.Properties.addProperty("Amount", "long", new Int32());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public eCTemplateEntityProxy Item
        {
            get
            {
                return (eCTemplateEntityProxy)Native.Properties["Item"].Object;
            }
            set
            {
                Native.Properties["Item"].Object = value;
                OnChange("Item");
            }
        }

        public Int32 Amount
        {
            get
            {
                return (Int32)Native.Properties["Amount"].Object;
            }
            set
            {
                Native.Properties["Amount"].Object = value;
                OnChange("Amount");
            }
        }
    }
    public class gCInfoCommandAddNPCInfo_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandAddNPCInfo_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandAddNPCInfo_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandAddNPCInfo(), "gCInfoCommandAddNPCInfo");
            Native.Properties.addProperty("NPC", "bCString", new bCString());
            Native.Properties.addProperty("Location", "bTPropertyContainer<enum gEInfoLocation>", new gEInfoLocation());
            Native.Properties.addProperty("Type", "bTPropertyContainer<enum gEInfoNPCType>", new gEInfoNPCType());
            Native.Properties.addProperty("Description", "gCNPCInfoLocString", new gCNPCInfoLocString());
        }

        public bCString NPC
        {
            get
            {
                return (bCString)Native.Properties["NPC"].Object;
            }
            set
            {
                Native.Properties["NPC"].Object = value;
                OnChange("NPC");
            }
        }

        public gEInfoLocation Location
        {
            get
            {
                return (gEInfoLocation)Native.Properties["Location"].Object;
            }
            set
            {
                Native.Properties["Location"].Object = value;
                OnChange("Location");
            }
        }

        public gEInfoNPCType Type
        {
            get
            {
                return (gEInfoNPCType)Native.Properties["Type"].Object;
            }
            set
            {
                Native.Properties["Type"].Object = value;
                OnChange("Type");
            }
        }

        public gCNPCInfoLocString Description
        {
            get
            {
                return (gCNPCInfoLocString)Native.Properties["Description"].Object;
            }
            set
            {
                Native.Properties["Description"].Object = value;
                OnChange("Description");
            }
        }
    }
    public class gCInfoCommandSaySVM_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandSaySVM_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandSaySVM_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandSaySVM(), "gCInfoCommandSaySVM");
            Native.Properties.addProperty("Speaker", "bCString", new bCString());
            Native.Properties.addProperty("Listener", "bCString", new bCString());
            Native.Properties.addProperty("SVMID", "bCString", new bCString());
            Native.Properties.addProperty("Text", "gCInfoLocString", new gCInfoLocString());
            Native.Properties.addProperty("Gesture", "bTPropertyContainer<enum gEInfoGesture>", new gEInfoGesture());
        }

        public bCString Speaker
        {
            get
            {
                return (bCString)Native.Properties["Speaker"].Object;
            }
            set
            {
                Native.Properties["Speaker"].Object = value;
                OnChange("Speaker");
            }
        }

        public bCString Listener
        {
            get
            {
                return (bCString)Native.Properties["Listener"].Object;
            }
            set
            {
                Native.Properties["Listener"].Object = value;
                OnChange("Listener");
            }
        }

        public bCString SVMID
        {
            get
            {
                return (bCString)Native.Properties["SVMID"].Object;
            }
            set
            {
                Native.Properties["SVMID"].Object = value;
                OnChange("SVMID");
            }
        }

        public gCInfoLocString Text
        {
            get
            {
                return (gCInfoLocString)Native.Properties["Text"].Object;
            }
            set
            {
                Native.Properties["Text"].Object = value;
                OnChange("Text");
            }
        }

        public gEInfoGesture Gesture
        {
            get
            {
                return (gEInfoGesture)Native.Properties["Gesture"].Object;
            }
            set
            {
                Native.Properties["Gesture"].Object = value;
                OnChange("Gesture");
            }
        }
    }
    public class gCInfoCommandTeach_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandTeach_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandTeach_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandTeach(), "gCInfoCommandTeach");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("TeachSkill", "gCSkillValue", new gCSkillValue());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public gCSkillValue TeachSkill
        {
            get
            {
                return (gCSkillValue)Native.Properties["TeachSkill"].Object;
            }
            set
            {
                Native.Properties["TeachSkill"].Object = value;
                OnChange("TeachSkill");
            }
        }
    }
    public class gCInfoConditionTeachEnabled_Wrapper : InfoCommandWrapper
    {
        public gCInfoConditionTeachEnabled_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoConditionTeachEnabled_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoConditionTeachEnabled(), "gCInfoConditionTeachEnabled");
            Native.Properties.addProperty("TeachEnabled", "bool", new Boolean());
        }

        public Boolean TeachEnabled
        {
            get
            {
                return (Boolean)Native.Properties["TeachEnabled"].Object;
            }
            set
            {
                Native.Properties["TeachEnabled"].Object = value;
                OnChange("TeachEnabled");
            }
        }
    }
    public class gCInfoConditionCanTeachSkill_Wrapper : InfoCommandWrapper
    {
        public gCInfoConditionCanTeachSkill_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoConditionCanTeachSkill_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoConditionCanTeachSkill(), "gCInfoConditionCanTeachSkill");
            Native.Properties.addProperty("TeachSkill", "gCSkillValue", new gCSkillValue());
        }

        public gCSkillValue TeachSkill
        {
            get
            {
                return (gCSkillValue)Native.Properties["TeachSkill"].Object;
            }
            set
            {
                Native.Properties["TeachSkill"].Object = value;
                OnChange("TeachSkill");
            }
        }
    }
    public class gCInfoConditionSkillValue_Wrapper : InfoCommandWrapper
    {
        public gCInfoConditionSkillValue_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoConditionSkillValue_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoConditionSkillValue(), "gCInfoConditionSkillValue");
            Native.Properties.addProperty("Entity", "bCString", new bCString());
            Native.Properties.addProperty("CompareOperation", "bTPropertyContainer<enum gECompareOperation>", new gECompareOperation());
            Native.Properties.addProperty("SkillValue", "gCSkillValue", new gCSkillValue());
        }

        public bCString Entity
        {
            get
            {
                return (bCString)Native.Properties["Entity"].Object;
            }
            set
            {
                Native.Properties["Entity"].Object = value;
                OnChange("Entity");
            }
        }

        public gECompareOperation CompareOperation
        {
            get
            {
                return (gECompareOperation)Native.Properties["CompareOperation"].Object;
            }
            set
            {
                Native.Properties["CompareOperation"].Object = value;
                OnChange("CompareOperation");
            }
        }

        public gCSkillValue SkillValue
        {
            get
            {
                return (gCSkillValue)Native.Properties["SkillValue"].Object;
            }
            set
            {
                Native.Properties["SkillValue"].Object = value;
                OnChange("SkillValue");
            }
        }
    }
    public class gCInfoConditionQuestStatus_Wrapper : InfoCommandWrapper
    {
        public gCInfoConditionQuestStatus_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoConditionQuestStatus_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoConditionQuestStatus(), "gCInfoConditionQuestStatus");
            Native.Properties.addProperty("Quest", "bCString", new bCString());
            Native.Properties.addProperty("CondType", "bTPropertyContainer<enum gEInfoCondType>", new gEInfoCondType());
            Native.Properties.addProperty("IsCurrentStatus", "bool", new Boolean());
        }

        public bCString Quest
        {
            get
            {
                return (bCString)Native.Properties["Quest"].Object;
            }
            set
            {
                Native.Properties["Quest"].Object = value;
                OnChange("Quest");
            }
        }

        public gEInfoCondType CondType
        {
            get
            {
                return (gEInfoCondType)Native.Properties["CondType"].Object;
            }
            set
            {
                Native.Properties["CondType"].Object = value;
                OnChange("CondType");
            }
        }

        public Boolean IsCurrentStatus
        {
            get
            {
                return (Boolean)Native.Properties["IsCurrentStatus"].Object;
            }
            set
            {
                Native.Properties["IsCurrentStatus"].Object = value;
                OnChange("IsCurrentStatus");
            }
        }
    }
    public class gCInfoCommandGive_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandGive_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandGive_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandGive(), "gCInfoCommandGive");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("Entity2", "bCString", new bCString());
            Native.Properties.addProperty("Item", "eCTemplateEntityProxy", new eCTemplateEntityProxy());
            Native.Properties.addProperty("Amount", "long", new Int32());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public bCString Entity2
        {
            get
            {
                return (bCString)Native.Properties["Entity2"].Object;
            }
            set
            {
                Native.Properties["Entity2"].Object = value;
                OnChange("Entity2");
            }
        }

        public eCTemplateEntityProxy Item
        {
            get
            {
                return (eCTemplateEntityProxy)Native.Properties["Item"].Object;
            }
            set
            {
                Native.Properties["Item"].Object = value;
                OnChange("Item");
            }
        }

        public Int32 Amount
        {
            get
            {
                return (Int32)Native.Properties["Amount"].Object;
            }
            set
            {
                Native.Properties["Amount"].Object = value;
                OnChange("Amount");
            }
        }
    }
    public class gCInfoCommandRunAIScript_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandRunAIScript_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandRunAIScript_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandRunAIScript(), "gCInfoCommandRunAIScript");
            Native.Properties.addProperty("AIScriptName", "bCString", new bCString());
            Native.Properties.addProperty("Self", "bCString", new bCString());
            Native.Properties.addProperty("Other", "bCString", new bCString());
            Native.Properties.addProperty("Param", "int", new Int32());
        }

        public bCString AIScriptName
        {
            get
            {
                return (bCString)Native.Properties["AIScriptName"].Object;
            }
            set
            {
                Native.Properties["AIScriptName"].Object = value;
                OnChange("AIScriptName");
            }
        }

        public bCString Self
        {
            get
            {
                return (bCString)Native.Properties["Self"].Object;
            }
            set
            {
                Native.Properties["Self"].Object = value;
                OnChange("Self");
            }
        }

        public bCString Other
        {
            get
            {
                return (bCString)Native.Properties["Other"].Object;
            }
            set
            {
                Native.Properties["Other"].Object = value;
                OnChange("Other");
            }
        }

        public Int32 Param
        {
            get
            {
                return (Int32)Native.Properties["Param"].Object;
            }
            set
            {
                Native.Properties["Param"].Object = value;
                OnChange("Param");
            }
        }
    }
    public class gCInfoCommandSetGuardStatus_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandSetGuardStatus_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandSetGuardStatus_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandSetGuardStatus(), "gCInfoCommandSetGuardStatus");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("GuardStatus", "bTPropertyContainer<enum gEGuardStatus>", new gEGuardStatus());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public gEGuardStatus GuardStatus
        {
            get
            {
                return (gEGuardStatus)Native.Properties["GuardStatus"].Object;
            }
            set
            {
                Native.Properties["GuardStatus"].Object = value;
                OnChange("GuardStatus");
            }
        }
    }
    public class gCInfoCommandSucceedQuest_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandSucceedQuest_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandSucceedQuest_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandSucceedQuest(), "gCInfoCommandSucceedQuest");
            Native.Properties.addProperty("QuestName", "bCString", new bCString());
        }

        public bCString QuestName
        {
            get
            {
                return (bCString)Native.Properties["QuestName"].Object;
            }
            set
            {
                Native.Properties["QuestName"].Object = value;
                OnChange("QuestName");
            }
        }
    }
    public class gCInfoCommandSayConfirm_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandSayConfirm_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandSayConfirm_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandSayConfirm(), "gCInfoCommandSayConfirm");
            Native.Properties.addProperty("Speaker", "bCString", new bCString());
            Native.Properties.addProperty("Listener", "bCString", new bCString());
        }

        public bCString Speaker
        {
            get
            {
                return (bCString)Native.Properties["Speaker"].Object;
            }
            set
            {
                Native.Properties["Speaker"].Object = value;
                OnChange("Speaker");
            }
        }

        public bCString Listener
        {
            get
            {
                return (bCString)Native.Properties["Listener"].Object;
            }
            set
            {
                Native.Properties["Listener"].Object = value;
                OnChange("Listener");
            }
        }
    }
    public class gCInfoCommandCloseQuest_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandCloseQuest_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandCloseQuest_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandCloseQuest(), "gCInfoCommandCloseQuest");
            Native.Properties.addProperty("QuestName", "bCString", new bCString());
        }

        public bCString QuestName
        {
            get
            {
                return (bCString)Native.Properties["QuestName"].Object;
            }
            set
            {
                Native.Properties["QuestName"].Object = value;
                OnChange("QuestName");
            }
        }
    }
    public class gCInfoCommandSetTradeEnabled_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandSetTradeEnabled_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandSetTradeEnabled_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandSetTradeEnabled(), "gCInfoCommandSetTradeEnabled");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("TradeEnabled", "bool", new Boolean());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public Boolean TradeEnabled
        {
            get
            {
                return (Boolean)Native.Properties["TradeEnabled"].Object;
            }
            set
            {
                Native.Properties["TradeEnabled"].Object = value;
                OnChange("TradeEnabled");
            }
        }
    }
    public class gCInfoCommandSetPartyEnabled_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandSetPartyEnabled_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandSetPartyEnabled_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandSetPartyEnabled(), "gCInfoCommandSetPartyEnabled");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("PartyEnabled", "bool", new Boolean());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public Boolean PartyEnabled
        {
            get
            {
                return (Boolean)Native.Properties["PartyEnabled"].Object;
            }
            set
            {
                Native.Properties["PartyEnabled"].Object = value;
                OnChange("PartyEnabled");
            }
        }
    }
    public class gCInfoCommandAddQuestActor_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandAddQuestActor_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandAddQuestActor_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandAddQuestActor(), "gCInfoCommandAddQuestActor");
            Native.Properties.addProperty("QuestName", "bCString", new bCString());
            Native.Properties.addProperty("Actor", "gCQuestActor", new gCQuestActor());
        }

        public bCString QuestName
        {
            get
            {
                return (bCString)Native.Properties["QuestName"].Object;
            }
            set
            {
                Native.Properties["QuestName"].Object = value;
                OnChange("QuestName");
            }
        }

        public gCQuestActor Actor
        {
            get
            {
                return (gCQuestActor)Native.Properties["Actor"].Object;
            }
            set
            {
                Native.Properties["Actor"].Object = value;
                OnChange("Actor");
            }
        }
    }
    public class gCInfoConditionItemAmount_Wrapper : InfoCommandWrapper
    {
        public gCInfoConditionItemAmount_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoConditionItemAmount_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoConditionItemAmount(), "gCInfoConditionItemAmount");
            Native.Properties.addProperty("Owner", "eCEntityStringProxy", new eCEntityStringProxy());
            Native.Properties.addProperty("Item", "eCTemplateEntityProxy", new eCTemplateEntityProxy());
            Native.Properties.addProperty("ItemAmount", "long", new Int32());
        }

        public eCEntityStringProxy Owner
        {
            get
            {
                return (eCEntityStringProxy)Native.Properties["Owner"].Object;
            }
            set
            {
                Native.Properties["Owner"].Object = value;
                OnChange("Owner");
            }
        }

        public eCTemplateEntityProxy Item
        {
            get
            {
                return (eCTemplateEntityProxy)Native.Properties["Item"].Object;
            }
            set
            {
                Native.Properties["Item"].Object = value;
                OnChange("Item");
            }
        }

        public Int32 ItemAmount
        {
            get
            {
                return (Int32)Native.Properties["ItemAmount"].Object;
            }
            set
            {
                Native.Properties["ItemAmount"].Object = value;
                OnChange("ItemAmount");
            }
        }
    }
    public class gCInfoCommandSleep_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandSleep_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandSleep_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandSleep(), "gCInfoCommandSleep");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("TargetHour", "long", new Int32());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public Int32 TargetHour
        {
            get
            {
                return (Int32)Native.Properties["TargetHour"].Object;
            }
            set
            {
                Native.Properties["TargetHour"].Object = value;
                OnChange("TargetHour");
            }
        }
    }
    public class gCInfoCommandSetTeachEnabled_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandSetTeachEnabled_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandSetTeachEnabled_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandSetTeachEnabled(), "gCInfoCommandSetTeachEnabled");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("TeachEnabled", "bool", new Boolean());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public Boolean TeachEnabled
        {
            get
            {
                return (Boolean)Native.Properties["TeachEnabled"].Object;
            }
            set
            {
                Native.Properties["TeachEnabled"].Object = value;
                OnChange("TeachEnabled");
            }
        }
    }
    public class gCInfoCommandInfoWait_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandInfoWait_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandInfoWait_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandInfoWait(), "gCInfoCommandInfoWait");
            Native.Properties.addProperty("Time", "long", new Int32());
        }

        public Int32 Time
        {
            get
            {
                return (Int32)Native.Properties["Time"].Object;
            }
            set
            {
                Native.Properties["Time"].Object = value;
                OnChange("Time");
            }
        }
    }
    public class gCInfoConditionScript_Wrapper : InfoCommandWrapper
    {
        public gCInfoConditionScript_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoConditionScript_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoConditionScript(), "gCInfoConditionScript");
            Native.Properties.addProperty("ScriptName", "bCString", new bCString());
            Native.Properties.addProperty("Self", "bCString", new bCString());
            Native.Properties.addProperty("Other", "bCString", new bCString());
            Native.Properties.addProperty("Param", "int", new Int32());
        }

        public bCString ScriptName
        {
            get
            {
                return (bCString)Native.Properties["ScriptName"].Object;
            }
            set
            {
                Native.Properties["ScriptName"].Object = value;
                OnChange("ScriptName");
            }
        }

        public bCString Self
        {
            get
            {
                return (bCString)Native.Properties["Self"].Object;
            }
            set
            {
                Native.Properties["Self"].Object = value;
                OnChange("Self");
            }
        }

        public bCString Other
        {
            get
            {
                return (bCString)Native.Properties["Other"].Object;
            }
            set
            {
                Native.Properties["Other"].Object = value;
                OnChange("Other");
            }
        }

        public Int32 Param
        {
            get
            {
                return (Int32)Native.Properties["Param"].Object;
            }
            set
            {
                Native.Properties["Param"].Object = value;
                OnChange("Param");
            }
        }
    }
    public class gCInfoCommandRemoveNPCInfo_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandRemoveNPCInfo_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandRemoveNPCInfo_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandRemoveNPCInfo(), "gCInfoCommandRemoveNPCInfo");
            Native.Properties.addProperty("NPC", "bCString", new bCString());
            Native.Properties.addProperty("Location", "bTPropertyContainer<enum gEInfoLocation>", new gEInfoLocation());
            Native.Properties.addProperty("Type", "bTPropertyContainer<enum gEInfoNPCType>", new gEInfoNPCType());
        }

        public bCString NPC
        {
            get
            {
                return (bCString)Native.Properties["NPC"].Object;
            }
            set
            {
                Native.Properties["NPC"].Object = value;
                OnChange("NPC");
            }
        }

        public gEInfoLocation Location
        {
            get
            {
                return (gEInfoLocation)Native.Properties["Location"].Object;
            }
            set
            {
                Native.Properties["Location"].Object = value;
                OnChange("Location");
            }
        }

        public gEInfoNPCType Type
        {
            get
            {
                return (gEInfoNPCType)Native.Properties["Type"].Object;
            }
            set
            {
                Native.Properties["Type"].Object = value;
                OnChange("Type");
            }
        }
    }
    public class gCInfoCommandRunInfo_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandRunInfo_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandRunInfo_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandRunInfo(), "gCInfoCommandRunInfo");
            Native.Properties.addProperty("Info", "bCString", new bCString());
        }

        public bCString Info
        {
            get
            {
                return (bCString)Native.Properties["Info"].Object;
            }
            set
            {
                Native.Properties["Info"].Object = value;
                OnChange("Info");
            }
        }
    }
    public class gCInfoCommandClearGameEvent_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandClearGameEvent_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandClearGameEvent_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandClearGameEvent(), "gCInfoCommandClearGameEvent");
            Native.Properties.addProperty("GameEvent", "bCString", new bCString());
        }

        public bCString GameEvent
        {
            get
            {
                return (bCString)Native.Properties["GameEvent"].Object;
            }
            set
            {
                Native.Properties["GameEvent"].Object = value;
                OnChange("GameEvent");
            }
        }
    }
    public class gCInfoCommandAttack_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandAttack_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandAttack_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandAttack(), "gCInfoCommandAttack");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("Entity2", "bCString", new bCString());
            Native.Properties.addProperty("Reason", "bCString", new bCString());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public bCString Entity2
        {
            get
            {
                return (bCString)Native.Properties["Entity2"].Object;
            }
            set
            {
                Native.Properties["Entity2"].Object = value;
                OnChange("Entity2");
            }
        }

        public bCString Reason
        {
            get
            {
                return (bCString)Native.Properties["Reason"].Object;
            }
            set
            {
                Native.Properties["Reason"].Object = value;
                OnChange("Reason");
            }
        }
    }
    public class gCInfoCommandStartFixCamera_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandStartFixCamera_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandStartFixCamera_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandStartFixCamera(), "gCInfoCommandStartFixCamera");
            Native.Properties.addProperty("CameraTarget", "bCString", new bCString());
            Native.Properties.addProperty("CameraPosition", "bCString", new bCString());
            Native.Properties.addProperty("TargetOffset", "bCVector", new Vector3());
            Native.Properties.addProperty("BestDistance", "float", new Single());
            Native.Properties.addProperty("MinDistance", "float", new Single());
            Native.Properties.addProperty("MaxDistance", "float", new Single());
            Native.Properties.addProperty("CameraYawAngle", "float", new Single());
            Native.Properties.addProperty("CameraPitchAngle", "float", new Single());
            Native.Properties.addProperty("FOV", "float", new Single());
            Native.Properties.addProperty("FOVChangeSpeed", "float", new Single());
            Native.Properties.addProperty("MoveSpeedMax", "float", new Single());
            Native.Properties.addProperty("RotationSpeedMax", "float", new Single());
            Native.Properties.addProperty("MoveAcceleration", "float", new Single());
            Native.Properties.addProperty("MoveDecceleration", "float", new Single());
            Native.Properties.addProperty("RotationAcceleration", "float", new Single());
            Native.Properties.addProperty("RotationDecceleration", "float", new Single());
            Native.Properties.addProperty("ForceCameraReset", "bool", new Boolean());
        }

        public bCString CameraTarget
        {
            get
            {
                return (bCString)Native.Properties["CameraTarget"].Object;
            }
            set
            {
                Native.Properties["CameraTarget"].Object = value;
                OnChange("CameraTarget");
            }
        }

        public bCString CameraPosition
        {
            get
            {
                return (bCString)Native.Properties["CameraPosition"].Object;
            }
            set
            {
                Native.Properties["CameraPosition"].Object = value;
                OnChange("CameraPosition");
            }
        }

        public Vector3 TargetOffset
        {
            get
            {
                return (Vector3)Native.Properties["TargetOffset"].Object;
            }
            set
            {
                Native.Properties["TargetOffset"].Object = value;
                OnChange("TargetOffset");
            }
        }

        public Single BestDistance
        {
            get
            {
                return (Single)Native.Properties["BestDistance"].Object;
            }
            set
            {
                Native.Properties["BestDistance"].Object = value;
                OnChange("BestDistance");
            }
        }

        public Single MinDistance
        {
            get
            {
                return (Single)Native.Properties["MinDistance"].Object;
            }
            set
            {
                Native.Properties["MinDistance"].Object = value;
                OnChange("MinDistance");
            }
        }

        public Single MaxDistance
        {
            get
            {
                return (Single)Native.Properties["MaxDistance"].Object;
            }
            set
            {
                Native.Properties["MaxDistance"].Object = value;
                OnChange("MaxDistance");
            }
        }

        public Single CameraYawAngle
        {
            get
            {
                return (Single)Native.Properties["CameraYawAngle"].Object;
            }
            set
            {
                Native.Properties["CameraYawAngle"].Object = value;
                OnChange("CameraYawAngle");
            }
        }

        public Single CameraPitchAngle
        {
            get
            {
                return (Single)Native.Properties["CameraPitchAngle"].Object;
            }
            set
            {
                Native.Properties["CameraPitchAngle"].Object = value;
                OnChange("CameraPitchAngle");
            }
        }

        public Single FOV
        {
            get
            {
                return (Single)Native.Properties["FOV"].Object;
            }
            set
            {
                Native.Properties["FOV"].Object = value;
                OnChange("FOV");
            }
        }

        public Single FOVChangeSpeed
        {
            get
            {
                return (Single)Native.Properties["FOVChangeSpeed"].Object;
            }
            set
            {
                Native.Properties["FOVChangeSpeed"].Object = value;
                OnChange("FOVChangeSpeed");
            }
        }

        public Single MoveSpeedMax
        {
            get
            {
                return (Single)Native.Properties["MoveSpeedMax"].Object;
            }
            set
            {
                Native.Properties["MoveSpeedMax"].Object = value;
                OnChange("MoveSpeedMax");
            }
        }

        public Single RotationSpeedMax
        {
            get
            {
                return (Single)Native.Properties["RotationSpeedMax"].Object;
            }
            set
            {
                Native.Properties["RotationSpeedMax"].Object = value;
                OnChange("RotationSpeedMax");
            }
        }

        public Single MoveAcceleration
        {
            get
            {
                return (Single)Native.Properties["MoveAcceleration"].Object;
            }
            set
            {
                Native.Properties["MoveAcceleration"].Object = value;
                OnChange("MoveAcceleration");
            }
        }

        public Single MoveDecceleration
        {
            get
            {
                return (Single)Native.Properties["MoveDecceleration"].Object;
            }
            set
            {
                Native.Properties["MoveDecceleration"].Object = value;
                OnChange("MoveDecceleration");
            }
        }

        public Single RotationAcceleration
        {
            get
            {
                return (Single)Native.Properties["RotationAcceleration"].Object;
            }
            set
            {
                Native.Properties["RotationAcceleration"].Object = value;
                OnChange("RotationAcceleration");
            }
        }

        public Single RotationDecceleration
        {
            get
            {
                return (Single)Native.Properties["RotationDecceleration"].Object;
            }
            set
            {
                Native.Properties["RotationDecceleration"].Object = value;
                OnChange("RotationDecceleration");
            }
        }

        public Boolean ForceCameraReset
        {
            get
            {
                return (Boolean)Native.Properties["ForceCameraReset"].Object;
            }
            set
            {
                Native.Properties["ForceCameraReset"].Object = value;
                OnChange("ForceCameraReset");
            }
        }
    }
    public class gCInfoCommandRemoveQuestActor_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandRemoveQuestActor_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandRemoveQuestActor_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandRemoveQuestActor(), "gCInfoCommandRemoveQuestActor");
            Native.Properties.addProperty("QuestName", "bCString", new bCString());
            Native.Properties.addProperty("Actor", "gCQuestActor", new gCQuestActor());
        }

        public bCString QuestName
        {
            get
            {
                return (bCString)Native.Properties["QuestName"].Object;
            }
            set
            {
                Native.Properties["QuestName"].Object = value;
                OnChange("QuestName");
            }
        }

        public gCQuestActor Actor
        {
            get
            {
                return (gCQuestActor)Native.Properties["Actor"].Object;
            }
            set
            {
                Native.Properties["Actor"].Object = value;
                OnChange("Actor");
            }
        }
    }
    public class gCInfoConditionOwnerInArea_Wrapper : InfoCommandWrapper
    {
        public gCInfoConditionOwnerInArea_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoConditionOwnerInArea_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoConditionOwnerInArea(), "gCInfoConditionOwnerInArea");
            Native.Properties.addProperty("AreaPropertyName", "bCString", new bCString());
        }

        public bCString AreaPropertyName
        {
            get
            {
                return (bCString)Native.Properties["AreaPropertyName"].Object;
            }
            set
            {
                Native.Properties["AreaPropertyName"].Object = value;
                OnChange("AreaPropertyName");
            }
        }
    }
    public class gCInfoCommandWear_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandWear_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandWear_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandWear(), "gCInfoCommandWear");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("Item", "eCTemplateEntityProxy", new eCTemplateEntityProxy());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public eCTemplateEntityProxy Item
        {
            get
            {
                return (eCTemplateEntityProxy)Native.Properties["Item"].Object;
            }
            set
            {
                Native.Properties["Item"].Object = value;
                OnChange("Item");
            }
        }
    }
    public class gCInfoConditionOwnerNearEntity_Wrapper : InfoCommandWrapper
    {
        public gCInfoConditionOwnerNearEntity_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoConditionOwnerNearEntity_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoConditionOwnerNearEntity(), "gCInfoConditionOwnerNearEntity");
            Native.Properties.addProperty("Entity", "eCEntityStringProxy", new eCEntityStringProxy());
        }

        public eCEntityStringProxy Entity
        {
            get
            {
                return (eCEntityStringProxy)Native.Properties["Entity"].Object;
            }
            set
            {
                Native.Properties["Entity"].Object = value;
                OnChange("Entity");
            }
        }
    }
    public class gCInfoCommandKillNPC_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandKillNPC_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandKillNPC_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandKillNPC(), "gCInfoCommandKillNPC");
            Native.Properties.addProperty("NPC", "bCString", new bCString());
        }

        public bCString NPC
        {
            get
            {
                return (bCString)Native.Properties["NPC"].Object;
            }
            set
            {
                Native.Properties["NPC"].Object = value;
                OnChange("NPC");
            }
        }
    }
    public class gCInfoCommandDestroyItem_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandDestroyItem_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandDestroyItem_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandDestroyItem(), "gCInfoCommandDestroyItem");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("Item", "eCTemplateEntityProxy", new eCTemplateEntityProxy());
            Native.Properties.addProperty("Amount", "long", new Int32());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public eCTemplateEntityProxy Item
        {
            get
            {
                return (eCTemplateEntityProxy)Native.Properties["Item"].Object;
            }
            set
            {
                Native.Properties["Item"].Object = value;
                OnChange("Item");
            }
        }

        public Int32 Amount
        {
            get
            {
                return (Int32)Native.Properties["Amount"].Object;
            }
            set
            {
                Native.Properties["Amount"].Object = value;
                OnChange("Amount");
            }
        }
    }
    public class gCInfoCommandSetSectorStatus_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandSetSectorStatus_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandSetSectorStatus_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandSetSectorStatus(), "gCInfoCommandSetSectorStatus");
            Native.Properties.addProperty("SectorName", "bCString", new bCString());
            Native.Properties.addProperty("SectorEnabled", "bool", new Boolean());
        }

        public bCString SectorName
        {
            get
            {
                return (bCString)Native.Properties["SectorName"].Object;
            }
            set
            {
                Native.Properties["SectorName"].Object = value;
                OnChange("SectorName");
            }
        }

        public Boolean SectorEnabled
        {
            get
            {
                return (Boolean)Native.Properties["SectorEnabled"].Object;
            }
            set
            {
                Native.Properties["SectorEnabled"].Object = value;
                OnChange("SectorEnabled");
            }
        }
    }
    public class gCInfoCommandSetMobEnabled_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandSetMobEnabled_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandSetMobEnabled_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandSetMobEnabled(), "gCInfoCommandSetMobEnabled");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("MobEnabled", "bool", new Boolean());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public Boolean MobEnabled
        {
            get
            {
                return (Boolean)Native.Properties["MobEnabled"].Object;
            }
            set
            {
                Native.Properties["MobEnabled"].Object = value;
                OnChange("MobEnabled");
            }
        }
    }
    public class gCInfoCommandBack_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandBack_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandBack_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandBack(), "gCInfoCommandBack");
        }
    }
    public class gCInfoCommandShowPicture_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandShowPicture_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandShowPicture_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandShowPicture(), "gCInfoCommandShowPicture");
            Native.Properties.addProperty("FileName", "bCString", new bCString());
        }

        public bCString FileName
        {
            get
            {
                return (bCString)Native.Properties["FileName"].Object;
            }
            set
            {
                Native.Properties["FileName"].Object = value;
                OnChange("FileName");
            }
        }
    }
    public class gCInfoCommandStartEffect_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandStartEffect_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandStartEffect_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandStartEffect(), "gCInfoCommandStartEffect");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("EffectName", "bCString", new bCString());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public bCString EffectName
        {
            get
            {
                return (bCString)Native.Properties["EffectName"].Object;
            }
            set
            {
                Native.Properties["EffectName"].Object = value;
                OnChange("EffectName");
            }
        }
    }
    public class gCInfoCommandShowSubtitle_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandShowSubtitle_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandShowSubtitle_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandShowSubtitle(), "gCInfoCommandShowSubtitle");
            Native.Properties.addProperty("Text", "gCInfoLocString", new gCInfoLocString());
        }

        public gCInfoLocString Text
        {
            get
            {
                return (gCInfoLocString)Native.Properties["Text"].Object;
            }
            set
            {
                Native.Properties["Text"].Object = value;
                OnChange("Text");
            }
        }
    }
    public class gCInfoCommandSetSlaveryEnabled_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandSetSlaveryEnabled_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandSetSlaveryEnabled_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandSetSlaveryEnabled(), "gCInfoCommandSetSlaveryEnabled");
            Native.Properties.addProperty("Entity1", "bCString", new bCString());
            Native.Properties.addProperty("SlaveryEnabled", "bool", new Boolean());
        }

        public bCString Entity1
        {
            get
            {
                return (bCString)Native.Properties["Entity1"].Object;
            }
            set
            {
                Native.Properties["Entity1"].Object = value;
                OnChange("Entity1");
            }
        }

        public Boolean SlaveryEnabled
        {
            get
            {
                return (Boolean)Native.Properties["SlaveryEnabled"].Object;
            }
            set
            {
                Native.Properties["SlaveryEnabled"].Object = value;
                OnChange("SlaveryEnabled");
            }
        }
    }
    public class gCInfoCommandThink_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandThink_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandThink_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandThink(), "gCInfoCommandThink");
            Native.Properties.addProperty("Speaker", "bCString", new bCString());
            Native.Properties.addProperty("Text", "gCInfoLocString", new gCInfoLocString());
            Native.Properties.addProperty("Gesture", "bTPropertyContainer<enum gEInfoGesture>", new gEInfoGesture());
        }

        public bCString Speaker
        {
            get
            {
                return (bCString)Native.Properties["Speaker"].Object;
            }
            set
            {
                Native.Properties["Speaker"].Object = value;
                OnChange("Speaker");
            }
        }

        public gCInfoLocString Text
        {
            get
            {
                return (gCInfoLocString)Native.Properties["Text"].Object;
            }
            set
            {
                Native.Properties["Text"].Object = value;
                OnChange("Text");
            }
        }

        public gEInfoGesture Gesture
        {
            get
            {
                return (gEInfoGesture)Native.Properties["Gesture"].Object;
            }
            set
            {
                Native.Properties["Gesture"].Object = value;
                OnChange("Gesture");
            }
        }
    }
    public class gCInfoCommandRemoveNPC_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandRemoveNPC_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandRemoveNPC_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandRemoveNPC(), "gCInfoCommandRemoveNPC");
            Native.Properties.addProperty("NPC", "bCString", new bCString());
        }

        public bCString NPC
        {
            get
            {
                return (bCString)Native.Properties["NPC"].Object;
            }
            set
            {
                Native.Properties["NPC"].Object = value;
                OnChange("NPC");
            }
        }
    }
    public class gCInfoCommandStopFixCamera_Wrapper : InfoCommandWrapper
    {
        public gCInfoCommandStopFixCamera_Wrapper(bCAccessorPropertyObject O, InfoWrapper P) : base(O, P) { }
        public gCInfoCommandStopFixCamera_Wrapper(InfoWrapper P)
            : base(P)
        {
            Native = new bCAccessorPropertyObject(new gCInfoCommandStopFixCamera(), "gCInfoCommandStopFixCamera");
        }
    }
#endregion
}
