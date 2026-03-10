#if !DISABLESTEAMWORKS  && STEAM_INSTALLED

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Provides functionality related to currencies, including their codes and symbols.
    /// </summary>
    public static class Currency
    {
        /// <summary>
        /// Represents a set of currency codes corresponding to various currencies
        /// used in different countries. These codes are used to identify currencies
        /// in operations involving international monetary transactions.
        /// </summary>
        public enum Code
        {
            Unknown,
            Aed,
            Ars,
            Aud,
            Brl,
            Cad,
            Chf,
            Clp,
            Cny,
            Cop,
            CRC,
            Eur,
            Gbp,
            Hkd,
            Ils,
            Idr,
            Inr,
            Jpy,
            Krw,
            Kwd,
            Kzt,
            Mxn,
            Myr,
            Nok,
            Nzd,
            Pen,
            Php,
            Pln,
            Qar,
            Rub,
            Sar,
            Sgd,
            Thb,
            Try,
            Twd,
            Uah,
            Usd,
            Uyu,
            Vnd,
            Zar,
        }

        /// <summary>
        /// Retrieves the currency symbol for a given currency code.
        /// </summary>
        /// <param name="code">The <see cref="Currency.Code"/> representing the desired currency.</param>
        /// <returns>A string containing the symbol of the specified currency. If the currency code is unknown, an empty string or a default value is returned.</returns>
        public static string GetSymbol(Code code)
        {
            return code switch
            {
                Code.Unknown => string.Empty,
                Code.Aed => "د.إ",
                Code.Brl => "R$",
                Code.Chf => "CHF",
                Code.Cny => "¥",
                Code.CRC => "₡",
                Code.Eur => "€",
                Code.Gbp => "£",
                Code.Ils => "₪",
                Code.Idr => "Rp",
                Code.Inr => "₹",
                Code.Jpy => "¥",
                Code.Krw => "₩",
                Code.Kwd => "د.ك",
                Code.Kzt => "лв",
                Code.Myr => "RM",
                Code.Nok => "kr",
                Code.Pen => "S/.",
                Code.Php => "₱",
                Code.Pln => "zł",
                Code.Qar => "﷼",
                Code.Rub => "₽",
                Code.Sar => "﷼",
                Code.Thb => "฿",
                Code.Try => "₺",
                Code.Twd => "NT$",
                Code.Uah => "₴",
                Code.Uyu => "$U",
                Code.Vnd => "₫",
                Code.Zar => "R",
                _ => "$"
            };
        }
    }
}
#endif