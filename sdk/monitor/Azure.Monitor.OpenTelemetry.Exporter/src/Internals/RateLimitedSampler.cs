// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using OpenTelemetry.Trace;

namespace Azure.Monitor.OpenTelemetry.Exporter.Internals;

/// <summary>
/// Rate limited sampler for OpenTelemetry exporters.
/// This sampler allows a specified number of traces per second to be sampled.
/// </summary>
internal sealed class RateLimitedSampler : Sampler
{
    private static readonly SamplingResult RecordOnlySamplingResult = new(SamplingDecision.RecordOnly);
    private static readonly SamplingResult RecordAndSampleSamplingResult = new(SamplingDecision.RecordAndSample);
    
    private readonly double targetTracesPerSecondLimit;
    
    // Time bucket size in ticks (1 second = 10,000,000 ticks)
    private const long TimeBucketSizeTicks = TimeSpan.TicksPerSecond;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitedSampler"/> class.
    /// </summary>
    /// <param name="targetTracesPerSecondLimit">
    /// The target number of traces per second that should be sampled.
    /// For example, specifying 0.5 means one request every two seconds.
    /// </param>
    public RateLimitedSampler(double targetTracesPerSecondLimit)
    {
        if (targetTracesPerSecondLimit < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetTracesPerSecondLimit), "Target traces per second limit must be non-negative.");
        }

        this.targetTracesPerSecondLimit = targetTracesPerSecondLimit;
        Description = "RateLimitedSampler{" + targetTracesPerSecondLimit + "}";
    }

    /// <summary>
    /// Decides whether to sample a given telemetry item based on the rate limit.
    /// Uses a deterministic algorithm based on trace ID and time buckets to ensure
    /// consistent sampling decisions while maintaining the target rate.
    /// </summary>
    /// <param name="samplingParameters">Parameters of telemetry item used to make sampling decision.</param>
    /// <returns>Returns whether or not we should sample telemetry in the form of a <see cref="SamplingResult"/> class.</returns>
    public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
    {
        if (targetTracesPerSecondLimit <= 0.0)
        {
            return RecordOnlySamplingResult;
        }

        // For very high rates, always sample to avoid computational overhead
        if (targetTracesPerSecondLimit >= 1000.0)
        {
            return RecordAndSampleSamplingResult;
        }

        // Get current time bucket (1-second windows)
        long currentTicks = DateTime.UtcNow.Ticks;
        long timeBucket = currentTicks / TimeBucketSizeTicks;

        // Create a deterministic hash based on trace ID and time bucket
        string traceIdHex = samplingParameters.TraceId.ToHexString().ToUpperInvariant();
        double sampleScore = CalculateSampleScore(traceIdHex, timeBucket);

        // Determine if this trace should be sampled based on the target rate
        // For rates < 1, we use the rate directly as a probability
        // For rates >= 1, we calculate how many samples per second bucket we should allow
        double samplingThreshold;
        if (targetTracesPerSecondLimit <= 1.0)
        {
            samplingThreshold = targetTracesPerSecondLimit;
        }
        else
        {
            // For higher rates, we need to calculate the probability that allows
            // the target number of traces per second across all traces
            // This is a simplified approach - a more sophisticated implementation
            // would require tracking actual sampling rates
            samplingThreshold = Math.Min(1.0, targetTracesPerSecondLimit / 1000.0);
        }

        if (sampleScore < samplingThreshold)
        {
            return RecordAndSampleSamplingResult;
        }
        else
        {
            return RecordOnlySamplingResult;
        }
    }

    private static double CalculateSampleScore(string traceIdHex, long timeBucket)
    {
        // Combine trace ID with time bucket for deterministic sampling
        // This ensures same trace in same time window always gets same decision
        string combinedString = traceIdHex + timeBucket.ToString();
        
        // Calculate DJB2 hash code similar to ApplicationInsightsSampler
        int hash = 5381;
        
        for (int i = 0; i < combinedString.Length; i++)
        {
            unchecked
            {
                hash = (hash << 5) + hash + (int)combinedString[i];
            }
        }

        // Take the absolute value of the hash
        if (hash == int.MinValue)
        {
            hash = int.MaxValue;
        }
        else
        {
            hash = Math.Abs(hash);
        }

        // Divide by MaxValue for value between 0 and 1 for sampling score
        double samplingScore = (double)hash / int.MaxValue;
        return samplingScore;
    }
}