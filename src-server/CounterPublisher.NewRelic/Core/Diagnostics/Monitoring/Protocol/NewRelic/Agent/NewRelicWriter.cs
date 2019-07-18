// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewRelicWriter.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   NewRelic writer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExitGames.Diagnostics.Monitoring.Protocol.NewRelic.Agent
{
    #region using directives

    using System;
    
    using ExitGames.Diagnostics.Configuration;
    using ExitGames.Diagnostics.Monitoring;
    using ExitGames.Diagnostics.Monitoring.Protocol;

    #endregion

    /// <summary>
    /// Metric data is sent as an HTTP POST of JSON data using this URI:
    /// https://platform-api.newrelic.com/platform/v1/metrics
    /// The MIME-type for the POST is application/json.
    /// The New Relic Plugins product is optimized to post the data at a frequency of once every 60 seconds. 
    /// 
    /// POST frequency and limitations from https://docs.newrelic.com/docs/plugins/plugin-developer-resources/developer-reference/plugin-api-specification:
    /// Components:	500	Number of distinct components currently tracked per account.
    /// Metrics per component:	10,000	Total number of unique metrics per component. Take precautions to ensure metric names are not generated too dynamically. Even if the number of metrics being sent in each individual post is small, over time they may add up to a large number of unique metrics. When the number of metrics for a given component exceeds this limit, the server may start aggregating metrics together by globbing segments of the metric name with an asterisk (*).
    /// Metrics per post:	20,000	Number of metrics sent per post. A post may send data for multiple components in a single request as long as the total number of metrics in the request does not exceed this limit.
    /// Frequency of post:	2 per minute	Frequency of update. Agents are expected to send data no more frequently than 1 per minute.
    /// </summary>
    public class NewRelicWriter : ICounterSampleWriter
    {
        #region Constants and Fields
        
        public const int MinSendInterval = 60;

        private readonly NewRelicAgentSettings settings;
        
        private CounterSampleSenderBase sender;

        private bool _disposed;

        private NewRelicAgent agent;
        
        #endregion

        #region Properties

        public bool Ready
        {
            get { return true; }
        }

        public string SenderId
        {
            get { return this.sender.SenderId; }
        }

        #endregion

        #region Constructors and Destructors

        public NewRelicWriter(NewRelicAgentSettings s)
        {
            this.settings = s;
        }

        ~NewRelicWriter()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods
        
        public void Start(CounterSampleSenderBase sender)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Resource was disposed.");
            }

            if (this.sender != null)
            {
                throw new InvalidOperationException("Already started(), can't call for second time");
            }

            this.sender = sender;

            if (this.sender.SendInterval < MinSendInterval)
            {
                throw new ArgumentOutOfRangeException("sender", "sender.SendInterval is out of range. Min value is " + MinSendInterval);
            }

            this.agent = new NewRelicAgent(this.settings);
        }

        public void Publish(CounterSampleCollection[] packages)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Resource was disposed.");
            }

            if (!this.Ready)
            {
                sender.RaiseOnDisconnetedEvent();
                return;
            }
            
            this.agent.PollCycle(sender.SenderId, packages);
            this.agent.Context.SendMetricsToService();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    this.agent = null;
                    this.sender = null;
                }

                _disposed = true;
            }
        }


        #endregion
    }
}
