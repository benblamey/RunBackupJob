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
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;

namespace BeginBackupJob
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(Window1_Loaded);

        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            m_job = new BackupJob();
            m_job.StatusChanged += new EventHandler(job_StatusChanged);
        }

        void job_StatusChanged(object sender, EventArgs e)
        {
            Status.Text = m_job.Status;
        }

        BackupJob m_job;

        

        
        
    }
}
