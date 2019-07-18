// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewRelicAgent.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   NewRelic Photon agent.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExitGames.Diagnostics.Monitoring.Protocol.NewRelic.Agent
{
    #region using directives

    using System;

    using global::NewRelic.Platform.Sdk.Binding;

    using ExitGames.Diagnostics.Configuration;
    using ExitGames.Diagnostics.Monitoring;
    using ExitGames.Logging;

    #endregion

    /// <summary>
    /// Counter have format: System.CPU or System.Memory or System.BytesInPerSecond
    /// Metric have format: Component/Label[/Attribute/...]
    /// Transform: System.CPU.Load -> System.CPU/Load[/SenderId]
    /// 
    /// 1.1. If have Per suffix then format should be: {ValueUnit}{YourMetricIn1WordAction}Per{Interval}. Like System.InBytesRecvPerSecond.
    /// 1.2. If have Count suffix then format should be: {CountUnit}{YourMetric}Count. Like CounterPublisher.EventsSentCount.
    /// 2.1. (Value/Interval) or Value/Interval. Like bytes in/sec, TCP: Disconnected Peers +/sec. or
    /// 2.1. If have in the end: (Value) like Time Spent In Server: In (ms).
    /// 3. Split words. Take first as Value unit name.
    /// 4. Just "values"
    /// </summary>
    public class NewRelicAgent
    {
        #region

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        #endregion

        #region Constructors and Destructors

        public NewRelicAgent(NewRelicAgentSettings s)
        {
            this.Guid = s.AgentId;
            this.Version = s.Version;

            this.Name = s.AgentName;

            if (String.IsNullOrEmpty(this.Guid))
            {
                throw new ArgumentNullException("c", "Guid can't be empty");
            }

            if (String.IsNullOrEmpty(this.Version))
            {
                throw new ArgumentNullException("c", "Version can't be empty");
            }

            if (!string.IsNullOrEmpty(this.Name))
            {
                this.Name = this.Name.Replace("{0}", Environment.MachineName);
            }


            this.Context = new Context(s.LicenseKey, new NewRelicConfig(s));
            this.Context.Version = this.Version;
        }

        #endregion

        #region Properties

        public string Guid { get; private set; }

        public string Version { get; private set; }

        public string Name { get; private set; }

        public IContext Context { get; private set;  }

        #endregion

        #region Methods
        
        public void ReportMetric(string metricName, string units, float? value)
        {
            this.Context.ReportMetric(this.Guid, this.GetAgentName(), metricName, units, value);
        }

        public void ReportMetric(string metricName, string units, float value,
            int count, float min, float max, float sumOfSquares)
        {
            this.Context.ReportMetric(this.Guid, this.GetAgentName(), metricName, units, value, count, min, max,
                sumOfSquares);
        }

        public string GetAgentName()
        {
            if (String.IsNullOrEmpty(this.Name))
            {
                return "Agent Name: " + this.Guid + ". Agent Version: " + this.Version;
            }
            else
            {
                return this.Name;
            }
        }

        public void PollCycle(string senderId, CounterSampleCollection[] packages)
        {
            foreach (var package in packages)
            {
                if (package.CounterName.IndexOf('.') < 0)
                {
                    Log.WarnFormat("Skipping metric: {0}, aggregated from: {1}. Translation method for this name is unknown", package.CounterName, package.Count);
                    continue;
                }

                var data = new MetricData(package);
                if (!String.IsNullOrEmpty(senderId))
                {
                    // Apply attribute
                    data.Name += '/' + senderId;
                }

                if (Log.IsDebugEnabled)
                {
                    Log.DebugFormat("Report metric: {0}, aggregated from: {1}, {2}", package.CounterName, package.Count, data.ToString());
                }

                ReportMetric(data.Name, data.Units, data.Value, data.Count, data.MinValue, data.MaxValue, data.SumOfSquares);
            }
        }
        
        #endregion
    }
}
