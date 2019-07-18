// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewRelicAgentSettings.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   NewRelic agent settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExitGames.Diagnostics.Configuration
{
    #region using directives

    using System;
    using System.Configuration;

    #endregion

    public class NewRelicAgentSettings : CounterSampleSenderSettings
    {
        #region Fields

        private const string DefaultEndpoint = "https://platform-api.newrelic.com/platform/v1/metrics";
        
        #endregion

        #region Properties

        /// <summary>
        ///   NewRelic Agent id
        /// </summary>
        [ConfigurationProperty("agentId", IsRequired = false, DefaultValue = "com.exitgames.plugins.newrelic.agent")]
        public string AgentId
        {
            get
            {
                return (string)this["agentId"];
            }
            set
            {
                this["agentId"] = value;
            }
        }

        /// <summary>
        ///   NewRelic Agent version
        /// </summary>
        [ConfigurationProperty("version", IsRequired = false, DefaultValue = "1.0.0")]
        public string Version
        {
            get
            {
                return (string)this["version"];
            }
            set
            {
                this["version"] = value;
            }
        }

        /// <summary>
        ///   A human readable string denotes the name of this Agent in the New Relic service.
        /// </summary>
        [ConfigurationProperty("agentName", IsRequired = false, DefaultValue = "")]
        public string AgentName
        {
            get
            {
                return (string)this["agentName"];
            }
            set
            {
                this["agentName"] = value;
            }
        }

        /// <summary>
        ///   License key.
        /// </summary>
        [ConfigurationProperty("licenseKey", IsRequired = true)]
        public string LicenseKey
        {
            get
            {
                return (string)this["licenseKey"];
            }
            set
            {
                this["licenseKey"] = value;
            }
        }
        
        #endregion

        #region Constructors and Destructors

        public NewRelicAgentSettings()
        {
            this.Endpoint = new Uri(DefaultEndpoint);
        }

        public NewRelicAgentSettings(CounterSampleSenderSettings s) : base(s)
        {
            if (this.Endpoint == null)
            {
                this.Endpoint = new Uri(DefaultEndpoint);
            }
        }

        #endregion
    }
}
