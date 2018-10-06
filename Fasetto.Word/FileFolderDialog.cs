using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Windows.Forms;

namespace Movex.View
{
    public class FileFolderDialog : CommonDialog
    {
        private OpenFileDialog mDialog = new OpenFileDialog();

        public OpenFileDialog Dialog
        {
            get => mDialog;
            set => mDialog = value;
        }

        public new DialogResult ShowDialog()
        {
            return ShowDialog(null);
        }

        public new DialogResult ShowDialog(IWin32Window owner)
        {
            // Set validate names to false otherwise windows will not let you select "Folder Selection."
            mDialog.ValidateNames = false;
            mDialog.CheckFileExists = false;
            mDialog.CheckPathExists = true;

            try
            {
                // Set initial directory (used when dialog.FileName is set from outside)
                if (mDialog.FileName != null && mDialog.FileName != "")
                {
                    if (Directory.Exists(mDialog.FileName))
                        mDialog.InitialDirectory = mDialog.FileName;
                    else
                        mDialog.InitialDirectory = Path.GetDirectoryName(mDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                // Do nothing
            }

            // Always default to Folder Selection.
            mDialog.FileName = "Folder Selection.";

            if (owner == null)
                return mDialog.ShowDialog();
            else
                return mDialog.ShowDialog(owner);
        }

        /// <summary>
        // Helper property. Parses FilePath into either folder path (if Folder Selection. is set)
        // or returns file path
        /// </summary>
        public string SelectedPath
        {
            get
            {
                try
                {
                    if (mDialog.FileName != null &&
                        (mDialog.FileName.EndsWith("Folder Selection.") || !File.Exists(mDialog.FileName)) &&
                        !Directory.Exists(mDialog.FileName))
                    {
                        return Path.GetDirectoryName(mDialog.FileName);
                    }
                    else
                    {
                        return mDialog.FileName;
                    }
                }
                catch (Exception ex)
                {
                    return mDialog.FileName;
                }
            }
            set
            {
                if (value != null && value != "")
                {
                    mDialog.FileName = value;
                }
            }
        }

        /// <summary>
        /// When multiple files are selected returns them as semi-colon seprated string
        /// </summary>
        public string SelectedPaths
        {
            get
            {
                if (mDialog.FileNames != null && mDialog.FileNames.Length > 1)
                {
                    var sb = new StringBuilder();
                    foreach (var fileName in mDialog.FileNames)
                    {
                        try
                        {
                            if (File.Exists(fileName))
                                sb.Append(fileName + ";");
                        }
                        catch (Exception ex)
                        {
                            // Go to next
                        }
                    }
                    return sb.ToString();
                }
                else
                {
                    return null;
                }
            }
        }

        public override void Reset()
        {
            mDialog.Reset();
        }

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            return true;
        }
    }
}
