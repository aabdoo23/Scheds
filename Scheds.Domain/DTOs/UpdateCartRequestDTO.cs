using System.Collections.Generic;

namespace Scheds.Domain.DTOs
{
    public class UpdateCartRequestDTO
    {
        public List<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();
    }
}
