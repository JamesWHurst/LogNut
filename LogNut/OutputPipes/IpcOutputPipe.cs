using System;
using System.Text;
using Hurst.BaseLib;


namespace Hurst.LogNut.OutputPipes
{
    /// <summary>
    /// This class contains configuration-settings for LogNut
    /// that pertain to Inter-Program-Communication (IPC).
    /// NOTE: This is NOT completed yet.  CBL
    /// </summary>
    public class IpcOutputPipe : IOutputPipe
    {
        #region public properties

        #region IsEnabled
        /// <summary>
        /// Get or set whether output to this OutputPipe is enabled. This defaults to false.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Enable logging output to this output.
        /// Calling this is the same as setting the IsEnabled property to true.
        /// </summary>
        /// <remarks>
        /// This duplicates the function of the IsEnabled property setter, in order to provide a fluent API.
        /// </remarks>
        /// <returns>a reference to this IpcOutputPipe object, such that further method calls may be chained</returns>
        public IpcOutputPipe Enable()
        {
            this.IsEnabled = true;
            return this;
        }

        /// <summary>
        /// Turn off logging output to this output.
        /// Calling this is the same as setting the IsEnabled property to false.
        /// </summary>
        /// <remarks>
        /// This duplicates the function of the IsEnabled property setter, in order to provide a fluent API.
        /// </remarks>
        /// <returns>a reference to this IpcOutputPipe object, such that further method calls may be chained</returns>
        public IpcOutputPipe Disable()
        {
            this.IsEnabled = false;
            return this;
        }
        #endregion

        #region IsFailing
        /// <summary>
        /// Get or set whether output to this OutputPipe is has been failing.
        /// This is used to avoid repeatedly trying to send a log over a non-working channel.
        /// </summary>
        public bool IsFailing { get; set; }
        #endregion

        #region Name
        /// <summary>
        /// Get the Name - the string-literal that uniquely identifies this class of IOutputPipe.
        /// </summary>
        public string Name
        {
            get { return "IPC"; }
        }
        #endregion

        #region IpcScope
        /// <summary>
        /// Get or set the effective value to use for IpcScope.
        /// </summary>
        public IpcScope IpcScope
        {
            get
            {
                if (_ipcScope.HasValue)
                {
                    return _ipcScope.Value;
                }
                else
                {
                    return _ipcScope_DefaultValue;
                }
            }
            set { _ipcScope = value; }
        }

        /// <summary>
        /// Get or set the explicitly-set value for this property, with null indicating no explicit value.
        /// </summary>
        public IpcScope? IpcScope_ExplicitValue
        {
            get { return _ipcScope; }
            set
            {
                if (value != _ipcScope)
                {
                    _ipcScope = value;
                    //if (LogConfig.IsUsingRegistry)
                    //{
                    //    LogNutRegistryService.The.SetIpcScope(_ipcScope, null, null);
                    //}
                }
            }
        }

        /// <summary>
        /// Get or set the default value for this property.
        /// </summary>
        public IpcScope IpcScopeDefaultValue
        {
            get { return _ipcScope_DefaultValue; }
            set { _ipcScope_DefaultValue = value; }
        }

        /// <summary>
        /// Set the effective value to use for IpcScope, which determines the mode of communication when IPC is used.
        /// This is the same as setting the IpcScope property.
        /// </summary>
        /// <returns>a reference to this IpcOutputPipe object so that methods may be chained together</returns>
        public IpcOutputPipe SetIpcScope( IpcScope? nullableIpcScopeValue )
        {
            _ipcScope = nullableIpcScopeValue;
            return this;
        }
        #endregion

        #region IpcType
        /// <summary>
        /// Get or set the effective value to use for IpcType, which determines the mode of communication when IPC is used.
        /// </summary>
        public IpcType IpcType
        {
            get
            {
                if (_ipcType.HasValue)
                {
                    return _ipcType.Value;
                }
                else
                {
                    return _ipcType_DefaultValue;
                }
            }
            set { _ipcType = value; }
        }

        /// <summary>
        /// Get or set the explicitly-set value for this property, with null indicating no explicit value.
        /// </summary>
        public IpcType? IpcType_ExplicitValue
        {
            get { return _ipcType; }
            set
            {
                if (value != _ipcType)
                {
                    _ipcType = value;
                    //if (LogConfig.IsUsingRegistry)
                    //{
                    //    LogNutRegistryService.The.SetIpcType(_ipcType, null, null);
                    //}
                }
            }
        }

        /// <summary>
        /// Get or set the default value for this property.
        /// </summary>
        public IpcType IpcTypeDefaultValue
        {
            get { return _ipcType_DefaultValue; }
            set { _ipcType_DefaultValue = value; }
        }

        /// <summary>
        /// Set the effective value to use for IpcType, which determines the mode of communication when IPC is used.
        /// This is the same as setting the IpcType property.
        /// </summary>
        /// <returns>a reference to this IpcOutputPipe object so that methods may be chained together</returns>
        public IpcOutputPipe SetIpcType( IpcType? nullableIpcTypeValue )
        {
            _ipcType = nullableIpcTypeValue;
            return this;
        }
        #endregion

        #region ControlPanelHostIPv4Address
        /// <summary>
        /// Get or set the effective value of the
        /// IPv4 address that the LogNutControlPanel desktop-application is executing on, if applicable.
        /// </summary>
        public string ControlPanelHostIPv4Address
        {
            get
            {
                // If no value has been explicitly set, return the default value.
                if (StringLib.HasSomething( _controlPanelIPv4Address ))
                {
                    return _controlPanelIPv4Address;
                }
                else
                {
                    return _controlPanelIPv4Address_DefaultValue;
                }
            }
            set
            {
                _controlPanelIPv4Address = value;
                //                LogNutRegistryService.The.SetVUrHostIPv4Address(value, null, null);
            }
        }

        /// <summary>
        /// Get or set the explicitly-set value for this property, with null indicating no explicit value.
        /// </summary>
        public string ControlPanelIPv4Address_ExplicitValue
        {
            get { return _controlPanelIPv4Address; }
            set { _controlPanelIPv4Address = value; }
        }

        /// <summary>
        /// Get or set the default value for this property.
        /// </summary>
        public string ControlPanelIPv4Address_DefaultValue
        {
            get { return _controlPanelIPv4Address_DefaultValue; }
            set { _controlPanelIPv4Address_DefaultValue = value; }
        }

        /// <summary>
        /// Set the effective value of the
        /// IPv4 address that the LogNutControlPanel desktop-application is executing on, if applicable.
        /// This is the same as setting the ControlPanelHostIPv4Address property.
        /// </summary>
        /// <param name="ipAddressTextValue">the IPv4 address to set it to, as a string</param>
        /// <returns>a reference to this IpcOutputPipe object, such that further method calls may be chained</returns>
        public IpcOutputPipe SetControlPanelIPv4Address( string ipAddressTextValue )
        {
            ControlPanelHostIPv4Address = ipAddressTextValue;
            return this;
        }
        #endregion ControlPanelHostIPv4Address

        #region IpcServerIdentifier
        /// <summary>
        /// Get or set the identification (whether domain-name or IP-address)
        /// used for the LogNut IPC server. Default is lognut.designforge.com
        /// </summary>
        public string IpcServerIdentifier
        {
            get
            {
                if (_ipcServerIdentifier != null)
                {
                    return _ipcServerIdentifier;
                }
                return _ipcServerIdentifier_DefaultValue;
            }
            set
            {
                _ipcServerIdentifier = value;
            }
        }

        /// <summary>
        /// Get or set the explicitly-set value for this property, with null indicating no explicit value.
        /// </summary>
        public string IpcServerIdentifier_ExplicitValue
        {
            get { return _ipcServerIdentifier; }
            set { _ipcServerIdentifier = value; }
        }

        /// <summary>
        /// Get or set the default value for this property.
        /// </summary>
        public string IpcServerIdentifier_DefaultValue
        {
            get { return _ipcServerIdentifier_DefaultValue; }
            set { _ipcServerIdentifier_DefaultValue = value; }
        }

        /// <summary>
        /// Set the effective value of the
        /// IPv4 address that the LogNut server is executing on, if applicable.
        /// This is the same as setting the IpcServerIdentifier property.
        /// </summary>
        /// <param name="ipAddressTextValue">the IPv4 address to set it to, as a string</param>
        /// <returns>a reference to this IpcOutputPipe object, such that further method calls may be chained</returns>
        public IpcOutputPipe SetLogNutServerIdentifier( string ipAddressTextValue )
        {
            IpcServerIdentifier = ipAddressTextValue;
            return this;
        }
        #endregion

        #region LogNutSupportServiceIPv4Address
        /// <summary>
        /// Get or set the effective value of the
        /// IPv4 address that the LogNutSupportService is executing on, if applicable.
        /// </summary>
        public string LogNutSupportServiceIPv4Address
        {
            get
            {
                // If no value has been explicitly set, return the default value.
                if (StringLib.HasSomething( _lognutSupportServiceIPv4Address ))
                {
                    return _lognutSupportServiceIPv4Address;
                }
                else
                {
                    return _lognutSupportServiceIPv4Address_DefaultValue;
                }
            }
            set
            {
                _lognutSupportServiceIPv4Address = value;
                //CBL
                //LogNutRegistryService.The.Lo SetLastKnownSsIPv4Address(value);
            }
        }

        /// <summary>
        /// Get or set the explicitly-set value for this property, with null indicating no explicit value.
        /// </summary>
        public string LogNutSupportServiceIPv4Address_ExplicitValue
        {
            get { return _lognutSupportServiceIPv4Address; }
            set { _lognutSupportServiceIPv4Address = value; }
        }

        /// <summary>
        /// Get or set the default value for this property.
        /// </summary>
        public string LogNutSupportServiceIPv4Address_DefaultValue
        {
            get { return _lognutSupportServiceIPv4Address_DefaultValue; }
            set { _lognutSupportServiceIPv4Address_DefaultValue = value; }
        }

        /// <summary>
        /// Set the effective value of the
        /// IPv4 address that the LogNutSupportService is executing on, if applicable.
        /// This is the same as setting the LogNutSupportService property.
        /// </summary>
        /// <param name="ipAddressTextValue">the IPv4 address to set it to, as a string</param>
        /// <returns>a reference to this IpcOutputPipe object, such that further method calls may be chained</returns>
        public IpcOutputPipe SetLogNutSupportServiceIPv4Address( string ipAddressTextValue )
        {
            LogNutSupportServiceIPv4Address = ipAddressTextValue;
            return this;
        }
        #endregion LogNutSupportServiceIPv4Address

        #region LogRecordFormatter
        /// <summary>
        /// Get the formatter used to generate the log-records.
        /// Leave this null unless you wish to override it.
        /// </summary>
        public ILogRecordFormatter LogRecordFormatter
        {
            get
            {
                return LogManager.LogRecordFormatter;
            }
        }
        #endregion

        #region MQController
        /// <summary>
        /// Get or set the controller object that provides our message-queuing capabilities.
        /// </summary>
        /// <remarks>
        /// This is intended to be the place to put code that decides which message-queuing implementation
        /// to use.
        /// </remarks>>
        public IMQController MQController
        {
            get
            {
                //CBL  The assignment of the IMQController needs to be done somewhere.
                if (_mqController == null)
                {
                    //                   _mqController = new MQController();
                }
                return _mqController;
            }
            set
            {
                _mqController = value;
            }
        }
        #endregion

        #region MyIPv4Address
        /// <summary>
        /// Get or set the IPv4 address to use for this local machine.
        /// </summary>
        public string MyIPv4Address
        {
            get
            {
                // If no value has been explicitly set, return the default value.
                if (StringLib.HasSomething( _myIPv4Address ))
                {
                    return _myIPv4Address;
                }
                else
                {
                    return _myIPv4Address_DefaultValue;
                }
            }
            set
            {
                _myIPv4Address = value;
            }
        }

        /// <summary>
        /// Get or set the explicitly-set value for this property, with null indicating no explicit value.
        /// </summary>
        public string MyIPv4Address_ExplicitValue
        {
            get { return _myIPv4Address; }
            set { _myIPv4Address = value; }
        }

        /// <summary>
        /// Get or set the default value for this property.
        /// </summary>
        public string MyIPv4Address_DefaultValue
        {
            get { return _myIPv4Address_DefaultValue; }
            set { _myIPv4Address_DefaultValue = value; }
        }
        #endregion

        #region PortNumberForMqWithLocalHost
        /// <summary>
        /// Get or set the TCP Port Number to use for Message-Queuing when the other end is on the same host.
        /// </summary>
        public int PortNumberForMqWithLocalHost
        {
            get
            {
                return _portNumberForMqWithLocalHost;
            }
            set
            {
                _portNumberForMqWithLocalHost = value;
            }
        }

        /// <summary>
        /// Get or set the default value of the TCP Port Number to use for Message-Queuing when the other end is on the same host.
        /// </summary>
        public static int PortNumberForMqWithLocalHost_DefaultValue
        {
            get { return _portNumberForMqWithLocalHost_DefaultValue; }
        }
        #endregion

        #region PortNumberForMqWithLan
        /// <summary>
        /// Get or set the TCP Port Number to use for Message-Queuing when the other end is on the Local Area Network (LAN).
        /// </summary>
        public int PortNumberForMqWithLan
        {
            get
            {
                return _portNumberForMqWithLan;
            }
            set
            {
                _portNumberForMqWithLan = value;
            }
        }

        /// <summary>
        /// Get or set the default value of the TCP Port Number to use for Message-Queuing when the other end is on the LAN.
        /// </summary>
        public static int PortNumberForMqWithLan_DefaultValue
        {
            get { return _portNumberForMqWithLan_DefaultValue; }
        }
        #endregion

        #region PortNumberForMqServer
        /// <summary>
        /// Get or set the TCP Port Number to use for Message-Queuing when the other end is on the Local Area Network (LAN).
        /// </summary>
        public int PortNumberForMqServer
        {
            get
            {
                return _portNumberForMqServer;
            }
            set
            {
                _portNumberForMqServer = value;
            }
        }

        private int _portNumberForMqServer = 60000;

        #endregion

#if FALSE
        #region PortNumberForLogOutputToLan
        /// <summary>
        /// Get or set the TCP Port Number to use for IPC.
        /// </summary>
        public int PortNumberForLogOutputToLan
        {
            get
            {
                // If no value has been explicitly set, return the default value.
                if (_portNumberForLogOutputToLan.HasValue)
                {
                    return _portNumberForLogOutputToLan.Value;
                }
                else
                {
                    return _portNumberForLogOutputToLan_DefaultValue;
                }
            }
            set
            {
                _portNumberForLogOutputToLan = value;
            }
        }

        /// <summary>
        /// Get or set the explicitly-set value for this property, with null indicating no explicit value.
        /// </summary>
        public int? PortNumberForLogOutputToLan_ExplicitValue
        {
            get { return _portNumberForLogOutputToLan; }
            set { _portNumberForLogOutputToLan = value; }
        }

        /// <summary>
        /// Get or set the default value for this property.
        /// </summary>
        public int PortNumberForLogOutputToLan_DefaultValue
        {
            get { return _portNumberForLogOutputToLan_DefaultValue; }
            set { _portNumberForLogOutputToLan_DefaultValue = value; }
        }
        #endregion
#endif

        #region ReasonForIpcFailure
        /// <summary>
        /// Get or set a bit of text that, hopefully, helps explain why the IsIpcFailing flag is set.
        /// </summary>
        /// <remarks>
        /// This is not included within the IpcOutputPipe class because it reflects an operational status,
        /// as opposed to a configuration-setting.
        /// </remarks>
        public string ReasonForIpcFailure
        {
            get { return _reasonForIpcFailure; }
            set { _reasonForIpcFailure = value; }
        }
        #endregion

        #region VUrHostIPv4Address
        /// <summary>
        /// Get or set the IPv4 address that the LogNutVUr desktop-application is executing on, if applicable.
        /// </summary>
        public string VUrHostIPv4Address
        {
            get
            {
                // If no value has been explicitly set, return the default value.
                if (StringLib.HasSomething( _VUrHostIPv4Address ))
                {
                    return _VUrHostIPv4Address;
                }
                else
                {
                    return _VUrHostIPv4Address_DefaultValue;
                }
            }
            set
            {
                _VUrHostIPv4Address = value;
                //                LogNutRegistryService.The.SetVUrHostIPv4Address(value, null, null);
            }
        }

        /// <summary>
        /// Get or set the explicitly-set value for this property, with null indicating no explicit value.
        /// </summary>
        public string VUrHostIPv4Address_ExplicitValue
        {
            get { return _VUrHostIPv4Address; }
            set { _VUrHostIPv4Address = value; }
        }

        /// <summary>
        /// Get or set the default value for this property.
        /// </summary>
        public string VUrHostIPv4Address_DefaultValue
        {
            get { return _VUrHostIPv4Address_DefaultValue; }
            set { _VUrHostIPv4Address_DefaultValue = value; }
        }

        /// <summary>
        /// Set the effective value of the
        /// IPv4 address that the LogNutVUr desktop-application is executing on, if applicable.
        /// This is the same as setting the VUrHostIPv4Address property.
        /// </summary>
        /// <param name="ipAddressTextValue">the IPv4 address to set it to, as a string</param>
        /// <returns>a reference to this IpcOutputPipe object, such that further method calls may be chained</returns>
        public IpcOutputPipe SetVUrHostIPv4Address( string ipAddressTextValue )
        {
            VUrHostIPv4Address = ipAddressTextValue;
            return this;
        }
        #endregion VUrHostIPv4Address

        #endregion public properties

        #region public methods

        #region Clear
        /// <summary>
        /// Set all of the configuration parameter default values to their initial values,
        /// and the explicit values to null,
        /// as though no configuration-data source had ever been accessed.
        /// </summary>
        public void Clear()
        {
            _ipcType_DefaultValue = IpcType.None;
            _ipcServerIdentifier_DefaultValue = "LogNut.DesignForge.com";
            _reasonForIpcFailure = null;
            if (_mqController != null)
            {
                _mqController.Close();
                _mqController = null;
            }
            this.SetToDefaults();
        }
        #endregion

        #region FinalizeWriteToFile
        /// <summary>
        /// Write whatever logging needs to be written to the log-output text-file and close it.
        /// In the case of this IpcOutputPipe, this does nothing.
        /// </summary>
        /// <param name="filenameExtension">Extension to change the filename to. Null = accept the appropriate default.</param>
        /// <remarks>
        /// You may not need to call this for most logging-output pipes.
        /// The original motivation for this was to provide a way for ETW output to be rendered
        /// from the .ETL file to a text file at the end of logging.
        /// </remarks>
        public void FinalizeWriteToFile( string filenameExtension )
        {
            // NOP
        }
        #endregion

        #region InitializeUponAttachment
        /// <summary>
        /// This gets called whenever an instance of this IpcOutputPipe gets attached to the logging system
        /// to perform any needed initialization.
        /// </summary>
        /// <param name="logConfig">This is provided in case this output-pipe needs to affect it.</param>
        public void InitializeUponAttachment( LogConfig logConfig )
        {
        }
        #endregion

        #region SaveConfiguration
        /// <summary>
        /// Save the configuration values to the persistent store.
        /// </summary>
        public void SaveConfiguration()
        {
            // CBL Implement
        }
        #endregion

        #region SetConfigurationFromText
        /// <summary>
        /// Given strings that are intended to represent a property-name and a value,
        /// if these apply to this output-pipe, set the appropriate configuration-value.
        /// </summary>
        /// <param name="propertyName">the name of the configuration-property to set</param>
        /// <param name="propertyValue">a string representing the value to set it to</param>
        /// <returns>true only if the given property-name matched a configuration-setting for this particular IOutputPipe implementation</returns>
        public bool SetConfigurationFromText( string propertyName, string propertyValue )
        {
            bool ok = false;
            switch (propertyName)
            {
                //CBL
                //case "LowestLevelToSend":
                //    LowestLevelToSend = (LogOutputLevel)Enum.Parse( typeof( LogOutputLevel ), propertyValue.PutIntoTypeOfCasing( StringLib.TypeOfCasing.Titlecased ) );
                //     break;
                default:
                    break;
            }
            return ok;
        }
        #endregion

        #region SetToDefaults
        /// <summary>
        /// Set all of the configuration parameters to their default values,
        /// which is the same as removing all of the the explicit values such that the properties revert to their default values.
        /// The default values retain their current settings.
        /// </summary>
        public void SetToDefaults()
        {
            _controlPanelIPv4Address = null;
            _ipcType = null;
            _ipcServerIdentifier = null;
            _myIPv4Address = null;
            _VUrHostIPv4Address = null;
        }
        #endregion

        #region Read.. methods

        #region ReadSettingsFromRegistry
#if !NETFX_CORE
        /// <summary>
        /// Retrieve the values from the Windows Registry
        /// to use as the actual values for the configuration settings within this class.
        /// </summary>
        public void ReadSettingsFromRegistry()
        {
            var reg = LogNutRegistryService.The;

            //            _controlPanelIPv4Address = reg.GetControlPanelIPv4Address(null, null);
            _ipcScope = reg.GetIpcScope( null, null );
            _ipcType = reg.GetIpcType( null, null );
            _myIPv4Address = reg.GetMyIPv4Address( null, null );
            _VUrHostIPv4Address = reg.GetVUrHostIPv4Address( null, null );

            // IpcType
            IpcType? nullableIpcTypeValueFromRegistry = reg.GetIpcType( null, null );
            if (nullableIpcTypeValueFromRegistry.HasValue)
            {
                _ipcType_DefaultValue = nullableIpcTypeValueFromRegistry.Value;
            }
            else // if nothing is in the Registry, set it to it's initial default value.
            {
                _ipcType_DefaultValue = default( IpcType );
            }

            // IpcServerIdentifier
            string stringValue = reg.IpcServerIdentifier;
            if (stringValue != null)
            {
                _ipcServerIdentifier = stringValue;
            }

            // MyIPv4Address
        }
#endif
        #endregion ReadSettingsFromRegistry

        /// <summary>
        /// Retrieve the values from the given file
        /// to use as the actual values for the configuration settings within this class.
        /// </summary>
        /// <param name="fileNameOrPath">the filename or complete pathname of the file to read the values from</param>
        public void ReadFromFile( string fileNameOrPath )
        {
            //CBL  Implement
        }
        #endregion Read.. methods

        #region WriteConfiguration.. methods
#if !NETFX_CORE
        /// <summary>
        /// Save the explicitly-set values in the Windows Registry.
        /// </summary>
        public void WriteSettingsToRegistry()
        {
            var reg = LogNutRegistryService.The;

            //reg.SetIpcScope( IpcScope_ExplicitValue, null, null );
            //reg.SetIpcType( IpcType_ExplicitValue, null, null );
            //reg.SetIpcPortNumber(PortNumberForLogOutputToLan_ExplicitValue, null, null);
            //reg.SetMyIPv4Address( this.MyIPv4Address_ExplicitValue, null, null );
            //reg.SetVUrHostIPv4Address( this.VUrHostIPv4Address_ExplicitValue, null, null );
        }
#endif

        /// <summary>
        /// Save the explicitly-set values in the given file.
        /// </summary>
        /// <param name="fileNameOrPath">the filename or complete pathname of the file to write the values to</param>
        public void WriteToFile( string fileNameOrPath )
        {
            // CBL Implement
        }
        #endregion

        #region ToString
        /// <summary>
        /// Override the ToString method to provide a more useful display.
        /// </summary>
        /// <returns>a string the denotes the state of this object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder( "IpcOutputPipe(" );
#if DEBUG
            // Display the properties that are at other than their default state.
            if (_ipcType != null)
            {
                sb.Append( " IpcType=" ).Append( IpcType ).Append( "," );
            }
            if (_ipcScope != null)
            {
                sb.Append( " IpcScope=" ).Append( IpcScope ).Append( "," );
            }
#endif
            return sb.ToStringAndEndList();
        }
        #endregion

        #region Write
        /// <summary>
        /// Send output to this OutputPipe.
        /// </summary>
        /// <param name="request">what to write</param>
        /// <returns>true if the request actually went out</returns>
        public bool Write( LogSendRequest request )
        {
            bool wasSentOut = false;
            if (!IsFailing)
            {
                try
                {
                    //CBL  WTF is this? !!!
                    if (IpcType != IpcType.None)
                    {
                        string myIpAddress = MyIPv4Address;
                        if (StringLib.HasSomething( myIpAddress ))
                        {
                            LogRecord logRecord = request.Record;

                            // CBL This copy of 'Config' needs to be set! ?
                            string textToXmt = logRecord.AsText( LogManager.Config );
                            MQController.TransmitLog( myIpAddress, textToXmt );
                            wasSentOut = true;
                        }
                        else
                        {
                            IsFailing = true;
                            ReasonForIpcFailure = "The property MyIPv4Address is not set.";
                        }
                    }
                }
                catch (Exception x)
                {
                    NutUtil.WriteToConsole( "We had a problem in LogManager.SendViaIpc: " + StringLib.ExceptionDetails( x, true ) );
                    IsFailing = true;
                }
            }
            return wasSentOut;
        }
        #endregion

        #endregion public methods

        #region fields

        /// <summary>
        /// This determines the mode of communication when IPC is used.
        /// </summary>
        private IpcType? _ipcType;
        /// <summary>
        /// The default value for the above variable; this is initially IpcType.None.
        /// </summary>
        private IpcType _ipcType_DefaultValue;

        private IpcScope? _ipcScope;
        private IpcScope _ipcScope_DefaultValue;

        /// <summary>
        /// This is the identification (whether domain-name or IP-address)
        /// of the LogNut IPC server.
        /// Default is null, meaning no value is set.
        /// </summary>
        private string _ipcServerIdentifier;
        /// <summary>
        /// This is the default value for that instance-variable, initially set to "LogNut.DesignForge.com".
        /// </summary>
        private string _ipcServerIdentifier_DefaultValue = "LogNut.DesignForge.com";

        /// <summary>
        /// The singleton-instance of the message-queuing controller that all LogNut components are to use.
        /// </summary>
        private IMQController _mqController;

        /// <summary>
        /// This is the IPv4 address to use for the local machine.
        /// </summary>
        private string _myIPv4Address;
        /// <summary>
        /// This is the default value for IPv4Address.
        /// </summary>
        private string _myIPv4Address_DefaultValue = "127.0.0.1";

        private string _reasonForIpcFailure;

        /// <summary>
        /// This is the IPv4 address that the LogNutVUr desktop-application is executing on, if applicable.
        /// </summary>
        private string _VUrHostIPv4Address;
        /// <summary>
        /// This is the default value for VUrHostIPv4Address.
        /// </summary>
        private string _VUrHostIPv4Address_DefaultValue = "127.0.0.1";

        /// <summary>
        /// This is the IPv4 address that the LogNutControlPanel desktop-application is executing on, if applicable.
        /// </summary>
        private string _controlPanelIPv4Address;
        /// <summary>
        /// This is the default value for VUrHostIPv4Address.
        /// </summary>
        private string _controlPanelIPv4Address_DefaultValue = "127.0.0.1";

        /// <summary>
        /// This is the IPv4 address that the LogNut Support-Service is executing on, if applicable.
        /// </summary>
        private string _lognutSupportServiceIPv4Address;
        /// <summary>
        /// This is the default value for _lognutSupportServiceIPv4Address.
        /// </summary>
        private string _lognutSupportServiceIPv4Address_DefaultValue = "127.0.0.1";

        /// <summary>
        /// When the transmission-mode is Message-Queuing against a receiver that is on the local host, this is the port-number to use.
        /// </summary>
        private int _portNumberForMqWithLocalHost = 60001;

        /// <summary>
        /// This is the default value of the TCP Port Number to use for Message-Queuing when the other end is on the same host.
        /// </summary>
        private const int _portNumberForMqWithLocalHost_DefaultValue = 60001;

        /// <summary>
        /// When the transmission-mode is Message-Queuing against a receiver that is on the Local Area Network (LAN), this is the port-number to use.
        /// </summary>
        private int _portNumberForMqWithLan = 5557;

        /// <summary>
        /// This is the default value of the TCP Port Number to use for Message-Queuing when the other end is on the LAN.
        /// </summary>
        private const int _portNumberForMqWithLan_DefaultValue = 5557;

        #endregion fields
    }
}
