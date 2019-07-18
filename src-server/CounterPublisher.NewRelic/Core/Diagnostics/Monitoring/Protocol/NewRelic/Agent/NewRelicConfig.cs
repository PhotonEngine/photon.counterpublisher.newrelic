// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewRelicConfig.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   NewRelic configuration provider.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExitGames.Diagnostics.Monitoring.Protocol.NewRelic.Agent
{
    #region using directives
    
    using System.IO;
    using System.Reflection;

    using global::NewRelic.Platform.Sdk.Configuration;
    using global::NewRelic.Platform.Sdk.Extensions;

    using ExitGames.Diagnostics.Configuration;
    
    #endregion

    public class NewRelicConfig : INewRelicConfig
    {
        #region Fields

        private const string DefaultEndpoint = "https://platform-api.newrelic.com/platform/v1/metrics";
        private const string DefaultLogFileName = "newrelic_plugin.log";
        private const string DefaultLogFilePath = @"logs";
        private const int DefaultLogLimitInKiloBytes = 25600;

        #endregion

        #region Properties

        public string LicenseKey { get; set; }
        
        public string Endpoint { get; set; }
        
        public LogLevel LogLevel { get; set; }
        public string LogFileName { get; set; }
        public string LogFilePath { get; set; }
        public long LogLimitInKiloBytes { get; set; }

        public string ProxyHost { get; set; }
        public int? ProxyPort { get; set; }
        public string ProxyUserName { get; set; }
        public string ProxyPassword { get; set; }

        public int? PollInterval { get; set; }

        // Exposed for testing
        public int? NewRelicMaxIterations { get; set; }

        #endregion

        #region Constructors and Destructors

        public NewRelicConfig()
        {
            // set default values
            this.Endpoint = DefaultEndpoint;

            this.LogLevel = LogLevel.Info;
            this.LogFileName = DefaultLogFileName;
            this.LogFilePath = Path.Combine(Assembly.GetExecutingAssembly().GetLocalPath(), DefaultLogFilePath);
            this.LogLimitInKiloBytes = DefaultLogLimitInKiloBytes;
        }

        public NewRelicConfig(NewRelicAgentSettings s)
        {
            this.LicenseKey = s.LicenseKey;
            
            this.Endpoint = s.Endpoint.OriginalString;

            this.LogLevel = global::NewRelic.Platform.Sdk.Configuration.NewRelicConfig.Instance.LogLevel;
            this.LogFileName = global::NewRelic.Platform.Sdk.Configuration.NewRelicConfig.Instance.LogFileName;
            this.LogFilePath = global::NewRelic.Platform.Sdk.Configuration.NewRelicConfig.Instance.LogFilePath;
            this.LogLimitInKiloBytes = global::NewRelic.Platform.Sdk.Configuration.NewRelicConfig.Instance.LogLimitInKiloBytes;
        }

        #endregion
    }
}
