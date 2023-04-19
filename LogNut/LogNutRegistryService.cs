#if PRE_4
#define PRE_5
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Hurst.BaseLib;


namespace Hurst.LogNut
{
    /// <summary>
    /// This provides access to the Windows Registry for the LogNut facility.
    /// </summary>
    /// <remarks>
    /// Registry Organization:
    /// 
    /// When the option is chosen to persist the settings to the Windows Registry,
    /// LogNut's settings are stored under Software\DesignForge\LogNut.
    /// Definition: In the following, "LogNut key" is a shorthand for Software\DesignForge\LogNut.
    /// 
    /// Host-wide settings are stored directly within that LogNut key, as "default" values - which means
    /// they are possibly null and only have an effect if they are non-null.
    /// 
    /// Settings intended to apply only to a certain subject-program, on any host, are under
    /// the LogNut key thus: LogNut\Programs\{SubjectProgramName}
    /// or only for a given host, under
    /// LogNut\Hosts\{SourceHost}\Programs\{SubjectProgramName}
    /// These values are also possibly null, and only when non-null represent default values that any explicit setting
    /// within the program code would override.
    /// 
    /// For settings that apply to the local source-host, the settings are directly under "LogNut".
    /// 
    /// Viewer-specific settings are under the subkey LogNutVUr (that is, LogNut\LogNutVUr).
    /// 
    /// All loggers that are recorded, from other hosts, are under the subkey LogNut\Hosts\{SourceHostName}\Programs\{SubjectProgramName}\Loggers.
    /// When the source-host is the local computer, it is under LogNut\Programs.
    /// For each logger, only that information that is reasonably useful for a viewer is kept.
    /// That includes the logger's Name, Category, IsFileOutputEnabled, and whether we are viewing it.
    /// The SourceHost and SubjectProgramName are already known from it's registry-path.
    /// </remarks>
    public class LogNutRegistryService : RegistryService
    {
        #region The
        /// <summary>
        /// Get the singleton instance of this class.
        /// </summary>
        public static LogNutRegistryService The
        {
            get
            {
                if (_theInstance == null)
                {
                    // For simplicity, default to just standardizing on using the 32-bit view of the Registry.
                    // Thus all programs, 32 and 64 bit, will look at the same thing. 
                    _theInstance = new LogNutRegistryService { IsSpecificToUser = true, IsRegistry32Bit = true };
                }
                return _theInstance;
            }
        }

        /// <summary>
        /// Return the instance of LogNutRegistryService that uses the LogNutTest Registry key instead of LogNut,
        /// to facilitate running tests.
        /// </summary>
        /// <returns>the instance of this class that is intended for testing purposes</returns>
        public static LogNutRegistryService GetInstanceForTest()
        {
            if (_theInstanceForTesting == null)
            {
                _theInstanceForTesting = new LogNutRegistryService { IsSpecificToUser = true, IsRegistry32Bit = true };
                // By setting this true, the method GetProductNameInRegistry will return LogNutTest instead of LogNut,
                // and thus the Registry settings wont be impacted for LogNut.
                _theInstanceForTesting._isForTesting = true;
            }
            return _theInstanceForTesting;
        }

        private static LogNutRegistryService _theInstance;
        private static LogNutRegistryService _theInstanceForTesting;
        private bool _isForTesting;

        #endregion The

        private string SubjectProgramName
        {
            get
            {
                //CBL This is obviously not correct -- LogManager is not a type of the calling assembly.
                return LogManager.Config.GetSubjectProgramName( typeof( LogManager ) );
            }
        }

        #region the enablement flags

        #region IsLoggingEnabled
        /// <summary>
        /// Get the IsLoggingEnabled property for the given subject-program and host
        /// as a nullable-boolean value, which is null if no setting applies.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the IsLoggingEnabled property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsLoggingEnabled( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsLoggingEnabled", subkeyPath );
        }

        /// <summary>
        /// Set the IsLoggingEnabled property for the given subject-program and host.
        /// </summary>
        /// <param name="isDisabled">if non-null, then this has the value to set the IsLoggingEnabled property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isEnabled argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsLoggingEnabled( bool? isEnabled, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsLoggingEnabled", isEnabled, subkeyPath );
        }
        #endregion

        #region GetLowestLevelThatIsEnabled
        /// <summary>
        /// Return the minimum LogOutputLevel that is enabled for the given subject-program and host
        /// as a nullable-LogOutputLevel value, which is null if no setting applies.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-LogOutputLevel which, if non-null, has the lowest level that is enabled property for the given subject-program; null indicates no value</returns>
        public LogOutputLevel? GetLowestLevelThatIsEnabled( string subjectProgramName, string sourceHost )
        {
            LogOutputLevel? answer = null;
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            int v = GetInteger( "LevelEnabled", -1, subkeyPath );
            if (v > -1)
            {
                answer = (LogOutputLevel)v;
            }
            return answer;
        }
        #endregion

        #region SetLowestLevelThatIsEnabled
        /// <summary>
        /// Set the minimum level to enable down to for the given subject-program and host.
        /// </summary>
        /// <param name="level">if non-null, then this has the value to set for the minimum LogOutputLevel for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the level argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetLowestLevelThatIsEnabled( LogOutputLevel? level, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            if (level.HasValue)
            {
                int integerValue = (int)level.Value;
                SetValue( "LevelEnabled", integerValue, subkeyPath );
            }
            else
            {
                DeleteValue( "LevelEnabled", subkeyPath );
            }
        }
        #endregion

        #region IsSystemEventLogOutputEnabled
        /// <summary>
        /// Return true if logging output to the operating-system logging facility is enabled.
        /// Default is null, which means no-override.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>the nullable-boolean value representing the whether output to the Windows Event Log is enabled, or null if no value has been assigned</returns>
        public bool? GetIsSystemEventLogOutputEnabled( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsSystemEventLogOutputEnabled", subkeyPath );
        }

        /// <summary>
        /// Set whether output to the operating-system logging facility is enabled.
        /// </summary>
        /// <param name="isEnabled">if non-null, then this has the value to set the IsSystemEventLogOutputEnabled property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isEnabled argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsSystemEventLogOutputEnabled( bool? isEnabled, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsSystemEventLogOutputEnabled", isEnabled, subkeyPath );
        }
        #endregion

        #region sound

        #region IsAudioOutputEnabled
        /// <summary>
        /// Get or set whether sounds can be generated along with the log output.
        /// Default is null, which means no-override.
        /// </summary>
        public bool? IsAudioOutputEnabled
        {
            get
            {
                string subkeyPath = this.GetSubkeyForProgram( SubjectProgramName, LogManager.Config.SourceHostName );
                return GetNullableBoolean( "IsAudioOutputEnabled", subkeyPath );
            }
            set
            {
                string subkeyPath = this.GetSubkeyForProgram( SubjectProgramName, LogManager.Config.SourceHostName );
                SetValue( "IsAudioOutputEnabled", value, subkeyPath );
            }
        }
        #endregion

        #region IsAudioOutputEnabledForDebug
        /// <summary>
        /// Get or set whether sounds can be generated for Debug-level log output.
        /// Default is null, which means no-override.
        /// </summary>
        public bool? IsAudioOutputEnabledForDebug
        {
            get
            {
                string subkeyPath = this.GetSubkeyForProgram( SubjectProgramName, LogManager.Config.SourceHostName );
                return GetNullableBoolean( "IsAudioOutputEnabledForDebug", subkeyPath );
            }
            set
            {
                string subkeyPath = this.GetSubkeyForProgram( SubjectProgramName, LogManager.Config.SourceHostName );
                SetValue( "IsAudioOutputEnabledForDebug", value, subkeyPath );
            }
        }
        #endregion

        #endregion sound

        #endregion the enablement flags

        #region how to express a log-record

        #region IsToShowFractionsOfASecond
        /// <summary>
        /// Get the IsToShowFractionsOfASecond property.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsToShowFractionsOfASecond( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsToShowFractionsOfASecond", subkeyPath );
        }

        /// <summary>
        /// Set the IsToShowFractionsOfASecond property.
        /// </summary>
        /// <param name="isEnabled">if non-null, then this has the value to set the property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isEnabled argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsToShowFractionsOfASecond( bool? isEnabled, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsToShowFractionsOfASecond", isEnabled, subkeyPath );
        }
        #endregion

        #region IsToShowLevel
        /// <summary>
        /// Get the IsToShowLevel property.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsToShowLevel( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsToShowLevel", subkeyPath );
        }

        /// <summary>
        /// Set the IsToShowLevel property.
        /// </summary>
        /// <param name="isEnabled">if non-null, then this has the value to set the property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isEnabled argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsToShowLevel( bool? isEnabled, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsToShowLevel", isEnabled, subkeyPath );
        }
        #endregion

        #region IsToShowLoggerName
        /// <summary>
        /// Get the IsToShowLoggerName property.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsToShowLoggerName( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsToShowLoggerName", subkeyPath );
        }

        /// <summary>
        /// Set the IsToShowLoggerName property.
        /// </summary>
        /// <param name="isEnabled">if non-null, then this has the value to set the property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isEnabled argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsToShowLoggerName( bool? isEnabled, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsToShowLoggerName", isEnabled, subkeyPath );
        }
        #endregion

        #region IsToShowPrefix
        /// <summary>
        /// Get the IsToShowPrefix property.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsToShowPrefix( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsToShowPrefix", subkeyPath );
        }

        /// <summary>
        /// Set the IsToShowPrefix property.
        /// </summary>
        /// <param name="isEnabled">if non-null, then this has the value to set the property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isEnabled argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsToShowPrefix( bool? isEnabled, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsToShowPrefix", isEnabled, subkeyPath );
        }
        #endregion

        #region LogNutVUrIsToShowSourceHost
        /// <summary>
        /// Get or set whether LogNutVUr is to include the machine-name in the prefix for each log record.
        /// Default is null, which indicates no-override.
        /// </summary>
        public bool? LogNutVUrIsToShowSourceHost
        {
            get
            {
                return GetNullableBoolean( "IsToShowSourceHost", "LogNutVUr" );
            }
            set
            {
                SetValue( "IsToShowSourceHost", value, "LogNutVUr" );
            }
        }
        #endregion

        #region IsToShowSourceHost
        /// <summary>
        /// Get whether to include the machine-name in the prefix for each log record.
        /// Default is null, indicating no override.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program that this setting applies to</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean that indicates whether to show it, or if null then no preference</returns>
        public bool? GetIsToShowSourceHost( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsToShowSourceHost", subkeyPath );
        }

        /// <summary>
        /// Set whether to display the source computer-name within the log-trace prefix.
        /// </summary>
        /// <param name="isShowingIt">a nullable-boolean indicating whether to show the subject-program. Null means no preference. This must not be null.</param>
        /// <param name="subjectProgramName">the name of the subject-program that this setting applies to</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        public void SetIsToShowSourceHost( bool? isShowingIt, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsToShowSourceHost", isShowingIt, subkeyPath );
        }
        #endregion

        #region IsToShowStackTraceForExceptions
        /// <summary>
        /// Get the IsToShowStackTraceForExceptions property.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsToShowStackTraceForExceptions( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsToShowStackTraceForExceptions", subkeyPath );
        }

        /// <summary>
        /// Set the IsToShowStackTraceForExceptions property.
        /// </summary>
        /// <param name="isEnabled">if non-null, then this has the value to set the property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isEnabled argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsToShowStackTraceForExceptions( bool? isEnabled, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsToShowStackTraceForExceptions", isEnabled, subkeyPath );
        }
        #endregion

        #region IsToShowSubjectProgram

        #region LogNutVUrIsToShowSubjectProgram
        /// <summary>
        /// Get or set whether LogNutVUr is to include the name of the subject-program in the prefix for each log record.
        /// Default is null, which indicates no-override.
        /// </summary>
        public bool? LogNutVUrIsToShowSubjectProgram
        {
            get
            {
                return GetNullableBoolean( "IsToShowSubjectProgram", "LogNutVUr" );
            }
            set
            {
                SetValue( "IsToShowSubjectProgram", value, "LogNutVUr" );
            }
        }
        #endregion

        #region IsToShowSubjectProgram
        /// <summary>
        /// Get the value of the IsToShowSubjectProgram property from the Registry.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program that this setting applies to</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean that indicates whether to show it, or if null then no preference</returns>
        public bool? GetIsToShowSubjectProgram( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsToShowSubjectProgram", subkeyPath );
        }

        /// <summary>
        /// Set the value of the IsToShowSubjectProgram property in the Registry.
        /// </summary>
        /// <param name="isShowingIt">a nullable-boolean indicating whether to show it. Null means no preference.</param>
        /// <param name="subjectProgramName">the name of the subject-program that this setting applies to</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        public void SetIsToShowSubjectProgram( bool? isShowingIt, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsToShowSubjectProgram", isShowingIt, subkeyPath );
        }
        #endregion

        #endregion

        #region IsToShowSubjectProgramVersion
        /// <summary>
        /// Get the IsToShowSubjectProgramVersion property.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsToShowSubjectProgramVersion( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsToShowSubjectProgramVersion", subkeyPath );
        }

        /// <summary>
        /// Set the IsToShowSubjectProgramVersion property.
        /// </summary>
        /// <param name="isEnabled">if non-null, then this has the value to set the property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isEnabled argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsToShowSubjectProgramVersion( bool? isEnabled, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsToShowSubjectProgramVersion", isEnabled, subkeyPath );
        }
        #endregion

        #region IsToShowThread
        /// <summary>
        /// Get the IsToShowThread property.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsToShowThread( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsToShowThread", subkeyPath );
        }

        /// <summary>
        /// Set the IsToShowThread property.
        /// </summary>
        /// <param name="isEnabled">if non-null, then this has the value to set the property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isEnabled argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsToShowThread( bool? isEnabled, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsToShowThread", isEnabled, subkeyPath );
        }
        #endregion

        #region IsToShowTimestamp
        /// <summary>
        /// Get the IsToShowTimestamp property for the given subject-program and host.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program that this setting applies to</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the IsToShowTimestamp property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsToShowTimestamp( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsToShowTimestamp", subkeyPath );
        }

        /// <summary>
        /// Set the IsToShowTimestamp property for the given subject-program and host.
        /// </summary>
        /// <param name="isShowingIt">a nullable-boolean indicating whether to show it. Null means no preference.</param>
        /// <param name="subjectProgramName">the name of the subject-program that this setting applies to</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isShowingIt argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsToShowTimestamp( bool? isShowingIt, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsToShowTimestamp", isShowingIt, subkeyPath );
        }

        /// <summary>
        /// Get or set whether LogNutVUr is to include the date-and-time in the prefix for each log record.
        /// Default is null, which indicates no-override.
        /// </summary>
        public bool? LogNutVUrIsToShowTimestamp
        {
            get
            {
                return GetNullableBoolean( "IsToShowTimestamp", "LogNutVUr" );
            }
            set
            {
                SetValue( "IsToShowTimestamp", value, "LogNutVUr" );
            }
        }
        #endregion

        #region LogNutVUrIsToShowThread
        /// <summary>
        /// Get or set whether LogNutVUr to include the thread-identifier of the process-thread that created this log-record
        /// in the prefix for each log record.
        /// Default is null, which indicates no-override.
        /// </summary>
        public bool? LogNutVUrIsToShowThread
        {
            get
            {
                return GetNullableBoolean( "IsToShowThread", "LogNutVUr" );
            }
            set
            {
                SetValue( "IsToShowThread", value, "LogNutVUr" );
            }
        }
        #endregion

        #region IsToShowUser
        /// <summary>
        /// Get the IsToShowUser property.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsToShowUser( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsToShowUser", subkeyPath );
        }

        /// <summary>
        /// Set the IsToShowUser property.
        /// </summary>
        /// <param name="isEnabled">if non-null, then this has the value to set the property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isEnabled argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsToShowUser( bool? isEnabled, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsToShowUser", isEnabled, subkeyPath );
        }
        #endregion

        #endregion how to express a log-record

        #region IPC

        #region IpcScope
        /// <summary>
        /// Get the mode of communication when IPC is used.
        /// Default is null, which means no-override.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>the nullable-IpcScope value representing the current IPC scope, or null if no value has been assigned</returns>
        public IpcScope? GetIpcScope( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableEnum<IpcScope>( "IpcScope", subkeyPath );
        }

        /// <summary>
        /// Set the mode of communication when IPC is used.
        /// </summary>
        /// <param name="ipcScope">if non-null, then this has the value to set the IpcType property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the IpcScope argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIpcScope( IpcScope? ipcScope, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue<IpcScope>( "IpcScope", ipcScope, subkeyPath );
        }
        #endregion

        #region IpcType
        /// <summary>
        /// Get the mode of communication when IPC is used.
        /// Default is null, which means no-override.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>the nullable-IpcType value representing the current IPC type, or null if no value has been assigned</returns>
        public IpcType? GetIpcType( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableEnum<IpcType>( "IpcType", subkeyPath );
        }

        /// <summary>
        /// Set the mode of communication when IPC is used.
        /// </summary>
        /// <param name="ipcMode">if non-null, then this has the value to set the IpcType property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the IpcType argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIpcType( IpcType? ipcMode, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue<IpcType>( "IpcType", ipcMode, subkeyPath );
        }
        #endregion

        #region IpcServerIdentifier
        /// <summary>
        /// Get or set the identification (whether domain-name or IP-address)
        /// used for the LogNut IPC server. Default is lognut.designforge.com
        /// </summary>
        public string IpcServerIdentifier
        {
            get
            {
                string subkeyPath = this.GetSubkeyForProgram( SubjectProgramName );
                return GetString( "IpcServerIdentifier", subkeyPath );
            }
            set
            {
                string subkeyPath = this.GetSubkeyForProgram( SubjectProgramName );
                SetValue( "IpcServerIdentifier", value, subkeyPath );
            }
        }
        #endregion

        #region PortNumberForLogOutputToLan
        /// <summary>
        /// Get the port number to use when communicating via IPC.
        /// Default is null, which means no value.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>the nullable-integer value representing the Port Number used for IPC, or null if no value has been assigned</returns>
        public int? GetIpcPortNumber( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableInteger( "PortNumberForLogOutputToLan", subkeyPath );
        }

        /// <summary>
        /// Set the port number to use when communicating via IPC.
        /// </summary>
        /// <param name="portNumber">if non-null, then this has the port-number to use for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the portNumber argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIpcPortNumber( int? portNumber, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "PortNumberForLogOutputToLan", portNumber, subkeyPath );
        }
        #endregion

        #region MyIPv4Address
        /// <summary>
        /// Get the MyIPv4Address property.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a string which, if non-empty, has the value of the property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public string GetMyIPv4Address( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            string result = GetString( "MyIPv4Address", null, subkeyPath );
            if (StringLib.HasSomething( result ))
            {
                return result;
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Set the MyIPv4Address property.
        /// </summary>
        /// <param name="stringValue">if non-null, then this has the value to set the property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the stringValue argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetMyIPv4Address( string stringValue, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "MyIPv4Address", stringValue, subkeyPath );
        }
        #endregion

        #region LastKnownSsAddress
        /// <summary>
        /// Return the last-known-good IPv4-Address for the LogNutSupportService program-component on the LAN, or null if not found.
        /// </summary>
        /// <returns>a string which, if non-empty, has the IPv4-address, or null if not found</returns>
        public string GetLastKnownSsAddress()
        {
            string result = null;
            if (IsKeyPresentUnderBasePath( subkeyForSsIa ))
            {
                result = GetString( "IPv4Address", null, subkeyForSsIa );
            }
            return result;
        }

        /// <summary>
        /// Return an IdentityAnnouncement object that denotes the last-known-good location for the LogNutSupportService program-component on the LAN,
        /// or null if not found.
        /// </summary>
        /// <returns>an IdentityAnnouncement object that has the last-known location, or null if not found</returns>
        public IdentityAnnouncement GetLastKnownSsLocation()
        {
            IdentityAnnouncement result = null;
            if (IsKeyPresentUnderBasePath( subkeyForSsIa ))
            {
                string address = GetString( "IPv4Address", null, subkeyForSsIa );
                if (StringLib.HasSomething( address ))
                {
                    string hostName = GetString( "MachineName", null, subkeyForSsIa );
                    result = new IdentityAnnouncement( address, hostName );
                    result.When = GetDateTime( "When", subkeyForSsIa );
                }
            }
            return result;
        }

        private const string subkeyForSsIa = @"LastKnownAddress\LogNutSS\1";

        /// <summary>
        /// Set the last-known-good IPv4-Address, machine-name, and time for the LogNutSupportService program-component on the LAN.
        /// </summary>
        /// <param name="ipAddress">the IPv4 address to store</param>
        /// <param name="machineName">the name of the host-machine that is associated with that IP-address, if known</param>
        /// <param name="when">the time-of-last-announcement for this component and location</param>
        public void SetLastKnownSsIPv4Address( string ipAddress, string machineName, DateTime when )
        {
            CreateSubkeyUnderBasekey( subkeyForSsIa );
            SetValue( "IPv4Address", ipAddress, subkeyForSsIa );
            SetValue( "MachineName", machineName, subkeyForSsIa );
            SetDateTimeValue( "When", when, subkeyForSsIa );
        }

        /// <summary>
        /// Remove any last-known-location information from the Windows Registry for the LogNutControlPanel component.
        /// </summary>
        public void ClearLastKnownSsLocations()
        {
            if (IsKeyPresentUnderBasePath( @"LastKnownAddress\LogNutSS" ))
            {
                DeleteKey( @"LastKnownAddress\LogNutSS" );
            }
        }
        #endregion

        #region LastKnownCPAddress
        /// <summary>
        /// Return the last-known-good IPv4-Address for the desktop LogNutControlPanel program on the LAN, or null if not found.
        /// </summary>
        /// <returns>a string which, if non-empty, has the IPv4-address, or null if not found</returns>
        public string GetLastKnownCPAddress()
        {
            string result = null;
            if (IsKeyPresentUnderBasePath( subkeyForCPIA ))
            {
                result = GetString( "IPv4Address", null, subkeyForCPIA );
            }
            return result;
        }

        /// <summary>
        /// Return an IdentityAnnouncement object that denotes the last-known-good location for the desktop LogNutControlPanel program on the LAN,
        /// or null if not found.
        /// </summary>
        /// <returns>an IdentityAnnouncement object that has the last-known location of the LogNutControlPanel component, or null if not found</returns>
        public IdentityAnnouncement GetLastKnownCPLocation()
        {
            IdentityAnnouncement result = null;
            if (IsKeyPresentUnderBasePath( subkeyForCPIA ))
            {
                string address = GetString( "IPv4Address", null, subkeyForCPIA );
                if (StringLib.HasSomething( address ))
                {
                    string hostName = GetString( "MachineName", null, subkeyForCPIA );
                    result = new IdentityAnnouncement( address, hostName );
                    result.When = GetDateTime( "When", subkeyForCPIA );
                }
            }
            return result;
        }

        private const string subkeyForCPIA = @"LastKnownAddress\LogNutCP\1";

        /// <summary>
        /// Set the last-known-good IPv4-Address, machine-name, and time for the desktop LogNutControlPanel program on the LAN.
        /// </summary>
        /// <param name="ipAddress">the IPv4 address to store</param>
        /// <param name="machineName">the name of the host-machine that is associated with that IP-address, if known</param>
        /// <param name="when">the time-of-last-announcement for this component and location</param>
        public void SetLastKnownCPIPv4Address( string ipAddress, string machineName, DateTime when )
        {
            CreateSubkeyUnderBasekey( subkeyForCPIA );
            SetValue( "IPv4Address", ipAddress, subkeyForCPIA );
            SetValue( "MachineName", machineName, subkeyForCPIA );
            SetDateTimeValue( "When", when, subkeyForCPIA );
        }

        /// <summary>
        /// Remove any last-known-location information from the Windows Registry for the LogNutControlPanel component.
        /// </summary>
        public void ClearLastKnownCpLocations()
        {
            if (IsKeyPresentUnderBasePath( @"LastKnownAddress\LogNutCP" ))
            {
                DeleteKey( @"LastKnownAddress\LogNutCP" );
            }
        }
        #endregion

        #region LastKnownVUrIPv4Address1
        /// <summary>
        /// Return the last-known-good IPv4-Address for the desktop LogNutVUr program on the LAN, or null if not found.
        /// </summary>
        /// <param name="whichGeneration">If multiple values are available, this indicates which one (1 for most recent, 2 for the previous value, etc). Currently ignored</param>
        /// <returns>a string which, if non-empty, has the value of the property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public string GetLastKnownVUrIPv4Address1( int whichGeneration )
        {
            //CBL  The whichGeneration thing needs to be implemented
            string result = null;
            if (IsKeyPresentUnderBasePath( subkeyForVUrIA ))
            {
                result = GetString( "IPv4Address", null, subkeyForVUrIA );
            }
            return result;
        }

        /// <summary>
        /// Return an IdentityAnnouncement object that denotes the last-known-good location for the desktop LogNutVUr program on the LAN,
        /// or null if not found.
        /// </summary>
        /// <returns>an IdentityAnnouncement object that has the last-known location of the LogNutVUr component</returns>
        public IdentityAnnouncement GetLastKnownVUrLocation()
        {
            IdentityAnnouncement result = null;
            if (IsKeyPresentUnderBasePath( subkeyForVUrIA ))
            {
                string address = GetString( "IPv4Address", null, subkeyForVUrIA );
                if (StringLib.HasSomething( address ))
                {
                    string hostName = GetString( "MachineName", null, subkeyForVUrIA );
                    result = new IdentityAnnouncement( address, hostName );
                    result.When = GetDateTime( "When", subkeyForVUrIA );
                }
            }
            return result;
        }

        private const string subkeyForVUrIA = @"LastKnownAddress\LogNutVUr\1";

        /// <summary>
        /// Set the last-known-good IPv4-Address, machine-name, and time for the desktop LogNutVUr program on the LAN.
        /// </summary>
        /// <param name="ipAddress">the IPv4 address to store</param>
        /// <param name="machineName">the name of the host-machine that is associated with that IP-address, if known</param>
        /// <param name="when">the time-of-last-announcement for this component and location</param>
        /// <remarks>
        /// Use null for the ipAddress argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetLastKnownVUrIPv4Address1( string ipAddress, string machineName, DateTime when )
        {
            CreateSubkeyUnderBasekey( subkeyForVUrIA );
            SetValue( "IPv4Address", ipAddress, subkeyForVUrIA );
            SetValue( "MachineName", machineName, subkeyForVUrIA );
            SetDateTimeValue( "When", when, subkeyForVUrIA );
        }

        /// <summary>
        /// Remove any last-known-location information from the Windows Registry for the LogNutVUr component.
        /// </summary>
        public void ClearLastKnownVUrLocations()
        {
            if (IsKeyPresentUnderBasePath( @"LastKnownAddress\LogNutVUr" ))
            {
                DeleteKey( @"LastKnownAddress\LogNutVUr" );
            }
        }
        #endregion

        #endregion IPC

        #region file-output

        #region FileOutputFilename
        /// <summary>
        /// Get or set the filename to use for file output.
        /// </summary>
        public string FileOutputFilename
        {
            get
            {
                string subkeyPath = this.GetSubkeyForProgram( SubjectProgramName );
                return GetString( "FileOutputFilename", subkeyPath );
            }
            set
            {
                string subkeyPath = this.GetSubkeyForProgram( SubjectProgramName );
                SetValue( "FileOutputFilename", value, subkeyPath );
            }
        }

        /// <summary>
        /// Get the FileOutputFilename Registry setting for the given subject-program on the given host.
        /// String.Empty is returned if the subject-program is not found or there is no setting.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>the value of the FileOutputFilename property for the given subject-program, or String.Empty if no explicit setting applies</returns>
        public string GetFileOutputFilename( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            string result = GetString( "FileOutputFilename", String.Empty, subkeyPath );
            if (StringLib.HasSomething( result ))
            {
                return result;
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Set the FileOutputFilename for the given subject-program and host.
        /// </summary>
        /// <param name="newFilenameValue">the value to set the FileOutputFilename property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the newFilenameValue argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetFileOutputFilename( string newFilenameValue, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            //CBL  Do I want to delete this value if it happens to be the same as the default value?
            //if (String.IsNullOrWhiteSpace(newFilenameValue) || newFilenameValue.Equals(LogManager.GetFileOutputFilenameDefaultValue(subjectProgramName)))
            if (StringLib.HasNothing( newFilenameValue ))
            {
                DeleteValue( "FileOutputFilename", subkeyPath );
            }
            else
            {
                SetValue( "FileOutputFilename", newFilenameValue, subkeyPath );
            }
        }
        #endregion

        #region FileOutputFolder
        /// <summary>
        /// Get or set the folder that loggers will write the log file output to.
        /// </summary>
        public string FileOutputFolder
        {
            get
            {
                string subkeyPath = this.GetSubkeyForProgram( SubjectProgramName );
                return GetString( "FileOutputFolder", subkeyPath );
            }
            set
            {
                string subkeyPath = this.GetSubkeyForProgram( SubjectProgramName );
                SetValue( "FileOutputFolder", value, subkeyPath );
            }
        }

        /// <summary>
        /// Get the FileOutputFolder Registry setting for the given subject-program on the given host.
        /// String.Empty is returned if the subject-program is not found or there is no setting.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>the value of the FileOutputFolder property for the given subject-program, or String.Empty if no explicit setting applies</returns>
        public string GetFileOutputFolder( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            string result = GetString( "FileOutputFolder", String.Empty, subkeyPath );
            if (StringLib.HasSomething( result ))
            {
                return result;
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Set the FileOutputFolder for the given subject-program and host.
        /// </summary>
        /// <param name="newFolderValue">The filesystem-folder value to set the FileOutputFolder property to for the given subject-program and host.</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the newFolderValue argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetFileOutputFolder( string newFolderValue, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            if (StringLib.HasNothing( newFolderValue ))
            {
                DeleteValue( "FileOutputFolder", subkeyPath );
            }
            else
            {
                SetValue( "FileOutputFolder", newFolderValue, subkeyPath );
            }
        }
        #endregion

        #region IsFileOutputToCompressFiles
        /// <summary>
        /// Get the IsFileOutputToCompressFiles property for the given subject-program and host
        /// as a nullable-boolean value, which is null if no setting applies.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the IsFileOutputToCompressFiles property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsCompressingOutputFiles( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsFileOutputToCompressFiles", subkeyPath );
        }

        /// <summary>
        /// Set the IsFileOutputToCompressFiles property for the given subject-program and host.
        /// </summary>
        /// <param name="isEnabled">if non-null, then this has the value to set the IsFileOutputToCompressFiles property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isEnabled argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsCompressingOutputFiles( bool? isEnabled, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsFileOutputToCompressFiles", isEnabled, subkeyPath );
        }
        #endregion

        #region IsFileOutputEnabled
        /// <summary>
        /// Get the IsFileOutputEnabled property for the given subject-program and host
        /// as a nullable-boolean value, which is null if no setting applies.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the IsFileOutputEnabled property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsFileOutputEnabled( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsFileOutputEnabled", subkeyPath );
        }

        /// <summary>
        /// Set the IsFileOutputEnabled property for the given subject-program and host.
        /// </summary>
        /// <param name="isEnabled">if non-null, then this has the value to set the IsFileOutputEnabled property to for the given subject-program and host.</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isFileOutputEnabled argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsFileOutputEnabled( bool? isEnabled, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsFileOutputEnabled", isEnabled, subkeyPath );
        }
        #endregion

        #region FileOutputRolloverMode
        /// <summary>
        /// Get the basis on which logging output is to be rolled over to new files,
        /// as a nullable-RolloverMode value, which is null if no setting applies.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-RolloverMode, which if null indicates no explicit setting applies</returns>
        public RolloverMode? GetFileRolloverMode( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableEnum<RolloverMode>( "FileOutputRolloverMode", subkeyPath );
        }

        /// <summary>
        /// Set the basis on which logging output is to be rolled over to new files,
        /// as a nullable-RolloverMode value, which is null if no setting applies.
        /// </summary>
        /// <param name="rolloverMode">if non-null, then this has the value to set the FileOutputRolloverMode property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the rolloverMode argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetFileRolloverMode( RolloverMode? rolloverMode, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue<RolloverMode>( "FileOutputRolloverMode", rolloverMode, subkeyPath );
        }
        #endregion

        #region FileOutputRollPoint
        /// <summary>
        /// Get the frequency with which the logging output is rolled over to new files,
        /// if the RolloverMode is set to either Date or Composite.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-RollPoint, which if null indicates no explicit setting applies</returns>
        public RollPoint? GetFileTimeRollPoint( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableEnum<RollPoint>( "FileOutputRollPoint", subkeyPath );
        }

        /// <summary>
        /// Set the frequency with which the logging output is rolled over to new files,
        /// if the RolloverMode is set to either Date or Composite.
        /// </summary>
        /// <param name="rollpointValue">if non-null, then this has the value to set the FileOutputRollPoint property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the rollpointValue argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetFileTimeRollPoint( RollPoint? rollpointValue, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue<RollPoint>( "FileOutputRollPoint", rollpointValue, subkeyPath );
        }
        #endregion

        #region IsFileOutputToInsertHeader
        /// <summary>
        /// Get the IsFileOutputToInsertHeader property for the given subject-program and host
        /// as a nullable-boolean value, which is null if no setting applies.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the IsFileOutputToInsertHeader property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsInsertingBannerText( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsFileOutputToInsertHeader", subkeyPath );
        }

        /// <summary>
        /// Set the IsFileOutputToInsertHeader property for the given subject-program and host.
        /// </summary>
        /// <param name="isInsertingBannerText">if non-null, then this has the value to set the IsFileOutputToInsertHeader property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isInsertingBannerText argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsInsertingBannerText( bool? isInsertingBannerText, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsFileOutputToInsertHeader", isInsertingBannerText, subkeyPath );
        }
        #endregion

        #region IsFileOutputToInsertLineBetweenTraces
        /// <summary>
        /// Get whether to override the LogManager setting for the localhost that dictates whether to
        /// separate log traces within the log file with an empty line. Default is null, meaning no override.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the IsFileOutputToInsertLineBetweenTraces property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsInsertingLineBetweenTraces( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsFileOutputToInsertLineBetweenTraces", subkeyPath );
        }

        /// <summary>
        /// Set whether to override the LogManager setting for the localhost that dictates whether to
        /// separate log traces within the log file with an empty line. Default is null, meaning no override.
        /// </summary>
        /// <param name="isInsertingBannerText">if non-null, then this has the value to set the IsFileOutputToInsertLineBetweenTraces property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isInsertingBannerText argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsInsertingLineBetweenTraces( bool? isInsertingBannerText, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsFileOutputToInsertLineBetweenTraces", isInsertingBannerText, subkeyPath );
        }
        #endregion

        #region IsToCreateNewOutputFileUponStartup
        /// <summary>
        /// Get whether to create a new, empty output-file upon the first log-output when the subject-program is run,
        /// rolling-over any pre-existing output file to a new pathname in order to save it's content -
        /// as opposed to just appending to the exiting contents of the file.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-boolean which, if non-null, has the value of the IsToCreateNewOutputFileUponStartup property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public bool? GetIsToCreateNewOutputFileUponStartup( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetNullableBoolean( "IsToCreateNewOutputFileUponStartup", subkeyPath );
        }

        /// <summary>
        /// Set whether to create a new, empty output-file upon the first log-output when the subject-program is run,
        /// rolling-over any pre-existing output file to a new pathname in order to save it's content -
        /// as opposed to just appending to the exiting contents of the file.
        /// </summary>
        /// <param name="isEnabled">if non-null, then this has the value to set the IsToCreateNewOutputFileUponStartup property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isEnabled argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetIsToCreateNewOutputFileUponStartup( bool? isEnabled, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "IsToCreateNewOutputFileUponStartup", isEnabled, subkeyPath );
        }
        #endregion

        #region MaxFileSize
        /// <summary>
        /// Get the MaxFileSize property for the given subject-program and host
        /// as a nullable-boolean value, which is null if no setting applies.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-Int64 which, if non-null, has the value of the MaxFileSize property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public Int64? GetMaxFileSize( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return this.GetNullableInt64( "MaxFileSize", subkeyPath );
        }

        /// <summary>
        /// Set the MaxFileSize property for the given subject-program and host.
        /// </summary>
        /// <param name="nullableMaxSize">if non-null, then this has the value to set the MaxFileSize property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the isInsertingBannerText argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetMaxFileSize( Int64? nullableMaxSize, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "MaxFileSize", nullableMaxSize, subkeyPath );
        }
        #endregion

        #region MaxNumberOfFileRollovers
        /// <summary>
        /// Get the MaxNumberOfFileRollovers property for the given subject-program and host
        /// as a nullable-boolean value, which is null if no setting applies.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a nullable-Integer which, if non-null, has the value of the MaxNumberOfFileRollovers property for the given subject-program, and if null indicates no explicit setting applies</returns>
        /// <remarks>
        /// If this is set to zero, there will be no backup files and the log file
        /// will be truncated when it reaches MaxFileSize.
        /// This applies regardless of the FileOutputRolloverMode.
        /// </remarks>
        public int? GetMaxNumberOfFileRollovers( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return this.GetNullableInteger( "MaxNumberOfFileRollovers", subkeyPath );
        }

        /// <summary>
        /// Set the MaxNumberOfFileRollovers property for the given subject-program and host.
        /// </summary>
        /// <param name="nullableMaxNumber">if non-null, then this has the value to set the MaxNumberOfFileRollovers property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the MaxNumberOfFileRollovers argument if you want to set this to indicate no-preference.
        /// If this is set to zero, there will be no backup files and the log file
        /// will be truncated when it reaches MaxFileSize.
        /// This applies regardless of the FileOutputRolloverMode.
        /// </remarks>
        public void SetMaxNumberOfFileRollovers( int? nullableMaxNumber, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "MaxNumberOfFileRollovers", nullableMaxNumber, subkeyPath );
        }
        #endregion

        #endregion file-output

        #region AddLogger
        /// <summary>
        /// Put values describing the essential characteristics of the given Logger into the Windows Registry.
        /// The Logger's Name, SourceHostName, and FileOutputPath are saved with it.
        /// </summary>
        /// <param name="logger">the Logger to store in the Registry</param>
        /// <param name="subjectProgramName">the name of the subject-program that this logger is associated with</param>
        /// <param name="sourceHost">the computer upon which the subject-program was running (use null to indicate localhost</param>
        /// <returns>true if a logger by that name already existed (although the values are still overwritten), false if it is a new logger</returns>
        public bool AddLogger( Logger logger, string subjectProgramName, string sourceHost )
        {
            // Decide what name to use for this logger..
            string nameToUse;
            if (StringLib.HasNothing( logger.Name ))
            {
                nameToUse = LogManager.NameOfDefaultLogger;
            }
            else
            {
                nameToUse = logger.Name;
            }
            // If the source-host is the same as the local computer, substitute the string "localhost" for it.
            if (sourceHost == null
                || sourceHost.Equals( "LOCALHOST", StringComparison.OrdinalIgnoreCase )
                || sourceHost.Equals( Environment.MachineName, StringComparison.OrdinalIgnoreCase ))
            {
                sourceHost = "localhost";
            }

            if (IsLoggerInRegistry( nameToUse, subjectProgramName, sourceHost ))
            {
                // Ensure these program-level properties are set to the current value.
                SetFileOutputFolder( LogManager.Config.FileOutputFolder, subjectProgramName, sourceHost );
                SetFileOutputFilename( LogManager.Config.FileOutputFilename, subjectProgramName, sourceHost );
                // Return true to indicate to the caller that we did find this logger in the Registry.
                return true;
            }

            // If we reach this point, then the given logger was not found in the Registry - so add it.
            string subkeyForLoggers = GetSubkeyForLoggers( subjectProgramName, sourceHost );
            string subkeyForThisLogger = GetSubkeyForLogger( nameToUse, subjectProgramName, sourceHost );
            SetKey( subkeyForLoggers, nameToUse );
            SetFileOutputFolder( LogManager.Config.FileOutputFolder, subjectProgramName, sourceHost );
            SetFileOutputFilename( LogManager.Config.FileOutputFilename, subjectProgramName, sourceHost );
            string stringTimeStamp = LogManager.LogRecordFormatter.GetTimeStamp( DateTime.Now, null, false );
            SetValue( "WhenAdded", stringTimeStamp, subkeyForThisLogger );
            // Return false to indicate to the caller that we did not find this logger in the Registry.
            return false;
        }
        #endregion

        #region ClearLogger
        /// <summary>
        /// Remove the setting for the given Logger.
        /// </summary>
        /// <param name="loggerName">the name of the logger - this is how we identify it</param>
        /// <param name="subjectProgramName">the subject-program</param>
        /// <param name="sourceHost">the computer-host upn which the subject-program is executing</param>
        public void ClearLogger( string loggerName, string subjectProgramName, string sourceHost )
        {
            var keyPath = GetSubkeyForLogger( loggerName, subjectProgramName, sourceHost );
            DeleteKey( keyPath );
        }
        #endregion

        #region ClearAllLoggers
        /// <summary>
        /// Remove all Registry keys and values that specify the individual loggers,
        /// without effecting anything else under the source-hosts or subject-program settings.
        /// </summary>
        public void ClearAllLoggers()
        {
            var hosts = GetLoggerSourceHosts();
            if (hosts != null)
            {
                foreach (var host in hosts)
                {
                    ClearLoggersForSourceHost( host );
                }
            }
        }
        #endregion

        #region ClearAllSourceHosts
        /// <summary>
        /// Remove all source-host trees from the Registry, by simply deleting the "LogNut\Hosts" key.
        /// </summary>
        public void ClearAllSourceHosts()
        {
            DeleteKey( HostsSubkeyName );
        }
        #endregion

        #region ClearLoggersForSourceHost
        /// <summary>
        /// Remove the setting for all Loggers on the given computer.
        /// </summary>
        /// <param name="sourceHost">the computer-host upn which the subject-program is executing</param>
        public void ClearLoggersForSourceHost( string sourceHost )
        {
            var programs = GetLoggerSubjectProgramsForSourceHost( sourceHost );
            if (programs != null)
            {
                foreach (var program in programs)
                {
                    ClearLoggersForSubjectProgram( program, sourceHost );
                }
            }
        }
        #endregion

        #region ClearLoggersForSubjectProgram
        /// <summary>
        /// Remove the setting for all Loggers for the given subject-program on the given computer.
        /// </summary>
        /// <param name="subjectProgramName">the subject-program</param>
        /// <param name="sourceHost">the computer-host upn which the subject-program is executing</param>
        public void ClearLoggersForSubjectProgram( string subjectProgramName, string sourceHost )
        {
            var keyPath = GetSubkeyForLoggers( subjectProgramName, sourceHost );
            DeleteKey( keyPath );
        }
        #endregion

        #region ClearSourceHost
        /// <summary>
        /// Remove all settings for the given computer.
        /// </summary>
        /// <param name="sourceHost">the computer-host upn which the subject-program is executing</param>
        public void ClearSourceHost( string sourceHost )
        {
            var keyPath = GetSubkeyForSourceHost( sourceHost );
            DeleteKey( keyPath );
        }
        #endregion

        #region ClearSubjectProgram
        /// <summary>
        /// Remove the Registry settings for the given subject-program from under the given source-host.
        /// </summary>
        /// <param name="subjectProgramName">the subject-program to remove</param>
        /// <param name="sourceHost">the computer-name that the subject-program comes under (optional - defaults to "localhost"</param>
        public void ClearSubjectProgram( string subjectProgramName, string sourceHost )
        {
            var keyPath = GetSubkeyForProgram( subjectProgramName, sourceHost );
            DeleteKey( keyPath );
        }
        #endregion

        #region ClearSubjectProgramsForSourceHost
        /// <summary>
        /// Remove the Registry settings for any and all subject-programs from under the given source-host.
        /// </summary>
        /// <param name="sourceHost">the computer-name that the subject-program comes under (optional - null indicates the local host)</param>
        public void ClearSubjectProgramsForSourceHost( string sourceHost )
        {
            var programs = GetLoggerSubjectProgramsForSourceHost( sourceHost );
            if (programs != null)
            {
                // Iterate through all of the subject-program keys under this host, and delete them..
                foreach (var program in programs)
                {
                    ClearSubjectProgram( program, sourceHost );
                }
            }
        }
        #endregion

        #region EnsureLogNutRegistryTreeExists
        /// <summary>
        /// If it is not present already, create the Registry keys that this application needs to run.
        /// 
        /// </summary>
        /// <param name="sourceHost">the name of the computer that the subject-program is running on. If null - "localhost" is assumed (optional)</param>
        /// <param name="subjectProgramName">the name of the program that would be using LogNut to do logging (optional)</param>
        /// <remarks>
        /// The Registry key path is of the form:
        /// {HKLM or HKCU}\Software\DesignForge\LogNut\Programs\{source-host}\{subject-program name}\Loggers
        /// If you omit both sourceHost and subjectProgramName, then the key path is only checked for LogNut itself.
        /// If you specify only the sourceHost, then the key path is checked down to LogNut\Programs\{source-host}
        /// If you specify subjectProgramName and omit sourceHost or set it to null, then "localhost" is used for sourceHost
        /// and the key path down to Loggers is checked.
        /// </remarks>
        public void EnsureLogNutRegistryTreeExists( string subjectProgramName, string sourceHost )
        {
            if (sourceHost == null && subjectProgramName == null)
            {
                // If the HKLM\Software\DesignForge\LogNut doesn't exist, create that.
                EnsureBaseKeyPathExists( false );
            }
            else // one or the other param is non-null
            {
                if (subjectProgramName == null)
                {
                    // sourceHost must be non-null, so just check that.
                    string keyPathForHost = this.GetSubkeyForSourceHost( sourceHost );
                    EnsureExists( keyPathForHost );
                }
                else // a subjectProgramName is specified.
                {
                    // If sourceHost wasn't specified, default it to the local box.
                    if (sourceHost == null)
                    {
                        sourceHost = "localhost";
                    }
                    // If the HKLM\Software\DesignForge\LogNut\Programs\{source-host}\{subject-program}\Loggers doesn't exist, create that.
                    string loggersKeyPath = this.GetSubkeyForLoggers( subjectProgramName, sourceHost );
                    EnsureExists( loggersKeyPath );
                }
            }
        }
        #endregion

        #region GetLoggerNamesForProgram
        /// <summary>
        /// Get the logger names that are listed under LogNut\Programs\{source-host}\{subject-program name}\Loggers, as an array of strings.
        /// These are only written when a logger actually sends out a log record, and is used by LogNutVUr.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program that we are interested in</param>
        /// <param name="sourceHost">the name of the computer that the subject-program would be running on. Leave this null to indicate the localhost</param>
        /// <returns>a list of the names of all loggers for whom an entry is found within the Registry for the given host and subject-program</returns>
        public IList<string> GetLoggerNamesForProgram( string subjectProgramName, string sourceHost )
        {
            string loggersKeyPath = this.GetSubkeyForLoggers( subjectProgramName, sourceHost );
            var keyNames = GetSubkeyNames( loggersKeyPath, false );
            if (keyNames.Count > 0)
            {
                // Exclude from our array, keys that the user has only just manually created.
                // They always start with the name "New Key #1", and so forth, so use that to distinquish them.
                var okayNames = (from x in keyNames where (x.IndexOf( "New Key #" ) == -1) select x);
                return okayNames.ToList();
            }
            else
            {
                //TODO: Make a static var for the empty List?
                return new List<string>();
            }
        }
        #endregion

        #region GetLoggerSourceHosts
        /// <summary>
        /// Get all of the source-hosts (that is, the computer names) that are indicated as having hosted subject-programs
        /// that have used the LogNut API.
        /// </summary>
        /// <returns>a list of strings containing the source-hosts, or an empty list if none (never null)</returns>
        public IList<string> GetLoggerSourceHosts()
        {
            if (IsKeyPresentUnderBasePath( HostsSubkeyName ))
            {
                _hosts = GetSubkeyNames( HostsSubkeyName, false );
            }
            else
            {
                //CBL
                // Actually, this should be an observable model of some kind,
                // since computers may be added to this list as we're running!
                _hosts.Clear();
            }
            return _hosts;
        }

        #endregion

        #region GetAllLoggerSubjectPrograms
        /// <summary>
        /// Get all of the subject-program names that are indicated as being available,
        /// ordered by their respective source-host.
        /// </summary>
        /// <returns>an array of strings containing the subject-program names</returns>
        public IList<string> GetAllLoggerSubjectPrograms()
        {
            List<string> allPrograms = new List<string>();
            var hosts = this.GetLoggerSourceHosts();
            foreach (string host in hosts)
            {
                var programs = GetLoggerSubjectProgramsForSourceHost( host );
                foreach (string program in programs)
                {
                    var loggersForThisProgram = this.GetLoggerNamesForProgram( program, host );
                    if (loggersForThisProgram.Count > 0)
                    {
                        foreach (string loggerName in loggersForThisProgram)
                        {
                            allPrograms.Add( loggerName );
                        }
                    }
                }
            }
            return allPrograms;
        }
        #endregion

        #region GetLoggerSubjectProgramsForSourceHost
        /// <summary>
        /// Get all of the subject-program names that are associated with the given source-host.
        /// </summary>
        /// <param name="sourceHost">the name of the computer for which the subject-program names are desired</param>
        /// <returns>a list of strings containing the subject-program names</returns>
        public IList<string> GetLoggerSubjectProgramsForSourceHost( string sourceHost )
        {
            string subkey = this.GetSubkeyForSourceHost( sourceHost );
            return GetSubkeyNames( subkey + @"\" + ProgramNamesSubkeyName, false );
        }
        #endregion

        #region IsLoggerInRegistry
        /// <summary>
        /// Return true if the Windows Registry contains an entry for the given source-host, subject-program and logger.
        /// </summary>
        /// <param name="loggerName">the name of the logger to check for</param>
        /// <param name="subjectProgramName">the name of the subject-program that we are interested in</param>
        /// <param name="sourceHost">the name of the computer that the subject-program would be running on. Leave this null to indicate the localhost</param>
        /// <returns>true if the Windows Registry contains an entry for this logger</returns>
        public bool IsLoggerInRegistry( string loggerName, string subjectProgramName, string sourceHost )
        {
            bool isFound = false;
            bool isLocal = IsLocal( sourceHost );
            // Check for sourceHost and subjectProgramName..
#if (PRE_4)
            if (IsSubjectProgramInRegistry( subjectProgramName, sourceHost ))
            {
                // Check for loggerName..
                var loggerNames = GetLoggerNamesForProgram( subjectProgramName, sourceHost );
                if (loggerNames.Contains( loggerName ))
                {
                    isFound = true;
                }
            }
#else
            if (IsSubjectProgramInRegistry( subjectProgramName: subjectProgramName, sourceHost: sourceHost ))
            {
                // Check for loggerName..
                var loggerNames = GetLoggerNamesForProgram( subjectProgramName: subjectProgramName, sourceHost: sourceHost );
                if (loggerNames.Contains( loggerName ))
                {
                    isFound = true;
                }
            }
#endif
            return isFound;
        }
        #endregion

        #region IsSubjectProgramInRegistry
        /// <summary>
        /// Return true if a Windows Registry entry for the given subject-program and source-host is present.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program that we are interested in</param>
        /// <param name="sourceHost">the name of the computer that the subject-program would be running on. Leave this null to indicate the localhost</param>
        /// <returns>true if the Windows Registry contains an entry for any logger for the given subject-program on the indicated source-host</returns>
        public bool IsSubjectProgramInRegistry( string subjectProgramName, string sourceHost )
        {
            bool isFound = false;
            bool isLocal = IsLocal( sourceHost );
            if (isLocal)
            {
                // If the subject-program is empty and it's on the local host,
                // then the answer is automatically true since no sub-keys are required.
                if (StringLib.HasNothing( subjectProgramName ))
                {
                    isFound = true;
                }
                else
                {
                    isFound = IsKeyPresentUnderBasePath( ProgramNamesSubkeyName + @"\" + subjectProgramName );
                }
            }
            else // sourceHost refers to a non-local box
            {
                // Check for sourceHost..
                if (IsSourceHostInRegistry( sourceHost ))
                {
                    // Check for subjectProgram..
                    var programs = GetLoggerSubjectProgramsForSourceHost( sourceHost );
                    if (programs.Contains( subjectProgramName ))
                    {
                        isFound = true;
                    }
                }
            }
            return isFound;
        }
        #endregion

        #region IsLocal
        /// <summary>
        /// Return true if the given host-name is localhost, the local machine-name, or empty.
        /// </summary>
        /// <param name="hostName">a computer-host name</param>
        /// <returns>true if hostName is the local box</returns>
        public static bool IsLocal( string hostName )
        {
            if (StringLib.HasNothing( hostName ))
            {
                return true;
            }
            else
            {
                string hostLowercase = hostName.ToLower();
                return hostLowercase == "localhost" || hostLowercase == Environment.MachineName.ToLower();
            }
        }
        #endregion

        #region IsSourceHostInRegistry
        /// <summary>
        /// Return true if the given computer-host is listed within the Window Registry under the
        /// appropriate LogNut key, which would be LogNut/Hosts.
        /// </summary>
        /// <param name="sourceHost">the name of the computer that the subject-program would be running on. Leave this null to indicate the localhost</param>
        /// <returns>true if the Windows Registry contains a reference to the given source-host</returns>
        public bool IsSourceHostInRegistry( string sourceHost )
        {
            var hosts = GetLoggerSourceHosts();
            return hosts.Contains( sourceHost );
        }
        #endregion

        #region LogNutVUr settings

        #region IsWatchingForLogger
        /// <summary>
        /// Get the IsWatching value from the Registry for the given logger-name, which indicates whether a viewer is currently set to watch it.
        /// False is returned if the logger-name is not found.
        /// </summary>
        /// <param name="sourceHost">the host computer, or null to indicate the local host</param>
        /// <param name="subjectProgramName">the name of the subject-program that the given logger is associated with</param>
        /// <param name="loggerName">The name of the logger to look up, which is the same would be read from the Registry</param>
        /// <returns>true if it is currently set to be viewed by a LogNutVUr</returns>
        public bool GetIsWatchingForLogger( string sourceHost, string subjectProgramName, string loggerName )
        {
#if DEBUG
            if (loggerName == null)
            {
                throw new ArgumentNullException( "loggerName" );
            }
#endif
            string subkey = this.GetSubkeyForLogger( loggerName,
                                                    subjectProgramName,
                                                    sourceHost );
            return GetBoolean( "IsWatching", false, subkey );
        }

        /// <summary>
        /// Set the IsWatching value in the Registry for the given logger-name, which is what a viewer uses to determine whether to watch that logger.
        /// </summary>
        /// <param name="sourceHost">the host computer, or null to indicate the local host</param>
        /// <param name="subjectProgramName">the name of the subject-program that the given logger is associated with</param>
        /// <param name="loggerName">The name of the logger as listed under Registry key Loggers. This must not be null or empty.</param>
        /// <param name="isWatching">The value to set for the Registry value IsWatching.</param>
        public void SetIsWatchingForLogger( string sourceHost, string subjectProgramName, string loggerName, bool isWatching )
        {
#if DEBUG
            if (sourceHost == null)
            {
                throw new ArgumentNullException( "sourceHost" );
            }
            if (subjectProgramName == null)
            {
                throw new ArgumentNullException( "subjectProgramName" );
            }
            if (loggerName == null)
            {
                throw new ArgumentNullException( "loggerName" );
            }
            if (StringLib.HasNothing( loggerName ))
            {
                throw new ArgumentException( "Must not be an empty string!", "loggerName" );
            }
#endif
            string keyPath = this.GetSubkeyForLogger( loggerName, subjectProgramName, sourceHost );
            SetValue( "IsWatching", isWatching, keyPath );
        }
        #endregion

        #region ViewerMonitorMethod
        /// <summary>
        /// Get or set the the method that the LogNutVUr uses to watch the log output.
        /// 0 = unknown
        /// 1 = file
        /// </summary>
        public int ViewerMonitorMethod
        {
            get
            {
                // Only read it once, until something gives it a non-zero value, and thereafter use the saved value whenever needed.
                if (_viewerMonitorMethod == 0)
                {
                    _viewerMonitorMethod = GetInteger( "MonitorMethod", 0, "Viewer" );
                }
                return _viewerMonitorMethod;
            }
            set
            {
                if (value != _viewerMonitorMethod)
                {
                    _viewerMonitorMethod = value;
                    SetValue( "MonitorMethod", value, "Viewer" );
                }
            }
        }
        #endregion

        #region ViewingWhatLogger
        /// <summary>
        /// Get or set the name of the logger that the LogNutVUr currently cares about.
        /// </summary>
        public string ViewingWhatLogger
        {
            get
            {
                // Only read it once, and thereafter use the saved value whenever needed.
                if (String.IsNullOrEmpty( _viewingWhatLogger ))
                {
                    _viewingWhatLogger = GetString( "CurrentLogger", String.Empty, "Viewer" );
                }
                return _viewingWhatLogger;
            }
            set
            {
                if (value != _viewingWhatLogger)
                {
                    _viewingWhatLogger = value;
                    SetValue( "CurrentLogger", value, "Viewer" );
                }
            }
        }
        #endregion

        #region VUrHostIPv4Address
        /// <summary>
        /// Get the VUrHostIPv4Address property.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <returns>a string which, if non-empty, has the value of the property for the given subject-program, and if null indicates no explicit setting applies</returns>
        public string GetVUrHostIPv4Address( string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            return GetString( "VUrHostIPv4Address", null, subkeyPath );
        }

        /// <summary>
        /// Set the VUrHostIPv4Address property.
        /// </summary>
        /// <param name="stringValue">if non-null, then this has the value to set the property to for the given subject-program and host</param>
        /// <param name="subjectProgramName">the name of the subject-program whose property we are interested in</param>
        /// <param name="sourceHost">the computer the subject-program would be running on when the setting is applied. Leave this null to indicate localhost.</param>
        /// <remarks>
        /// Use null for the stringValue argument if you want to set this to indicate no-preference.
        /// </remarks>
        public void SetVUrHostIPv4Address( string stringValue, string subjectProgramName, string sourceHost )
        {
            string subkeyPath = this.GetSubkeyForProgram( subjectProgramName, sourceHost );
            SetValue( "VUrHostIPv4Address", stringValue, subkeyPath );
        }
        #endregion

        #endregion LogNutVUr settings

        #region GetEntityNameInRegistry
        /// <summary>
        /// Return the owner/organization name to use within the Windows Registry to contain all of the settings there for this program.
        /// </summary>
        /// <returns>the string DesignForge</returns>
        public override string GetEntityNameInRegistry()
        {
            return "DesignForge";
        }
        #endregion

        #region GetProductNameInRegistry
        /// <summary>
        /// Return the owner/organization name to use within the Windows Registry to contain all of the settings there for this program.
        /// </summary>
        /// <returns>the string "LogNut" unless this is in test-mode in which case it is "LogNutTest"</returns>
        public override string GetProductNameInRegistry()
        {
            if (!_isForTesting)
            {
                return "LogNut";
            }
            else
            {
                return "LogNutTest";
            }
        }
        #endregion

        #region internal implementation

        #region GetSubkeyFor Program, Loggers, Logger, LogNutVUr, LogNut Control Panel

        #region GetSubkeyForLogger
        /// <summary>
        /// Return the string to use for the Registry subkey within LogNut for a given host, program, and logger.
        /// This does not actually access the Registry. It simply computes a string value.
        /// </summary>
        /// <param name="loggerName">the name of the logger</param>
        /// <param name="subjectProgramName">the program that the logger is for</param>
        /// <param name="sourceHost">the computer that the subject-program is running on</param>
        /// <returns>a string denoting the Windows Registry Key path containing the settings for that logger</returns>
        public string GetSubkeyForLogger( string loggerName, string subjectProgramName, string sourceHost )
        {
            //TODO
            // I'd like to make this private or protected, but can I still unit-test it if I do?
#if DEBUG
            if (StringLib.HasNothing( loggerName ))
            {
                throw new ArgumentException( "loggerName must not be empty." );
            }
#endif
            return GetSubkeyForLoggers( subjectProgramName, sourceHost ) + @"\" + loggerName;
        }
        #endregion

        #region GetSubkeyForLoggers
        /// <summary>
        /// Return the string to use for the Registry subkey under LogNut for the Loggers key for a given host and program.
        /// This should be of the form "Programs\{source-hosts}\{subject-program}\Loggers".
        /// </summary>
        /// <param name="subjectProgramName">the program that the logger is for</param>
        /// <param name="sourceHost">the computer that the subject-program is running on</param>
        /// <returns>a string denoting the Windows Registry Key path under which the logger settings exist</returns>
        public string GetSubkeyForLoggers( string subjectProgramName, string sourceHost )
        {
            string programSubkey = GetSubkeyForProgram( subjectProgramName, sourceHost );
            if (StringLib.HasNothing( programSubkey ))
            {
                return @"\Loggers";
            }
            else
            {
                return programSubkey + @"\Loggers";
            }
        }
        #endregion

        #region GetSubkeyForLogNutControlPanel
        /// <summary>
        /// Return the key name used for LogNutControlPanel within the Windows Registry.
        /// </summary>
        /// <returns>the string literal that is used within the Windows Registry for the subkey containing the LogNut-Control-Panel settings (this is "LogNutCP")</returns>
        public string GetSubkeyForLogNutControlPanel()
        {
            return @"LogNutCP";
        }
        #endregion

        #region GetSubkeyForLogNutVUr
        /// <summary>
        /// Return the key name used for LogNutVUr within the Windows Registry.
        /// </summary>
        /// <returns>the string literal that is used within the Windows Registry for the subkey containing the LogNutVUr settings (this is "LogNutVUr")</returns>
        public string GetSubkeyForLogNutVUr()
        {
            return @"LogNutVUr";
        }
        #endregion

        #region GetSubkeyForProgram
        /// <summary>
        /// Return the key name to use for the given program when on the local host.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program</param>
        /// <returns>the subkey name</returns>
        public string GetSubkeyForProgram( string subjectProgramName )
        {
            return GetSubkeyForProgram( subjectProgramName, null );
        }

        /// <summary>
        /// Return the key name to use for the given program when on the given host.
        /// </summary>
        /// <param name="subjectProgramName">the name of the subject-program</param>
        /// <param name="sourceHost">the computer upon which the subject-program would be running</param>
        /// <returns>the subkey name</returns>
        public string GetSubkeyForProgram( string subjectProgramName, string sourceHost )
        {
            bool isLocal = IsLocal( sourceHost );
            // If the sourceHost is the local box, leave that part off.
            if (isLocal)
            {
                if (subjectProgramName.HasNothing())
                {
                    return null;
                }
                else
                {
                    return ProgramNamesSubkeyName + @"\" + subjectProgramName;
                }
            }
            else // sourceHost refers to a non-local box.
            {
                if (subjectProgramName.HasNothing())
                {
                    return HostsSubkeyName + @"\" + sourceHost;
                }
                else // there is both a non-local sourceHost and a subjectProgramName
                {
                    return HostsSubkeyName + @"\" + sourceHost + @"\" + ProgramNamesSubkeyName + @"\" + subjectProgramName;
                }
            }
        }
        #endregion

        #region GetSubkeyForSourceHost
        /// <summary>
        /// Return the key name to use for the given host.
        /// </summary>
        /// <param name="sourceHost">the computer upon which the subject-program would be running</param>
        /// <returns>the subkey name</returns>
        public string GetSubkeyForSourceHost( string sourceHost )
        {
            return GetSubkeyForProgram( null, sourceHost );
        }
        #endregion

        #endregion

        #region HostsSubkeyName
        /// <summary>
        /// Get the name of the Registry key (under the BaseKeyPath) from within which
        /// all of the other hosting computers will be accessed. Presently this is simply "Hosts".
        /// </summary>
        public static string HostsSubkeyName
        {
            get { return "Hosts"; }
        }
        #endregion

        #region ProgramNamesSubkeyName
        /// <summary>
        /// Get the name of the Registry key (under the BaseKeyPath) from within which
        /// all of the subject-program names will be accessed. Presently this is simply "Programs".
        /// </summary>
        public static string ProgramNamesSubkeyName
        {
            get { return "Programs"; }
        }
        #endregion

        #region fields

        /// <summary>
        /// This is the collection of hosts that may be running LogNut IPC.
        /// </summary>
        private IList<string> _hosts = new List<string>();

        /// <summary>
        /// This indicates how the LogNutVUr is watching the log output.
        /// 0 = unknown
        /// 1 = file
        /// </summary>
        private int _viewerMonitorMethod;

        /// <summary>
        /// This saves the name of the logger that the LogNutVUr currently cares about.
        /// </summary>
        private string _viewingWhatLogger;

        #endregion fields

        #endregion internal implementation
    }
}
