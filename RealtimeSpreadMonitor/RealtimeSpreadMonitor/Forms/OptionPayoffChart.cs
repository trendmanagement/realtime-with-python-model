﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealtimeSpreadMonitor.Forms
{
    public partial class OptionPayoffChart : Form
    {
        public OptionPayoffChart()
        {
            InitializeComponent();
        }

        private void OptionPayoffChart_FormClosed(object sender, FormClosedEventArgs e)
        {
            Dispose();
            GC.Collect();
        }
    }
}
