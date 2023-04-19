using System;


namespace Hurst.LogNut.OutputPipes
{
    /// <summary>
    /// This interface represents a simplified message-queuing library.
    /// NOTE: This is NOT completed yet.  CBL
    /// </summary>
    public interface IMQController
    {
        /// <summary>
        /// Get or set whether to listen for identity-announcement messages.
        /// </summary>
        bool IsListeningForIAsEnabled { get; set; }

#if FALSE
        /// <summary>
        /// Get or set the currently-selected IPv4 address that the user selected for this local host.
        /// </summary>
        //string MyIpV4Address { get; set; }
#endif

        /// <summary>
        /// Listen for IAs on the SocketListenForIA that is bound to the local IP-address.
        /// </summary>
        void ListenForIAs( string destinationIPv4Address );

        /// <summary>
        /// Publish notifications of this program's IP-address.
        /// </summary>
        /// <remarks>
        /// This are transmitted in the form: MyAddressIs:V,255.255.255.255,hostname
        /// where the "V" is the program-symbol, which is "V" for LogNutVUr, or "C" for LogNutControlPanel.
        /// </remarks>
        void Transmit( IdentityAnnouncement identityAnnouncement );

        /// <summary>
        /// Send a log-record via the IPC channel.
        /// </summary>
        /// <param name="myIpAddress">the IP-address of the host upon which this subject-program is running</param>
        /// <param name="stringRepresentationOfLogRecord">the log-record to be sent</param>
        void TransmitLog( string myIpAddress, string stringRepresentationOfLogRecord );

        /// <summary>
        /// Close the IPC-subsystem down and dispose of any applicable resources.
        /// </summary>
        void Close();

        /// <summary>
        /// This event is raised when an announcement of a program's identity-and-location (as in IP-address)
        /// is received.
        /// </summary>
        event EventHandler<IdentityAnnouncementEventArgs> IdentityAnnounced;

        /// <summary>
        /// This event is raised when a log is received via one of the IPC channels.
        /// </summary>
        event EventHandler<LogReceivedEventArgs> LogReceived;
    }
}
