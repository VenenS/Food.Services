using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using ITWebNet.Food.Core.DataContracts.Common;

namespace Food.Services.Tests.FakeFactories
{
    public static class TagFactory
    {
        public static Tag Create(User creator = null)
        {
           creator = creator ?? UserFactory.CreateUser();
            var tag = new Tag
            {
                CreateDate = DateTime.Now.AddDays(-30),
                CreatorId = creator.Id,
                IsDeleted = false, IsActive = true,
                Name = Guid.NewGuid().ToString("N")

            };
            ContextManager.Get().Tags.Add(tag);
            return tag;
        }

        public static List<Tag> CreateFew(int count = 3, User creator = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            var tags = new List<Tag>();
            for (var i = 0; i < count; i++)
                tags.Add(Create(creator));
            return tags;
        }

        public static TagModel CreateModel()
        {
            var tag = new TagModel
            {
                IsActive = true,
                Name = Guid.NewGuid().ToString("N"), Children = new List<TagModel>()
            };
            return tag;
        }

        public static List<TagModel> CreateFewModels(int count = 3)
        {
            var tags = new List<TagModel>();
            for (var i = 0; i < count; i++)
                tags.Add(CreateModel());
            return tags;
        }
    }
}
