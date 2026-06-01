namespace CarRentalSystem.DTO.Review;
public class ReviewDto
{
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int ReservationId { get; set; }
        public int CarId { get; set; }
        public string CarName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
}