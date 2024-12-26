using BaiTap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using NLog;
using System.Threading.Tasks;
using System.Runtime.Caching;
using BaiTap.Repositories;
using BaiTap.UnitOfWork;
using BaiTap.IRepository;
using BaiTap.Services;
using System.Data.Entity;


namespace BaiTap.Controllers
{
    [RoutePrefix("api/quanlysanpham")]
    public class QuanLySanPhamAPIController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static ObjectCache cache = MemoryCache.Default;
        private readonly ProductService _productService;
        private static readonly HttpClient client = new HttpClient();
        private readonly ProductService productService = new ProductService();
        private readonly IUnitOfWork unitOfWork;
        private readonly IRepository<SanPham> sanPhamRepository;
        private readonly IRepository<ChiTietSanPham> chiTietSanPhamRepository;
        private static Model1 db = new Model1();

        public QuanLySanPhamAPIController()
        {
            unitOfWork = new UnitOfWork.UnitOfWork(db);
            sanPhamRepository = unitOfWork.SanPhamRepository;
            chiTietSanPhamRepository = unitOfWork.ChiTietSanPhamRepository;
            _productService = new ProductService();
        }
        public QuanLySanPhamAPIController(IUnitOfWork unitOfWork, ProductService productService)
        {
            this.unitOfWork = unitOfWork;
            sanPhamRepository = unitOfWork.SanPhamRepository;
            chiTietSanPhamRepository = unitOfWork.ChiTietSanPhamRepository;
            _productService = productService;
        }

        [HttpGet]
        [Route("sanpham")]
        public async Task<IHttpActionResult> GetSanPham()
        {
            try
            {
                string cacheKey = "sanpham_cache";
                List<SanPham> sanpham;

                if (cache.Contains(cacheKey))
                {
                    sanpham = (List<SanPham>)cache.Get(cacheKey);
                    logger.Info("Lấy danh sách sản phẩm từ cache.");
                }
                else
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    sanpham = (await sanPhamRepository.GetAllAsync()).ToList();
                    
                    CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10) };
                    cache.Add(cacheKey, sanpham, policy);
                    logger.Info("Lấy danh sách sản phẩm từ cơ sở dữ liệu và lưu vào cache.");
                }

                return Ok(sanpham);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Lỗi khi lấy danh sách sản phẩm.");
                return InternalServerError(ex);
            }
        }


        [HttpGet]
        [Route("suasanpham/{id}")]
        public async Task<IHttpActionResult> GetSanPham(int id)
        {
            try
            {
               
                string cacheKey = $"sanpham_{id}_cache";
                SanPham sanpham;

                if (cache.Contains(cacheKey))
                {
                    sanpham = (SanPham)cache.Get(cacheKey);
                    logger.Info("Lấy thông tin sản phẩm từ cache. ID: {0}", id);
                }
                else
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    sanpham = await sanPhamRepository.GetByIdAsync(id);
                    if (sanpham == null)
                    {
                        logger.Warn("Không tìm thấy sản phẩm với ID: {0}", id);
                        return NotFound();
                    }
                    CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10) };
                    cache.Add(cacheKey, sanpham, policy);
                    logger.Info("Lấy thông tin sản phẩm từ cơ sở dữ liệu và lưu vào cache. ID: {0}", id);
                }

                return Ok(sanpham);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Lỗi khi lấy thông tin sản phẩm với ID: {0}", id);
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("suasanpham/{id}")]
        public async Task<IHttpActionResult> Sua(int id, SanPham sanpham)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var update = await sanPhamRepository.GetByIdAsync(id);
                if (update == null)
                {
                    return NotFound();
                }

                update.TenSanPham = sanpham.TenSanPham;
                update.Soluong = sanpham.Soluong;
                update.Gia = sanpham.Gia;
                update.MoTa = sanpham.MoTa;
                update.HangID = sanpham.HangID;
                update.DanhMucID = sanpham.DanhMucID;
                await sanPhamRepository.UpdateAsync(update);
                await unitOfWork.SaveAsync();

                string cacheKey = $"sanpham_{id}_cache";
                cache.Set(cacheKey, update, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10) });

                logger.Info("Cập nhật sản phẩm thành công. ID: {0}", sanpham.SanPhamID);
                return Ok("Cập nhật thành công");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Lỗi khi cập nhật thông tin sản phẩm.");
                return InternalServerError(ex);
            }
        }



        // GET: api/quanlysanpham/dschitiet
        // GET: api/quanlysanpham/dschitiet
        [HttpGet]
        [Route("dschitiet")]
        public async Task<IHttpActionResult> GetDSChiTiet()
        {
            try
            {
                
                string cacheKey = "chitiet_cache";
                List<ChiTietSanPham> chitiet;

                if (cache.Contains(cacheKey))
                {
                    chitiet = (List<ChiTietSanPham>)cache.Get(cacheKey);
                    logger.Info("Lấy danh sách chi tiết sản phẩm từ cache.");
                }
                else
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    var allDetails = await chiTietSanPhamRepository.GetAllAsync();
                    chitiet = allDetails.ToList();
                    CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10) };
                    cache.Set(cacheKey, chitiet, policy);
                    logger.Info("Lấy danh sách chi tiết sản phẩm từ cơ sở dữ liệu và lưu vào cache.");
                }

                return Ok(chitiet);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Lỗi khi lấy danh sách chi tiết sản phẩm.");
                return InternalServerError(ex);
            }
        }

        // GET: api/quanlysanpham/xemchitiet/{id}
        [HttpGet]
        [Route("xemchitiet/{id}")]
        public async Task<IHttpActionResult> XemChiTiet(int id)
        {
            try
            {
                string cacheKey = $"chitiet_{id}_cache";
                List<ChiTietSanPham> chitiet;

                if (cache.Contains(cacheKey))
                {
                    chitiet = (List<ChiTietSanPham>)cache.Get(cacheKey);
                    logger.Info("Lấy chi tiết sản phẩm từ cache. ID: {0}", id);
                }
                else
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    chitiet = await db.ChiTietSanPham.Where(c => c.SanPhamID == id).ToListAsync();

                    if (!chitiet.Any())
                    {
                        // Nếu không tìm thấy chi tiết sản phẩm trong cơ sở dữ liệu
                        logger.Warn("Không tìm thấy chi tiết sản phẩm với ID: {0}", id);

                        // Tìm thông tin sản phẩm từ Google Custom Search API
                        var sanpham = await db.SanPham.FindAsync(id);
                        if (sanpham == null)
                        {
                            logger.Warn("Không tìm thấy sản phẩm với ID: {0}", id);
                            return NotFound();
                        }

                        var productDetails = await productService.GetProductDetailsFromWebAsync(sanpham.TenSanPham);
                        if (productDetails != null)
                        {
                            productDetails.SanPhamID = id;
                            db.ChiTietSanPham.Add(productDetails);
                            await db.SaveChangesAsync();
                            chitiet = new List<ChiTietSanPham> { productDetails };
                        }
                        else
                        {
                            logger.Warn("Không thể lấy thông tin chi tiết sản phẩm từ API. ID: {0}", id);
                            return NotFound();
                        }
                    }

                    CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10) };
                    cache.Set(cacheKey, chitiet, policy);
                    logger.Info("Lấy chi tiết sản phẩm từ cơ sở dữ liệu và lưu vào cache. ID: {0}", id);
                }

                return Ok(chitiet);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Lỗi khi lấy chi tiết sản phẩm với ID: {0}", id);
                return InternalServerError(ex);
            }
        }

        // POST: api/quanlysanpham/them
        [HttpPost]
        [Route("them")]
        public async Task<IHttpActionResult> ThemSanPham(PhieuNhapKhoViewModel model)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                if (!ModelState.IsValid)
                {
                    logger.Warn("ModelState không hợp lệ.");
                    return BadRequest(ModelState);
                }

                var sanpham = model.SanPham;

                if (sanpham.Gia == null)
                {
                    return BadRequest("Giá sản phẩm không được để trống");
                }
                if (sanpham.HangID == null)
                {
                    return BadRequest("Vui lòng chọn hãng sản xuất.");
                }
                if (sanpham.DanhMucID == null)
                {
                    return BadRequest("Vui lòng chọn danh mục.");
                }

                // Khởi tạo ChiTietSanPham nếu nó null
                if (model.ChiTietSanPham == null)
                {
                    model.ChiTietSanPham = new ChiTietSanPham();
                }

                // Lấy thông tin chi tiết sản phẩm từ trang web
                ChiTietSanPham productDetails = await _productService.GetProductDetailsFromWebAsync(sanpham.TenSanPham);

                if (productDetails != null)
                {
                    model.ChiTietSanPham.ManHinh = productDetails.ManHinh ?? model.ChiTietSanPham.ManHinh;
                    model.ChiTietSanPham.HeDieuHanh = productDetails.HeDieuHanh ?? model.ChiTietSanPham.HeDieuHanh;
                    model.ChiTietSanPham.CameraTruoc = productDetails.CameraTruoc ?? model.ChiTietSanPham.CameraTruoc;
                    model.ChiTietSanPham.CameraSau = productDetails.CameraSau ?? model.ChiTietSanPham.CameraSau;
                    model.ChiTietSanPham.Chip = productDetails.Chip ?? model.ChiTietSanPham.Chip;
                    model.ChiTietSanPham.RAM = productDetails.RAM ?? model.ChiTietSanPham.RAM;
                    model.ChiTietSanPham.BoNhoTrong = productDetails.BoNhoTrong ?? model.ChiTietSanPham.BoNhoTrong;
                    model.ChiTietSanPham.Sim = productDetails.Sim ?? model.ChiTietSanPham.Sim;
                    model.ChiTietSanPham.Pin = productDetails.Pin ?? model.ChiTietSanPham.Pin;
                }

                string url = await _productService.GetProductImageAsync(sanpham.TenSanPham);
                if (!string.IsNullOrEmpty(url))
                {
                    sanpham.HinhAnh = url;
                }
                else
                {
                    logger.Warn("Không thể lấy URL hình ảnh.");
                }

                await sanPhamRepository.InsertAsync(sanpham);

                model.ChiTietSanPham.SanPhamID = sanpham.SanPhamID;
                await chiTietSanPhamRepository.InsertAsync(model.ChiTietSanPham);

                cache.Remove("sanpham_cache");

                logger.Info("Thêm sản phẩm thành công. ID: {0}", sanpham.SanPhamID);
                return Ok(model);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Lỗi khi thêm sản phẩm.");
                return InternalServerError(ex);
            }
        }

        // GET: QuanLySanPham/Xoa/{id}
        public async Task<IHttpActionResult> Xoa(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var sp = await sanPhamRepository.GetByIdAsync(id);
            if (sp == null)
            {
                return NotFound();
            }
            return Ok(sp);
        }

        // DELETE: api/quanlysanpham/xoa/{id}
        [HttpDelete]
        [Route("xoa/{id}")]
        public async Task<IHttpActionResult> XoaSanPham(int id)
        {
            using (var transaction = unitOfWork.BeginTransaction())
            {
                try
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    var sanpham = await sanPhamRepository.GetByIdAsync(id);
                    if (sanpham == null)
                    {
                        logger.Warn("Không tìm thấy sản phẩm với ID: {0}", id);
                        return NotFound();
                    }

                    var chiTietSanPham = (await chiTietSanPhamRepository.GetAllAsync()).Where(x => x.SanPhamID == id).ToList();
                    foreach (var chitiet in chiTietSanPham)
                    {
                        await chiTietSanPhamRepository.DeleteAsync(chitiet.ChiTietSanPhamID);
                    }
                    await sanPhamRepository.DeleteAsync(id);
                    await unitOfWork.SaveAsync();

                    transaction.Commit();
                    cache.Remove("sanpham_cache");

                    logger.Info("Xóa sản phẩm thành công. ID: {0}", id);
                    return Ok();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    logger.Error(ex, "Lỗi khi xóa sản phẩm với ID: {0}", id);
                    return InternalServerError(ex);
                }
            }
        }
    }
}
