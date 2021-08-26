using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class ExtFactory
    {
        public static Ext Create(User creator = null, bool saveDB = true)
        {
            creator = creator ?? UserFactory.CreateUser();
            var ext = new Ext
            {
                IsDeleted = false,
                CreatorId = creator.Id,
                Name = Guid.NewGuid().ToString("N")
            };
            if(saveDB) ContextManager.Get().Ext.Add(ext);
            return ext;
        }

        public static List<Ext> CreateFew(int count = 3, bool saveDB = true, User creator = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            var exts = new List<Ext>();
            for (int i = 0; i < count; i++)
                exts.Add(Create(creator));
            return exts;
        }
    }
}
