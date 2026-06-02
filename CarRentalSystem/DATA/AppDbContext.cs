using Microsoft.EntityFrameworkCore;
using CarRentalSystem.Models;
using BCrypt.Net;
namespace CarRentalSystem.DATA
{
    public class AppDbContext:DbContext
    {
       public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<PaymentStatus> PaymentStatuses { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ReservationStatus> ReservationStatuses { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<CarBrand> CarBrands { get; set; }
        public DbSet<CarCategory> CarCategories { get; set; }
        public DbSet<FuelType> FuelTypes { get; set; }
        public DbSet<CarStatus> CarStatuses { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<CarImage> CarImages { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<MaintenanceAlert> MaintenanceAlerts { get; set; }
        public DbSet<CheckoutDetails> CheckoutDetails { get; set; }
        public DbSet<CheckinDetails> CheckinDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().HasKey(r => r.RoleId);
            modelBuilder.Entity<Role>().Property(r => r.RoleId).UseIdentityColumn();

            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<User>().Property(u => u.UserId).UseIdentityColumn();

            modelBuilder.Entity<Review>().HasKey(r => r.ReviewId);
            modelBuilder.Entity<Review>().Property(r => r.ReviewId).UseIdentityColumn();

            modelBuilder.Entity<PaymentMethod>().HasKey(pm => pm.PaymentMethodId);
            modelBuilder.Entity<PaymentMethod>().Property(pm => pm.PaymentMethodId).UseIdentityColumn();

            modelBuilder.Entity<PaymentStatus>().HasKey(ps => ps.PaymentStatusId);
            modelBuilder.Entity<PaymentStatus>().Property(ps => ps.PaymentStatusId).UseIdentityColumn();

            modelBuilder.Entity<Payment>().HasKey(p => p.PaymentId);
            modelBuilder.Entity<Payment>().Property(p => p.PaymentId).UseIdentityColumn();

            modelBuilder.Entity<ReservationStatus>().HasKey(rs => rs.ReservationStatusId);
            modelBuilder.Entity<ReservationStatus>().Property(rs => rs.ReservationStatusId).UseIdentityColumn();

            modelBuilder.Entity<Location>().HasKey(l => l.LocationId);
            modelBuilder.Entity<Location>().Property(l => l.LocationId).UseIdentityColumn();

            modelBuilder.Entity<CarBrand>().HasKey(cb => cb.BrandId);
            modelBuilder.Entity<CarBrand>().Property(cb => cb.BrandId).UseIdentityColumn();

            modelBuilder.Entity<CarCategory>().HasKey(cc => cc.CategoryId);
            modelBuilder.Entity<CarCategory>().Property(cc => cc.CategoryId).UseIdentityColumn();

            modelBuilder.Entity<FuelType>().HasKey(ft => ft.FuelTypeId);
            modelBuilder.Entity<FuelType>().Property(ft => ft.FuelTypeId).UseIdentityColumn();

            modelBuilder.Entity<CarStatus>().HasKey(cs => cs.CarStatusId);
            modelBuilder.Entity<CarStatus>().Property(cs => cs.CarStatusId).UseIdentityColumn();

            modelBuilder.Entity<Car>().HasKey(c => c.CarId);
            modelBuilder.Entity<Car>().Property(c => c.CarId).UseIdentityColumn();

            modelBuilder.Entity<CarImage>().HasKey(ci => ci.ImageId);
            modelBuilder.Entity<CarImage>().Property(ci => ci.ImageId).UseIdentityColumn();

            modelBuilder.Entity<Reservation>().HasKey(r => r.ReservationId);
            modelBuilder.Entity<Reservation>().Property(r => r.ReservationId).UseIdentityColumn();

            modelBuilder.Entity<Promotion>().HasKey(p => p.PromotionId);
            modelBuilder.Entity<Promotion>().Property(p => p.PromotionId).UseIdentityColumn();

            modelBuilder.Entity<MaintenanceAlert>().HasKey(ma => ma.MaintenanceAlertId);
            modelBuilder.Entity<MaintenanceAlert>().Property(ma => ma.MaintenanceAlertId).UseIdentityColumn();

            modelBuilder.Entity<CheckoutDetails>().HasKey(cod => cod.CheckoutDetailsId);
            modelBuilder.Entity<CheckoutDetails>().Property(cod => cod.CheckoutDetailsId).UseIdentityColumn();

            modelBuilder.Entity<CheckinDetails>().HasKey(cid => cid.CheckinDetailsId);
            modelBuilder.Entity<CheckinDetails>().Property(cid => cid.CheckinDetailsId).UseIdentityColumn();

            modelBuilder.Entity<Car>()
                .Property(c => c.PricePerDay)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Reservation>()
                .Property(r => r.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Customer" }
            );

            modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod { PaymentMethodId = 1, MethodName = "Credit Card" },
                new PaymentMethod { PaymentMethodId = 2, MethodName = "PayPal" },
                new PaymentMethod { PaymentMethodId = 3, MethodName = "Apple Pay" }
            );

            modelBuilder.Entity<PaymentStatus>().HasData(
                new PaymentStatus { PaymentStatusId = 1, StatusName = "Paid" },
                new PaymentStatus { PaymentStatusId = 2, StatusName = "Pending" },
                new PaymentStatus { PaymentStatusId = 3, StatusName = "Refunded" }
            );

            modelBuilder.Entity<ReservationStatus>().HasData(
                new ReservationStatus { ReservationStatusId = 1, StatusName = "Confirmed" },
                new ReservationStatus { ReservationStatusId = 2, StatusName = "Completed" },
                new ReservationStatus { ReservationStatusId = 3, StatusName = "Cancelled" }
            );

            modelBuilder.Entity<Location>().HasData(
                new Location { LocationId = 1, LocationName = "San Francisco" },
                new Location { LocationId = 2, LocationName = "New York" },
                new Location { LocationId = 3, LocationName = "Denver" },
                new Location { LocationId = 4, LocationName = "Los Angeles" }
            );

            modelBuilder.Entity<CarBrand>().HasData(
                new CarBrand { BrandId = 1, BrandName = "Tesla", LogoUrl = "https://images.unsplash.com/photo-1617788138017-80ad40651399?auto=format&fit=crop&q=80&w=200", IsActive = true },
                new CarBrand { BrandId = 2, BrandName = "Toyota", LogoUrl = "https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?auto=format&fit=crop&q=80&w=200", IsActive = true },
                new CarBrand { BrandId = 3, BrandName = "Jeep", LogoUrl = "https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?auto=format&fit=crop&q=80&w=200", IsActive = true },
                new CarBrand { BrandId = 4, BrandName = "Honda", LogoUrl = "https://images.unsplash.com/photo-1599912027806-cfec9f5944b6?auto=format&fit=crop&q=80&w=200", IsActive = true }
            );

            modelBuilder.Entity<CarCategory>().HasData(
                new CarCategory { CategoryId = 1, CategoryName = "Luxury", Description = "Premium driving experience with latest high-tech systems" },
                new CarCategory { CategoryId = 2, CategoryName = "Sedan", Description = "Comfortable commute cars with solid fuel mileage" },
                new CarCategory { CategoryId = 3, CategoryName = "SUV", Description = "Robust outdoor utility vehicles with extra cargo capacity" },
                new CarCategory { CategoryId = 4, CategoryName = "Compact", Description = "Eco-friendly, easy-to-park small cruisers" }
            );

            modelBuilder.Entity<FuelType>().HasData(
                new FuelType { FuelTypeId = 1, FuelTypeName = "Electric" },
                new FuelType { FuelTypeId = 2, FuelTypeName = "Gasoline" },
                new FuelType { FuelTypeId = 3, FuelTypeName = "Hybrid" }
            );

            modelBuilder.Entity<CarStatus>().HasData(
                new CarStatus { CarStatusId = 1, StatusName = "Available" },
                new CarStatus { CarStatusId = 2, StatusName = "Rented" },
                new CarStatus { CarStatusId = 3, StatusName = "In Maintenance" },
                new CarStatus { CarStatusId = 4, StatusName = "Clean-up Required" }
            );

            modelBuilder.Entity<Promotion>().HasData(
                new Promotion { PromotionId = 1, Code = "ROADDEAL10", DiscountPercent = 10, Description = "10% Off Your Next Car Reservation!", Active = true },
                new Promotion { PromotionId = 2, Code = "SUMMER25", DiscountPercent = 25, Description = "Seasonal 25% Discount for summer rentals", Active = false },
                new Promotion { PromotionId = 3, Code = "WEEKENDVIP", DiscountPercent = 15, Description = "Weekend getaway discount for premium vehicles", Active = true }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    FirstName = "System",
                    LastName = "Admin",
                    Email = "admin@roadready.com",
                    Phone = "+15550199",
                    PasswordHash = "$2a$11$7rM0gV3cf8VUy/Lc49Wtce4FISYjyln1iDrRka/tBoed9uK8DDz6u",//admin123
                    RoleId = 1, // Admin
                    IsActive = true
                },
                new User
                {
                    UserId = 2,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "user@roadready.com",
                    Phone = "+15550100",
                    PasswordHash = "$2a$11$s9JQbOQPQP/Ngrtjc1ChxO8x0V5slmPpZ0w.AgslPk9NKxrE9ycSa",//password123
                    RoleId = 2, // Customer
                    IsActive = true
                }
            );

            modelBuilder.Entity<Car>().HasData(
                new Car 
                { 
                    CarId = 1, 
                    BrandId = 1, 
                    CategoryId = 1, 
                    FuelTypeId = 1, 
                    CarStatusId = 1, 
                    LocationId = 1, 
                    Model = "Model 3", 
                    CarYear = 2023, 
                    Color = "Midnight Silver", 
                    NoSeats = 5, 
                    Transmission = "Automatic", 
                    Mileage = "Brand New", 
                    PricePerDay = 120.00m 
                },
                new Car 
                { 
                    CarId = 2, 
                    BrandId = 2, 
                    CategoryId = 2, 
                    FuelTypeId = 2, 
                    CarStatusId = 1, 
                    LocationId = 2, 
                    Model = "Camry", 
                    CarYear = 2022, 
                    Color = "Super White", 
                    NoSeats = 5, 
                    Transmission = "Automatic", 
                    Mileage = "15,000 mi", 
                    PricePerDay = 55.00m 
                },
                new Car 
                { 
                    CarId = 3, 
                    BrandId = 3, 
                    CategoryId = 3, 
                    FuelTypeId = 2, 
                    CarStatusId = 1, 
                    LocationId = 3, 
                    Model = "Wrangler", 
                    CarYear = 2021, 
                    Color = "Sarge Green", 
                    NoSeats = 4, 
                    Transmission = "Automatic", 
                    Mileage = "22,000 mi", 
                    PricePerDay = 85.00m 
                },
                new Car 
                { 
                    CarId = 4, 
                    BrandId = 4, 
                    CategoryId = 4, 
                    FuelTypeId = 3, 
                    CarStatusId = 1, 
                    LocationId = 4, 
                    Model = "Civic", 
                    CarYear = 2022, 
                    Color = "Sonic Gray", 
                    NoSeats = 5, 
                    Transmission = "Automatic", 
                    Mileage = "10,000 mi", 
                    PricePerDay = 45.00m 
                }
            );
            modelBuilder.Entity<Reservation>()
        .HasOne(r => r.PickupLocation)
        .WithMany()
        .HasForeignKey(r => r.PickupLocationId)
        .OnDelete(DeleteBehavior.NoAction);

    modelBuilder.Entity<Reservation>()
        .HasOne(r => r.DropoffLocation)
        .WithMany()
        .HasForeignKey(r => r.DropoffLocationId)
        .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Review>()
    .HasOne(r => r.User)
    .WithMany()
    .HasForeignKey(r => r.UserId)
    .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<CarImage>().HasData(
                new CarImage { ImageId = 1, CarId = 1, ImageUrl = "https://images.unsplash.com/photo-1560958089-b8a1929cea89?auto=format&fit=crop&q=80&w=800" },
                new CarImage { ImageId = 2, CarId = 2, ImageUrl = "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?auto=format&fit=crop&q=80&w=800" },
                new CarImage { ImageId = 3, CarId = 3, ImageUrl = "https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?auto=format&fit=crop&q=80&w=800" },
                new CarImage { ImageId = 4, CarId = 4, ImageUrl = "https://images.unsplash.com/photo-1599912027806-cfec9f5944b6?auto=format&fit=crop&q=80&w=800" }
            );
        }
    }
}

