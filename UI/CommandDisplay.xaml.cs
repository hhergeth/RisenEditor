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
    /// Interaktionslogik für CommandDisplay.xaml
    /// </summary>
    public partial class CommandDisplay : UserControl
    {
        public CommandDisplay()
        {
            InitializeComponent();
        }

        public InfoCommandWrapper Wrapper
        {
            get
            {
                return (DataContext as InfoCommandWrapper.c0).A;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            label1.Content = Wrapper.CommandName;
        }

        private void label1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            xInfoElement.g_Singleton.setCmd(Wrapper, Wrapper.Parent);
        }
    }
}
