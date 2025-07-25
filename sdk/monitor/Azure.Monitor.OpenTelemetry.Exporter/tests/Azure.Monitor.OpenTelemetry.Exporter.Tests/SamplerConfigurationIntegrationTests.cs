// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Azure.Monitor.OpenTelemetry.Exporter.Internals.Platform;
using Xunit;

namespace Azure.Monitor.OpenTelemetry.Exporter.Tests;

public class SamplerConfigurationIntegrationTests
{
    [Fact]
    public void EndToEnd_RateLimitedSamplerFromEnvironmentVariable()
    {
        // Arrange
        Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER, "microsoft.rate_limited");
        Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER_ARG, "2.5");

        try
        {
            var options = new AzureMonitorExporterOptions
            {
                SamplingRatio = 0.8F, // This should be ignored
                TracesPerSecond = 10.0 // This should also be ignored due to env var precedence
            };

            // Act
            var sampler = SamplerFactory.CreateSampler(options);

            // Assert
            Assert.IsType<RateLimitedSampler>(sampler);
            Assert.Equal("RateLimitedSampler{2.5}", sampler.Description);
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER, null);
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER_ARG, null);
        }
    }

    [Fact]
    public void EndToEnd_FixedPercentageSamplerFromEnvironmentVariable()
    {
        // Arrange
        Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER, "microsoft.fixed_percentage");
        Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER_ARG, "0.25");

        try
        {
            var options = new AzureMonitorExporterOptions
            {
                SamplingRatio = 0.8F, // This should be ignored
                TracesPerSecond = 10.0 // This should also be ignored due to env var precedence
            };

            // Act
            var sampler = SamplerFactory.CreateSampler(options);

            // Assert
            Assert.IsType<ApplicationInsightsSampler>(sampler);
            Assert.Equal("ApplicationInsightsSampler{0.25}", sampler.Description);
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER, null);
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER_ARG, null);
        }
    }

    [Fact]
    public void EndToEnd_PrecedenceTracesPerSecondOverSamplingRatio()
    {
        // Arrange - No environment variables
        var options = new AzureMonitorExporterOptions
        {
            SamplingRatio = 0.8F,
            TracesPerSecond = 5.0 // This should take precedence
        };

        // Act
        var sampler = SamplerFactory.CreateSampler(options);

        // Assert
        Assert.IsType<RateLimitedSampler>(sampler);
        Assert.Equal("RateLimitedSampler{5}", sampler.Description);
    }

    [Fact]
    public void EndToEnd_DefaultToApplicationInsightsSampler()
    {
        // Arrange - No environment variables, no TracesPerSecond
        var options = new AzureMonitorExporterOptions
        {
            SamplingRatio = 0.6F
        };

        // Act
        var sampler = SamplerFactory.CreateSampler(options);

        // Assert
        Assert.IsType<ApplicationInsightsSampler>(sampler);
        Assert.Equal("ApplicationInsightsSampler{0.6}", sampler.Description);
    }

    [Fact]
    public void EndToEnd_DefaultSamplingRatio()
    {
        // Arrange - No configuration at all
        var options = new AzureMonitorExporterOptions(); // Uses default SamplingRatio = 1.0F

        // Act
        var sampler = SamplerFactory.CreateSampler(options);

        // Assert
        Assert.IsType<ApplicationInsightsSampler>(sampler);
        Assert.Equal("ApplicationInsightsSampler{1}", sampler.Description);
    }
}