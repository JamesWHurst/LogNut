using System;
using System.Text;


namespace Hurst.LogNut
{
    /// <summary>
    /// This EventArgs subclass adds information denoting the reception of a log-record.
    /// </summary>
    public class LogReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new <c>LogReceivedEventArgs</c> instance
        /// given a LogRecord and a flag indicating whether this is in testing.
        /// </summary>
        /// <param name="record">the log-record that is the subject of this event</param>
        /// <param name="howReceived">this indicates how this log-record was sent to us</param>
        /// <param name="isTest">this indicates whether we're testing this</param>
        public LogReceivedEventArgs(LogRecord record, LogTransmissionMethod howReceived, bool isTest)
        {
            this.Record = record;
            this.HowReceived = howReceived;
            this.IsTest = isTest;
        }

         /// <summary>
        /// Create a new <c>LogReceivedEventArgs</c> instance given a LogRecord.
        /// </summary>
        /// <param name="record">the log-record that is the subject of this event</param>
        /// <param name="howReceived">this indicates how this log-record was sent to us</param>
        /// <remarks>
        /// The <see cref="IsTest"/> property is set to <c>false</c>.
        /// </remarks>
        public LogReceivedEventArgs(LogRecord record, LogTransmissionMethod howReceived)
        {
            this.Record = record;
            this.HowReceived = howReceived;
            this.IsTest = false;
        }

         /// <summary>
        /// Get or set the LogRecord that was detected, that is the subject of this event.
        /// </summary>
        public LogRecord Record { get; set; }

       /// <summary>
        /// Get or set how this log-record was received.
        /// </summary>
        public LogTransmissionMethod HowReceived { get; set; }

        /// <summary>
        /// Get or set whether this program is being tested.
        /// </summary>
        public bool IsTest { get; set; }

        /// <summary>
        /// Override the ToString method to provide a more informative denotation of the state of this object.
        /// </summary>
        /// <returns>a string that concisely denotes the properties</returns>
        public override string ToString()
        {
            var sb = new StringBuilder("LogReceivedEventArgs(");
            if (this.IsTest)
            {
                sb.Append("IsTest,");
            }
            sb.Append("HowReceived=");
            sb.Append(this.HowReceived);
            sb.Append(")");
            return sb.ToString();
        }
    }
}
