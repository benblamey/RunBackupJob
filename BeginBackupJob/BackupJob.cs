using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Win32;

namespace BeginBackupJob
{
    class BackupJob : DispatcherObject
    {
        public BackupJob ()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(BeginJob), this);
        }

        public event EventHandler StatusChanged;

        public string Status
        {
            get
            {
                return m_status;
            }
            set
            {
                if (value != m_status)
                {
                    m_status = value;
                    Dispatcher.BeginInvoke(new Action(OnStatusChanged), null);
                }
            }
        }

        private void OnStatusChanged()
        {
            if (StatusChanged != null)
            {
                StatusChanged(this, EventArgs.Empty);
            }
        }

        

        private void BeginJob(object nullObj)
        {
            string rootDirectory = string.Empty;
            int attempts = 0;
            const int maxAttempts = 60;

            while (string.IsNullOrEmpty(rootDirectory) && (attempts < maxAttempts))
            {
                DriveInfo[] infos = DriveInfo.GetDrives();

                foreach (var driveInfo in infos)
                {
                    if (driveInfo.DriveType == DriveType.Fixed)
                    {

                        if (driveInfo.VolumeLabel == "TrekStor Backup")
                        {
                            if (driveInfo.IsReady)
                            {
                                rootDirectory = driveInfo.RootDirectory.FullName;
                            }
                            else
                            {
                                Status= "Found drive 'TrekStor Backup'\n\n\nWaiting for drive to be ready.";
                            }
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(rootDirectory))
                {
                    Status= "Searching for backup drive: 'TrekStor Backup'...\n\n\n attempt " + attempts + " of " + maxAttempts
                        + "\n\n\nMake sure the drive is plugged into the computer and powered.";

                    Thread.Sleep(1000);
                }

                attempts++;
            }

            if (attempts == maxAttempts)
            {
                Status= "The drive 'TrekStor Backup' could not be found. Check the drive is powered and plugged in to the computer.";
            }
            else
            {
                PerformBackup(rootDirectory);
            }

        }

        private void PerformBackup(string driveRoot)
        {
            int backupCount = (int)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\BFN", "backupCount", 0);

            backupCount = backupCount % 7;

            string backupMode;
            if (backupCount == 0)
            {
                backupMode = "normal";
            }
            else
            {
                backupMode = "differential";
            }

            string jobName;
                        string description = jobName = string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}-{4:D2} ({5})",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                DateTime.Now.Hour, DateTime.Now.Minute, backupMode);

            string fileName = string.Format("{0}backups-tucoyse\\{1}.bkf", driveRoot, jobName);

            string arguments = string.Format(

                "backup \"@{0}\" /J \"{1}\" /F \"{2}\" /D \"{3}\""
                // backup list, job name, file name, description.

                + " /V:no /L:f"
                // Don't verify, full log file.

                + " /M {4}"
                // backup mode, 
                , backupListFileName, jobName, fileName, description, backupMode);

            ProcessStartInfo info = new ProcessStartInfo("ntbackup", arguments);
            info.WindowStyle = ProcessWindowStyle.Hidden;
            Process ntbackup = Process.Start(info);

            Status= "Backup operation in progess. Please wait...";

            ntbackup.WaitForExit();

            int returnCode = ntbackup.ExitCode;


            if (returnCode == 0)
            {
                backupCount++;
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\BFN", "backupCount", backupCount, RegistryValueKind.DWord);
                Status= "The backup job completed successfully (unless you cancelled it!). Please safely remove the disk and unplug it.";
            }
            else
            {
                Status= "The backup job did not complete successfully!\n\n\nFile name: " + fileName;
            }
        }

        private readonly static string backupListFileName = @"D:\data\backupselection.bks";
        string m_status;

    }
}
