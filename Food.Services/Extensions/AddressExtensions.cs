using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Common;

namespace Food.Services.Extensions
{
    public static class AddressExtensions
    {
        public static DeliveryAddressModel GetContract(this Address address)
        {
            return (address == null)
                ? null
                : new DeliveryAddressModel
                {
                    BuildingNumber = address.BuildingNumber,
                    CityName = address.CityName,
                    ExtraInfo = address.ExtraInfo,
                    FlatNumber = address.FlatNumber,
                    HouseNumber = address.HouseNumber,
                    Id = address.Id,
                    EntranceNumber = address.EntranceNumber,
                    StoreyNumber =  address.StoreyNumber,
                    IntercomNumber = address.IntercomNumber,
                    OfficeNumber = address.OfficeNumber,
                    PostalCode = address.PostalCode,
                    StreetName = address.StreetName,
                    RawAddress = address.RawAddress ?? $"г {address.CityName}, " +
                          $"ул. {address.StreetName} " +
                          $"д. {address.HouseNumber}{address.BuildingNumber} " +
                          $"оф. {address.OfficeNumber}",
                    AddressComment = address.AddressComment,
                    CityId = address.CityId,
                    City = address.City.GetContract()
                };
        }

        public static Address GetEntity(this DeliveryAddressModel address)
        {
            return (address == null)
                ? new Address()
                : new Address
                {
                    BuildingNumber = address.BuildingNumber,
                    CityName = address.CityName,
                    ExtraInfo = address.ExtraInfo,
                    FlatNumber = address.FlatNumber,
                    HouseNumber = address.HouseNumber,
                    OfficeNumber = address.OfficeNumber,
                    EntranceNumber = address.EntranceNumber,
                    StoreyNumber = address.StoreyNumber,
                    IntercomNumber = address.IntercomNumber,
                    PostalCode = address.PostalCode,
                    StreetName = address.StreetName,
                    AddressComment = address.AddressComment,
                    RawAddress = address.RawAddress,
                    CityId = address.CityId
                };
        }

        public static DeliveryAddressModel GetAddressCompanyModel(this CompanyAddress address)
        {
            return (address?.Address == null || address.Address.IsDeleted)
                ? null
                : new DeliveryAddressModel
                {
                    BuildingNumber = address.Address.BuildingNumber,
                    CityName = address.Address.CityName,
                    ExtraInfo = address.Address.ExtraInfo,
                    FlatNumber = address.Address.FlatNumber,
                    HouseNumber = address.Address.HouseNumber,
                    CompanyAddressId = address.Id,
                    Id = address.Address.Id,
                    OfficeNumber = address.Address.OfficeNumber,
                    PostalCode = address.Address.PostalCode,
                    StreetName = address.Address.StreetName,
                    RawAddress = address.Address.RawAddress,
                    AddressComment = address.Address.AddressComment,
                    DisplayType = (DisplayAddressType)address.DisplayType,
                    IsActive = address.IsActive,
                };
        }
    }
}
