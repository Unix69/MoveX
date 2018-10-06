using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Collections.Concurrent;

namespace Movex.MessageView
{
    /// <summary>
    /// The settings menu state as a view model
    /// </summary>
    public class WhereViewModel : BaseViewModel
    {
        #region Public Properties

        public string Text { get; set; }
        private string Path { get; set; }
        private ManualResetEvent CloseWindowEvent { get; set; }
        private ConcurrentStack<string> Response { get; set; }

        #endregion

        #region Public Command

        /// <summary>
        /// The command to select the path
        /// </summary>
        public ICommand SaveCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public WhereViewModel()
        {
            SaveCommand = new RelayCommand(Save);
            
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the message for the Window
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text) {
            if (text != null)
            {
                Text = text;
            }
        }
        
        /// <summary>
        /// Send the Path to the caller
        /// </summary>
        public void Save()
        {
            if (Path != null)
            {
                Response.Push(Path);
            }

            if (CloseWindowEvent != null)
            {
                CloseWindowEvent.Set();
            }
        }

        internal void SetCloseWindowEvent(ManualResetEvent closeWindowEvent)
        {
            CloseWindowEvent = closeWindowEvent;
        }

        internal void SetResponse(ConcurrentStack<string> response)
        {
            Response = response;
        }

        internal void SetPath(string path)
        {
            Path = path;
        }

        #endregion
    }
}