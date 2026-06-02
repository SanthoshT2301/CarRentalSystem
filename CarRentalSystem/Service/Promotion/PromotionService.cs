using AutoMapper;
using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Promotion;
using CarRentalSystem.Models;
using Microsoft.EntityFrameworkCore;


namespace CarRentalSystem.Service.Promotions;
 public class PromotionService : IPromotionService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public PromotionService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

    public async  Task<PromotionDto> AddPromotionAsync(CreatePromotionRequest req)
    {
         if (req == null) throw new ArgumentNullException(nameof(req));

            var p = _mapper.Map<Promotion>(req);

            _context.Promotions.Add(p);
            await _context.SaveChangesAsync();
            return _mapper.Map<PromotionDto>(p);
    }

    public async Task<bool> DeletePromotionAsync(int id)
    {
        var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return false;

            _context.Promotions.Remove(promo);
            await _context.SaveChangesAsync();
            return true;
    }

    public async Task<List<PromotionDto>> GetAllPromotionsAsync()
    {
          var list = await _context.Promotions.ToListAsync();
        if (list.Count() == 0)
        {
            return null;
        }
            return _mapper.Map<List<PromotionDto>>(list);
    }

    public async Task<PromotionDto?> GetPromotionByIdAsync(int id)
    {   var p = await _context.Promotions.FindAsync(id);
            if (p == null) return null;
            return _mapper.Map<PromotionDto>(p);
       
    }

    public async Task<bool> TogglePromoStatusAsync(int id)
    {
        var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) throw new KeyNotFoundException("Promotion not found");

            promo.Active = !promo.Active;
            await _context.SaveChangesAsync();
            return promo.Active;
    }

    public async Task<PromotionDto?> ValidatePromoCodeAsync(string code)
    {
       if (string.IsNullOrWhiteSpace(code)) return null;
            var promo = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Code.ToUpper() == code.ToUpper() && p.Active);
            
            if (promo == null) return null;
            return _mapper.Map<PromotionDto>(promo);
    }
}