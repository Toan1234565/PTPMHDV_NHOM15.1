namespace BaiTap.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Linq;

    [Table("SanPham")]
    public partial class SanPham
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SanPham()
        {
            ChiTietDonHang = new HashSet<ChiTietDonHang>();
            ChiTietPhieuNhap = new HashSet<ChiTietPhieuNhap>();
            ChiTietPhieuXuat = new HashSet<ChiTietPhieuXuat>();
            ChiTietSanPham = new HashSet<ChiTietSanPham>();
            SanPhamKhuyenMai = new HashSet<SanPhamKhuyenMai>();
            TonKho = new HashSet<TonKho>();
        }

        public int SanPhamID { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Bạn chưa nhập tên cho sản phẩm!")]
        public string TenSanPham { get; set; }

        public int? Soluong { get; set; }

        [StringLength(500)]
        public string MoTa { get; set; }

        public double? Gia { get; set; }

        public int? DanhMucID { get; set; }

        public int? HangID { get; set; }

        [StringLength(400)]
        public string HinhAnh { get; set; }

        // Tính toán số lượng đã bán bằng số lượng ban đầu trừ đi số lượng tồn kho
        public int? SoLuongDaBan
        {
            get
            {
                return Soluong.HasValue ? (int?)(Soluong.Value - TonKho.Sum(tk => tk.SoLuongTon)) : (int?)null;
            }
        }

        public double? TongDoanhThu
        {
            get
            {
                return SoLuongDaBan.HasValue ? SoLuongDaBan.Value * Gia.GetValueOrDefault() : (double?)null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietDonHang> ChiTietDonHang { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhap { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietPhieuXuat> ChiTietPhieuXuat { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietSanPham> ChiTietSanPham { get; set; }

        public virtual DanhMuc DanhMuc { get; set; }

        public virtual Hang Hang { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SanPhamKhuyenMai> SanPhamKhuyenMai { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TonKho> TonKho { get; set; }
    }
}
