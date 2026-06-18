using AutoMapper;
using CarRentalSystem.Models;
using CarRentalSystem.DTO;
using CarRentalSystem.DTO.Promotion;
using CarRentalSystem.DTO.Reservation;
using CarRentalSystem.DTO.Review;
using CarRentalSystem.DTO.Cars;
using CarRentalSystem.DTO.User;
using CarRentalSystem.DTO.Check;
using CarRentalSystem.DTO.Maintenance;
namespace CarRentalSystem.Migrations;
 public class MappingProfile : Profile
    {
        public MappingProfile()
        {
    
            CreateMap<Promotion, PromotionDto>();

        CreateMap<CreatePromotionRequest, Promotion>()
.ForMember(d => d.PromotionId, o => o.Ignore())
.ForMember(d => d.Code, o => o.MapFrom(s => s.Code.ToUpper()));

        CreateMap<MaintenanceAlert, MaintenanceAlertDto>()
                .ForMember(dest => dest.CarName, opt => opt.MapFrom(src => src.Car != null 
                    ? (src.Car.Brand != null ? $"{src.Car.Brand.BrandName} {src.Car.Model}" : src.Car.Model) 
                    : "Vehicle"));
                        CreateMap<CreateMaintenanceAlertRequest, MaintenanceAlert>()
                 .ForMember(d => d.MaintenanceAlertId, o => o.Ignore())
                 .ForMember(d => d.Status, o => o.Ignore())
                 .ForMember(d => d.CreatedAt, o => o.Ignore())
                 .ForMember(d => d.Car, o => o.Ignore());


                        CreateMap<CheckoutDetails, CheckoutDetailsDto>();
                        CreateMap<GateCheckoutRequest, CheckoutDetails>()
                .ForMember(d => d.CheckoutDetailsId, o => o.Ignore())
                .ForMember(d => d.ReservationId, o => o.Ignore())
                .ForMember(d => d.CompletedAt, o => o.Ignore())
                .ForMember(d => d.Reservation, o => o.Ignore());


                    CreateMap<CheckinDetails, CheckinDetailsDto>();
                    CreateMap<GateCheckinRequest, CheckinDetails>()
             .ForMember(d => d.CheckinDetailsId, o => o.Ignore())
             .ForMember(d => d.ReservationId, o => o.Ignore())
             .ForMember(d => d.CompletedAt, o => o.Ignore())
             .ForMember(d => d.Reservation, o => o.Ignore());


        CreateMap<Reservation, ReservationDto>()
    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ReservationId))
    .ForMember(dest => dest.PickupLocation, opt => opt.MapFrom(src => src.PickupLocation != null ? src.PickupLocation.LocationName : "San Francisco"))
    .ForMember(dest => dest.DropoffLocation, opt => opt.MapFrom(src => src.DropoffLocation != null ? src.DropoffLocation.LocationName : "San Francisco"))
    .ForMember(dest => dest.PickupDate, opt => opt.MapFrom(src => src.PickupDate.ToString("yyyy-MM-dd")))
    .ForMember(dest => dest.DropoffDate, opt => opt.MapFrom(src => src.DropDate.ToString("yyyy-MM-dd")))
    .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount ?? 0.00m))
    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.ReservationStatus != null ? src.ReservationStatus.StatusName.ToLower() : "confirmed"));
                        CreateMap<CreateReservationRequest, Reservation>()
                 .ForMember(d => d.ReservationId, o => o.Ignore())
                 .ForMember(d => d.UserId, o => o.Ignore())
                 .ForMember(d => d.PickupLocationId, o => o.Ignore())
                 .ForMember(d => d.DropoffLocationId, o => o.Ignore())
                 .ForMember(d => d.ReservationStatusId, o => o.Ignore())
                 .ForMember(d => d.DropDate, o => o.Ignore())
                 .ForMember(d => d.CreatedAt, o => o.Ignore())
                 .ForMember(d => d.UpdatedAt, o => o.Ignore())
                 .ForMember(d => d.User, o => o.Ignore())
                 .ForMember(d => d.Car, o => o.Ignore())
                 .ForMember(d => d.ReservationStatus, o => o.Ignore());


                 CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.CarId, opt => opt.MapFrom(src => src.Reservation != null ? src.Reservation.CarId : 0))
                .ForMember(dest => dest.CarName, opt => opt.MapFrom(src => src.Reservation != null && src.Reservation.Car != null 
                    ? $"{(src.Reservation.Car.Brand != null ? src.Reservation.Car.Brand.BrandName : "Vehicle")} {src.Reservation.Car.Model}" 
                    : "Vehicle"))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : "Valued Customer"))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating ?? 5))
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment ?? string.Empty))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt != null ? src.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm") : DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")));

                        CreateMap<CreateReviewRequest, Review>()
                .ForMember(d => d.ReviewId, o => o.Ignore())
                .ForMember(d => d.UserId, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.IsDisputed, o => o.Ignore())
                .ForMember(d => d.DisputeResolution, o => o.Ignore())
                .ForMember(d => d.User, o => o.Ignore())
                .ForMember(d => d.Reservation, o => o.Ignore());


                    CreateMap<Car, CarDto>()
                .ForMember(d => d.Rating, o => o.MapFrom(s => 0))
                .ForMember(d => d.ReviewsCount, o => o.MapFrom(s => 0))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CarId))
                .ForMember(dest => dest.Make, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.BrandName : "Unknown"))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.CarYear ?? 2023))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : "Sedan"))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location != null ? src.Location.LocationName : "San Francisco"))
                .ForMember(dest => dest.PricePerDay, opt => opt.MapFrom(src => src.PricePerDay ?? 50.00m))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address ?? ("123 Rental Blvd, " + (src.Location != null ? src.Location.LocationName : "San Francisco"))))
                .ForMember(dest => dest.Available, opt => opt.MapFrom(src => src.CarStatus != null && src.CarStatus.StatusName == "Available"))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.CarImages != null && src.CarImages.Any() ? src.CarImages.First().ImageUrl : "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?auto=format&fit=crop&q=80&w=800"))
                .ForMember(dest => dest.Features, opt => opt.MapFrom(src => new List<string> {
                    src.FuelType != null ? src.FuelType.FuelTypeName : "",
                    src.Transmission ?? "",
                    src.Mileage ?? "",
                    src.Color ?? ""
                }.Where(s => !string.IsNullOrEmpty(s)).ToList()));

                  CreateMap<CreateCarRequest, Car>()
                .ForMember(d => d.CarId, o => o.Ignore())
                .ForMember(d => d.BrandId, o => o.Ignore())
                .ForMember(d => d.CategoryId, o => o.Ignore())
                .ForMember(d => d.FuelTypeId, o => o.Ignore())
                .ForMember(d => d.CarStatusId, o => o.Ignore())
                .ForMember(d => d.LocationId, o => o.Ignore())
                .ForMember(d => d.CarYear, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.Brand, o => o.Ignore())
                .ForMember(d => d.Category, o => o.Ignore())
                .ForMember(d => d.FuelType, o => o.Ignore())
                .ForMember(d => d.CarStatus, o => o.Ignore())
                .ForMember(d => d.CarImages, o => o.Ignore());


        CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : "Customer"))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ?? true));
        }
    }