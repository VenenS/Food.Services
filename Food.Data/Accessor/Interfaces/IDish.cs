using Food.Data.Accessor.Extensions;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IDish
    {
        #region Dish

        /// <summary>
        /// Проверяет уникальное ли имя блюда в рамках кафе
        /// </summary>
        bool CheckUniqueNameWithinCafe(string dishName, long cafeId, long dishId = -1);

        /// <summary>
        /// Возвращает список блюд указанной категории
        /// </summary>
        /// <param name="foodCategoryId">идентификатор категории</param>
        /// <returns></returns>
        List<Dish> GetFoodDishesByCategoryId(long foodCategoryId);

        /// <summary>
        /// Возвращает список блюд кафе на указанную дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="wantedDate">дата (опционально)</param>
        /// <returns></returns>
        List<Dish> GetFoodDishesByCafeId(long cafeId, DateTime? wantedDate = null);

        /// <summary>
        /// Возвращает снапшот всех блюд кафе, которые были активны на указанную дату,
        /// сгруппированые по категориям блюд.
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе</param>
        /// <param name="snapshotDate"></param>
        /// <remarks>Внимание: возвращает блюда только существующих категорий. Если на указанную дату
        /// существовали блюда, но связанная категория была удалена - эта категория и связанные с ней
        /// блюда не будут включены в результат.</remarks>
        IEnumerable<Tuple<DishCategory, IEnumerable<Dish>>> GroupDishesByCategory(long cafeId, DateTime snapshotDate);

        /// <summary>
        /// Возвращает список блюд для кафе в указанной категории на дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="categoryId">идентификатор категории</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        List<Dish> GetFoodDishesByCategoryIdAndCafeId(Int64 cafeId, Int64 categoryId, DateTime date);

        /// <summary>
        /// Возвращает список блюд по идентификтаорам
        /// </summary>
        /// <param name="dishId">список идентификаторов</param>
        /// <returns></returns>
        List<Dish> GetFoodDishesById(long[] dishId);

        /// <summary>
        /// Возвращает версионное блюдо
        /// </summary>
        /// <param name="dishId">идентификатор блюда</param>
        /// <returns></returns>
        List<DishVersion> GetFoodDishVersionsById(long dishId);

        /// <summary>
        /// Добавляет блюдо
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="categoryId">идентификатор категории</param>
        /// <param name="dish">блюдо (сущность)</param>
        /// <param name="userId">идентификтаор пользователя</param>
        /// <returns></returns>
        long AddDish(long cafeId, long categoryId, Dish dish, long userId);

        /// <summary>
        /// Редактирует блюдо
        /// </summary>
        /// <param name="dish">блюдо</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        long EditDish(Dish dish, long userId);

        /// <summary>
        /// Удаляет блюдо (сейчас перемещает блюдо в категорию "Удаленные блюда")
        /// </summary>
        /// <param name="dishId">идентификатор блюда</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        long RemoveDish(long dishId, long userId);

        /// <summary>
        /// Изменяет порядок блюд в категории от newIndex до oldIndex
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе</param>
        /// <param name="categoryId">Идентификатор категории</param>
        /// <param name="newIndex">Новый индекс блюда</param>
        /// <param name="oldIndex">Старый индекс блюда</param>
        /// <param name="dishId">Идентификатор блюда</param>
        void ChangeDishIndex(long cafeId, long categoryId, int newIndex, int oldIndex, int? dishId = null);

        /// <summary>
        /// Обновляет индексы в категории, в которую перенесли блюдо
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе</param>
        /// <param name="categoryId">Идентификатор категории</param>
        /// <param name="newIndex">Новый индекс блюда</param>
        /// <param name="dishId">Идентификатор блюда</param>
        void UpdateDishIndexInSecondCategory(long cafeId, long categoryId, int newIndex, long dishId);

        /// <summary>
        /// Обновляет индексы в категории, из которой перенесли блюдо. И меняет категорию у блюда.
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе</param>
        /// <param name="newCategoryId">Идентификатор категории, в которую перенесли</param>
        /// <param name="oldCategoryId">Идентификатор категории, из которой взяли блюдо</param>
        /// <param name="newIndex">Новый индекс блюда</param>
        /// <param name="oldIndex">Старый индекс блюда</param>
        /// <param name="dishId">Идентификатор блюда</param>
        void UpdateDishIndexInFirstCategory(long cafeId, long newCategoryId, long oldCategoryId, int newIndex,
            int oldIndex, long dishId);

        Dictionary<DishCategory, List<Dish>> GetDishesByScheduleByDate(long cafeId, DateTime? date);

        #endregion
    }
}