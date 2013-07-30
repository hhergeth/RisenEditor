using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace RisenEditor
{
    public class KeyManager
    {
        struct KeyState
        {
            public Keys m_eKeys;
            public bool m_bCtrl;
            public bool m_bShift;
            public bool m_bAlt;
        }

        const string g_pPath = "KeyBindings.txt";
        Dictionary<KeyState, string> m_pActions;

        public KeyManager()
        {
            m_pActions = new Dictionary<KeyState, string>();
            bool b = File.Exists(g_pPath);
            if (b)
            {
                string[] g = File.ReadAllLines(g_pPath);
                foreach (string c in g)
                {
                    string s = c.Replace(" ", "");
                    if (s == null || s.Length == 0 || s.StartsWith("//"))
                        continue;
                    string[] p = s.Replace(":", "").Replace("]", "").Split('[', '=');
                    KeyState k = new KeyState();
                    k.m_bAlt = k.m_bCtrl = k.m_bShift = false;
                    if (p.Length == 3)
                    {
                        string[] p2 = p[1].ToLower().Split(',');
                        if (p2.Length != 3)
                            throw new Exception();
                        p = new string[] { p[0], p[2]};//use offset
                        k.m_bCtrl = p2[0] == "d" ? true : false;
                        k.m_bShift = p2[1] == "d" ? true : false;
                        k.m_bAlt = p2[2] == "d" ? true : false;
                    }
                    else if (p.Length != 2)
                        throw new Exception();
                    p[0] = p[0].Replace("System.Windows.Forms.Keys.", "");
                    k.m_eKeys = (Keys)Enum.Parse(typeof(Keys), p[0]);
                    m_pActions.Add(k, p[1]);
                }
            }
            else
            {
                string[] g = new string[]
                {
                    "//Keys := Action, have a look at msdn (System.Windows.Forms.Keys) for possible keys",
                    "//Use [Control, Shift, Alt] to specify specific states, where 'D' indicates 'Pressed' and 'U' indictes 'Up'",
                    "//For example G [D, U, D] := RandomAction",
                    "//Use whitespace as needed, it will  be ignored, all actions start with a tag, UI specifies that the action performs UI actions",
                    "//SEL states that a selection is performed and MOD that lrent modifying happens. These tags are only for the sake of readability",
                    "",
                    "//Start of declerations",
                    "I := UI_Open_Inventory",
                    "A [D, U, U] := SEL_All",
                };
                File.WriteAllLines(g_pPath, g);
            }
        }

        public void Process(PreviewKeyDownEventArgs a_Args, Form1 a_Window)
        {
            KeyState k = new KeyState();
            k.m_bCtrl = a_Args.Control;
            k.m_bShift = a_Args.Shift;
            k.m_bAlt = a_Args.Alt;
            k.m_eKeys = a_Args.KeyCode;
            if (m_pActions.ContainsKey(k))
            {
                string s = m_pActions[k];
                System.Reflection.MethodInfo m = a_Window.GetType().GetMethod(s, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if(m != null)
                    m.Invoke(a_Window, null);
                GameLibrary.SystemLog.Append(GameLibrary.LogImportance.Information, "Action performed : " + s);
            }
        }
    }
}
