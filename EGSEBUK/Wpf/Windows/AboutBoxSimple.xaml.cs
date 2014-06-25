﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Egse.Defaults
{
    /// <summary>
    /// Interaction logic for AboutBoxSimple.xaml
    /// </summary>
    public partial class AboutBoxSimple : Window
    {
        public AboutBoxSimple()
        {
            InitializeComponent();
        }

        public AboutBoxSimple(Window parent)
            : this()
        {
            this.Owner = parent;
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();                                        
            version.Content = assembly.GetName().Version.ToString();
        }
    }
}
