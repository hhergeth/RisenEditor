using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RisenEditor.UI.PS_Modifier
{
    public class PS_Modifier : UserControl
    {
        protected Form1 P;

        public static Dictionary<string, string> wrappedSets = new Dictionary<string, string>()
        {
            {"gCNegZone_PS", "gCNavZone_PS"},
            {"gCNavPath_PS", "gCPrefPath_PS"},
        };
        internal static PS_Modifier fetchModifier(string SetName, Form1 F)
        {
            if (wrappedSets.ContainsKey(SetName))
                SetName = wrappedSets[SetName];
            SetName = SetName + "_Modifier";
            Type t = Type.GetType(typeof(PS_Modifier).Namespace + "." + SetName);
            if (t == null) return null;
            object o = Activator.CreateInstance(t);
            if (o != null)
                (o as PS_Modifier).Initialize(F);
            return o as PS_Modifier;
        }
        internal static bool canfetchModifier(string SetName)
        {
            if (wrappedSets.ContainsKey(SetName))
                SetName = wrappedSets[SetName];
            SetName = SetName + "_Modifier";
            Type t = Type.GetType(typeof(PS_Modifier).Namespace + "." + SetName);
            return t != null;
        }

        public PS_Modifier() { }

        public virtual string SetName
        {
            get
            {
                return this.GetType().Name.Replace("_Modifier", "");
            }
        }

        public virtual void Initialize(Form1 F) { P = F; }

        public virtual void Activate() { }

        public virtual void Closeing() { }
    }
}
