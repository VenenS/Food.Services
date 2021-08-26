﻿using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;

namespace Food.Services.Tests.Tools
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(new Fixture().Customize(new AutoMoqCustomization()))
        { }
    }
}
