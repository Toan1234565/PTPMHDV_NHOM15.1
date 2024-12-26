using BaiTap.IRepository;
using BaiTap.Models;
using System.Data.Entity;
using System.Threading.Tasks;

namespace BaiTap.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<SanPham> SanPhamRepository { get; }
        IRepository<ChiTietSanPham> ChiTietSanPhamRepository { get; }
        IRepository<TonKho> TonKhoRepository { get; }

        Task SaveAsync();
        DbContextTransaction BeginTransaction();
    }
}
