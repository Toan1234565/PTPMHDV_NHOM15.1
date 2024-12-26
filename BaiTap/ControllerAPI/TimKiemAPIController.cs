using BaiTap.Models;
using NLog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace BaiTap.Controllers
{
    [RoutePrefix("api/timkiem")]
    public class TimKiemApiController : ApiController
    {
        private Model1 db = new Model1();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static ObjectCache cache = MemoryCache.Default;

        [HttpGet]
        [Route("index")]
        public IHttpActionResult Index()
        {
            return Ok();
        }

        [HttpGet]
        [Route("timkiemsanpham")]
        public IHttpActionResult TimKiem(string name)
        {
            string cacheKey = $"TimKiem_{name}_cache";
            List<SanPham> kq;
            if (cache.Contains(cacheKey))
            {
                kq = cache.Get(cacheKey) as List<SanPham>;
                logger.Info("Lấy danh sách sản phẩm từ cache thành công.");
            }
            else
            {
                try
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    kq = db.SanPham.Where(sp => sp.TenSanPham.Contains(name) || sp.MoTa.Contains(name)).ToList();
                    logger.Info("Lấy danh sách sản phẩm thành công.");
                    CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10) };
                    cache.Add(cacheKey, kq, policy);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Lỗi khi lấy danh sách sản phẩm.");
                    return InternalServerError(ex);
                }
            }
            return Ok(kq);
        }

        [HttpGet]
        [Route("locsanpham")]
        public IHttpActionResult LocSP(string name = null, int? IDHang = null, int? IDDanhMuc = null, double? to = null, double? from = null, string sx = null, int page = 1, int pageSize = 10)
        {
            try
            {
                string cacheKey = $"LocSanPham_{name}_{IDHang}_{IDDanhMuc}_{to}_{from}_{sx}_{page}_{pageSize}";
                object response;

                if (cache.Contains(cacheKey))
                {
                    response = cache.Get(cacheKey);
                    logger.Info("Lấy danh sách sản phẩm từ cache thành công.");
                }
                else
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    IQueryable<SanPham> kq = db.SanPham;

                    if (IDDanhMuc.HasValue && IDDanhMuc.Value != 0)
                    {
                        kq = kq.Where(sp => sp.DanhMucID == IDDanhMuc.Value);
                    }

                    if (IDHang.HasValue && IDHang.Value != 0)
                    {
                        kq = kq.Where(sp => sp.HangID == IDHang.Value);
                    }

                    if (from.HasValue && to.HasValue && from.Value > 0 && to.Value > 0)
                    {
                        kq = kq.Where(sp => sp.Gia >= from.Value && sp.Gia <= to.Value);
                    }

                    if (!string.IsNullOrEmpty(name))
                    {
                        kq = kq.Where(sp => sp.TenSanPham.Contains(name));
                    }

                    switch (sx)
                    {
                        case "Giatang":
                            kq = kq.OrderBy(sp => sp.Gia);
                            break;
                        case "Giagiam":
                            kq = kq.OrderByDescending(sp => sp.Gia);
                            break;
                        default:
                            kq = kq.OrderBy(sp => sp.Gia);
                            break;
                    }

                    // Tính năng phân trang
                    var totalItems = kq.Count();
                    var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                    var pagedResult = kq.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                    response = new
                    {
                        TotalItems = totalItems,
                        TotalPages = totalPages,
                        Items = pagedResult
                    };

                    CacheItemPolicy policy = new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10)
                    };
                    cache.Add(cacheKey, response, policy);
                    logger.Info("Lấy danh sách sản phẩm từ cơ sở dữ liệu và lưu vào cache.");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Lấy danh sách thất bại.");
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("giatang")]
        public IHttpActionResult GiaTang()
        {
            try
            {
                string cacheKey = "giatang_cache";
                List<SanPham> kq;
                if (cache.Contains(cacheKey))
                {
                    kq = (List<SanPham>)cache.Get(cacheKey);
                    logger.Info("Lấy danh sách sản phẩm từ cache thành công.");
                }
                else
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    kq = db.SanPham.OrderBy(x => x.Gia).ToList();
                    CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10) };
                    cache.Add(cacheKey, kq, policy);
                    logger.Info("Lấy danh sách sản phẩm từ cơ sở dữ liệu và lưu vào cache.");
                }
                return Ok(kq);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Lỗi khi lấy danh sách sản phẩm.");
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("giagiam")]
        public IHttpActionResult GiaGiam()
        {
            try
            {
                string cacheKey = "giagiam_cache";
                List<SanPham> kq;
                if (cache.Contains(cacheKey))
                {
                    kq = (List<SanPham>)cache.Get(cacheKey);
                    logger.Info("Lấy danh sách sản phẩm từ cache thành công.");
                }
                else
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    kq = db.SanPham.OrderByDescending(x => x.Gia).ToList();
                    CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10) };
                    cache.Add(cacheKey, kq, policy);
                    logger.Info("Lấy danh sách sản phẩm từ cơ sở dữ liệu và lưu vào cache.");
                }
                return Ok(kq);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Lỗi khi lấy danh sách sản phẩm.");
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("banchay1")]
        public IHttpActionResult Banchay1()
        {
            try
            {
                string cacheKey = "banchay1_cache";
                List<SanPham> kq;

                if (cache.Contains(cacheKey))
                {
                    kq = (List<SanPham>)cache.Get(cacheKey);
                    logger.Info("Lấy danh sách sản phẩm bán chạy từ cache thành công.");
                }
                else
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    var sanpham = db.SanPham.ToList();
                    kq = sanpham.Select(x => new
                    {
                        SanPham = x,
                        SoLuongDaban = x.Soluong.GetValueOrDefault() - x.TonKho.Sum(tk => tk.SoLuongTon)
                    }).OrderByDescending(x => x.SoLuongDaban).Take(10).Select(x => x.SanPham).ToList();

                    CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10) };
                    cache.Add(cacheKey, kq, policy);
                    logger.Info("Lấy danh sách sản phẩm bán chạy từ cơ sở dữ liệu và lưu vào cache.");
                }
                return Ok(kq);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Lỗi khi lấy danh sách sản phẩm bán chạy.");
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("SOSANH")]
        public async Task<IHttpActionResult> Sosanh([FromUri] int[] id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                if (id == null || id.Length < 2)
                {
                    return BadRequest("Không hợp lệ.");
                }

                var cacheKey = $"sosanh_{string.Join("_", id)}_cache";
                if (cache.Contains(cacheKey))
                {
                    var cachedComparison = cache.Get(cacheKey) as List<Dictionary<string, string>>;
                    logger.Info("Lấy thông tin so sánh sản phẩm từ cache thành công.");
                    return Ok(cachedComparison);
                }

              
                var sanpham = await db.ChiTietSanPham.Where(p => id.Contains(p.ChiTietSanPhamID)).ToListAsync();
                if (sanpham.Count != id.Length)
                {
                    return NotFound();
                }

                var sosanhsp = new List<Dictionary<string, string>>();
                foreach (var sp in sanpham)
                {
                    var SoSanh = new Dictionary<string, string>
            {
                {"Mã Sản Phẩm", sp.SanPhamID.ToString()},
                {"Tên Sản Phẩm", sp.SanPham.TenSanPham},
                {"Màn Hình", sp.ManHinh},
                {"Hệ Điều Hành", sp.HeDieuHanh},
                {"Camera Trước", sp.CameraTruoc},
                {"Camera Sau", sp.CameraSau},
                {"Chip", sp.Chip},
                {"RAM", sp.RAM},
                {"Bộ Nhớ Trong", sp.BoNhoTrong},
                {"Sim", sp.Sim},
                {"Pin", sp.Pin}
            };
                    sosanhsp.Add(SoSanh);
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10) };
                cache.Add(cacheKey, sosanhsp, policy);
                logger.Info("Lấy thông tin so sánh sản phẩm từ cơ sở dữ liệu và lưu vào cache.");

                return Ok(sosanhsp);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Lỗi khi lấy thông tin so sánh sản phẩm.");
                return InternalServerError(ex);
            }
        }
    }
}