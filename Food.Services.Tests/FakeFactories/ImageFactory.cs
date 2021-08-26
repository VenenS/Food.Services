using Food.Data.Entities;
using Food.Services.Tests.Context;
using System;
using System.Collections.Generic;

namespace Food.Services.Tests.FakeFactories
{
    public static class ImageFactory
    {
        static Random rand = new Random();

        public static Image Create(User creator = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            var image = new Image
            {
                CreatorId = creator.Id,
                IsDeleted = false,
                Hash = Guid.NewGuid().ToString(),
                ObjectId = rand.Next(1, 1000)
            };
            ContextManager.Get().Images.Add(image);
            return image;
        }

        public static List<Image> CreateFew(int count = 3, User creator = null)
        {
            var lstEntity = new List<Image>();
            creator = creator ?? UserFactory.CreateUser();
            var image = new List<Image>();
            for (var i = 0; i < count; i++)
                lstEntity.Add(Create(creator));
            return lstEntity;
        }
    }
}
