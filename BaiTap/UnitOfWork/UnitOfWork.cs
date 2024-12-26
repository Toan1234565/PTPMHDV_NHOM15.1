using BaiTap.IRepository;
using BaiTap.Models;
using BaiTap.Repositories;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace BaiTap.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Model1 context;
        private IRepository<SanPham> sanPhamRepository;
        private IRepository<ChiTietSanPham> chiTietSanPhamRepository;
        private IRepository<TonKho> tonKhoRepository; // Thêm biến này

        public UnitOfWork(Model1 context)
        {
            this.context = context;
        }

        public IRepository<SanPham> SanPhamRepository
        {
            get
            {
                return sanPhamRepository ?? (sanPhamRepository = new Repository<SanPham>(context));
            }
        }

        public IRepository<ChiTietSanPham> ChiTietSanPhamRepository
        {
            get
            {
                return chiTietSanPhamRepository ?? (chiTietSanPhamRepository = new Repository<ChiTietSanPham>(context));
            }
        }

        public IRepository<TonKho> TonKhoRepository // Triển khai thuộc tính này
        {
            get
            {
                return tonKhoRepository ?? (tonKhoRepository = new Repository<TonKho>(context));
            }
        }

        public async Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }

        public DbContextTransaction BeginTransaction()
        {
            return context.Database.BeginTransaction();
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
