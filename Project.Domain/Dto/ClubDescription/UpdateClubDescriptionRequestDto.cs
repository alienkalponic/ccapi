using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Project.Domain.Dto.ClubDescription
{
    public class UpdateClubDescriptionRequestDto
    {
        public long ClubDescriptionId { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }

        /// <summary>
        /// IDs of existing images to delete (must belong to this ClubDescription).
        /// For multipart/form-data, send as repeated fields: DeletedImageIds=1&DeletedImageIds=2
        /// </summary>
        public List<long> DeletedImageIds { get; set; } = new();

        /// <summary>
        /// New images to add during update.
        /// </summary>
        public List<IFormFile> NewImages { get; set; } = new();
    }
}
