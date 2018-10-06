using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System;
using System.Diagnostics;

namespace Movex.View.Windows
{
    public static class ViewManager
    {
        /*
        #region Private Static Members
        // Main thread and data Structure needed
        private static Thread MessageViewThread;        
        private static Queue<Window> WindowsQueue = new Queue<Window>();
        private static ManualResetEvent Available = new ManualResetEvent(false);
        #endregion
        */

        #region Public Static Methods
        public static void Init() { }
        public static void Start()
        {
            /* MESSAGE VIEW FUNCTIONALITY
             * MessageView allows to show 3 types of windows: 
             * - MessageWindow
             * - YesNoWindow
             * - WhereWindow
             * - ProgressWindow
             * 
             * MessageWindow display a message, does not have to return anything
             * YesNoWindow display a question and return the response
             * WhereWindow display a question and return a path
             * ProgressWindow display a set of informations
             * 
             * CONSTRAINTS:
             * MessageView contains lots of UI Elements and,
             * as of the WPF Framework Documentation, has to
             * be launched with a Single-Thread App (STA) Thread.
             * 
             * MessageView is launched with a Thread once.
             * Then it can receive requests to display windows,
             * and return responses.
             * 
             */

            //var MessageViewThread = new Thread(new ThreadStart(() => DoWork(WindowsQueue, Available)));
            //MessageViewThread.SetApartmentState(ApartmentState.STA);
            //MessageViewThread.Start();
        }
        /*
        public static void Stop()
        {
            MessageViewThread.Join();
            MessageViewThread.Interrupt();
            MessageViewThread = null;
        }
        public static string LaunchWindow<T>(Window<T>.WindowType type, T message)
        {
            Window<T> w;
            ConcurrentStack<string> response;
            ManualResetEvent responseAvailability;
            
            try
            {
                switch (type)
                {
                    case Window<T>.WindowType.MessageWindow:
                        w = new Window<T>(Window<T>.WindowType.MessageWindow, message, null, null);
                        WindowsQueue.Enqueue(w);
                        Available.Set();
                        return null;

                    case Window<T>.WindowType.YesNoWindow:
                        responseAvailability = new ManualResetEvent(false);
                        response = new ConcurrentStack<string>();
                        w = new Window<T>(Window<T>.WindowType.YesNoWindow, message, responseAvailability, response);
                        WindowsQueue.Enqueue(w);
                        Available.Set();
                        responseAvailability.WaitOne();
                        response.TryPop(out string responseString);
                        return responseString;

                    case Window.WindowType.WhereWindow:
                        responseAvailability = new ManualResetEvent(false);
                        response = new ConcurrentStack<string>();
                        w = new Window(Window.WindowType.WhereWindow, message, responseAvailability, response);
                        WindowsQueue.Enqueue(w);
                        Available.Set();
                        responseAvailability.WaitOne();
                        response.TryPop(out string responseStr);
                        return responseStr;

                    case Window.WindowType.ProgressWindow:
                        w = new Window(Window.WindowType.ProgressWindow, message, null, null);
                        WindowsQueue.Enqueue(w);
                        Available.Set();
                        return null;

                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Boom!");
                return null;
            }
        }
        #endregion

        #region Private Static Methods
        /// <summary>
        /// Background method to handle new requests and display windows
        /// </summary>
        /// <param name="WindowsQueue"></param>
        /// <param name="available"></param>
        private static void BackgroundWorker(Queue<Window> WindowsQueue, ManualResetEvent available)
        {
            // if something is available
            waitForRequests: available.WaitOne();

            // Extract from a fifo queue the windows to display
            var Window = WindowsQueue.Dequeue();
            Thread WindowThread = null;
            ManualResetEvent openWindowEvent = null;
            ManualResetEvent closeWindowEvent = null;
            ManualResetEvent responseAvailability = null;
            ConcurrentStack<string> response = null;

            if (Window.GetType().Equals(Window.WindowType.MessageWindow))
            {
                closeWindowEvent = Window.GetCloseWindowEvent();
                WindowThread = new Thread(() =>
                {
                    new MessageView.MessageWindow(Window.GetText(), closeWindowEvent).Show();
                    System.Windows.Threading.Dispatcher.Run();
                });
                WindowThread.SetApartmentState(ApartmentState.STA);
                WindowThread.Start();
            }
            else if (Window.GetType().Equals(Window.WindowType.YesNoWindow))
            {
                closeWindowEvent = Window.GetCloseWindowEvent();
                responseAvailability = Window.GetResponseAvailablity();

                WindowThread = new Thread(() => {
                    response = Window.GetResponse();
                    new MessageView.YesNoWindow(Window.GetText(), closeWindowEvent, response).Show();
                    System.Windows.Threading.Dispatcher.Run();
                });
                WindowThread.SetApartmentState(ApartmentState.STA);
                WindowThread.Start();

            }
            else if (Window.GetType().Equals(Window.WindowType.WhereWindow))
            {
                closeWindowEvent = Window.GetCloseWindowEvent();
                responseAvailability = Window.GetResponseAvailablity();

                WindowThread = new Thread(() => {
                    response = Window.GetResponse();
                    new MessageView.WhereWindow(Window.GetText(), closeWindowEvent, response).Show();
                    System.Windows.Threading.Dispatcher.Run();
                });
                WindowThread.SetApartmentState(ApartmentState.STA);
                WindowThread.Start();

            }
            else if (Window.GetType().Equals(Window.WindowType.ProgressWindow))
            {
                openWindowEvent = Window.GetOpenWindowEvent();
                closeWindowEvent = Window.GetCloseWindowEvent();
                WindowThread = new Thread(() =>
                {
                    openWindowEvent.WaitOne();
                    new View.ProgressWindow(Window.GetText(), closeWindowEvent).Show();
                    System.Windows.Threading.Dispatcher.Run();
                });
                WindowThread.SetApartmentState(ApartmentState.STA);
                WindowThread.Start();
            }

            // Wait for responses
            if (WindowThread != null)
            {
                if (closeWindowEvent != null)
                    closeWindowEvent.WaitOne();

                if (response != null)
                {
                    if (!response.IsEmpty)
                        responseAvailability.Set();
                }

                WindowThread.Interrupt();
                if (WindowThread.IsAlive)
                    WindowThread.Abort();

                WindowThread = null;
            }

            //Reset the availability
            if (WindowsQueue.Count == 0)
            {
                available.Reset();
            }
            Window = null;
            goto waitForRequests;
        }
        /// <summary>
        /// Main method to handle the Message View App
        /// </summary>
        /// <param name="WindowsQueue"></param>
        /// <param name="available"></param>
        private static void DoWork(Queue<Window> WindowsQueue, ManualResetEvent available)
        {
            // Launching the Message View App, without showing it
            //var MessageViewApp = new MessageView.App();
            //MessageViewApp.InitializeComponent();

            // Set the background worker
            var backgroundWorkerThread = new Thread(new ThreadStart(() =>
            {
                BackgroundWorker(WindowsQueue, available);
            }));
            backgroundWorkerThread.SetApartmentState(ApartmentState.STA);
            backgroundWorkerThread.Start();

            // Show the Message View App
            //MessageViewApp.Run();
        }*/
        #endregion

    }
}
