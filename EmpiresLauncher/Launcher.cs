﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace EmpiresLauncher
{
    public partial class Launcher : Form
    {
        private const string sourceSdkBase2007Name = "source sdk base 2007";
        private const string sourceSdkBase2007DriveNotice = "Source SDK Base 2007 must be installed on the same drive as Empires. If it's not, remove Empires using Steam and install it again to the same drive as Source SDK Base 2007.";
        private const string hl2ExeName = "hl2.exe";
        private const string empiresName = "empires";
        private const string installSourceSdkBase2007Uri = "steam://run/218";

        public Launcher()
        {
            InitializeComponent();
            Show();
            BringToFront();
            RunEmpires(String.Join(" ", Environment.GetCommandLineArgs()));
            Environment.Exit(0);
        }
        
        private void RunEmpires(string args)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var steamappsDirectory = Directory.GetParent(currentDirectory).Parent.FullName;
            var sourceSdkBase2007Directory = FindGameDirectory(sourceSdkBase2007Name, steamappsDirectory);

            var sourceSdkBase2007Exists = !String.IsNullOrEmpty(sourceSdkBase2007Directory);
            if (sourceSdkBase2007Exists)
            {
                var hl2ExeExists = DirectoryContainsFileName(sourceSdkBase2007Directory, hl2ExeName);
                if (hl2ExeExists)
                {
                    var sourceSdkBase2007Hl2Exe = Path.Combine(sourceSdkBase2007Directory, hl2ExeName);
                    var empiresModDirectory = Path.Combine(currentDirectory, empiresName);
                    var launchArguments = String.Format("-game \"{0}\" {1}", empiresModDirectory, args);

                    var startInfo = new ProcessStartInfo()
                    {
                        FileName = sourceSdkBase2007Hl2Exe,
                        Arguments = launchArguments
                    };

                    try
                    {
                        using (var process = Process.Start(startInfo))
                        {
                            new Thread(delegate()
                            {
                                Thread.Sleep(2000);
                                Hide();
                            }).Start();
                            process.WaitForExit();
                        }
                    }
                    catch (Win32Exception)
                    {
                        MessageBox.Show("Can't start Empires because there was an error when running hl2.exe", "Empires Mod", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        throw;
                    }
                }
                else
                {
                    var result = MessageBox.Show("Can't start Empires because hl2.exe in Source SDK Base 2007 was not found. Click OK to run Source SDK Base 2007 and generate hl2.exe. After Source SDK Base 2007 has run, quit it, and start Empires again.\n\n" + sourceSdkBase2007DriveNotice, "Empires Mod", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (result == DialogResult.OK)
                    {
                        Process.Start(installSourceSdkBase2007Uri);
                    }
                }
            }
            else
            {
                var result = MessageBox.Show("Can't start Empires because Source SDK Base 2007 was not found. Click OK to install and run Source SDK Base 2007. After Source SDK Base 2007 has run, quit it, and start Empires again.\n\n" + sourceSdkBase2007DriveNotice, "Empires Mod", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (result == DialogResult.OK)
                {
                    Process.Start(installSourceSdkBase2007Uri);
                }
            }
        }

        private static string FindGameDirectory(string targetGameName, string searchRoot)
        {
            var steamappDirectories = Directory.GetDirectories(searchRoot);
            foreach (var steamappDirectory in steamappDirectories)
            {
                var gameDirectories = Directory.GetDirectories(steamappDirectory);
                foreach (var gameDirectory in gameDirectories)
                {
                    var gameDirectoryName = Path.GetFileName(gameDirectory);
                    if (gameDirectoryName.Equals(targetGameName, StringComparison.OrdinalIgnoreCase))
                    {
                        return gameDirectory;
                    }
                }
            }

            return null;
        }

        private static bool DirectoryContainsFileName(string directoryName, string fileName)
        {
            foreach (var file in Directory.GetFiles(directoryName))
            {
                if (Path.GetFileName(file).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
