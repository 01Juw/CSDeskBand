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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CSDeskband.Wpf;
using System.Runtime.InteropServices;
using CSDeskband;

namespace Sample.Wpf
{
    [Guid("89BF6B36-A0B0-4C95-A666-87A55C226986")]
    [CSDeskBandRegistration(Name = "Sample Deskband")]
    public partial class UserControl1 : CSDeskBand
    {
        public UserControl1()
        {
            InitializeComponent();
        }
    }
}
