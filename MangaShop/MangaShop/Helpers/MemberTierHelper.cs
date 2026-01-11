namespace MangaShop.Helpers
{
    public static class MemberTierHelper
    {
        public static string GetTier(double tongChiTieu)
        {
            if (tongChiTieu >= 10_000_000) return "Vàng";
            if (tongChiTieu >= 5_000_000) return "Bạc";
            if (tongChiTieu >= 1_000_000) return "Đồng";
            return "Thường";
        }
    }
}
