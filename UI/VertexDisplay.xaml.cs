using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RisenEditor.Code;

namespace RisenEditor.UI
{
    /// <summary>
    /// Interaktionslogik für VertexDisplay.xaml
    /// </summary>
    public partial class VertexDisplay : UserControl
    {
        static ContextMenu g_pCtxMenuCmd = null, g_pCtxMenuCnd = null, g_pCtxMenuAll;

        public VertexDisplay()
        {
            
            //if (g_pCtxMenuCmd == null)
            {
                List<Type> T = GENOMEMath.FindAllDerivedTypes<InfoCommandWrapper>();
                Action<MenuItem, RoutedEventHandler, string> B = (y, a, f) =>
                {
                    foreach (Type t in T)
                    {
                        if (!t.Name.Contains(f))
                            continue;
                        MenuItem m = new MenuItem();
                        m.Header = t.Name;
                        m.Click += a;
                        m.Tag = t;
                        y.Items.Add(m);
                    }
                };
                Func<RoutedEventHandler, RoutedEventHandler, RoutedEventHandler, string, ContextMenu> A = (r,z, w, F) =>
                {
                    ContextMenu x = new System.Windows.Controls.ContextMenu();
                    MenuItem mc = new MenuItem() { Header = "Remove"};
                    x.Items.Add(mc);
                    mc.Click += r;
                    mc = new MenuItem() { Header = "Add before" };
                    x.Items.Add(mc);
                    B(mc, z, F);
                    mc = new MenuItem() { Header = "Add after" };
                    x.Items.Add(mc);
                    B(mc, w, F);
                    return x;
                };
                g_pCtxMenuCnd = A(MenuItem_Click, MenuItem_Click_1, MenuItem_Click_2, "Condition");
                g_pCtxMenuCmd = A(MenuItem_ClickA, MenuItem_Click_1A, MenuItem_Click_2A, "Command");

                g_pCtxMenuAll = new ContextMenu();
                MenuItem mcmd = new MenuItem() { Header = "Add Command" };
                g_pCtxMenuAll.Items.Add(mcmd);
                B(mcmd, MenuItem_Click_1B, "Command");
                MenuItem mcnd = new MenuItem() { Header = "Add Condition" };
                g_pCtxMenuAll.Items.Add(mcnd);
                B(mcnd, MenuItem_Click_2B, "Condition");
            }
            this.Resources["contextMenu"] = g_pCtxMenuCmd;
            this.Resources["contextMenu2"] = g_pCtxMenuCnd;
            this.Resources["contextMenu3"] = g_pCtxMenuAll;
            InitializeComponent();
        }

        public InfoWrapper Wrapper
        {
            get
            {
                return (DataContext as InfoWrapper.c0).A;
            }
        }

        void setWrapper()
        {
            label1.Content = Wrapper.Name;
            listbox1.ItemsSource = Wrapper.Commands;
            listbox2.ItemsSource = Wrapper.Conditions;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            setWrapper();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            InfoCommandWrapper i = listbox2.SelectedItem as InfoCommandWrapper;
            Wrapper.RemoveCondition(i);
            setWrapper();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            InfoCommandWrapper a = InfoCommandWrapper.getWrapper((sender as MenuItem).Tag as Type, Wrapper);
            InfoCommandWrapper i = listbox2.SelectedItem as InfoCommandWrapper;
            int lastSel = Array.IndexOf(Wrapper.Conditions, i);
            Wrapper.InsertCondition(a, Math.Max(0, lastSel));
            setWrapper();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            InfoCommandWrapper a = InfoCommandWrapper.getWrapper((sender as MenuItem).Tag as Type, Wrapper);
            InfoCommandWrapper i = listbox2.SelectedItem as InfoCommandWrapper;
            int lastSel = Array.IndexOf(Wrapper.Conditions, i);
            Wrapper.InsertCondition(a, Math.Min(Wrapper.Conditions.Length, lastSel + 1));
            setWrapper();
        }

        private void MenuItem_ClickA(object sender, RoutedEventArgs e)
        {
            InfoCommandWrapper i = listbox1.SelectedItem as InfoCommandWrapper;
            Wrapper.RemoveCommand(i);
            setWrapper();
        }

        private void MenuItem_Click_1A(object sender, RoutedEventArgs e)
        {
            InfoCommandWrapper a = InfoCommandWrapper.getWrapper((sender as MenuItem).Tag as Type, Wrapper);
            InfoCommandWrapper i = listbox1.SelectedItem as InfoCommandWrapper;
            int lastSel = Array.IndexOf(Wrapper.Commands, i);
            Wrapper.InsertCommand(a, Math.Max(0, lastSel));
            setWrapper();
        }

        private void MenuItem_Click_2A(object sender, RoutedEventArgs e)
        {
            InfoCommandWrapper a = InfoCommandWrapper.getWrapper((sender as MenuItem).Tag as Type, Wrapper);
            InfoCommandWrapper i = listbox1.SelectedItem as InfoCommandWrapper;
            int lastSel = Array.IndexOf(Wrapper.Commands, i);
            Wrapper.InsertCommand(a, Math.Min(Wrapper.Commands.Length, lastSel + 1));
            setWrapper();
        }

        private void MenuItem_Click_1B(object sender, RoutedEventArgs e)
        {
            InfoCommandWrapper a = InfoCommandWrapper.getWrapper((sender as MenuItem).Tag as Type, Wrapper);
            Wrapper.AddCommand(a);
            setWrapper();
        }

        private void MenuItem_Click_2B(object sender, RoutedEventArgs e)
        {
            InfoCommandWrapper a = InfoCommandWrapper.getWrapper((sender as MenuItem).Tag as Type, Wrapper);
            Wrapper.AddCondition(a);
            setWrapper();
        }
    }
}
