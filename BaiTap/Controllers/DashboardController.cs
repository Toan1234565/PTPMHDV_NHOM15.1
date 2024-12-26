using System.Linq;
using System.Web.Mvc;
using BaiTap.Models;
using System.Collections.Generic;
using NLog;
using System;

namespace BaiTap.Controllers
{
    public class DashboardController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Model1 db = new Model1();

        public ActionResult Index()
        {
            try
            {
                var tonKhoList = db.TonKho.Include("SanPham").ToList();
                var sanPhamList = db.SanPham.Include("Hang").Include("DanhMuc").ToList();

                var tonKhoData = tonKhoList.Select(t => new DataPoint
                {
                    Label = t.SanPham?.TenSanPham ?? "Unknown",
                    Value = t.SoLuongTon.GetValueOrDefault() // Đổi tên từ Y thành Value
                }).ToList();

                var sanPhamData = sanPhamList.Select(sp => new SanPhamData
                {
                    TenSanPham = sp.TenSanPham,
                    Gia = sp.Gia,
                    SoLuong = sp.Soluong,
                    Hang = sp.Hang?.TenHang ?? "Unknown", // Kiểm tra null cho Hang
                    DanhMuc = sp.DanhMuc?.TenDanhMuc ?? "Unknown" // Kiểm tra null cho DanhMuc
                }).ToList();

                var dashboardViewModel = new DashboardViewModel
                {
                    TonKhoData = tonKhoData,
                    SanPhamData = sanPhamData
                };

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Lỗi khi lấy dữ liệu cho dashboard.");
                return View(new DashboardViewModel());
            }
        }
    }

    public class DataPoint
    {
        public string Label { get; set; }
        public int Value { get; set; } // Đổi tên từ Y thành Value
    }

    public class SanPhamData
    {
        public string TenSanPham { get; set; }
        public double? Gia { get; set; }
        public int? SoLuong { get; set; }
        public string Hang { get; set; }
        public string DanhMuc { get; set; }
    }

    public class DashboardViewModel
    {
        public List<DataPoint> TonKhoData { get; set; }
        public List<SanPhamData> SanPhamData { get; set; }
    }
}
