using System;
using System.Text;
using Hurst.BaseLib;


namespace Hurst.LogNut
{
    /// <summary>
    /// This EventArgs subclass adds information pertaining to an announcement of a component-program's
    /// identity (mainly, the IP-address) - as encapsulated within an IdentityAnnouncement object.
    /// </summary>
    public class IdentityAnnouncementEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new instance of a IdentityAnnouncementEventArgs that signals
        /// information regarding a program's IP-address.
        /// </summary>
        /// <param name="newIdentity">the new IdentityAnnouncement which reflects new information that has just been received</param>
        public IdentityAnnouncementEventArgs( IdentityAnnouncement newIdentity )
        {
            Identity = newIdentity;
        }

        /// <summary>
        /// Given the encoded text, create a new instance of a IdentityAnnouncementEventArgs that signals
        /// information regarding a program's IP-address.
        /// </summary>
        /// <param name="textThatEncodesIt">the text that encodes the new IdentityAnnouncement</param>
        public IdentityAnnouncementEventArgs( string textThatEncodesIt )
        {
            if (textThatEncodesIt == null)
            {
                throw new ArgumentNullException( "textThatEncodesIt" );
            }
            // Parse the text into a new IdentityAnnouncement object..
            string reason;
            IdentityAnnouncement ia;
            if (IdentityAnnouncement.TryParseIntoIa( textThatEncodesIt, out ia, out reason ))
            {
                this.Identity = ia;
            }
            else
            {
                string s = "Invalid text-encoding for IA: " + StringLib.AsString( textThatEncodesIt ) + ", " + reason;
                throw new ArgumentException( s, textThatEncodesIt );
            }
        }

        /// <summary>
        /// Create a new instance of a IdentityAnnouncementEventArgs that signals
        /// information regarding a program's IP-address.
        /// </summary>
        /// <param name="newIdentity">the new IdentityAnnouncement which reflects new information that has just been received</param>
        /// <param name="previousAddress">what the address-value was before this change</param>
        public IdentityAnnouncementEventArgs( IdentityAnnouncement newIdentity, string previousAddress )
        {
            Identity = newIdentity;
            //CBL  Is PreviousAddress needed?  The IdentityAnnouncement object already has a PreviousIPv4Address property.
            PreviousValue = previousAddress;
        }

        /// <summary>
        /// Get or set the IdentityAnnouncement that contains the new IP-address or related information.
        /// </summary>
        public IdentityAnnouncement Identity { get; set; }

        /// <summary>
        /// Get or set the old value of the IP-address - that which pertained just before the change that this event reflects.
        /// </summary>
        public string PreviousValue { get; set; }

        /// <summary>
        /// Override the ToString method in order to provide a better display.
        /// </summary>
        /// <returns>a string the denotes the state of this object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder( "IdentityAnnouncementEventArgs(" );
            sb.Append( StringLib.AsString( this.Identity ) );
            if (StringLib.HasSomething( this.PreviousValue ))
            {
                sb.Append( ",PreviousValue=" ).Append( this.PreviousValue );
            }
            sb.Append( ")" );
            return sb.ToString();
        }
    }
}
