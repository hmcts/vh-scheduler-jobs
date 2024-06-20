﻿using System;
using System.Diagnostics.CodeAnalysis;
using LaunchDarkly.Sdk;
using LaunchDarkly.Sdk.Server;
using LaunchDarkly.Sdk.Server.Interfaces;

namespace SchedulerJobs.Common.Configuration
{
    public interface IFeatureToggles
    {
        public bool StorePeopleIngestion();
    }
    
    [ExcludeFromCodeCoverage]
    public class FeatureToggles : IFeatureToggles
    {
        private readonly ILdClient _ldClient;
        private readonly Context _context;
        private const string LdUser = "vh-scheduler-jobs";
        private const string StorePeopleIngestionToggleKey = "store-people-ingestion";
        public FeatureToggles(string sdkKey, string environmentName)
        {
            var config = LaunchDarkly.Sdk.Server.Configuration.Builder(sdkKey).Build();
            
            _context = Context.Builder(LdUser).Name(environmentName).Build();
            _ldClient = new LdClient(config);
        }

        public bool StorePeopleIngestion() => GetBoolToggle(StorePeopleIngestionToggleKey);

        private bool GetBoolToggle(string key)
        {
            if (!_ldClient.Initialized)
            {
                throw new InvalidOperationException("LaunchDarkly client not initialized");
            }
            return _ldClient.BoolVariation(key, _context);
        }
    }
}
