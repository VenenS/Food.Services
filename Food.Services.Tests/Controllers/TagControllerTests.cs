using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Common;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;

namespace Food.Services.Tests.Controllers
{
    [TestFixture]
    [SingleThreaded]
    public class TagControllerTests
    {
        private FakeContext _context;
        private TagController _controller;
        private Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        private readonly Random _random = new Random();
        private User _user;

        private void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new TagController(_context, _accessor.Object, null);
            _user = UserFactory.CreateUser();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AddNewTagTest(bool expected)
        {
            SetUp();
            var model = TagFactory.CreateModel();
            _accessor.Setup(e => e.AddTag(It.IsAny<Tag>())).Returns(expected);
            var responce = _controller.AddNewTag(model);
            var result = TransformResult.GetPrimitive<bool>(responce);
            _accessor.Verify(e => e.AddTag(It.Is<Tag>(t => t.Name == model.Name)), Times.Once);
            Assert.IsTrue(result == expected);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AddTagToCafeTest(bool expected)
        {
            SetUp();

            var tagId = _random.Next();
            var cafe = CafeFactory.Create();
            _accessor.Setup(e => e.AddTagObject(It.Is<TagObject>(t => t.ObjectId == cafe.Id && t.TagId == tagId))).Returns(expected);
            _accessor.Setup(e => e.GetCafeById(cafe.Id)).Returns(cafe);
            var responce = _controller.AddTagToCafe(cafe.Id, tagId);
            var result = TransformResult.GetPrimitive<bool>(responce);
            _accessor.Verify(e => e.AddTagObject(It.IsAny<TagObject>()), Times.Once);
            Assert.IsTrue(result == expected);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AddTagToDishTest(bool expected)
        {
            SetUp();
            var tag = TagFactory.Create();
            var dish = DishFactory.Create();
            _accessor.Setup(e => e.AddTagObject(It.Is<TagObject>(t => t.ObjectId == dish.Id && t.TagId == tag.Id))).Returns(expected);
            _accessor.Setup(e => e.GetFoodDishesById(new[] { dish.Id })).Returns(new List<Dish>() { dish });
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategoryId)).Returns(true);
            _accessor.Setup(e =>
                    e.GetListOfTagsConnectedWithObjectAndHisChild(dish.DishCategoryLinks.First(
                        l => l.IsActive == true && !l.IsDeleted).CafeCategory.CafeId, (long)ObjectTypesEnum.CAFE))
                .Returns(new List<Tag>() { tag });
            var responce = _controller.AddTagToDish(dish.Id, tag.Id);
            var result = TransformResult.GetPrimitive<bool>(responce);
            _accessor.Verify(e => e.AddTagObject(It.IsAny<TagObject>()), Times.Once);
            Assert.IsTrue(result == expected);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EditTagTest(bool expected)
        {
            SetUp();
            var model = TagFactory.CreateModel();
            _accessor.Setup(e => e.EditTag(It.Is<Tag>(t => t.Name == model.Name))).Returns(expected);
            var responce = _controller.EditTag(model);
            var result = TransformResult.GetPrimitive<bool>(responce);
            _accessor.Verify(e => e.EditTag(It.IsAny<Tag>()), Times.Once);
            Assert.IsTrue(result == expected);
        }

        [Test]
        public void GetAllTagsTest()
        {
            SetUp();
            var tags = TagFactory.CreateFew();
            var cafes = CafeFactory.CreateFew();
            _accessor.Setup(e => e.GetCafesByUserId(_user.Id)).Returns(cafes);
            _accessor.Setup(e => e.GetTagsByCafesList(It.Is<List<long>>(c => c.Count == cafes.Count))).Returns(tags);
            var responce = _controller.GetAllTags();
            var result = TransformResult.GetObject<List<TagModel>>(responce);
            _accessor.Verify(e => e.GetTagsByCafesList(It.IsAny<List<long>>()), Times.Once);
            _accessor.Verify(e => e.GetCafesByUserId(_user.Id), Times.Once);
            Assert.IsTrue(result.Sum(e => e.Id) == tags.Sum(e => e.Id));
        }

        [Test]
        public void GetChildTagsByTagIdTest()
        {
            SetUp();
            var tagId = _random.Next();
            var tags = TagFactory.CreateFew();
            _accessor.Setup(e => e.GetChildListOfTagsByTagId(tagId)).Returns(tags);
            var responce = _controller.GetChildTagsByTagId(tagId);
            var result = TransformResult.GetObject<List<TagModel>>(responce);
            _accessor.Verify(e => e.GetChildListOfTagsByTagId(tagId), Times.Once);
            Assert.IsTrue(result.Sum(e => e.Id) == tags.Sum(e => e.Id));
        }

        [Test]
        public void GetDishesByTagListAndCafeIdTest()
        {
            SetUp();
            var tagId = new List<long>() { _random.Next() };
            var cafeId = _random.Next();
            var dishes = DishFactory.CreateFew();
            _accessor.Setup(e => e.GetDishesByTagListAndCafeId(tagId, cafeId)).Returns(dishes);
            var responce = _controller.GetDishesByTagListAndCafeId(tagId, cafeId);
            var result = TransformResult.GetObject<List<FoodDishModel>>(responce);
            _accessor.Verify(e => e.GetDishesByTagListAndCafeId(tagId, cafeId), Times.Once);
            Assert.IsTrue(result.Sum(e => e.Id) == dishes.Sum(e => e.Id));
        }

        [Test]
        public void GetFoodCategoriesSearchTermAndTagsAndCafeIdTest()
        {
            SetUp();
            var searchTerm = Guid.NewGuid().ToString("N");
            var tagId = new List<long>() { _random.Next() };
            var cafeId = _random.Next();
            var dishes = DishCategoryInCafeFactory.CreateFew();
            _accessor.Setup(e => e.GetCategorysBySearchTermAndTagListAndCafeId(tagId, cafeId, searchTerm)).Returns(dishes);
            var responce = _controller.GetFoodCategoriesSearchTermAndTagsAndCafeId(searchTerm, tagId, cafeId);
            var result = TransformResult.GetObject<List<FoodCategoryModel>>(responce);
            _accessor.Verify(e => e.GetCategorysBySearchTermAndTagListAndCafeId(tagId, cafeId, searchTerm), Times.Once);
            Assert.IsTrue(result.Sum(e => e.Index) == dishes.Sum(e => e.Index));
        }

        [Test]
        public void GetFoodCategoriesTagsAndCafeIdTest()
        {
            SetUp();
            var tagId = new List<long>() { _random.Next() };
            var cafeId = _random.Next();
            var dishes = DishCategoryInCafeFactory.CreateFew();
            _accessor.Setup(e => e.GetCategorysByTagListAndCafeId(tagId, cafeId)).Returns(dishes);
            var responce = _controller.GetFoodCategoriesTagsAndCafeId(tagId, cafeId);
            var result = TransformResult.GetObject<List<FoodCategoryModel>>(responce);
            _accessor.Verify(e => e.GetCategorysByTagListAndCafeId(tagId, cafeId), Times.Once);
            Assert.IsTrue(result.Sum(e => e.Index) == dishes.Sum(e => e.Index));
        }

        //Явно необходимо выносить лишний функционал из контроллера
        [Test]
        public void GetListOfCafesBySearchTermAndTagsListTest()
        {
            SetUp();
            var cafes = CafeFactory.CreateFew();
            var searchTerm = Guid.NewGuid().ToString("N");
            var tagId = new List<long>() { _random.Next() };
            var cafe = CafeFactory.Create();
            var dishCategory = DishCategoryFactory.Create();
            _accessor.Setup(e => e.GetCafesByTagList(tagId)).Returns(cafes);
            _accessor.Setup(e => e.GetDishesByScheduleByDate(It.Is<long>(l => cafes.Select(c => c.Id).Contains(l)), DateTime.Now.Date)).Returns(new Dictionary<DishCategory, List<Dish>>() { { dishCategory, new List<Dish>() } });
            _accessor.Setup(e => e.GetCafeById(It.IsAny<long>())).Returns(cafe);
            var responce = _controller.GetListOfCafesBySearchTermAndTagsList(searchTerm, tagId);
            var result = TransformResult.GetObject<List<CafeModel>>(responce);
            _accessor.Verify(e => e.GetCafesByTagList(tagId), Times.Once);
            _accessor.Verify(e => e.GetDishesByScheduleByDate(It.IsAny<long>(), DateTime.Now.Date), Times.Exactly(cafes.Count));
            _accessor.Verify(e => e.GetCafeById(It.IsAny<long>()), Times.Exactly(cafes.Count));
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetListOfCafesByTagsListTest()
        {
            SetUp();
            var cafes = CafeFactory.CreateFew();
            var tagId = new List<long>() { _random.Next() };
            _accessor.Setup(e => e.GetCafesByTagList(tagId)).Returns(cafes);
            var responce = _controller.GetListOfCafesByTagsList(tagId);
            var result = TransformResult.GetObject<List<CafeModel>>(responce);
            _accessor.Verify(e => e.GetCafesByTagList(tagId), Times.Once);
            Assert.IsTrue(result.Sum(e => e.Id) == cafes.Sum(e => e.Id));
        }

        [Test]
        public void GetListOfTagsByStringTest()
        {
            SetUp();
            var tags = TagFactory.CreateFew();
            var searchString = Guid.NewGuid().ToString("N");
            _accessor.Setup(e => e.GetListOfTagsByString(searchString)).Returns(tags);
            var responce = _controller.GetListOfTagsByString(searchString);
            var result = TransformResult.GetObject<List<TagModel>>(responce);
            _accessor.Verify(e => e.GetListOfTagsByString(searchString), Times.Once);
            Assert.IsTrue(result.Sum(e => e.Id) == tags.Sum(e => e.Id));
        }

        [Test]
        public void GetListOfTagsConnectedWithObjectAndHisChildTest()
        {
            SetUp();
            var tags = TagFactory.CreateFew();
            var typeOfObject = _random.Next();
            var objectId = _random.Next();
            _accessor.Setup(e => e.GetListOfTagsConnectedWithObjectAndHisChild(objectId, typeOfObject)).Returns(tags);
            var responce = _controller.GetListOfTagsConnectedWithObjectAndHisChild(objectId, typeOfObject);
            var result = TransformResult.GetObject<List<TagModel>>(responce);
            _accessor.Verify(e => e.GetListOfTagsConnectedWithObjectAndHisChild(objectId, typeOfObject), Times.Once);
            Assert.IsTrue(result.Sum(e => e.Id) == tags.Sum(e => e.Id));
        }

        [Test]
        public void GetRootTagsTest()
        {
            SetUp();
            var tags = TagFactory.CreateFew();
            var tag = TagFactory.Create();
            tag.ParentId = tags.First().Id;
            _accessor.Setup(e => e.GetRootTags()).Returns(tags);
            var responce = _controller.GetRootTags();
            var result = TransformResult.GetObject<List<TagModel>>(responce);
            _accessor.Verify(e => e.GetRootTags(), Times.Once);
            Assert.IsNull(result.FirstOrDefault(e => e.ParentId != null));
        }

        [Test]
        public void GetTagByTagIdTest()
        {
            SetUp();
            var tag = TagFactory.Create();
            _accessor.Setup(e => e.GetTagById(tag.Id)).Returns(tag);
            var responce = _controller.GetTagByTagId(tag.Id);
            var result = TransformResult.GetObject<TagModel>(responce);
            _accessor.Verify(e => e.GetTagById(tag.Id), Times.Once);
            Assert.IsTrue(result.Name == tag.Name);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void RemoveTagTest(bool expected)
        {
            SetUp();
            var tagId = _random.Next();
            _accessor.Setup(e => e.RemoveTag(tagId, _user.Id)).Returns(expected);
            var responce = _controller.RemoveTag(tagId);
            var result = TransformResult.GetPrimitive<bool>(responce);
            _accessor.Verify(e => e.RemoveTag(tagId, _user.Id), Times.Once);
            Assert.IsTrue(result == expected);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void RemoveTagObjectLinkTest(bool expected)
        {
            SetUp();
            var dish = DishFactory.Create();
            var tagObject = TagObjectFactory.Create();
            _accessor.Setup(e => e.RemoveTagObject(tagObject.TagId, _user.Id)).Returns(expected);
            _accessor.Setup(e => e.GetTagObjectById(tagObject.ObjectId, (int)tagObject.ObjectTypeId, tagObject.TagId)).Returns(tagObject);
            _accessor.Setup(e => e.GetObjectFromTagObject(tagObject)).Returns(dish);
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, dish.DishCategoryLinks.First(
                l => l.IsActive == true && !l.IsDeleted).CafeCategory.CafeId)).Returns(true);

            var responce = _controller.RemoveTagObjectLink(tagObject.ObjectId, (int)tagObject.ObjectTypeId, tagObject.TagId);
            var result = TransformResult.GetPrimitive<bool>(responce);
            _accessor.Verify(e => e.RemoveTagObject(tagObject.TagId, _user.Id), Times.Once);
            _accessor.Verify(e => e.GetTagObjectById(tagObject.ObjectId, (int)tagObject.ObjectTypeId, tagObject.TagId), Times.Once);
            _accessor.Verify(e => e.GetObjectFromTagObject(tagObject), Times.Once);
            _accessor.Verify(e => e.IsUserManagerOfCafe(_user.Id, dish.DishCategoryLinks.First(
                l => l.IsActive == true && !l.IsDeleted).CafeCategory.CafeId), Times.Once);
            Assert.IsTrue(result == expected);
        }
    }
}