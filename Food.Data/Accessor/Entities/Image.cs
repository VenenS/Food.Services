using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Images

        /// <summary>
        /// Добавляет картинку к объекту
        /// </summary>
        /// <param name="image">картинка (сущность)</param>
        /// <returns></returns>
        public bool AddImage(Image image)
        {
            try
            {
                bool isCorrectObject = false;
                using (var fc = GetContext())
                {
                    var imgInDb =
                        fc.Images.FirstOrDefault(
                            i => i.Hash == image.Hash
                            && i.ObjectId == image.ObjectId
                            && i.ObjectType == image.ObjectType
                            && i.IsDeleted == false
                    );

                    if (imgInDb == null)
                    {
                        switch (image.ObjectType)
                        {
                            case (int)ObjectTypesEnum.Cafe:
                                {
                                    var cafe = fc.Cafes.FirstOrDefault(
                                        c => c.Id == image.ObjectId
                                             && c.IsActive == true
                                             && c.IsDeleted == false
                                    );

                                    if (cafe != null)
                                        isCorrectObject = true;
                                    break;
                                }

                            case (int)ObjectTypesEnum.Dish:
                                {
                                    Dish dish =
                                        fc.Dishes.FirstOrDefault(
                                            d => d.Id == image.ObjectId
                                            && d.IsActive == true
                                            && d.IsDeleted == false
                                        );

                                    if (dish != null)
                                        isCorrectObject = true;
                                    break;
                                }
                        }

                        if (isCorrectObject)
                        {
                            fc.Images.Add(image);
                            fc.SaveChanges();
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        if(imgInDb.IsDeleted)
                        {
                            imgInDb.IsDeleted = false;
                            imgInDb.LastUpdateByUserId = image.LastUpdateByUserId;
                            imgInDb.LastUpdDate = image.LastUpdDate;
                            fc.SaveChanges();
                        }
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Удаляет картинку у объекта
        /// </summary>
        /// <param name="image">картинка (сущность)</param>
        /// <returns></returns>
        public bool RemoveImage(Image image)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var imgInDb =
                        fc.Images.FirstOrDefault(
                            i => i.Hash == image.Hash
                            && i.ObjectId == image.ObjectId
                            && i.ObjectType == image.ObjectType
                            && i.IsDeleted == false
                    );

                    if (imgInDb != null)
                    {
                        imgInDb.IsDeleted = true;
                        imgInDb.LastUpdateByUserId = image.LastUpdateByUserId;
                        imgInDb.LastUpdDate = image.LastUpdDate;
                        fc.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Возвращает список картинок для объекта
        /// </summary>
        /// <param name="objectId">идентификатор объекта</param>
        /// <param name="objectType">тип объекта</param>
        /// <returns></returns>
        public List<string> GetImages(long objectId, int objectType)
        {
            try
            {
                using (var fc = GetContext())
                {
                    return fc.Images.AsNoTracking()
                        .Where(i => i.ObjectId == objectId
                            && i.ObjectType == objectType
                            && i.IsDeleted == false)
                        .Select(i => i.Hash)
                        .ToList();
                }
            }
            catch
            {
                return new List<string>();
            }
        }

        #endregion
    }
}
