using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class ReportStylesheetFactory
    {
        public static ReportStylesheet Create(User creator = null, Ext ext = null, Cafe cafe = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            ext = ext ?? ExtFactory.Create(creator);
            cafe = cafe ?? CafeFactory.Create(creator);
            var stylesheet = new ReportStylesheet
            {
                IsDeleted = false,
                Cafe = cafe,
                CafeId = cafe.Id,
                CreatorId = creator.Id,
                Ext = ext, ExtId = ext.Id,
                Name = Guid.NewGuid().ToString("N"),
                Description = Guid.NewGuid().ToString("N")
            };
            ContextManager.Get().ReportStylesheets.Add(stylesheet);
            return stylesheet;
        }

        public static List<ReportStylesheet> CreateFew(int count = 3, User creator = null, Ext ext = null,
            Cafe cafe = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            ext = ext ?? ExtFactory.Create(creator);
            cafe = cafe ?? CafeFactory.Create(creator);
            var stylesheets = new List<ReportStylesheet>();
            for (int i = 0; i < count; i++)
                stylesheets.Add(Create(creator, ext, cafe));
            return stylesheets;
        }
    }
}
