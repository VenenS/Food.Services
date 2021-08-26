using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;

using NUnit.Framework;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    public class TagObjectTests
    {
        private FakeContext _context;

        private ObjectType _categoryObjectType;
        private ObjectType _cafeObjectType;
        private ObjectType _dishObjectType;

        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
            ContextManager.Set(_context);

            _categoryObjectType = new ObjectType { Description = "Category" };
            _cafeObjectType = new ObjectType { Description = "Cafe" };
            _dishObjectType = new ObjectType { Description = "Dish" };

            ContextManager.Get().ObjectType.Add(_categoryObjectType);
            ContextManager.Get().ObjectType.Add(_cafeObjectType);
            ContextManager.Get().ObjectType.Add(_dishObjectType);

            _categoryObjectType.Id = (long)ObjectTypesEnum.CATEGORY;
            _cafeObjectType.Id = (long)ObjectTypesEnum.CAFE;
            _dishObjectType.Id = (long)ObjectTypesEnum.DISH;
        }

        [Test]
        public void AddTagObjectTest()
        {
            var user = UserFactory.CreateUser();
            var tag = TagFactory.Create(user);
            var cafes = CafeFactory.CreateFew(10, user);
            var objectType = ObjectTypeFactory.Create();

            for (var i = 0; i < cafes.Count; i++)
            {
                ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddTagObject(new TagObject
                {
                    Tag = tag,
                    TagId = tag.Id,
                    CreatorId = user.Id,
                    CreateDate = DateTime.Now.AddDays(-5),
                    ObjectId = cafes[i].Id,
                    ObjectTypeId = objectType.Id,
                    ObjectType = objectType,
                    LastUpdateByUserId = user.Id,
                    LastUpdDate = DateTime.Now.AddDays(-5)
                });
            }

            Assert.AreEqual(10, _context.TagObject.Count());

            // Дубликаты игнорируются.
            ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddTagObject(
                ContextManager.Get().TagObject.First());
            Assert.AreEqual(10, _context.TagObject.Count());
        }

        [Test]
        public void GetCafesByTagListTest()
        {
            var cafes = CafeFactory.CreateFew(5);
            var deletedCafe = CafeFactory.Create();
            var inactiveCafe = CafeFactory.Create();
            var tagA = TagFactory.Create();
            var tagB = TagFactory.Create();
            var objectType = new ObjectType { Id = (long)ObjectTypesEnum.CAFE };

            // Создать связи между объектами (кафе в этом случае) и тэгами.
            {
                var allCafeIds = cafes.Select(x => x.Id).ToList().Concat(new long[] { deletedCafe.Id, inactiveCafe.Id }).ToList();
                for (var i = 0; i < allCafeIds.Count; i++)
                {
                    var link = TagObjectFactory.Create(null, i % 2 == 0 ? tagA : tagB, objectType);
                    link.ObjectId = allCafeIds[i];
                    ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddTagObject(link);
                }

                deletedCafe.IsDeleted = true;
                inactiveCafe.IsActive = false;

                // Кафе со снятым тэгом.
                var untaggedCafe = CafeFactory.Create();
                var deletedLink = TagObjectFactory.Create(null, tagA);
                deletedLink.ObjectId = untaggedCafe.Id;
                ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddTagObject(deletedLink);
                deletedLink.IsDeleted = true;
            }

            var tagged = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafesByTagList(new List<long> { });
            Assert.AreEqual(tagged.Count, 0);

            tagged = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafesByTagList(new List<long> { tagA.Id, tagB.Id });
            Assert.AreEqual(tagged.Count, cafes.Count);

            tagged = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafesByTagList(new List<long> { tagA.Id });
            Assert.AreEqual(tagged.Count, cafes.Count - cafes.Count / 2);

            tagged = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafesByTagList(new List<long> { tagB.Id });
            Assert.AreEqual(tagged.Count, cafes.Count / 2);

            tagged = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafesByTagList(new List<long> { tagB.Id, tagB.Id, tagB.Id + 1, tagB.Id * 1000 });
            Assert.AreEqual(tagged.Count, cafes.Count / 2);
        }

        [Test]
        public void GetCategorysByTagListAndCafeIdTest_ValidTag()
        {
            var cafe = CafeFactory.Create();
            var category = DishCategoryFactory.Create(null, cafe);
            var categoryLink = DishCategoryInCafeFactory.Create(null, cafe, category);
            var tag = TagFactory.Create();
            var tagLink = TagObjectFactory.Create(null, tag, _categoryObjectType);

            tagLink.ObjectId = category.Id;

            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCategorysByTagListAndCafeId(
                new List<long> { tag.Id }, cafe.Id);
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void GetCategorysByTagListAndCafeIdTest_TwoCafes()
        {
            // Тест чтобы метод не возвращал категории другого кафе.
            var cafe = CafeFactory.Create();
            var anotherCafe = CafeFactory.Create();
            var category = DishCategoryFactory.Create(null, cafe);

            DishCategoryInCafeFactory.Create(null, cafe, category);

            var tag = TagFactory.Create();
            var tagLink = TagObjectFactory.Create(null, tag, _categoryObjectType);

            tagLink.ObjectId = category.Id;

            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCategorysByTagListAndCafeId(
                new List<long> { tag.Id }, anotherCafe.Id);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetCategorysByTagListAndCafeIdTest_DeletedCategoryLink()
        {
            var cafe = CafeFactory.Create();
            var category = DishCategoryFactory.Create(null, cafe);
            var categoryLink = DishCategoryInCafeFactory.Create(null, cafe, category);
            var tag = TagFactory.Create();
            var tagLink = TagObjectFactory.Create(null, tag, _categoryObjectType);

            tagLink.ObjectId = category.Id;
            categoryLink.IsDeleted = true;

            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCategorysByTagListAndCafeId(
                new List<long> { tag.Id }, cafe.Id);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetCategorysByTagListAndCafeIdTest_DeletedTagCategoryLink()
        {
            var cafe = CafeFactory.Create();
            var category = DishCategoryFactory.Create(null, cafe);
            var tag = TagFactory.Create();

            DishCategoryInCafeFactory.Create(null, cafe, category);

            var deletedLink = TagObjectFactory.Create(null, tag, _categoryObjectType);
            deletedLink.ObjectId = category.Id;
            deletedLink.IsDeleted = true;

            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCategorysByTagListAndCafeId(
                new List<long> { tag.Id }, cafe.Id);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetDishesByTagListAndCafeIdTest()
        {
            var cafe = CafeFactory.Create();
            var category = DishCategoryFactory.Create(null, cafe);
            var categoryLink = DishCategoryInCafeFactory.Create(null, cafe, category);

            var dish = DishFactory.Create(null, categoryLink);
            dish.VersionFrom = DateTime.Now.AddDays(-1);
            dish.VersionTo = null;
            var dishTag = TagFactory.Create();

            var tagObjectType = ContextManager.Get().ObjectType.Add(new ObjectType { }).Entity;
            tagObjectType.Id = (long)ObjectTypesEnum.DISH;
            var tagObject = TagObjectFactory.Create(null, dishTag, tagObjectType);
            tagObject.ObjectId = dish.Id;

            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetDishesByTagListAndCafeId(
                new List<long> { dish.Id }, cafe.Id);
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void GetTagsByCafesListTest()
        {
            var cafes = CafeFactory.CreateFew();
            var tags = TagFactory.CreateFew(10);

            foreach (var cafe in cafes)
            {
                foreach (var tag in tags)
                {
                    var objectType = ObjectTypeFactory.Create();
                    objectType.Id = (long)ObjectTypesEnum.CAFE;

                    var tagObject = TagObjectFactory.Create(null, tag, objectType);
                    tagObject.ObjectId = cafe.Id;
                }
            }

            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetTagsByCafesList(
                cafes.Select(x => x.Id).ToList());
            Assert.AreEqual(tags.Count, result.Count);
        }
    }
}
