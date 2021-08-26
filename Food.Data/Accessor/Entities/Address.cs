using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Address

        /// <summary>
        /// Возвращает адрес по идентификатору
        /// </summary>
        /// <param name="id">идентификатор адреса</param>
        /// <returns></returns>
        public Address GetAddressById(long? id)
        {
            if (id == null) return null;
            var fc = GetContext();
            return fc.Addresses.Include(c => c.City).AsNoTracking().FirstOrDefault(a => a.Id == id);
        }

        /// <summary>
        /// Возвращает список всех адресов
        /// </summary>
        /// <returns></returns>
        public List<Address> GetAllAddresses()
        {
            var fc = GetContext();
            return fc.Addresses.Include(c => c.City).AsNoTracking().Where(x => !x.IsDeleted).ToList();
        }

        /// <summary>
        /// Возвращает адрес компоненты которого полностью соответствую заданным.
        /// </summary>
        /// <param name="cityName">Название города</param>
        /// <param name="streetName">Название улицы</param>
        /// <param name="houseNumber">Номер дома</param>
        /// <param name="officeNumber">Номер офиса</param>
        /// <returns>Возвращает адрес или null, если такого адреса не существует.</returns>
        public Address GetAddressByComponents(string cityName, string streetName, string houseNumber, string officeNumber)
        {
            var fc = GetContext();

            try
            {
                return fc.Addresses
                    .AsNoTracking()
                    .FirstOrDefault(c =>
                        c.CityName.Trim().ToLower() == cityName.Trim().ToLower()
                        && c.StreetName.Trim().ToLower() == streetName.Trim().ToLower()
                        && c.HouseNumber.Trim().ToLower() == houseNumber.Trim().ToLower()
                        && c.OfficeNumber.Trim().ToLower() == officeNumber.Trim().ToLower()
                        && !c.IsDeleted);
            }
            catch (Exception)
            {
                // TODO: handle exception.
            }
            return null;
        }

        /// <summary>
        /// Добавление адреса
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="streetId"></param>
        /// <param name="houseId"></param>
        /// <param name="creatorId"></param>
        /// <param name="cityName"></param>
        /// <param name="streetName"></param>
        /// <param name="houseNumber"></param>
        /// <param name="buildingNumber"></param>
        /// <param name="flatNumber"></param>
        /// <param name="officeNumber"></param>
        /// <param name="entranceNumber"></param>
        /// <param name="storeyNumber"></param>
        /// <param name="intercomNumber"></param>
        /// <param name="extraInfo"></param>
        /// <param name="postalCode"></param>
        /// <param name="addressComment"></param>
        /// <param name="rawAddress"></param>
        /// <returns></returns>
        public long AddAddress(
        long cityId,
        long streetId,
        long houseId,
        long creatorId,
        string cityName,
        string streetName,
        string houseNumber,
        string buildingNumber,
        string flatNumber,
        string officeNumber,
        string entranceNumber,
        string storeyNumber,
        string intercomNumber,
        string extraInfo,
        string postalCode,
        string addressComment,
        string rawAddress
       )
        {
            try
            {
                using (var fc = GetContext())
                {
                    Address address = new Address()
                    {
                        CityId = cityId,
                        StreetId = streetId,
                        HouseId = houseId,
                        CityName = cityName,
                        StreetName = streetName,
                        HouseNumber = houseNumber,
                        BuildingNumber = buildingNumber,
                        FlatNumber = flatNumber,
                        OfficeNumber = officeNumber,
                        EntranceNumber = entranceNumber,
                        StoreyNumber = storeyNumber,
                        IntercomNumber = intercomNumber,
                        ExtraInfo = extraInfo,
                        PostalCode = postalCode,
                        AddressComment = addressComment,
                        RawAddress = rawAddress,
                        CreationDate = DateTime.Now,
                        CreatorId = creatorId
                    };

                    fc.Addresses.Add(address);

                    fc.SaveChanges();

                    return address.Id;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Добавляет адрес в БД.
        /// </summary>
        /// <param name="address"></param>
        /// <returns>Возвращает ID новой записи.</returns>
        public long AddAddress(Address address)
        {
            try
            {
                using (var fc = GetContext())
                {
                    fc.Addresses.Add(address);
                    fc.SaveChanges();
                    return address.Id;
                }
            }
            catch (Exception)
            {
                // TODO: handle exception.
            }
            return -1;
        }

        /// <summary>
        /// Обновить адрес
        /// </summary>
        /// <param name="address"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public long UpdateAddress(Address address, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    Address oldAddress =
                        fc.Addresses.FirstOrDefault(
                            s => s.Id == address.Id
                            //&& s.IsActive == true
                            && s.IsDeleted == false
                            );

                    if (oldAddress != null)
                    {
                        oldAddress.CityId = address.CityId;
                        oldAddress.StreetId = address.StreetId;
                        oldAddress.HouseId = address.HouseId;
                        oldAddress.CityName = address.CityName;
                        oldAddress.StreetName = address.StreetName;
                        oldAddress.HouseNumber = address.HouseNumber;
                        oldAddress.BuildingNumber = address.BuildingNumber;
                        oldAddress.FlatNumber = address.FlatNumber;
                        oldAddress.OfficeNumber = address.OfficeNumber;
                        oldAddress.EntranceNumber = address.EntranceNumber;
                        oldAddress.StoreyNumber = address.StoreyNumber;
                        oldAddress.IntercomNumber = address.IntercomNumber;
                        oldAddress.ExtraInfo = address.ExtraInfo;
                        oldAddress.PostalCode = address.PostalCode;
                        oldAddress.RawAddress = string.Join(", ", new List<string> {
                            !string.IsNullOrWhiteSpace(address.StreetName) ? $"ул. {address.StreetName}" : "",
                            !string.IsNullOrWhiteSpace(address.HouseNumber) ? $"д. {address.HouseNumber}" : "",
                            !string.IsNullOrWhiteSpace(address.BuildingNumber) ? $"стр. {address.BuildingNumber}" : "",
                            !string.IsNullOrWhiteSpace(address.FlatNumber) ? $"кв. {address.FlatNumber}" : "",
                            !string.IsNullOrWhiteSpace(address.OfficeNumber) ? $"оф. {address.BuildingNumber}" : "",
                            !string.IsNullOrWhiteSpace(address.EntranceNumber) ? $"под. {address.EntranceNumber}" : "",
                            !string.IsNullOrWhiteSpace(address.StoreyNumber) ? $"э. {address.StoreyNumber}" : "",
                            !string.IsNullOrWhiteSpace(address.IntercomNumber) ? $"дом. {address.IntercomNumber}" : ""
                        }.Where(x => x.Length > 0));
                        oldAddress.AddressComment = address.AddressComment;
                        oldAddress.LastUpdateByUserId = userId;
                        oldAddress.LastUpdDate = DateTime.Now;
                        oldAddress.City = address.City;

                        fc.SaveChanges();
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return address.Id;
        }
        #endregion

        /// <summary>
        /// Возвращает список всех адресов компании
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <returns></returns>
        public List<CompanyAddress> GetCompanyAddresses(long companyId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    return fc.CompanyAddresses.AsNoTracking().Where(e => e.CompanyId == companyId && !e.IsDeleted).ToList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Удаляет адрес по идентификатору.
        /// </summary>
        /// <param name="id">ID адреса</param>
        /// <param name="userId">ID пользователя, производящего изменения</param>
        /// <returns>true если адрес был найден и успешно удален, false в противном случае.</returns>
        public bool DeleteAddress(long id, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var address = fc.Addresses.FirstOrDefault(c => c.Id == id);

                    if (address != null)
                    {
                        address.LastUpdateByUserId = userId;
                        address.LastUpdDate = DateTime.Now;
                        address.IsDeleted = true;
                        fc.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // TODO: handle exception.
            }
            return false;
        }
    }
}
