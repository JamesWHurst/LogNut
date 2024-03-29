LogNut
======

LogNut is a logging library, together with a set of desktop-applications that serve as a reader and control-panel, as well as an optional online service that comprises the LogNut logging system. LogNut's targeted platforms presently include the Windows and Linux operating systems, and the programming-language support includes C#, F#, Java, and Swift. There are tentative plans to add direct support for JavaScript and C++.

The work toward Release 1.0 is currently on implementing communication with the central server using Azure Service Bus and SignalR.

In the meantime..

LogNut can be used at this point on C# applications.


Features/Advantages of LogNut:
* You can start using this super-easily with only one line of code and zero configuration-files
* A simplified, well-documented API with C# examples to make this the easiest logging library to use
* Great care has been taken to provide an API that is clear, fluid, symmetrical and consistent
* For resource-constrained or embedded systems especially, can output to ETW for ultra-efficient operation.
* File Output includes a comprehensive selection of output-field information that is fully configurable
* Flexible rollover, file-management and formatting options (optional -- not required)
* File Output may optionally be formatted for input into a spreadsheet
* Other output options include Windows Event Log, IPC to the server, email and text-alerts
* Uniquely, when developing WPF desktop application, LogNut works well in design-time (as in Cider or Blend)
* Well-integrated to take advantage of Visual Studio IntelliSense on the .NET platforms
* Flexible configuration options including code-only, separate configuration-files, Registry and cloud settings
* Support for your unit-tests: you can verify that a given text was logged by your code
* Supports versions of Windows back to XP, with .NET Framework versions back to 3.5
* On Windows, can make use of a Windows Service to provide asynchronous logging for your application
* Support for logging of events or values in a form convenient for statistical analysis or charting
* Video how-to tutorials for quickly putting LogNut to use, to follow shortly
* (TBD) With the appropriate server-subscription, you can receive realtime log output on your Android phone or tablet


Original Author: James Witt Hurst

