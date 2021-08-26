using Food.Data.Entities;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ITag
    {
        #region Tag

        /// <summary>
        /// Возвращает список всех тегов
        /// </summary>
        /// <returns></returns>
        List<Tag> GetFullListOfTags();

        /// <summary>
        /// Получить всех "детей" тега
        /// </summary>
        /// <param name="tagId">идентификтаор тега</param>
        /// <returns></returns>
        List<Tag> GetChildListOfTagsByTagId(long tagId);

        /// <summary>
        /// Возвращает список тегов по тексту
        /// </summary>
        /// <param name="textToFind">текст</param>
        /// <returns></returns>
        List<Tag> GetListOfTagsByString(string textToFind);

        /// <summary>
        /// Возвращает список основных тегов
        /// </summary>
        /// <returns></returns>
        List<Tag> GetRootTags();

        /// <summary>
        /// Возвращает тег по идентификатору
        /// </summary>
        /// <param name="id">идентификатор тега</param>
        /// <returns></returns>
        Tag GetTagById(long id);

        /// <summary>
        /// добавить тег
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        bool AddTag(Tag tag);

        /// <summary>
        /// Редактировать тег
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        bool EditTag(Tag tag);

        /// <summary>
        /// Удалить тег
        /// </summary>
        /// <param name="tagId">идентификатор тега</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        bool RemoveTag(long tagId, long userId);

        /// <summary>
        /// Возвращает список всех тегов для объекта
        /// </summary>
        /// <param name="objectId">идентификатор объекта</param>
        /// <param name="typeOfObject">тип объекта</param>
        /// <returns></returns>
        List<Tag> GetListOfTagsConnectedWithObjectAndHisChild(long objectId, long typeOfObject);

        #endregion
    }
}
