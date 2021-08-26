using System;
using System.Linq;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.FoodService.Food.DbAccessor;
using NUnit.Framework;
// ReSharper disable PossibleInvalidOperationException

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    [SingleThreaded]
    public class TagTests
    {
        private void SetUp()
        {
            _context = new FakeContext();
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
            ContextManager.Set(_context);
        }

        private FakeContext _context;
        private readonly Random _random = new Random();

        [Test]
        public void AddTagTest()
        {
            SetUp();
            var tag = new Tag {Name = Guid.NewGuid().ToString("N")};
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddTag(tag);
            Assert.IsTrue(result);
            Assert.IsNotNull(_context.Tags.FirstOrDefault(e => e.Name == tag.Name));
        }

        [Test]
        public void EditTagTest()
        {
            SetUp();
            var tag = TagFactory.Create();
            var changed = new Tag {Name = Guid.NewGuid().ToString("N"), Id = tag.Id};
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditTag(changed);
            Assert.IsTrue(changed.Name == tag.Name);
            Assert.IsTrue(result);
        }

        [Test]
        public void GetChildListOfTagsByTagId_OnlyActive_Test()
        {
            SetUp();
            var nonActive = TagFactory.Create();
            nonActive.IsActive = false;
            var parent = TagFactory.Create();
            nonActive.ParentId = parent.Id;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetChildListOfTagsByTagId(parent.Id);
            Assert.IsFalse(result.Any());
        }

        [Test]
        public void GetChildListOfTagsByTagId_OnlyAlive_Test()
        {
            SetUp();
            var nonAlive = TagFactory.Create();
            nonAlive.IsDeleted = true;
            var parent = TagFactory.Create();
            nonAlive.ParentId = parent.Id;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetChildListOfTagsByTagId(parent.Id);
            Assert.IsFalse(result.Any());
        }

        [Test]
        public void GetChildListOfTagsByTagIdTest()
        {
            SetUp();
            var child = TagFactory.Create();
            var parent = TagFactory.Create();
            child.ParentId = parent.Id;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetChildListOfTagsByTagId(parent.Id);
            Assert.IsTrue(result.First() == child);
            Assert.IsTrue(result.Count() == 1);
        }

        [Test]
        public void GetFullListOfTags_OnlyActive_Test()
        {
            SetUp();
            var nonActive = TagFactory.Create();
            nonActive.IsActive = false;
            TagFactory.CreateFew();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetFullListOfTags();
            Assert.IsNull(result.FirstOrDefault(e => !e.IsActive));
        }

        [Test]
        public void GetFullListOfTags_OnlyAlive_Test()
        {
            SetUp();
            var nonActive = TagFactory.Create();
            nonActive.IsDeleted = true;
            TagFactory.CreateFew();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetFullListOfTags();
            Assert.IsNull(result.FirstOrDefault(e => e.IsDeleted));
        }

        [Test]
        public void GetListOfTagsByString_OnlyActive_Test()
        {
            SetUp();
            var tags = TagFactory.CreateFew();
            tags.Last().IsActive = false;
            var search = tags.Last().Name.Substring(0, 6);
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetListOfTagsByString(search);
            Assert.IsFalse(result.Any());
        }

        [Test]
        public void GetListOfTagsByString_OnlyAlive_Test()
        {
            SetUp();
            var tags = TagFactory.CreateFew();
            tags.Last().IsDeleted = true;
            var search = tags.Last().Name.Substring(0, 6);
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetListOfTagsByString(search);
            Assert.IsFalse(result.Any());
        }

        [Test]
        public void GetListOfTagsByStringTest()
        {
            SetUp();
            var tags = TagFactory.CreateFew();
            var search = tags.Last().Name.Substring(0, 6);
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetListOfTagsByString(search);
            Assert.IsTrue(result.First() == tags.Last());
            Assert.IsTrue(result.Count() == 1);
        }

        [Test]
        public void GetRootTags_OnlyActive_Test()
        {
            SetUp();
            var nonActive = TagFactory.Create();
            var parent = TagFactory.Create();
            parent.IsActive = false;
            nonActive.ParentId = parent.Id;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetRootTags();
            Assert.IsFalse(result.Any());
        }

        [Test]
        public void GetRootTags_OnlyAlive_Test()
        {
            SetUp();
            var nonAlive = TagFactory.Create();
            var parent = TagFactory.Create();
            nonAlive.ParentId = parent.Id;
            parent.IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetRootTags();
            Assert.IsFalse(result.Any());
        }

        [Test]
        public void GetRootTagsTest()
        {
            SetUp();
            var child = TagFactory.Create();
            var parent = TagFactory.Create();
            child.ParentId = parent.Id;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetRootTags();
            Assert.IsTrue(result.First() == parent);
            Assert.IsTrue(result.Count() == 1);
        }

        [Test]
        public void GetTagById_OnlyActive_Test()
        {
            SetUp();
            var tag = TagFactory.Create();
            tag.IsActive = false;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetTagById(tag.Id);
            Assert.IsNull(result);
        }

        [Test]
        public void GetTagById_OnlyAlive_Test()
        {
            SetUp();
            var tag = TagFactory.Create();
            tag.IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetTagById(tag.Id);
            Assert.IsNull(result);
        }

        [Test]
        public void GetTagByIdTest()
        {
            SetUp();
            var tag = TagFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetTagById(tag.Id);
            Assert.IsTrue(result == tag);
        }

        [Test]
        public void RemoveTagTest()
        {
            SetUp();
            var userId = _random.Next();
            var tag = TagFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveTag(tag.Id, userId);
            Assert.IsTrue(result);
            Assert.IsTrue(tag.IsDeleted);
            Assert.IsTrue(tag.LastUpdateByUserId == userId);
            Assert.IsTrue(tag.LastUpdDate.Value.Date == DateTime.Now.Date);
        }
    }
}