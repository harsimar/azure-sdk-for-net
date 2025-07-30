// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Azure.Monitor.OpenTelemetry.Exporter.Internals;
using Azure.Monitor.OpenTelemetry.Exporter.Internals.Platform;
using Xunit;

namespace Azure.Monitor.OpenTelemetry.Exporter.Tests;

public class SamplerFactoryTests
{
    [Fact]
    public void CreateSampler_WithoutEnvironmentVariables_UsesOptionsConfiguration()
    {
        // Arrange
        var options = new AzureMonitorExporterOptions
        {
            SamplingRatio = 0.5F
        };

        // Act
        var sampler = SamplerFactory.CreateSampler(options);

        // Assert
        Assert.IsType<ApplicationInsightsSampler>(sampler);
        Assert.Equal("ApplicationInsightsSampler{0.5}", sampler.Description);
    }

    [Fact]
    public void CreateSampler_WithTracesPerSecond_UsesRateLimitedSampler()
    {
        // Arrange
        var options = new AzureMonitorExporterOptions
        {
            SamplingRatio = 0.5F,
            TracesPerSecond = 2.0
        };

        // Act
        var sampler = SamplerFactory.CreateSampler(options);

        // Assert
        Assert.IsType<RateLimitedSampler>(sampler);
        Assert.Equal("RateLimitedSampler{2}", sampler.Description);
    }

    [Fact]
    public void CreateSampler_WithValidRateLimitedEnvironmentVariables_UsesRateLimitedSampler()
    {
        // Arrange
        Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER, "microsoft.rate_limited");
        Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER_ARG, "3.5");

        try
        {
            var options = new AzureMonitorExporterOptions
            {
                SamplingRatio = 0.5F
            };

            // Act
            var sampler = SamplerFactory.CreateSampler(options);

            // Assert
            Assert.IsType<RateLimitedSampler>(sampler);
            Assert.Equal("RateLimitedSampler{3.5}", sampler.Description);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER, null);
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER_ARG, null);
        }
    }

    [Fact]
    public void CreateSampler_WithValidFixedPercentageEnvironmentVariables_UsesApplicationInsightsSampler()
    {
        // Arrange
        Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER, "microsoft.fixed_percentage");
        Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER_ARG, "0.3");

        try
        {
            var options = new AzureMonitorExporterOptions
            {
                SamplingRatio = 0.5F
            };

            // Act
            var sampler = SamplerFactory.CreateSampler(options);

            // Assert
            Assert.IsType<ApplicationInsightsSampler>(sampler);
            Assert.Equal("ApplicationInsightsSampler{0.3}", sampler.Description);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER, null);
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER_ARG, null);
        }
    }

    [Fact]
    public void CreateSampler_WithInvalidEnvironmentVariables_FallsBackToOptions()
    {
        // Arrange
        Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER, "invalid_sampler");
        Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER_ARG, "invalid_arg");

        try
        {
            var options = new AzureMonitorExporterOptions
            {
                SamplingRatio = 0.7F
            };

            // Act
            var sampler = SamplerFactory.CreateSampler(options);

            // Assert
            Assert.IsType<ApplicationInsightsSampler>(sampler);
            Assert.Equal("ApplicationInsightsSampler{0.7}", sampler.Description);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER, null);
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.OTEL_TRACES_SAMPLER_ARG, null);
        }
    }

    [Fact]
    public void CreateSampler_RateLimitedSamplerFromEnvironmentVariable_IgnoresOptionsConfiguration()
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
    public void CreateSampler_FixedPercentageSamplerFromEnvironmentVariable_IgnoresOptionsConfiguration()
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
    public void CreateSampler_TracesPerSecondTakesPrecedenceOverSamplingRatio()
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
    public void CreateSampler_DefaultsToApplicationInsightsSamplerWithSamplingRatio()
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
    public void CreateSampler_DefaultsToApplicationInsightsSamplerWithDefaultSamplingRatio()
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