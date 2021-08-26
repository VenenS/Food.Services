using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Common;

namespace ITWebNet.Food.AuthorizationServer.Extensions
{
    public static class TagExtentions
    {
        public static TagModel GetContract(this Tag tag)
        {
            return tag == null
                ? null
                : new TagModel
                {
                    CreateDate = tag.CreateDate,
                    CreatorId = tag.CreatorId,
                    Id = tag.Id,
                    IsActive = tag.IsActive,
                    LastUpdateByUserId = tag.LastUpdateByUserId,
                    LastUpdDate = tag.LastUpdDate,
                    Name = tag.Name,
                    ParentId = tag.ParentId
                };
        }

        public static Tag GetEntity(this TagModel tag)
        {
            return tag == null
                ? new Tag()
                : new Tag
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    ParentId = tag.ParentId
                };
        }
    }
}