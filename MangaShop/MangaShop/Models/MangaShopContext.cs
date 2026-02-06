using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MangaShop.Models;

public partial class MangaShopContext : DbContext
{
    public MangaShopContext()
    {
    }

    public MangaShopContext(DbContextOptions<MangaShopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

    public virtual DbSet<DanhGia> DanhGias { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<NhaXuatBan> NhaXuatBans { get; set; }

    public virtual DbSet<QuanTri> QuanTris { get; set; }

    public virtual DbSet<TacGia> TacGias { get; set; }

    public virtual DbSet<TheLoai> TheLoais { get; set; }

    public virtual DbSet<Truyen> Truyens { get; set; }

    public virtual DbSet<TruyenTap> TruyenTaps { get; set; }

    public virtual DbSet<BaiViet> BaiViets { get; set; }

    public virtual DbSet<LichPhatHanh> LichPhatHanhs { get; set; }

    public virtual DbSet<TruyenImages> TruyenImages { get; set; }
    public virtual DbSet<PhieuNhap> PhieuNhaps { get; set; } = null!;
    public virtual DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = null!;


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-38V1G0B\\SQLEXPRESS;Database=MangaShop;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietD__CDF0A1148FD4300C");

            entity.ToTable("ChiTietDonHang");

            entity.HasOne(d => d.MaDonHangNavigation)
                .WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaDon__628FA481");

            entity.HasOne(d => d.MaTruyenNavigation)
                .WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaTruyen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaTru__6383C8BA");

            // ✅ DÒNG QUYẾT ĐỊNH – FIX LỖI MaTapNavigationMaTap
            entity.HasOne(d => d.MaTapNavigation)
                .WithMany()
                .HasForeignKey(d => d.MaTap)
                .HasConstraintName("FK_ChiTietDonHang_TruyenTap");
        });

        modelBuilder.Entity<ChiTietGioHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietG__CDF0A11499C83BF6");

            entity.ToTable("ChiTietGioHang");

            entity.Property(e => e.SoLuong).HasDefaultValue(1);

            entity.HasOne(d => d.MaGioHangNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaGioHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietGi__MaGio__59FA5E80");

            entity.HasOne(d => d.MaTruyenNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaTruyen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietGi__MaTru__5AEE82B9");
        });

        modelBuilder.Entity<DanhGia>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DanhGia__AA9515BF0B42C5FC");

            // ✅ QUAN TRỌNG: map đúng tên bảng trong SQL
            entity.ToTable("DanhGia");

            entity.Property(e => e.NgayDanhGia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaKhachHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DanhGia_KhachHang");

            entity.HasOne(d => d.MaTruyenNavigation).WithMany(p => p.DanhGias)
                .HasForeignKey(d => d.MaTruyen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DanhGia_Truyen");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__129584AD699FF694");

            entity.ToTable("DonHang");

            entity.Property(e => e.NgayDat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Chờ xử lý");

            entity.Property(e => e.PhuongThucThanhToan)   // ✅ thêm
                .HasMaxLength(50);

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaKhachHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHang__MaKhach__5FB337D6");
        });


        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.MaGioHang).HasName("PK__GioHang__F5001DA317924164");

            entity.ToTable("GioHang");

            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.MaKhachHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GioHang__MaKhach__5629CD9C");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KhachHan__88D2F0E57785D11D");

            entity.ToTable("KhachHang");

            entity.HasIndex(e => e.Email, "UQ__KhachHan__A9D10534713E3B45").IsUnique();

            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(200);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
        });

        modelBuilder.Entity<NhaXuatBan>(entity =>
        {
            entity.HasKey(e => e.MaNxb).HasName("PK__NhaXuatB__3A19482CFB2C0DE9");

            entity.ToTable("NhaXuatBan");

            entity.Property(e => e.MaNxb).HasColumnName("MaNXB");
            entity.Property(e => e.DiaChi).HasMaxLength(300);
            entity.Property(e => e.TenNxb)
                .HasMaxLength(200)
                .HasColumnName("TenNXB");
        });

        modelBuilder.Entity<QuanTri>(entity =>
        {
            entity.HasKey(e => e.MaAdmin).HasName("PK__QuanTri__49341E3845DA7FB7");

            entity.ToTable("QuanTri");

            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(200);
            entity.Property(e => e.TaiKhoan).HasMaxLength(50);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<TacGia>(entity =>
        {
            entity.HasKey(e => e.MaTacGia).HasName("PK__TacGia__F24E675613D25091");

            entity.Property(e => e.TenTacGia).HasMaxLength(200);
        });

        modelBuilder.Entity<TheLoai>(entity =>
        {
            entity.HasKey(e => e.MaTheLoai).HasName("PK__TheLoai__D73FF34A0D8F26B9");

            entity.ToTable("TheLoai");

            entity.Property(e => e.TenTheLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<Truyen>(entity =>
        {
            entity.HasKey(e => e.MaTruyen).HasName("PK__Truyen__6AD20A4B5655CABD");

            entity.ToTable("Truyen");

            entity.Property(e => e.AnhBia).HasMaxLength(300);
            entity.Property(e => e.MaNxb).HasColumnName("MaNXB");
            entity.Property(e => e.TacGia).HasMaxLength(100);
            entity.Property(e => e.TenTruyen).HasMaxLength(200);

            entity.HasOne(d => d.MaNxbNavigation).WithMany(p => p.Truyens)
                .HasForeignKey(d => d.MaNxb)
                .HasConstraintName("FK_Truyen_NXB");

            entity.HasOne(d => d.MaTacGiaNavigation).WithMany(p => p.Truyens)
                .HasForeignKey(d => d.MaTacGia)
                .HasConstraintName("FK_Truyen_TacGia");

            entity.HasOne(d => d.MaTheLoaiNavigation).WithMany(p => p.Truyens)
                .HasForeignKey(d => d.MaTheLoai)
                .HasConstraintName("FK__Truyen__MaTheLoa__52593CB8");
        });
        modelBuilder.Entity<TruyenTap>(entity =>
        {
            entity.HasKey(e => e.MaTap);

            entity.ToTable("TruyenTap");

            entity.HasOne(d => d.MaTruyenNavigation)
                .WithMany(p => p.TruyenTaps)
                .HasForeignKey(d => d.MaTruyen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TruyenTap_Truyen");
        });

        OnModelCreatingPartial(modelBuilder);
        modelBuilder.Entity<BaiViet>(entity =>
        {
            entity.HasKey(e => e.MaBaiViet);

            entity.ToTable("BaiViet");

            entity.Property(e => e.TieuDe).HasMaxLength(200);
            entity.Property(e => e.TomTat).HasMaxLength(500);
            entity.Property(e => e.AnhDaiDien).HasMaxLength(300);
            entity.Property(e => e.Loai).HasMaxLength(50).HasDefaultValue("Thông báo");

            entity.Property(e => e.NgayDang)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.TrangThai)
                .HasDefaultValue(true);
        });
        modelBuilder.Entity<DanhGia>(entity =>
        {
            entity.HasOne(d => d.MaTapNavigation)
                  .WithMany()
                  .HasForeignKey(d => d.MaTap)
                  .HasConstraintName("FK_DanhGia_TruyenTap");
        });
        modelBuilder.Entity<LichPhatHanh>(entity =>
        {
            entity.HasKey(e => e.MaLich);

            entity.ToTable("LichPhatHanh");

            entity.Property(e => e.NgayPhatHanh)
                .HasColumnType("date");

            entity.Property(e => e.GiaDuKien)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.GhiChu)
                .HasMaxLength(255);

            entity.Property(e => e.TrangThai)
                .HasDefaultValue(true);

            entity.Property(e => e.NgayTao)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.NgayCapNhat)
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaTruyenNavigation)
                .WithMany(p => p.LichPhatHanhs) // nếu bạn đã thêm ICollection ở Truyen
                .HasForeignKey(d => d.MaTruyen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichPhatHanh_Truyen");

            entity.HasOne(d => d.MaTapNavigation)
                .WithMany(p => p.LichPhatHanhs) // nếu bạn đã thêm ICollection ở TruyenTap
                .HasForeignKey(d => d.MaTap)
                .HasConstraintName("FK_LichPhatHanh_TruyenTap");
        });

        modelBuilder.Entity<PhieuNhap>(entity =>
        {
            entity.HasKey(e => e.MaPhieuNhap);

            entity.ToTable("PhieuNhap");

            entity.Property(e => e.NgayNhap)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.GhiChu)
                .HasMaxLength(255);

            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Đã nhập");

            entity.Property(e => e.TongTienNhap)
                .HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaAdminNavigation)
                .WithMany(p => p.PhieuNhaps)   // cần thêm ICollection<PhieuNhap> trong QuanTri
                .HasForeignKey(d => d.MaAdmin)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PhieuNhap_QuanTri");
        });
        modelBuilder.Entity<ChiTietPhieuNhap>(entity =>
        {
            entity.HasKey(e => e.MaChiTietPhieuNhap);

            entity.ToTable("ChiTietPhieuNhap");

            entity.Property(e => e.DonGiaNhap)
                .HasColumnType("decimal(18, 2)");

            // Nếu DB bạn có computed column ThanhTien:
            // entity.Property(e => e.ThanhTien)
            //     .HasColumnType("decimal(18, 2)")
            //     .ValueGeneratedOnAddOrUpdate()
            //     .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

            // Unique (MaPhieuNhap, MaTap)
            entity.HasIndex(e => new { e.MaPhieuNhap, e.MaTap })
                .IsUnique()
                .HasDatabaseName("UQ_CTPN_PhieuNhap_Tap");

            entity.HasOne(d => d.MaPhieuNhapNavigation)
                .WithMany(p => p.ChiTietPhieuNhaps)
                .HasForeignKey(d => d.MaPhieuNhap)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_CTPN_PhieuNhap");

            entity.HasOne(d => d.MaTruyenNavigation)
                .WithMany(p => p.ChiTietPhieuNhaps) // cần thêm ICollection<ChiTietPhieuNhap> trong Truyen
                .HasForeignKey(d => d.MaTruyen)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CTPN_Truyen");

            entity.HasOne(d => d.MaTapNavigation)
                .WithMany(p => p.ChiTietPhieuNhaps) // cần thêm ICollection<ChiTietPhieuNhap> trong TruyenTap
                .HasForeignKey(d => d.MaTap)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CTPN_TruyenTap");
        });

        modelBuilder.Entity<ChiTietGioHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietG__CDF0A11499C83BF6");
            entity.ToTable("ChiTietGioHang");

            entity.Property(e => e.SoLuong).HasDefaultValue(1);

            // ✅ ĐOẠN BẠN CẦN THÊM TRONG CONTEXT
            entity.HasOne(d => d.MaTapNavigation)
                .WithMany()
                .HasForeignKey(d => d.MaTap)
                .HasConstraintName("FK_ChiTietGioHang_TruyenTap");

            entity.HasOne(d => d.MaGioHangNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaGioHang)
                .HasConstraintName("FK__ChiTietGi__MaGio__59FA5E80");

            entity.HasOne(d => d.MaTruyenNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaTruyen)
                .HasConstraintName("FK__ChiTietGi__MaTru__5AEE82B9");
        });
        modelBuilder.Entity<TruyenImages>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("TruyenImages");

            entity.Property(e => e.Path).HasMaxLength(250);

            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);

            // Cấu hình mối quan hệ 1-N với bảng Truyen
            entity.HasOne(d => d.Truyen)
                .WithMany(p => p.TruyenImages) // Đảm bảo trong Model Truyen.cs đã có ICollection<TruyenImages>
                .HasForeignKey(d => d.MaTruyen)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TruyenImages_Truyen");
        });

    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
