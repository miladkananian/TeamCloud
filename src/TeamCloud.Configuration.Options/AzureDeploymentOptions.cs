﻿using System;

namespace TeamCloud.Configuration.Options
{
    [Options("Azure:Deployment")]
    public class AzureDeploymentOptions
    {
        public string DefaultLocation { get; set; } = Environment.GetEnvironmentVariable("REGION_NAME");
    }
}
