using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class TagObjectFactory
    {
        private static Random _rnd = new Random();
        public static TagObject Create(User creator = null, Tag tag = null, ObjectType type = null)
        {
            type = type ?? ObjectTypeFactory.Create();
            creator = creator ?? UserFactory.CreateUser();
            tag = tag ?? TagFactory.Create();
            var role = new TagObject
            {
                CreatorId = creator.Id, CreateDate = DateTime.Now.AddDays(-30), 
                Tag = tag, TagId = tag.Id, ObjectType = type, ObjectTypeId = type.Id, ObjectId = _rnd.Next()
            };
            ContextManager.Get().TagObject.Add(role);
            return role;
        }

        public static List<TagObject> CreateFew(int count)
        {
            var roles = new List<TagObject>();
            for (var i = 0; i < count; i++)
                roles.Add(Create());
            return roles;
        }
    }
}
