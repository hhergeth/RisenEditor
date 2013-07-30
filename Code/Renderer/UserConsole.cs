using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GameLibrary;
using GameLibrary.Rendering;
using SlimDX;
using System.Windows.Forms;

namespace RisenEditor.Code.Renderer
{
    public class UserConsole
    {
        class ConsolProcessor : PostProcessorH
        {
            SpriteBatch SP;
            ShaderResourceTexture m_pBoxTex, m_pRectTex;
            UserConsole m_parent;
            GameLibrary.AdvTextRenderer m_Rend;
            Form1 m_pParent;

            const int left = 5, top = 8;
            const int boxHeight = 220;
            static uint lCount = 0;

            public ConsolProcessor(Form1 F, UserConsole U)
                : base(F.Device)
            {
                m_pParent = F;
                SP = new SpriteBatch(F.Device);
                const int fSize = 13;
                lCount = (uint)(boxHeight / (fSize + 3)) - 1u;
                m_Rend = new GameLibrary.AdvTextRenderer(new Font("Arial", fSize), F.Device, Color.White, 4096);//extra strings = current line etc
                m_parent = U;
                m_pBoxTex = new ShaderResourceTexture(new Bitmap(1, 1), Device, System.Drawing.Imaging.ImageFormat.Bmp);//RisenEditor.Properties.Resources.console
                m_pRectTex = new ShaderResourceTexture(new Bitmap(1, 1), Device, System.Drawing.Imaging.ImageFormat.Bmp);//RisenEditor.Properties.Resources.conslRect
                SystemLog.OnAppend += new EventHandler<SystemLog.OnAppend_EventArgs>(SystemLog_OnAppend);
            }

            void SystemLog_OnAppend(object sender, SystemLog.OnAppend_EventArgs e)
            {
                m_parent.m_Strings.Add(e.Data);
                m_parent.lineIndex++;
            }

            public override void DoProcess(RenderInformation RI)
            {
                SP.StartBatch();
                m_Rend.StartStrip();
                Device.OutputMerger.IsDepthEnabled = false;
                int y2 = boxHeight - m_Rend.textHeight;
                m_pParent.BackBuffer.SetTarget();
                FullScreenQuad.DrawTexture(m_pBoxTex, new Rectangle(0, 0, m_pParent.BackBuffer.Width - 5, boxHeight));
                if (m_parent.m_Strings.Count > 0)
                {
                    List<string> lastL = new List<string>();
                    if (m_parent.lineIndex == m_parent.m_Strings.Count)
                    {
                        int c = m_parent.m_Strings.Count;
                        int c2 = (int)Math.Min(c, lCount);
                        lastL = m_parent.m_Strings.GetRange(c - c2, c2);
                    }
                    else
                    {
                        int t0 = m_parent.lineIndex - (int)lCount / 2;
                        int start = Math.Max(0, t0);
                        int end = Math.Min(m_parent.m_Strings.Count - 1, t0 + (int)lCount);
                        int off = t0 >= 0 ? 0 : Math.Abs(t0);
                        for (int i = 0; i < off; i++)
                            lastL.Add(string.Empty);
                        lastL.AddRange(m_parent.m_Strings.GetRange(start, end - start));
                    }

                    int y = top;
                    foreach (string s in lastL)
                    {
                        m_Rend.DrawString(s, left, y);
                        y += m_Rend.textHeight;
                    }
                }
                if (m_parent.currLine.Length > 0)
                    m_Rend.DrawString(m_parent.currLine, left, y2);
                if (m_parent.currLine.Length > 0)
                {
                    string bPart = m_parent.currLine.Substring(0, m_parent.caretIndex);
                    int dx = m_Rend.getStringWidthInPixels(bPart) + 2;
                    SP.FillLine(new Point(left + dx, y2), new Point(left + dx, y2 + m_Rend.textHeight - 2), Color.Wheat);
                }
                if (m_parent.selected != null && m_parent.selected.Length > 0)
                {
                    string bPart = m_parent.currLine.Substring(0, m_parent.currLine.IndexOf(m_parent.selected));
                    Rectangle r = new Rectangle();
                    r.X = m_Rend.getStringWidthInPixels(bPart) + left;
                    r.Y = y2;
                    r.Width = m_Rend.getStringWidthInPixels(m_parent.selected);
                    r.Height = m_Rend.textHeight;
                    //FullScreenQuad.DrawTexture(m_pRectTex, r);
                    SP.DrawRectangle(r.X, r.Y, r.Width, r.Height, Color.Red);
                }
                Device.OutputMerger.IsDepthEnabled = true;
                SP.EndBatch();
                m_Rend.Batch();
            }
        }

        List<string> m_Strings;
        ConsolProcessor drawer;
        string selected, currLine;
        int caretIndex;
        int lineIndex;
        internal static API_Device D;

        public UserConsole(Form1 F, PostProcessingManager a_Manager)
        {
            D = F.Device;
            drawer = new ConsolProcessor(F, this);
            a_Manager.AddPostProcessor(drawer);
            m_Strings = new List<string>();
            currLine = string.Empty;
        }

        public void ProcessInputKey(Keys a_Key, Keys a_KeyData, bool alt, bool shift, bool ctrl)
        {
            if (a_Key == Keys.ShiftKey || a_Key == Keys.ControlKey || a_Key == Keys.Alt) return;
            if ((a_Key & Keys.F12) == Keys.F12) return;
            a_Key = a_Key & Keys.KeyCode;
            if ((a_Key & Keys.V) == Keys.V && ctrl && Clipboard.ContainsText())
            {
                string strv = Clipboard.GetText();
                if (selected != null)
                    currLine = currLine.Replace(selected, strv);
                else currLine = currLine.Insert(caretIndex, strv);
                caretIndex = currLine.IndexOf(strv) + strv.Length;
                selected = null;
                caretIndex = Math.Max(0, Math.Min(caretIndex, currLine.Length));
            }
            else if ((a_Key & Keys.C) == Keys.C && ctrl && selected != null)
            {
                Clipboard.SetData(System.Windows.Forms.DataFormats.Text, selected);
                return;
            }
            else if ((a_Key & Keys.A) == Keys.A && ctrl)
            {
                selected = (String)currLine.Clone();
                caretIndex = selected.Length;
                return;//no shift pressed..
            }
            else if ((a_Key & Keys.Enter) == Keys.Enter && currLine.Length > 0)
            {
                ScriptManager.Interpret(currLine);
                m_Strings.Add(currLine);
                lineIndex++;
                currLine = string.Empty;
                caretIndex = 0;
            }
            else if (a_Key == Keys.Delete)
            {
                if (caretIndex < currLine.Length && selected == null)
                {
                    currLine = currLine.Remove(caretIndex, 1);
                }
                else if (selected != null)
                {
                    currLine = currLine.Replace(selected, "");
                    selected = null;
                    caretIndex = Math.Max(0, Math.Min(caretIndex, currLine.Length));
                }
            }
            else if (a_Key == Keys.Back)
            {
                if (currLine.Length > 0 && selected == null)
                {
                    currLine = currLine.Remove(caretIndex - 1, 1);
                    caretIndex--;
                }
                else if (selected != null)
                {
                    currLine = currLine.Replace(selected, "");
                    selected = null;
                    caretIndex = Math.Max(0, Math.Min(caretIndex, currLine.Length));
                }
            }
            else if (a_Key == Keys.End)
            {
                if (shift && caretIndex != currLine.Length)
                    selected = currLine.Substring(caretIndex, selected.Length - caretIndex);
                caretIndex = currLine.Length;
            }
            else if (a_Key == Keys.Home)
            {
                if (shift && caretIndex > 0)
                    selected = currLine.Substring(0, caretIndex);
                caretIndex = 0;
            }
            else if (a_Key == Keys.Left)
            {
                if (shift && caretIndex > 0)
                    selected = currLine.Substring(caretIndex - 1, 1) + (selected == null ? string.Empty : selected);
                caretIndex = Math.Max(caretIndex - 1, 0);
            }
            else if (a_Key == Keys.Right)
            {
                if (shift && caretIndex != currLine.Length)
                    selected = (string.Empty ?? selected) + currLine.Substring(caretIndex, 1);
                caretIndex = Math.Min(caretIndex + 1, currLine.Length);
            }
            else if (a_Key == Keys.Up && !ctrl)
            {
                int index = m_Strings.IndexOf(currLine);
                if (index == -1)
                    index = m_Strings.Count ;
                if (index > 0)
                {
                    currLine = m_Strings[index - 1];
                    selected = null;
                    caretIndex = currLine.Length;
                }
            }
            else if (a_Key == Keys.Down && !ctrl)
            {
                int index = m_Strings.IndexOf(currLine);
                if (index == -1) return;
                if (index < m_Strings.Count - 1)
                {
                    currLine = m_Strings[index + 1];
                    selected = null;
                    caretIndex = currLine.Length;
                }
            }
            else if (a_Key == Keys.Up && ctrl)
            {
                lineIndex = Math.Max(0, lineIndex - 1);
            }
            else if (a_Key == Keys.Down && ctrl)
            {
                lineIndex = Math.Min(m_Strings.Count, lineIndex + 1);
            }
            else if (!((a_Key & Keys.Enter) == Keys.Enter))
            {
                string s = User32Interop.GetCharsFromKeys(a_Key, shift, false);
                s = System.Text.RegularExpressions.Regex.Replace(System.Text.RegularExpressions.Regex.Replace(s, "[ÁÂàáâ]", "a"), "[ÈÉÊèéê]", "e");
                s = s.Replace("^", "");
                if (s.Length == 0 && a_Key == Keys.Space)
                    s = " ";
                if (selected == null)
                {
                    if (caretIndex == currLine.Length)
                    {
                        currLine = currLine + s[0];
                        caretIndex++;
                    }
                    else
                    {
                        currLine = currLine.Insert(caretIndex, s[0].ToString());
                    }
                }
                else if (currLine.Length != 0)
                    currLine = currLine.Replace(selected, s[0].ToString());
            }
            if (!shift) selected = null;
        }

        private bool m_Open = false;
        public bool Open
        {
            get
            {
                return m_Open;
            }
            set
            {
                if (m_Open != value)
                {
                    if (value)
                        drawer.Enabled = true;
                    else drawer.Enabled = false;
                }
                m_Open = value;
            }
        }
    }

    public static class ScriptManager
    {
        static Dictionary<string, Script_Object> m_Objs;

        static ScriptManager()
        {
            m_Objs = new Dictionary<string, Script_Object>();
            RegisterScriptObject(new SO_ROOT());
        }

        public static void RegisterScriptObject(Script_Object a_Object)
        {
            m_Objs.Add(a_Object.Name, a_Object);
        }

        public static void Interpret(string a_Input)
        {
            return;
            //only properties allowed
            Script_Object so_root = null;
            a_Input = a_Input.Replace(" ", "");
            int v_i = a_Input.IndexOf("=");
            string v_In = a_Input.Remove(v_i, a_Input.Length - v_i);
            string v_lP = a_Input.Remove(0, v_i).Replace("=", "");
            string[] v_parts = v_In.Split('.');
            if (v_parts.Length == 1)
            {
                Script_Object t = m_Objs["ROOT"][v_parts[0]];
                if (t != null)
                    m_Objs["ROOT"].SetChild(v_parts[0], parseObject(v_lP));
            }
            if (m_Objs.ContainsKey(v_parts[0]))
                so_root = m_Objs[v_parts[0]];
            if(so_root == null)
                so_root = m_Objs["ROOT"];
            for (int i = 1; i < v_parts.Length - 1; i++)
                so_root = so_root[v_parts[i]];
            so_root.SetChild(v_parts[v_parts.Length - 1], parseObject(v_lP));
        }

        private static object parseObject(string a_Value)
        {
            if(a_Value.StartsWith('"'.ToString()))
            {
                a_Value = a_Value.Replace('"'.ToString(), "");
                return a_Value;
            }
            else if (a_Value.StartsWith("{"))
            {
                a_Value = a_Value.Replace("{", "").Replace("}", "");
                string[] ps = a_Value.Split(',');
                switch (ps.Length)
                {
                    case 2:
                        return new Vector2(float.Parse(ps[0]), float.Parse(ps[1]));
                    case 3:
                        return new Vector3(float.Parse(ps[0]), float.Parse(ps[1]), float.Parse(ps[2]));
                    case 4:
                        return new Vector4(float.Parse(ps[0]), float.Parse(ps[1]), float.Parse(ps[2]), float.Parse(ps[3]));
                }
            }
            else if(a_Value.IsNumeric())
            {//parse as double, casts will restore destination type
                return double.Parse(a_Value);
            }
            else if (a_Value.ToLower() == "false" || a_Value.ToLower() == "true")
            {
                return bool.Parse(a_Value);
            }
            return null;
        }
    }

    public abstract class Script_Object
    {
        protected List<Script_Object> m_Objs;

        public virtual string Name
        {
            get
            {
                return this.GetType().Name.Replace("SO_", "");
            }
        }

        public List<Script_Object> Object
        {
            get
            {
                return m_Objs;
            }
        }

        public Script_Object this[string name]
        {
            get
            {
                foreach (Script_Object m in m_Objs)
                    if (m.Name == name)
                        return m;
                return null;
            }
        }

        public virtual void Method(string a_MethodName, params object[] a_Parameters)
        {
            this.GetType().GetMethod(a_MethodName).Invoke(this, a_Parameters);
        }

        public abstract void SetChild(string a_Name, object a_Object);
    }

    public class SO_Helper : Script_Object
    {
        private string name;

        public SO_Helper(string name)
        {
            this.name = name;
        }

        public override void Method(string a_MethodName, params object[] a_Parameters)
        {
            
        }

        public override string Name
        {
            get
            {
                return name;
            }
        }

        public override void SetChild(string a_Name, object a_Object)
        {
            
        }
    }

    public class SO_ROOT : Script_Object
    {
        const string m_FarDepth = "MaximalViewDistance";
        const string m_NearDepth = "MinimalViewDistance";

        public SO_ROOT()
        {
            m_Objs = new List<Script_Object>();
            m_Objs.Add(new SO_Helper(m_FarDepth));
            m_Objs.Add(new SO_Helper(m_NearDepth));
        }

        public override void SetChild(string a_Name, object a_Object)
        {
            switch (a_Name)
            {
                case m_FarDepth:
                    ManagedSettings.FarDepth = (float)(double)a_Object;
                    break;
                case m_NearDepth:
                    ManagedSettings.FarDepth = (float)(double)a_Object;
                    break;
            }
        }
    }
}
