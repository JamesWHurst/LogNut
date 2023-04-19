
==========================================================================

Std File-Output Log Format:
2011-4-15 23:51:10.123[Host/Subjectprogram:1.0.3,Ann|7;Logger1\Info,Cat*v]

==========================================================================

Compile-time pragma ENVDTE must be defined if you want to include the facilities of class VisualStudioLib.


Q: Do I want Logger.IsEnabled to override Config.LowestLevelThatIsEnabled ?


2019-03-29: I am thinking that loggers should NOT have the ability to set their own output-file or output-path, nor their output format.
            Managing that shit on a per-logger and per-LogManager basis just gets too complex.
            So today I am removing 
            Logger.FileOutputFilename_EffectiveValue and Logger.FileOutputFilename_Override
            _isFirstFileOutput
            _fileOutputFormatType_override
            FileOutputFormatType_Override
            FileOutputFormatType_EffectiveValue
            FileOutputPath_EffectiveValue
            FileOutputRolloverMode_Override
            FileOutputRolloverMode_EffectiveValue
            IsFileOutputEnabled_Override
            IsFileOutputEnabled_EffectiveValue
            OverrideOfIsFileOutputSpreadsheetCompatible
        Also, why have a separate property for controlling whether to show the logger-name? I can just set that to an empty string if need be.
            IsToShowLoggerName_Override
            IsToShowLoggerName_EffectiveValue


Events that signal logging is happening:
I'm thinking that providing two distinct events, ExceptionWasLogged and AnythingWasLogged, is warranted
because logging can happen very quickly, and if an app has an exception-counter it may not want to be handling an event
for every single log-output.

The calling-order for log file output, presently:

Synchronous:
Logger.LogInfo
  Logger.Log(LogLevel,string)
    LogManager.QueueOrSendToLog(LogRecord)
      LogManager.Send
        LogManager.OutputToFile
        SendOutputToPipes

Asynchronous:
Logger.LogInfo
  Logger.Log
    LogManager.QueueOrSendToLog(LogRecord)
	  LogManager.SendViaTransmissionTask
	    adds it to QueueOfLogRecordsToSend
	    Fires up LogManager.SendLogs if it is not running already
		SendLogs
          LogManager.Send
            LogManager.OutputToFile
            SendOutputToPipes

Using the service (served by LognutServiceApplic at present):
Logger.LogInfo
  Logger.Log
    LogManager.QueueOrSendToLog(LogRecord)
      LogManager.OutputToService( logSendRequest )
        which creates a task to run DoOutputToService


I am thinking that perhaps, when using the WindowsService, that the logging should be synchronous?
Q: when we have the crash, would the task that does the logging within Luviva (sending it out to the service) crash also,
   or does it continue on to finish the log-output?


I decided to remove the ability to assign a distinct output-folder to individual loggers. That was just creating too much complication, 
for what seems like a feature of very marginal utility.

I also removed Logger.IsToCreateNewOutputFileUponStartup_Override
and Logger.IsToCreateNewOutputFileUponStartup_EffectiveValue,
- these seem excessive.


I found that my logging was causing Luviva (and most likely GTSetup) to crash!
I'd been using synchronous logging, opening/closing the file each time.
Even doing it asynchronously caused a crash at the end of Calibration.

Running TestMultipleLoggersWritingFile_CorrectResult,
asynchronously, 25 tasks, 1000 log-outputs,
  with _queueLockObject, 100ms per TryTake
    Took 
	1.  2.34 seconds, and after Clear: 72.73 seconds. File is 1,907KB
	2.  0.33, 91.61
	3.  0.74, 95.80 seconds
  without _queueLockObject,
	4.  0.0430, 75.124 seconds
  50ms per TryTake:
   0.038, 124.2 (slower!)
   0.038, 82.92
   200ms per TryTake:
   0.036, 80.78

   without opening-closing the file every time (Using my method in NutUtil)..
   1000, 200ms per TryTake
   1. 0.05 seconds, 3.655 seconds!!!
   2. 0.045, 7.3449
   3. 0.040, 5.307

   *Clearly, this is vastly faster!!!
   100ms per TryTake:
   1. 0.0389, 4.020
   2. 0.036, 4.61
   1000ms per TryTake
   0.03889, 3.96

	But see this:
	https://designingefficientsoftware.wordpress.com/2011/03/03/efficient-file-io-from-csharp/


	But.. what if the flash-drive goes offline during this? !!!


*I'm going to repurpose FileOutputFolder2 to use it for flash-drives. 
The essential factor is that these may be unreliable, but is the preferred destination.
It checks that path for existence with every log-output, but if it fails - then on the next log-output it copies the log files over.
Rename SetFileOutputFolder2 to SetFileOutputToPreferRemovableDrive.

Operation: When the drive is not available, log output goes to FileOutputFolder.
When that drive becomes available once again, log output goes there but, also, the logs that had been diverted to FileOutputFolder
are also moved over to the removable drive, with the modified filename MyLogOutput_DIVERTED.txt


Plugin Architecture:

How do I achieve a true plug-in architecture,
such that if a user adds a new IOutputPipe,
Configuration can still read a config-file and set it's properties?
I want them to be able to define and add a pipe
without changing the original LogNut code.
Access it via a textual name.

  User defines a new IOutputPipe, including a name (just text), and a configuration-class.
  Call a method on Configuration or LogManager or whatever, to attach it.

  To attach it in code:
  LogManager.AttachPipe( myPipe );

  To turn it on:
  LogManager.Config.GetConfiguratorFor<MyPipe>("MyPipe").Enable();

  To load config-settings from a file:
  Configuration.ReadFromFile("thatfile")
  and
  Configuration.WriteToFile
    automatically traverses the settings for the attached pipe.


Q: Do we really want to provide a distinct rollover-mode setting for each individual logger object?
A: This may be excessive. However, I can see where we may indeed want everything to start fresh each time, and yet,
   for some specific logger -- to preserve the previous logs.


To Do:
  *Consider making something implement IDisposable, changing Clear to Dispose, or something - such that LogManager.Clear is called automatically
  within each unit-test, and a developer may easily cause it to be called upon exit from er program.
  Also, make Clear (or Dispose) write out any remaining log messages and close any open files.
  Should I create a separate LoggingContext class for this purpose?
  
  *For asynchronous logging, we should probably consider writing-out and closing the log-file whenever the list of log-records is fully expended.

  What about clearing or disposing of LogManager and the Loggers when the program terminates?

  What should happen when we log a bunch of shit asynchronously, and then switch over to synchronous logging and log something?

  Create a sample-applic that is a Windows Service, and ensure this works in that scenario. Also update the documentation to include that as a how-to.

Compilation-pragmas:
  define PRE_4 if this is to be compiled for versions of .NET earlier than 4.0 (this is, for .NET 3.5)
  define PRE_5 if this is for .NET earlier than 4.51 (eg, for .NET 4.0)
  define PRE_6 if this is for .NET 4.51 or 4.52

  define INCLUDE_JSON for this project if you want to produce JSON output.
  Note: To use the YAML serializer, add a reference to YamlDotNet.dll to this project. That assembly should be withiin the VendorLibs folder.
  define USE_IONIC_ZIP_COMPRESSION in this project (used within in NutUtil.cs) if you want to use ZIP-compression using the Ionic.Zip.dll library (which is in the VendorLibs folder).


**Perhaps I should define it as being a part of the recommended usage of LogNut, to handle the LoggingFaultOccurred event within your application.
That way, the user decides how best to reliably alert the user when the logging fails.
When using a message-box, the caption must identify that it is a logging fault, in addition to identifying the vendor-name and product-name.


If I am unable to log - what should I do?
  Write to console
  Raise an event. x
  Throw an exception?  -but sometimes I may NOT want to do that (control that with a setting)
  Try to log to other folders:
    Log to dir of executable,
	user's folder (the default),
	application-data folder for all users
	public documents folder - in that order.
  Display a messagebox, with a timeout? <- this is the subject-program's responsibility.
  Pop up a MessageBox? with no timeout? <- ditto


Be sure to test this for every possible execution-branch that "Log.." may follow!
For example, we have Logger.Log(level, object) and Logger.Log(level, formatj, args).


Sequence:

Logger.LogInfo
  Log( LogLevel.Info, ..)
    Logger.QueueOrSendToLog
      if (IsAsynchronous)
        LogManager.SendViaTransmissionTask
	      Starts a new Task, which executes SendLogs
          SendLogs <- throwing exception there, fails to raise the Logging-fault event.
            Send  
      else
        LogManager.Send  <- This is way too large! And, can we move it to class Logger?


***For every Task, throw an exception and verify that it gets handled safely,
   and that fault is logged some-fuckn-where!

*LogManager.Clear, waits on the _logTransmissionTask.



Behavior when the output-file cannot be written to:

  It is a different kind of rollover scenario, when it cannot write to the file because it is locked.
  We do at least want the new file to be within the same folder, and for it to be obvious via the name as to what happened.
  Existing rollover pattern:
  Delete backup 1 (the oldest), and renumber the rest to be 1 less.

E.G.,

1st rollover:
  Log.txt  -> rename to Log(1).txt
  create a new Log.txt

2nd rollover:

  Log.txt -> rename to Log(2).txt
  create a new Log.txt
  Now we have  Log(1).txt, Log(2).txt, Log.txt where Log(1).txt is the oldest.

4th rollover (where the limit is 4):

  Log.txt -> rename to Log(4).txt
  create a new Log.txt
  Now we have  Log(1).txt, Log(2).txt, Log(3).txt, Log(4).txt, Log.txt where Log(1).txt is the oldest.

5th rollover (where the limit is 4):

  Log.txt -> rename to Log(5).txt
  delete Log(1).txt
  create a new Log.txt
  Now we have  Log(2).txt, Log(3).txt, Log(4).txt, Log(5), Log.txt where Log(2).txt is the oldest.

*The number-suffices within parentheses, change. The advantage is that once a rolled-over file
is created, it does not get renamed - only deleted when the rollover-limit is reached.
I think that my previous strategy of renaming all the files with every rollover,
to keep the suffices at (1)..(4), could lead to confusion, plus the change in suffices to (2)..(5)
helps make it clear that a change has occurred. and Log(N).txt is always Log(N).txt

Old plan:

    Log.txt     -> rename to Log(3).txt
    Log(1).txt  -> delete
    Log(2).txt  -> rename to Log(1).txt
    Log(3).txt  -> rename to Log(2).txt
  
    file-is-not-writable pattern:
    Log.txt     -> write instead to Log_redirected.txt


Perhaps the default file-output location should be the program's own folder, instead of the user-documents folder?
Or we could have a fall-back system, whereby we first try to write to the app's own folder, and then if that fails - try the user-documents folder. ?


Dev Note: properties that serve to override that of another object, should have a distinct naming convention.
For example, LogManager.Config.IsFileOutputDisabled can be overridden by a specific Logger. So that property on the logger should NOT have the same name.
How about..
  IsFileOutputOn
  IsFileOutputDisabledOverride
  IsFileOutputOnOverride
  OverrideOfFileOutputOn  (it is now NOT an "enable" nor a "disable")

LogNut does not require external configuration-files or anything other than to reference LogNut.dll and BaseLib.dll

To use LogNut, as an minimum you can do this, to just accept the default settings:


using LogNut;
..
  var myLogger = LogManager.GetLogger();
  myLogger.LogInfo("This is some information I want to log.");



  By default, this writes to a log file at My Documents\Logs\{program-name}_Log.txt

  To change this, as for example to write logs to the folder C:\LuVivaLog and to the file "MyProgram_UI.log",
  you do this before *any* code does any logging :


  LogManager.Config.FileOutputConfig
      .SetFileOutputFolder(@"C:\LuVivaLog")
      .SetFileOutputFilename("MyProgram_UI.log");


See the LogNut library for more information on the various configuration options.



Our standard practice within Windows Forms applications at Guided Therapeutics, can be to provide
this propery on the Program class :


 using GTLibWinForms;
 ...
       #region Logger
        /// <summary>
        /// Get the Logger that will be used by this application.
        /// By default, the log output goes into the folder: C:\Users\{Username}\My Documents\Logs
        /// </summary>
        public static Logger Logger
        {
            get { return GTWinForms.Logger; }
        }
        #endregion


In this way you can access your Logger anywhere within your project code, with simply :
  Program.Logger

Thus to log an error:
  Program.Logger.LogError("Something is amiss!");


To provide a uniform means of handling exceptions, you may provide this within your Program class
(in this example, for the program DeviceControlPanel) :

        #region NotifyOfError
        /// <summary>
        /// Handle the given Exception. This combines logging it, and showing it to the user in a display-box.
        /// </summary>
        /// <param name="exception">an Exception to log and to notify the user of</param>
        /// <param name="additionalInformation">(optional) any additional information to describe the context of this error</param>
        public static void NotifyOfError(Exception exception, string additionalInformation = null)
        {
            GTWinForms.NotifyOfError("DeviceControlPanel", Logger, exception, additionalInformation);
        }
        #endregion


and then this is how you would handle exceptions :

try
{
    ....
}
catch (Exception x)
{
    Program.NotifyOfError(x);
}


That both logs the exception, and puts up a message-box to notify the user.
Note that this automatically uses your Logger that you have defined within your Program.Logger property.

Note: To output log-records in JSON format, define the compiler-pragma INCLUDE_JSON.

======================================================================
I'll want to test this with a destination drive for the file-output
that is full, or with other characteristics that could create difficulty.

Some thoughts, changing the API to keep it simple...

//CBL  LogFatal(null), comes here. Do I really want all that contextual information?  I'm thinking not!
// Possibilities:
// Just only have LogFatalWithContext, for both exceptions and w/o exception
// LogException, which is always at level Error
//   -but, some exceptions might not be thought of as errors, and I may not want to flag them as such. ?
//   -on the other hand, I can handle those separately.
//   and, it might be too confusing to have to choose how to log an exception: LogError, LogFatal, etc.
// Perhaps I have TOO MANY OVERLOADS, too many ways to log. ?
// Answer: Use ..WithContext, but also provide this for < 4.5, so that one can gracefully go back.
//         Provide LogException, to handle all of the exception cases. And this one does include Context also.

And I'm questioning whether it's worthwhile to even have that ISimpleLogger interface.


================
The level values have an implicit order, which only matters in a few
rare instances. That ordering is:

 0  Trace
 1  Debug
 2  Info
 3  Warn
 4  Error
 5  Fatal


 You can eliminate Trace-level logging from your application by setting LogManager.Config.LowestLevelThatIsEnabled .
 This disables logging that is at the log-level "Trace".
 However, to maximize the efficiency of your code when you want to eliminate Trace logging,
 ensure the TRACE conditional-compilation symbol is NOT defined for your build.
 The LogTrace methods are conditionally-compiled with that symbol, so without it -- even the calls to those log methods
 do not get injected into your .NET-assembly code.


