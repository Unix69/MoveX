using System.IO;
using System.Text;
using SharpShell.SharpContextMenu;
using SharpShell.Attributes;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System;

namespace Movex.ContextMenu
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.AllFiles)]
    [COMServerAssociation(AssociationType.Directory)]
    public class EntryWindowsContextMenu : SharpContextMenu
    {

        // Private Members
        private StringBuilder arguments = new StringBuilder();
        private int numFiles = 0;
        private int numFolders = 0;

        protected override bool CanShowMenu()
        {
            return true;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            // Create the menu strip.
            var menu = new ContextMenuStrip();

            // Create a 'Condividi con' item
            var itemShareWith = new ToolStripMenuItem
            {
                Text = "Condividi con MoveX",
                Image = Properties.Resources.icon
            };

            // When we click, we'll call the 'launchView' function.
            itemShareWith.Click += (sender, args) => launchView();

            // Add the item to the context menu.
            menu.Items.Add(itemShareWith);

            // Return the menu.
            return menu;
        }

        private void CollectFileList_Old(System.Collections.Generic.IEnumerable<string> paths)
        {
            // Go through each path
            foreach (var filePath in paths)
            {
                // Check if the filePath is a directory
                if (File.GetAttributes(filePath).HasFlag(FileAttributes.Directory))
                {
                    // Get the an array of files and subfolders
                    string[] pathsArray = Directory.GetFileSystemEntries(filePath);
                    
                    // Convert to list by default constructors and fill an Iterator
                    List<string> list = new List<string>(pathsArray);
                    System.Collections.Generic.IEnumerable<string> subFolderPaths = list;

                    // Follow the path recursively
                    CollectFileList(subFolderPaths);
                }
                else {
                    // Append the file
                    numFiles++;
                    arguments.Append(string.Format(" \"{0}\"", Path.GetFullPath(filePath)));
                }
            }

            return;
        }

        private void CollectFileList(System.Collections.Generic.IEnumerable<string> paths)
        {
            // Go through each path
            foreach (var path in paths)
            {
                // Check if the filePath is a directory
                if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                {
                    numFolders++;
                    arguments.Append(string.Format(" \"{0}\"", Path.GetFullPath(path)));
                }
                else
                {
                    // Append the file
                    numFiles++;
                    arguments.Append(string.Format(" \"{0}\"", Path.GetFullPath(path)));
                }
            }

            return;
        }

        private void CollectFileListAsTree(System.Collections.Generic.IEnumerable<string> paths, int depth)
        {
            // Go through each path
            foreach (var path in paths)
            {
                // Check if the filePath is a directory
                if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                {
                    // Append the folder
                    numFolders++;
                    arguments.AppendLine(string.Format("{0} {1}", path, depth));

                    // Get the an array of files and subfolders
                    string[] pathsArray = Directory.GetFileSystemEntries(path);

                    // Convert to list by default constructors and fill an Iterator
                    List<string> list = new List<string>(pathsArray);
                    System.Collections.Generic.IEnumerable<string> subFolderPaths = list;

                    // Follow the path recursively
                    CollectFileListAsTree(subFolderPaths, depth+1);
                }
                else
                {
                    // Append the file
                    numFiles++;
                    arguments.AppendLine(string.Format("{0} {1}", path, depth));
                }
            }

            return;
        }

        private void launchView()
        {
            string message;
            string appName;

            // Scan the path recursively
            CollectFileList(SelectedItemPaths);

            //  Show the ouput.
            appName = "moveX - Local Sharing Point";
            if (numFiles+numFolders == 1)
            {
                message = "Condividere 1 elemento?";
            }
            else if (numFiles+numFolders > 1)
            {
                message = "Condividere " + numFiles + " files e " + numFolders + " cartelle ?";
            }
            else
            {
                message = "Errore alla selezione degli elementi.";
            }
            MessageBoxResult result = System.Windows.MessageBox.Show(message, appName, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            switch (result)
            {
                case MessageBoxResult.Yes:

                    // The installer store the Movex.View.exe file in the SystemFolder\Movex\ folder
                    // where SystemFolder can be, accordingly to the documentation:
                    // C:\Windows\System32 (for 32-bit OS)
                    // C:\Windows\SysWow64 (for 34-bit OS)
                    //
                    // However, the shell extension start always from C:\Windows\System32
                    // so, starting from it, we browse towards one of those folders above
                    //
                    // Take reference here:
                    // https://msdn.microsoft.com/en-us/library/windows/desktop/aa372055(v=vs.85).aspx

                    if (Environment.Is64BitOperatingSystem)
                    {
                        
                        Process.Start(@".\..\SysWow64\Movex\Movex.View.exe", arguments.ToString());
                    }
                    else
                    {
                        Process.Start(@".\Movex\Movex.View.exe", arguments.ToString());
                    }
                    break;

                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.None:
                    break;
                case MessageBoxResult.Cancel:
                    break;
            }

        }

    }
}
