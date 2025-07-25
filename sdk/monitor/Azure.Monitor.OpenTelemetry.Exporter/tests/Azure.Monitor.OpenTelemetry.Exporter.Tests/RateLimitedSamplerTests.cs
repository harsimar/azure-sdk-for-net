// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.Exporter.Internals;
using OpenTelemetry.Trace;
using Xunit;

namespace Azure.Monitor.OpenTelemetry.Exporter.Tests;

public class RateLimitedSamplerTests
{
    [Fact]
    public void RateLimitedSamplerGoodArgs()
    {
        var sampler = new RateLimitedSampler(5.0);
        Assert.NotNull(sampler);
        Assert.Equal("RateLimitedSampler{5}", sampler.Description);
    }

    [Fact]
    public void RateLimitedSamplerZeroArg()
    {
        var sampler = new RateLimitedSampler(0.0);
        Assert.NotNull(sampler);
        Assert.Equal("RateLimitedSampler{0}", sampler.Description);
    }

    [Fact]
    public void RateLimitedSamplerFractionalArg()
    {
        var sampler = new RateLimitedSampler(0.5);
        Assert.NotNull(sampler);
        Assert.Equal("RateLimitedSampler{0.5}", sampler.Description);
    }

    [Fact]
    public void RateLimitedSamplerBadArgs()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RateLimitedSampler(-1.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RateLimitedSampler(-0.1));
    }

    [Fact]
    public void RateLimitedSamplerBasicSampling()
    {
        // For now, test the basic functionality with the placeholder implementation
        var sampler = new RateLimitedSampler(5.0);
        
        byte[] testBytes = new byte[] 
        {
            0x0F, 0x1F, 0x2F, 0x3F,
            0x4F, 0x5F, 0x6F, 0x7F,
            0x8F, 0x9F, 0xAF, 0xBF,
            0xCF, 0xDF, 0xEF, 0xFF,
        };
        ActivityTraceId testId = ActivityTraceId.CreateFromBytes(testBytes);
        ActivityContext parentContext = default;
        SamplingParameters testParams = new SamplingParameters(parentContext, testId, "TestActivity", ActivityKind.Internal);

        var result = sampler.ShouldSample(testParams);
        
        // With the placeholder implementation, it should always sample
        Assert.Equal(SamplingDecision.RecordAndSample, result.Decision);
    }
}