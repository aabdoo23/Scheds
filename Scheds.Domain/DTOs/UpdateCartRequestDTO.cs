using System.Collections.Generic;

namespace Scheds.Domain.DTOs
{
    public class CartItemDto
    {
        public string CourseCode { get; set; }
        public string Section { get; set; }
    }

    public class UpdateCartRequestDTO
    {
        public List<CartItemDto> Items { get; set; }
    }
}
