using Food.Data.Entities;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IImage
    {
        #region Images

        /// <summary>
        /// Добавляет картинку к объекту
        /// </summary>
        /// <param name="image">картинка (сущность)</param>
        /// <returns></returns>
        bool AddImage(Image image);

        /// <summary>
        /// Удаляет картинку у объекта
        /// </summary>
        /// <param name="image">картинка (сущность)</param>
        /// <returns></returns>
        bool RemoveImage(Image image);

        /// <summary>
        /// Возвращает список картинок для объекта
        /// </summary>
        /// <param name="objectId">идентификатор объекта</param>
        /// <param name="objectType">тип объекта</param>
        /// <returns></returns>
        List<string> GetImages(long objectId, int objectType);

        #endregion
    }
}
