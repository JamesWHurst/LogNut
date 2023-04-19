#if PRE_4
#define PRE_5
#endif
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Hurst.BaseLib;

// Compile - time pragma ENVDTE must be defined if you want to include the facilities of class VisualStudioLib.


namespace Hurst.LogNut
{
    #region class VisualStudioLib
    /// <summary>
    /// This class contains functions that apply toward control of the Visual Studio IDE.
    /// </summary>
    public class VisualStudioLib
    {
        #region MostRecentTrace
        /// <summary>
        /// Get or set the property that denotes the last Trace output that was sent to a Logger.
        /// This value is overwritten with each Trace output.
        /// It's for unit-testing.
        /// </summary>
        public static string MostRecentTrace
        {
            get
            {
                //CBL I'm not actually sure this is useful. May want to just delete this.
                if (String.IsNullOrEmpty(_mostRecentTrace))
                {
                    return null;
                }
                return _mostRecentTrace;
            }
            set { _mostRecentTrace = value; }
        }

        private static string _mostRecentTrace;

        #endregion

        //CBL This is previous code, which I've replaced (see below). 
#if FALSE
        #region class LoggerStream
        /// <summary>
        /// This class exists for the purpose of re-directing the output of a Stream to a Logger.
        /// </summary>
        class LoggerStream : Stream
        {
            public LoggerStream( LogNut.Logger destinationLogger )
            {
                _myLogger = destinationLogger;
            }

            public override void Flush()
            {
            }

#if !NETFX_CORE
            public override void Close()
            {
                //CBL  I may need this if Write is called to build up a line of text, but it is never written out.
                // Here, I could write out the remainder of what was written to this stream, before closing.
                base.Close();
            }
#endif

            public override long Seek( long offset, SeekOrigin origin )
            {
                return 0;
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override void SetLength( long value )
            {
            }

            public override int Read( byte[] buffer, int offset, int count )
            {
                return 0;
            }

            public override void Write( byte[] buffer, int offset, int count )
            {
                // Convert from byte array to a string
                string stringContent = Encoding.UTF8.GetString( buffer, offset, count );
                _myLogger.LogDebug( stringContent );
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override long Length
            {
                get { return 0; }
            }

            public override long Position
            {
                get
                {
                    return 0;
                }
                set
                {
                }
            }

            /// <summary>
            /// All writing to this LoggerStream object is intended to go to this Logger.
            /// </summary>
            private LogNut.Logger _myLogger;
        }
        #endregion class LoggerStream
#endif

        #region class LogNutTraceListener
        /// <summary>
        /// This subclass of TraceListener serves to log the text that comes into it
        /// using LogNut.
        /// </summary>
        class LogNutTraceListener : TraceListener
        {
            //CBL Or, could make this a subclass of TextWriterTraceListener. But that seems redundant.

            #region ctor
            /// <summary>
            /// Create a new LogNutTraceListener to handle trace-output using the given Logger.
            /// </summary>
            /// <param name="logger">the logger to use to handle this trace-output</param>
            /// <param name="levelToLogAt">the LogOutputLevel at which to log presentation-level errors</param>
            /// <remarks>
            /// It probably be argued that it makes sense to use any of Trace, Debug, Warn, or Error for the levelToLogAt.
            /// </remarks>
            public LogNutTraceListener(Logger logger, LogOutputLevel levelToLogAt)
                : base("LogNutTraceListener")
            {
                _levelToLogAt = levelToLogAt;
                _logger = logger;
                _sb = new StringBuilder();
            }
            #endregion

            /// <summary>
            /// Write out any accumulated log-text and clear the text-accumulation.
            /// </summary>
            public override void Flush()
            {
                if (_sb.Length > 0)
                {
                    _logger.Log(_levelToLogAt, LogCategory.Empty, _sb.ToString(), true);
#if !PRE_4
                    _sb.Clear();
#else
                    _sb.Length = 0;
#endif
                }
            }

            /// <summary>
            /// Accumulate the given text for later logging (at the next call to WriteLine or Flush).
            /// </summary>
            /// <param name="message">the text to log</param>
            public override void Write(string message)
            {
                // Just collect these and don't write them out until WriteLine is called.
                _sb.Append(message);
                _mostRecentTrace = message;
            }

            /// <summary>
            /// Log the given text.
            /// </summary>
            /// <param name="message"></param>
            public override void WriteLine(string message)
            {
                _sb.Append(message);
                //CBL Perhaps we need a distinct category for this? Or, determine one?
                _logger.Log(_levelToLogAt, LogCategory.Empty, _sb.ToString(), true);
#if !PRE_4
                _sb.Clear();
#else
                _sb.Length = 0;
#endif
                _mostRecentTrace = message;
            }

            #region private fields

            private readonly LogOutputLevel _levelToLogAt;
            private readonly Logger _logger;
            private readonly StringBuilder _sb;

            #endregion private fields
        }
        #endregion class LogNutTraceListener

        #region AddLoggerToTraceListeners
        /// <summary>
        /// Direct the Trace output of Visual Studio to the given Logger.
        /// Note: NOT implemented yet for any platform other than .NET Framework.
        /// </summary>
        /// <param name="destinationLogger">the Logger to direct Trace output to</param>
        /// <param name="levelToLogAt">which LogOutputLevel to listen for</param>
        public static void AddLoggerToTraceListeners(LogNut.Logger destinationLogger, LogOutputLevel levelToLogAt)
        {
            if (!destinationLogger.IsCatchingTraceOutput)
            {
                var listener = new LogNutTraceListener(logger: destinationLogger, levelToLogAt: levelToLogAt);
                Trace.Listeners.Add(listener);
                destinationLogger.IsCatchingTraceOutput = true;
            }
        }
        #endregion

        #region AddLoggerToPresentationTraceListeners
        /// <summary>
        /// Direct the Trace output of Visual Studio to the given Logger.
        /// Note: NOT implemented yet for any platform other than .NET Framework.
        /// </summary>
        /// <param name="destinationLogger">the Logger to direct Trace output to</param>
        /// <param name="levelToLogAt">the LogOutputLevel to use when logging trace output</param>
        public static void AddLoggerToPresentationTraceListeners(LogNut.Logger destinationLogger, LogOutputLevel levelToLogAt)
        {
            if (!destinationLogger.IsCatchingTraceOutput)
            {
                var listener = new LogNutTraceListener(logger: destinationLogger, levelToLogAt: levelToLogAt);
                PresentationTraceSources.DataBindingSource.Listeners.Add(listener);
                //CBL This is not quite true. Do we want this to be set to catch ALL Trace output?
                destinationLogger.IsCatchingTraceOutput = true;
            }

            // http://www.helixoft.com/blog/archives/20

            //CBL  Shit - I have redundant facilities for this. Need to recode this.

#if !NETFX_CORE
            //try
            //{
            // Create a new Stream that writes to the given Logger.
            //LoggerStream loggerStream = new LoggerStream( destinationLogger );

            // Create a new text writer using the output stream, and add it to the trace listeners.
            //TextWriterTraceListener myTextListener = new TextWriterTraceListener( loggerStream );
            //Trace.AutoFlush = true;
            //Trace.Listeners.Clear();
            //Trace.Listeners.Add( myTextListener );
            //destinationLogger.IsCatchingTraceOutput = true;
            //}
            //catch (Exception)
            //{
            // We don't want to engage in any troubleshooting here -- it's trace output.
            //}
#endif
        }
        #endregion

        // http://stackoverflow.com/questions/2525457/automating-visual-studio-with-envdte
        // http://stackoverflow.com/questions/2651617/is-it-possible-to-programmatically-clear-the-ouput-window-in-visual-studio
        // http://stackoverflow.com/questions/2391473/can-the-visual-studio-debug-output-window-be-programatically-cleared


        #region ClearOutputWindow
        /// <summary>
        /// Clear the Output window-pane of Visual Studio.
        /// Note: Causes a 1-second delay.
        /// Note: NOT implemented yet for any platform other than .NET Framework, AND this does nothing for versions of .NET < version 4.0
        /// </summary>
        public static void ClearOutputWindow()
        {
            // If NEVDTE is not defined, then this method does nothing.
#if ENVDTE
#if !PRE_4
            if (!Debugger.IsAttached)
            {
                return;
            }

#if !NETFX_CORE
            //CBL Need to implement yet for Universal Windows Platform.

            //Application.DoEvents();  // This is for Windows.Forms.
            // This delay to get it to work. Unsure why. See http://stackoverflow.com/questions/2391473/can-the-visual-studio-debug-output-window-be-programatically-cleared
            Thread.Sleep( 1000 );
            // In VS2008 use EnvDTE80.DTE2
#if !PRE_4
            EnvDTE.DTE ide = (EnvDTE.DTE)Marshal.GetActiveObject( "VisualStudio.DTE.10.0" );
#else
            EnvDTE80.DTE2 ide = (EnvDTE80.DTE2)Marshal.GetActiveObject( "VisualStudio.DTE.10.0" );
#endif
            if (ide != null)
            {
                //ide.ExecuteCommand( "Edit.ClearOutputWindow", "" );
                Marshal.ReleaseComObject( ide );
            }

            //            EnvDTE80.DTE2 ide = (EnvDTE80.DTE2)Marshal.GetActiveObject("VisualStudio.DTE.10.0");
            //            ide.ExecuteCommand("Edit.ClearOutputWindow", "");
            //            Marshal.ReleaseComObject(ide);
#endif
#endif
#endif
        }
        #endregion
    }
    #endregion class VisualStudioLib
}
