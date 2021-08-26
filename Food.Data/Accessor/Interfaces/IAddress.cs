using Food.Data.Entities;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IAddress
    {
        /// <summary>
        /// Возвращает адрес по идентификатору
        /// </summary>
        /// <param name="id">идентификатор адреса</param>
        /// <returns></returns>
        Address GetAddressById(long? id);

        /// <summary>
        /// Возвращает список всех адресов
        /// </summary>
        /// <returns></returns>
        List<Address> GetAllAddresses();

        /// <summary>
        /// Возвращает адрес компоненты которого полностью соответствую заданным.
        /// </summary>
        /// <param name="cityName">Название города</param>
        /// <param name="streetName">Название улицы</param>
        /// <param name="houseNumber">Номер дома</param>
        /// <param name="officeNumber">Номер офиса</param>
        /// <returns>Возвращает адрес или null, если такого адреса не существует.</returns>
        Address GetAddressByComponents(string cityName, string streetName, string houseNumber, string officeNumber);

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
        long AddAddress(
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
       );

        /// <summary>
        /// Добавляет адрес в БД.
        /// </summary>
        /// <param name="address"></param>
        /// <returns>Возвращает ID новой записи.</returns>
        long AddAddress(Address address);

        /// <summary>
        /// Обновить адрес
        /// </summary>
        /// <param name="address"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        long UpdateAddress(Address address, long userId);

        /// <summary>
        /// Возвращает список всех адресов компании
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <returns></returns>
        List<CompanyAddress> GetCompanyAddresses(long companyId);

        /// <summary>
        /// Удаляет адрес по идентификатору.
        /// </summary>
        /// <param name="id">ID адреса</param>
        /// <param name="userId">ID пользователя, производящего изменения</param>
        /// <returns>true если адрес был найден и успешно удален, false в противном случае.</returns>
        bool DeleteAddress(long id, long userId);
    }
}
