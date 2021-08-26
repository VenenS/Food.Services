using Food.Data.Entities;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ITagObject
    {
        #region TagObject

        /// <summary>
        /// Добавить связь объект-тег
        /// </summary>
        /// <param name="tagObject"></param>
        /// <returns></returns>
        bool AddTagObject(TagObject tagObject);

        /// <summary>
        /// Редактировать свзяь объект-тег
        /// </summary>
        /// <param name="tagObject"></param>
        /// <returns></returns>
        bool EditTagObject(TagObject tagObject);

        /// <summary>
        /// Удалить связь объект-тег
        /// </summary>
        /// <param name="tagObjectId">идентификатор связи</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        bool RemoveTagObject(long tagObjectId, long userId);


        /// <summary>
        /// Возвращает свзяь объект-тег
        /// </summary>
        /// <param name="objectId">идентификатор объекта</param>
        /// <param name="objectType">тип объекта</param>
        /// <param name="tagId">идентификатор тега</param>
        /// <returns></returns>
        TagObject GetTagObjectById(long objectId, int objectType, long tagId);

        /// <summary>
        /// Вернуть объект по связи тег-объект
        /// </summary>
        /// <param name="tagObject"></param>
        /// <returns></returns>
        object GetObjectFromTagObject(TagObject tagObject);

        /// <summary>
        /// Возвращает список кафе по списку тегов
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        List<Cafe> GetCafesByTagList(List<long> tags);

        /// <summary>
        /// Возвращает список категорий по списку тегов
        /// </summary>
        /// <param name="tags">список тегов</param>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        List<DishCategoryInCafe> GetCategorysByTagListAndCafeId(List<long> tags, long cafeId);

        /// <summary>
        /// Возвращает список блюд по списку тегов
        /// </summary>
        /// <param name="tags">список тегов</param>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        List<Dish> GetDishesByTagListAndCafeId(List<long> tags, long cafeId);

        /// <summary>
        /// Возвращает список тегов по списку кафе
        /// </summary>
        /// <param name="cafesList"></param>
        /// <returns></returns>
        List<Tag> GetTagsByCafesList(List<long> cafesList);

        #endregion
    }

}