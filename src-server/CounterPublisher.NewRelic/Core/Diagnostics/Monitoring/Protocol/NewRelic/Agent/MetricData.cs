// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetricData.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Metric data struct.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExitGames.Diagnostics.Monitoring.Protocol.NewRelic.Agent
{
    #region using directives

    using System;

    using System.Text.RegularExpressions;

    using ExitGames.Diagnostics.Monitoring;

    #endregion
    
    public struct MetricData
    {
        #region Fields

        private static readonly Regex MetricPattern = new Regex("([A-Z]+[a-z]*)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex MetricPerPattern = new Regex("([A-Z]+[a-z]*)Per([A-Z]+[a-z]*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex MetricCountPattern = new Regex("([A-Z]+[a-z]*)Count$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// (Value)
        /// </summary>
        private static readonly Regex MetricParenValuePattern = new Regex("(.+)\\s\\(([^()\\s/\\\\|]+)\\)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// (Value [/|\] Interval) or Value [/|\] Interval
        /// </summary>
        private static readonly Regex MetricParenValueIntervalPattern = new Regex("(.+)\\s\\(?([^()\\s/\\\\|]+)\\s*[/\\\\|]\\s*([^(()\\s/\\\\|]+)\\)?$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        #endregion

        public string Name;
        public string Units;
        public int Count;
        public float Value;
        public float MinValue;
        public float MaxValue;
        public float SumOfSquares;

        public MetricData(string name, string units, float value)
            : this(name, units, 1, value, value, value, value * value) { }

        public MetricData(string name, string units, int count, float value, float minValue, float maxValue, float sumOfSquares)
        {
            this.Name = name;
            this.Units = units;
            this.Count = count;
            this.Value = value;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.SumOfSquares = sumOfSquares;
        }

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
        public MetricData(CounterSampleCollection sample)
        {
            var i = sample.CounterName.LastIndexOf('.');
            if (i == -1)
            {
                throw new ArgumentException(
                    "Counter name format must contain component space in format: Component.Measure", "sample");
            }

            this.Name = null;
            this.Units = null;

            var component = sample.CounterName.Substring(0, i);
            var label = sample.CounterName.Substring(i + 1);

            // 1.1. {ValueUnit}{YourMetricIn1WordAction}Per{Interval}
            if (label.IndexOf("Per", StringComparison.InvariantCulture) > -1)
            {
                var match = MetricPerPattern.Match(label);
                if (match.Success)
                {
                    var action = match.Groups[1].Value;
                    var interval = match.Groups[2].Value;
                    var value = action;
                    if (match.Groups[1].Index > 0)
                    {
                        value = label.Substring(0, match.Groups[1].Index);
                        this.Name = component + '/' + value + action;
                    }
                    else
                    {
                        this.Name = component + '/' + action;
                    }
                    
                    this.Units = (value + '/' + interval).ToLowerInvariant();
                }
            }
            // 1.2. {CountUnit}{YourMetric}Count
            if (this.Name == null && label.EndsWith("Count", StringComparison.InvariantCulture))
            {
                var match = MetricCountPattern.Match(label);
                if (match.Success)
                {
                    var action = match.Groups[1].Value;
                    var value = action;
                    if (match.Groups[1].Index > 0)
                    {
                        value = label.Substring(0, match.Groups[1].Index);
                        this.Name = component + '/' + value + action;
                    }
                    else
                    {
                        this.Name = component + '/' + action;
                        
                    }
                    
                    this.Units = value.ToLowerInvariant();
                }
            }
            // 2.1. (Value/Interval) or Value/Interval
            if (this.Name == null)
            {
                var match = MetricParenValueIntervalPattern.Match(label);
                if (match.Success)
                {
                    this.Name = component + '/' + match.Groups[1].Value;

                    var value = match.Groups[2].Value;
                    var interval = match.Groups[3].Value;

                    this.Units = (value + '/' + interval).ToLowerInvariant();
                }
            }
            // 2.2. (Value)
            if (this.Name == null)
            {
                var match = MetricParenValuePattern.Match(label);
                if (match.Success)
                {
                    this.Name = component + '/' + match.Groups[1].Value;

                    var value = match.Groups[2].Value;

                    this.Units = (value).ToLowerInvariant();
                }
            }
            // 3. First words
            if (this.Name == null)
            {
                var match = MetricPattern.Matches(label);
                if (match.Count > 1)
                {
                    this.Name = component + '/' + label;
                    this.Units = match[0].Value.ToLowerInvariant();
                }
            }
            // 4. Simple
            if (this.Name == null)
            {
                this.Name = component + '/' + label;
                this.Units = "_";
            }

            // Fill values
            this.Count = 0;
            this.Value = 0.0f;
            this.MinValue = 0.0f;
            this.MaxValue = 0.0f;
            this.SumOfSquares = 0.0f;

            foreach (var s in sample)
            {
                this.Count ++;
                this.Value += s.Value;
                this.MinValue = Math.Min(this.MinValue, s.Value);
                this.MaxValue = Math.Max(this.MaxValue, s.Value);
                this.SumOfSquares += s.Value*s.Value;
            }
        }
        
        public void AggregateWith(MetricData other)
        {
            this.Count += other.Count;
            this.Value += other.Value;
            this.MinValue = Math.Min(MinValue, other.MinValue);
            this.MaxValue = Math.Max(MaxValue, other.MaxValue);
            this.SumOfSquares += other.SumOfSquares;
        }

        public Array Serialize()
        {
            return new [] { Value, Count, MinValue, MaxValue, SumOfSquares };
        }

        public override string ToString()
        {
            return "(" + this.Name + "[" + this.Units + "]" + 
                ", val=" + this.Value +
                ", cnt=" + this.Count +
                ", min=" + this.MinValue +
                ", max=" + this.MaxValue +
                ", sqr=" + this.SumOfSquares +
                ")";
        }
    }
}
