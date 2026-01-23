namespace MangaShop.Helpers
{
    public static class MemberTierHelper
    {
        public static string GetTier(double amount)
        {
            if (amount >= 5000000) return "Kim cương";
            if (amount >= 2000000) return "Vàng";
            if (amount >= 500000) return "Bạc";
            return ""; // Hoặc "Chưa có" tùy bạn đặt
        }
    }
}