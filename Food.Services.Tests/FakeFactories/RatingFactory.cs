using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class RatingFactory
    {
        private static Random _rnd = new Random();
        public static Rating Create(User user = null)
        {
            user = user ?? UserFactory.CreateUser();
            var rating = new Rating
            {
                CreateDate = DateTime.Now.AddDays(-30), User = user, UserId = user.Id, CreatorId = user.Id,
                IsDeleted = false, LastUpdateDate = DateTime.Now, ObjectType = _rnd.Next()

            };
            ContextManager.Get().Rating.Add(rating);
            return rating;
        }

        public static List<Rating> CreateFew(int count = 3, User user = null)
        {
            var ratings = new List<Rating>();
            for (var i = 0; i < count; i++)
                ratings.Add(Create(user));
            return ratings;
        }
    }
}
