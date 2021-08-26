using Food.Data.Entities;
using System;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ICategory
    {
        #region Category

        /// <summary>
        /// Возвращает категорию по идентификатору
        /// </summary>
        /// <param name="id">идентификатор категории</param>
        /// <returns></returns>
        DishCategory GetFoodCategoryById(long id);

        /// <summary>
        /// Возвращает список всех активных категорий
        /// </summary>
        /// <returns></returns>
        List<DishCategory> GetFoodCategories();

        //TODO: добавить в фейковую БД возможность вызова транзакции
        /// <summary>
        /// Получение категории с блюдами
        /// </summary>
        /// <param name="wantedDate"></param>
        /// <param name="cafeId"></param>
        /// <returns></returns>
        Dictionary<DishCategory, List<Dish>> GetFoodCategoriesForManager(DateTime? wantedDate, long cafeId);

        //TODO: добавить в фейковую БД возможность вызова транзакции
        /// <summary>
        /// Получение катгории с блюдами из таблицы dish_version
        /// </summary>
        /// <param name="wantedDate"></param>
        /// <param name="cafeId"></param>
        /// <returns></returns>
        Dictionary<DishCategory, List<DishVersion>> GetFoodCategoriesVersionForManager(DateTime? wantedDate,
            long cafeId);

        /// <summary>
        /// Возвращает список всех активных категорий кафе
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        List<DishCategoryInCafe> GetFoodCategoriesByCafeId(long cafeId);

        /// <summary>
        /// Создает связь кафе-категория
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="categoryId">идентификатор категории</param>
        /// <param name="categoryIndex">приоритет категории</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        Int64 AddCafeFoodCategory(long cafeId, long categoryId, int categoryIndex, long userId);

        /// <summary>
        /// Меняем приоритет у категории для кафе
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="categoryId">идентификатор категории</param>
        /// <param name="categoryIndex">назначаемый приоритет</param>
        /// <param name="currentIndex">текущий приоритет</param>
        /// <param name="shiftUp">сдвиг вверх</param>
        void ShiftCategories(
            long cafeId,
            long categoryId,
            int categoryIndex,
            int currentIndex,
            bool shiftUp
        );

        /// <summary>
        /// Удаляем привязку категории к кафе
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="categoryId">идентификатор категории</param>
        /// <param name="userId"></param>
        bool RemoveCafeFoodCategory(long cafeId, long categoryId, long userId);

        /// <summary>
        /// Меняем приоритет категории
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="categoryId">идентификтаор категории</param>
        /// <param name="categoryOrder">приоритет категории</param>
        /// <param name="userId">идентификатор пользователя</param>
        void ChangeFoodCategoryOrder(long cafeId, long categoryId, long categoryOrder, long userId);

        /// <summary>
        /// Обновить индексацию категорий у кафе
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе</param>
        void UpdateIndexCategories(long cafeId);

        /// <summary>
        /// Перемещение блюд из категории в категорию "Блюда удаленных категорий" c id=-1 для всех кафе
        /// </summary>
        void MoveDishesFromCategory(long categoryId);

        #endregion
    }
}
