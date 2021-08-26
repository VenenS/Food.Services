using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Tag

        /// <summary>
        /// Возвращает список всех тегов
        /// </summary>
        /// <returns></returns>
        public virtual List<Tag> GetFullListOfTags()
        {
            List<Tag> tags;

            using (var fc = GetContext())
            {
                tags = fc.Tags.AsNoTracking().Where(
                    t => t.IsActive
                         && t.IsDeleted == false
                    ).ToList();
            }

            return tags;
        }

        /// <summary>
        /// Получить всех "детей" тега
        /// </summary>
        /// <param name="tagId">идентификтаор тега</param>
        /// <returns></returns>
        public virtual List<Tag> GetChildListOfTagsByTagId(long tagId)
        {
            List<Tag> tags;

            using (var fc = GetContext())
            {
                tags = fc.Tags.AsNoTracking().Where(
                    t => t.ParentId == tagId
                         && t.IsActive
                         && t.IsDeleted == false
                    ).ToList();
            }

            return tags;
        }

        /// <summary>
        /// Возвращает список тегов по тексту
        /// </summary>
        /// <param name="textToFind">текст</param>
        /// <returns></returns>
        public virtual List<Tag> GetListOfTagsByString(string textToFind)
        {
            List<Tag> tags;

            using (var fc = GetContext())
            {
                tags = fc.Tags.AsNoTracking().Where(
                    t => t.Name.ToLower().Contains(
                        textToFind.ToLower()
                        )
                         && t.IsActive
                         && t.IsDeleted == false
                    ).ToList();
            }

            return tags;
        }

        /// <summary>
        /// Возвращает список основных тегов
        /// </summary>
        /// <returns></returns>
        public virtual List<Tag> GetRootTags()
        {
            List<Tag> tags;

            using (var fc = GetContext())
            {
                tags = fc.Tags.AsNoTracking().Where(
                    t => t.ParentId == null
                         && t.IsActive
                         && t.IsDeleted == false
                ).ToList();
            }

            return tags;
        }

        /// <summary>
        /// Возвращает тег по идентификатору
        /// </summary>
        /// <param name="id">идентификатор тега</param>
        /// <returns></returns>
        public virtual Tag GetTagById(long id)
        {
            Tag tag;

            using (var fc = GetContext())
            {
                tag = fc.Tags.AsNoTracking().FirstOrDefault(
                    t => t.Id == id
                         && t.IsActive
                         && t.IsDeleted == false
                    );
            }

            return tag;
        }

        /// <summary>
        /// добавить тег
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public virtual bool AddTag(Tag tag)
        {
            try
            {
                using (var fc = GetContext())
                {
                    fc.Tags.Add(tag);

                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Редактировать тег
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public virtual bool EditTag(Tag tag)
        {
            try
            {
                using (var fc = GetContext())
                {
                    Tag oldTag = fc.Tags.FirstOrDefault(
                        t => t.Id == tag.Id
                             && t.IsDeleted == false
                        );

                    if (oldTag != null)
                    {
                        oldTag.LastUpdDate = DateTime.Now;
                        oldTag.LastUpdateByUserId = tag.LastUpdateByUserId;
                        oldTag.Name = tag.Name;
                        oldTag.ParentId = tag.ParentId;
                        oldTag.IsActive = true;

                        fc.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Удалить тег
        /// </summary>
        /// <param name="tagId">идентификатор тега</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public virtual bool RemoveTag(long tagId, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    Tag oldTag = fc.Tags.FirstOrDefault(
                        t => t.Id == tagId
                             && t.IsDeleted == false
                        );

                    if (oldTag != null)
                    {
                        oldTag.IsActive = false;
                        oldTag.IsDeleted = true;
                        oldTag.LastUpdDate = DateTime.Now;
                        oldTag.LastUpdateByUserId = userId;

                        fc.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Возвращает список всех тегов для объекта
        /// </summary>
        /// <param name="objectId">идентификатор объекта</param>
        /// <param name="typeOfObject">тип объекта</param>
        /// <returns></returns>
        public virtual List<Tag> GetListOfTagsConnectedWithObjectAndHisChild(long objectId, long typeOfObject)
        {
            List<Tag> tags;

            using (var fc = GetContext())
            {
                tags = new List<Tag>();

                switch ((ObjectTypesEnum)typeOfObject)
                {
                    case ObjectTypesEnum.Cafe:
                        {
                            var queryToGetTagsFromCafe =
                                from tagObject in fc.TagObject.AsNoTracking()
                                where (
                                    tagObject.ObjectId == objectId
                                    && tagObject.ObjectTypeId == (int)ObjectTypesEnum.Cafe
                                    && tagObject.IsDeleted == false
                                )
                                select tagObject.Tag;
                            
                            tags = tags
                                       .Concat(queryToGetTagsFromCafe)
                                       .ToList();
                        }
                        break;
                    case ObjectTypesEnum.Category:
                        {
                            var queryToGetTagsFromDishes =
                                from tagObject in fc.TagObject.AsNoTracking()
                                from dish in fc.Dishes
                                where (
                                    tagObject.ObjectId == dish.Id
                                    && tagObject.ObjectTypeId == (int)ObjectTypesEnum.Dish
                                    && dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Select(l => l.CafeCategory.Id).Contains(objectId)
                                    && tagObject.IsDeleted == false
                                )
                                select tagObject.Tag;

                            tags = tags
                                       .Concat(queryToGetTagsFromDishes)
                                       .ToList();
                        }
                        break;
                    case ObjectTypesEnum.Dish:
                        {
                            var queryToGetTagsFromDishes =
                                from tagObject in fc.TagObject.AsNoTracking()
                                where (
                                    tagObject.ObjectId == objectId
                                    && tagObject.ObjectTypeId == (int)ObjectTypesEnum.Dish
                                    && tagObject.IsDeleted == false
                                )
                                select tagObject.Tag;

                            tags = tags
                                       .Concat(queryToGetTagsFromDishes)
                                       .ToList();
                        }
                        break;
                }

                tags = tags
                           .GroupBy(t => t.Id)
                           .Select(group => group.FirstOrDefault())
                           .ToList();
            }

            return tags;
        }
        #endregion
    }
}
