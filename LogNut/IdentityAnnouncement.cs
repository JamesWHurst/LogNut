using System;
using System.Text;
using Hurst.BaseLib;


// Notes:
//   This can probably be moved to a lower-level, not-LogNut-specific library,
//   where strings or something can be used to denote the various LogNut components.

namespace Hurst.LogNut
{
    /// <summary>
    /// This class serves to encapsulate the 'here-I-am' announcment that is transmitted via IPC.
    /// This is transmitted in the form: MyAddressIs:VU,255.255.255.255,hostname
    /// where the "VU" is the program-symbol, such as "VU" for LogNutVUr, or "CP" for LogNutControlPanel.
    /// </summary>
    public class IdentityAnnouncement
    {
        #region constructors

        /// <summary>
        /// default ctor
        /// </summary>
        public IdentityAnnouncement()
        {
            When = DateTime.Now;
        }

        /// <summary>
        /// Return a new instance of an IdentityAnnouncement with the given IPv4 address and hostname.
        /// </summary>
        /// <param name="ipv4Address">the IPv4 IP-address</param>
        /// <param name="machineName">the computer-name</param>
        /// <returns>a new IdentityAnnouncement instance</returns>
        public IdentityAnnouncement( string ipv4Address, string machineName )
        {
            When = DateTime.Now;
            MachineName = machineName;
            IPv4Address = ipv4Address;
        }

        /// <summary>
        /// Return a new IdentityAnnouncement for the LogNut-support-service
        /// (that is, a target-system upon which the LogNut logging service is running).
        /// </summary>
        /// <param name="ipv4Address">the IPv4 IP-address</param>
        /// <param name="machineName">the computer-name</param>
        /// <returns>a new IdentityAnnouncement instance</returns>
        public static IdentityAnnouncement NewLogNutLocation( string ipv4Address, string machineName )
        {
            var newThing = new IdentityAnnouncement( ipv4Address, machineName );
            newThing.ComponentCode = LogNutSupportSystemComponentCode;
            return newThing;
        }

        /// <summary>
        /// Return a new IdentityAnnouncement for the LogNutControlPanel.
        /// </summary>
        /// <param name="ipv4Address">the IPv4 IP-address</param>
        /// <param name="machineName">the computer-name</param>
        /// <returns>a new IdentityAnnouncement instance</returns>
        public static IdentityAnnouncement NewControlPanelLocation( string ipv4Address, string machineName )
        {
#if !PRE_4
            var newThing = new IdentityAnnouncement( ipv4Address: ipv4Address, machineName: machineName );
#else
            var newThing = new IdentityAnnouncement( ipv4Address, machineName );
#endif
            newThing.ComponentCode = LogNutControlPanelComponentCode;
            return newThing;
        }

        /// <summary>
        /// Return a new IdentityAnnouncement for the LogNutVUr.
        /// </summary>
        /// <param name="ipv4Address">the IPv4 IP-address</param>
        /// <param name="machineName">the computer-name</param>
        /// <returns>a new IdentityAnnouncement instance</returns>
        public static IdentityAnnouncement NewVUrLocation( string ipv4Address, string machineName )
        {
            var newThing = new IdentityAnnouncement( ipv4Address, machineName );
            newThing.ComponentCode = LogNutVUrComponentCode;
            return newThing;
        }
        #endregion

        #region public constants

        /// <summary>
        /// The text-field that indicates the LogNutControlPanel
        /// </summary>
        public const string LogNutControlPanelComponentCode = "CP";

        /// <summary>
        /// The text-field that indicates the LogNut-support-service
        /// </summary>
        public const string LogNutSupportSystemComponentCode = "SS";

        /// <summary>
        /// The text-field that indicates the LogNutVUr
        /// </summary>
        public const string LogNutVUrComponentCode = "VU";

        /// <summary>
        /// This is the textual prefix to which subscribers can subscribe
        /// to receive the publish-my-IP-address messages. This includes the colon.
        /// </summary>
        public const string PublishMyAddressPrefix = "MyAddressIs:";

        #endregion public constants

        #region public properties

        /// <summary>
        /// This is either "CP" (for LogNutControlPanel), VU (for LogNutVUr), or SS (Support-Service).
        /// </summary>
        public string ComponentCode { get; set; }

        /// <summary>
        /// Get or set the IPv4 IP-address. If this IdentityAnnouncement
        /// is to signal a local IP-address change, then this holds the newer value.
        /// </summary>
        public string IPv4Address { get; set; }

        /// <summary>
        /// Get whether this pertains to a LogNutControlPanel program.
        /// </summary>
        public bool IsControlPanel
        {
            get { return (ComponentCode == LogNutControlPanelComponentCode); }
        }

        /// <summary>
        /// Get or set whether this IdentityAnnouncement reflects a change of the IP-address for THIS program,
        /// as opposed to that indicated by ComponentCode.
        /// </summary>
        /// <remarks>
        /// This allows this class to also be used to signal a simple change of IP-address for the running program.
        /// </remarks>
        public bool IsForThisProgram { get; set; }

        /// <summary>
        /// Get whether this pertains to a LogNut-support-service.
        /// </summary>
        public bool IsLogNutSupportService
        {
            get { return (ComponentCode == LogNutSupportSystemComponentCode); }
        }

        /// <summary>
        /// Get whether this pertains to a LogNutVUr program.
        /// </summary>
        public bool IsVUr
        {
            get { return (ComponentCode == LogNutVUrComponentCode); }
        }

        /// <summary>
        /// Get or set the computer-name (ie "host-name") that this IdentityAnnouncement pertains to.
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Get or set the previous IPv4 IP-address - applicable only if this object is being used
        /// to signal a local IP-address change. IPv4Address would have the new value, and this has the old value.
        /// </summary>
        public string PreviousIPv4Address { get; set; }

        /// <summary>
        /// Get or set the DateTime associated with this IdentityAnnouncement.
        /// </summary>
        public DateTime When { get; set; }

        #endregion public properties

        #region public methods

        /// <summary>
        /// Add this IdentityAnnouncement to the Windows Registry.
        /// </summary>
        public void AddMyselfToRegistry()
        {
            //LogNutRegistryService.
        }

        /// <summary>
        /// Get a string representing this IdentityAnnouncement in the format
        /// to be used for a message to place into the message-queue.
        /// These are transmitted in the form: MyAddressIs:VU,255.255.255.255,hostname
        /// where the "VU" is the system-component symbol, which is
        ///   "VU" for LogNutVUr,
        ///   "CP" for LogNutControlPanel
        ///   "SS" for LogNut Support Service.
        /// </summary>
        public string AsIaMessage
        {
            get
            {
                string message;
                if (StringLib.HasSomething( this.MachineName ))
                {
                    message = String.Format( "{0}{1},{2},{3}", PublishMyAddressPrefix, ComponentCode, IPv4Address, MachineName );
                }
                else
                {
                    message = String.Format( "{0}{1},{2}", PublishMyAddressPrefix, ComponentCode, IPv4Address );
                }
                return message;
            }
        }

        /// <summary>
        /// Parse the given text into an IdentityAnnouncment object, if possible,
        /// and return true if successful.
        /// </summary>
        /// <param name="text">the text to parse</param>
        /// <param name="ia">a new object yielded by the parsing, or null if not successful</param>
        /// <param name="reason">if the parse fails, this contains a natural-language (English) explanation</param>
        /// <returns>true if the parse is successful</returns>
        public static bool TryParseIntoIa( string text, out IdentityAnnouncement ia, out string reason )
        {
            // Tests:
            // 1. First part is the fixed prefix: PublishMyAddressPrefix, which includes the colon
            // 2. Next 2 chars are one of the component symbols (VU, CP, or SS)
            // 3. Next char is a comma
            // 4. Following that - an IP-address.
            // 5. optionally, that may be followed by a comma and then a machine-name.
            ia = null;
            reason = null;
            bool ok = true;
            if (text == null)
            {
                throw new ArgumentNullException( "text" );
            }
            int len = text.Length;
            if (len < PublishMyAddressPrefix.Length + 11)
            {
                reason = "text is too short";
                return false;
            }
            if (text.StartsWith( IdentityAnnouncement.PublishMyAddressPrefix ))
            {
                string withoutPrefix = text.WithoutAtStart( IdentityAnnouncement.PublishMyAddressPrefix );
                if (StringLib.HasSomething( withoutPrefix ) && withoutPrefix.Length >= 2)
                {
                    string componentCode = withoutPrefix.Substring( 0, 2 );
                    if (componentCode == IdentityAnnouncement.LogNutControlPanelComponentCode ||
                        componentCode == IdentityAnnouncement.LogNutVUrComponentCode ||
                        componentCode == IdentityAnnouncement.LogNutSupportSystemComponentCode)
                    {
                        ia = new IdentityAnnouncement();
                        ia.ComponentCode = componentCode;
                        var parts = withoutPrefix.Split( new[] { ',' } );
                        if (parts.Length < 2)
                        {
                            reason = "text lacks commas-separated IP-address";
                            ok = false;
                        }
                        else
                        {
                            string ipAddress = parts[1];
                            ia.IPv4Address = ipAddress.Trim();
                            if (parts.Length > 2)
                            {
                                ia.MachineName = parts[2].Trim();
                            }
                        }
                    }
                    else
                    {
                        reason = "text has an invalid component-code after the prefix";
                        ok = false;
                    }
                }
                else
                {
                    reason = "text needs a 2-character component-code after the prefix";
                    ok = false;
                }
            }
            else
            {
                reason = "text does not start with correct prefix";
                ok = false;
            }
            return ok;
        }

        /// <summary>
        /// Override the ToString method to provide a more useful display.
        /// </summary>
        /// <returns>a string the denotes the state of this object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder( "IdentityAnnouncement(" );
            bool needsComma = false;
            if (StringLib.HasSomething( ComponentCode ))
            {
                sb.Append( ComponentCode );
                needsComma = true;
            }
            if (StringLib.HasSomething( MachineName ))
            {
                if (needsComma)
                {
                    sb.Append( "," );
                }
                needsComma = true;
                sb.Append( "MachineName=" ).Append( MachineName );
            }
            if (needsComma)
            {
                sb.Append( "," );
            }
            sb.Append( "IPv4Address=" ).Append( IPv4Address ).Append( ")" );
            return sb.ToString();
        }

        #endregion public methods
    }
}
