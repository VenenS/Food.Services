using System;
using System.Collections.Generic;
using Food.Data.Entities;

namespace Food.Services.Tests.FakeFactories
{
    public static class ObjectTypeFactory
    {
        public static ObjectType Create()
        {
            var role = new ObjectType
            {
                Description = Guid.NewGuid().ToString("N")
            };
            //ContextManager.Get().T.Add(role);
            return role;
        }

        public static List<ObjectType> CreateFew(int count)
        {
            var roles = new List<ObjectType>();
            for (var i = 0; i < count; i++)
                roles.Add(Create()); 
            return roles;
        }
    }
}