#nullable enable
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace FASTSURVEY.Services.Mobile
{
    public sealed class MobileService : IMobileService
    {
        // Padrões compilados para performance
        private static readonly Regex RxIphone = new(
            "iPhone",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
        private static readonly Regex RxAndroid = new(
            "Android",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
        private static readonly Regex RxAndroidNoMobile = new(
            @"Android(?!.*Mobile)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        ); // tablets Android
        private static readonly Regex RxIpad = new(
            "iPad",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
        private static readonly Regex RxWindowsPhone = new(
            "Windows Phone",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
        private static readonly Regex RxMobile = new(
            "Mobile",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
        private static readonly Regex RxTablet = new(
            "Tablet",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
        private static readonly Regex RxTouch = new(
            "Touch",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        // iPadOS 13+ às vezes vem como "Macintosh; Intel Mac OS X" com "Mobile" e "Safari"
        private static bool LooksLikeModernIPad(string ua)
        {
            if (string.IsNullOrEmpty(ua))
                return false;
            return ua.Contains("Macintosh", StringComparison.OrdinalIgnoreCase)
                && ua.Contains("Safari", StringComparison.OrdinalIgnoreCase)
                && ua.Contains("Mobile", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsMobileDevice(HttpContext context)
        {
            var ua = GetUserAgent(context);
            if (string.IsNullOrEmpty(ua))
                return false;

            // Client Hints: Sec-CH-UA-Mobile: ?1 (mobile) | ?0 (desktop)
            var chMobile = GetChUaMobile(context);
            if (chMobile.HasValue)
                return chMobile.Value;

            // Heurística por UA
            if (RxWindowsPhone.IsMatch(ua))
                return true;
            if (RxIphone.IsMatch(ua))
                return true;
            if (RxAndroid.IsMatch(ua) && RxMobile.IsMatch(ua))
                return true; // Android phones têm "Mobile"
            if (ua.Contains("Mobi", StringComparison.OrdinalIgnoreCase))
                return true;

            // iPad não é mobile phone
            return false;
        }

        public bool IsTabletDevice(HttpContext context)
        {
            var ua = GetUserAgent(context);
            if (string.IsNullOrEmpty(ua))
                return false;

            // iPad clássico
            if (RxIpad.IsMatch(ua))
                return true;

            // iPadOS moderno disfarçado de Mac
            if (LooksLikeModernIPad(ua))
                return true;

            // Android tablet costuma ser Android + !Mobile
            if (RxAndroidNoMobile.IsMatch(ua))
                return true;

            // Outros que se identificam como Tablet
            if (RxTablet.IsMatch(ua))
                return true;

            return false;
        }

        public bool IsTouchDevice(HttpContext context)
        {
            var ua = GetUserAgent(context);
            if (string.IsNullOrEmpty(ua))
                return false;

            // Client Hint é mais confiável quando presente
            var chMobile = GetChUaMobile(context);
            if (chMobile.HasValue && chMobile.Value)
                return true;

            // Heurística
            if (RxIphone.IsMatch(ua) || RxIpad.IsMatch(ua))
                return true;
            if (LooksLikeModernIPad(ua))
                return true;
            if (RxAndroid.IsMatch(ua))
                return true;
            if (RxTouch.IsMatch(ua))
                return true;

            return false;
        }

        public string GetDeviceType(HttpContext context)
        {
            // Overrides (útil para debug/QA): ?layout=desktop|mobile|tablet ou cookie "fs_layout"
            var forced = GetForcedLayout(context);
            if (!string.IsNullOrEmpty(forced))
                return forced!;

            if (IsTabletDevice(context))
                return "tablet";
            if (IsMobileDevice(context))
                return "mobile";
            return "desktop";
        }

        public MobileOptimizationOptions GetOptimizationOptions(HttpContext context)
        {
            var type = GetDeviceType(context);
            var touch = IsTouchDevice(context);

            return new MobileOptimizationOptions
            {
                UseTouchOptimizedButtons = touch,
                EnableSwipeGestures = touch && type != "desktop",
                UseSimplifiedLayout = type == "mobile",
                TouchTargetSize = touch ? 44 : 32,
                EnableHapticFeedback = touch,
                OptimizeImages = type != "desktop",
                UseMobileNavigation = type == "mobile",
            };
        }

        public bool ShouldUseMobileLayout(HttpContext context)
        {
            var type = GetDeviceType(context);
            return type == "mobile" || type == "tablet";
        }

        public DeviceInfo GetDeviceInfo(HttpContext context)
        {
            var ua = GetUserAgent(context);
            var type = GetDeviceType(context);

            var info = new DeviceInfo
            {
                UserAgent = ua,
                DeviceType = type,
                OperatingSystem = GetOperatingSystem(ua),
                Browser = GetBrowser(ua),
                IsTouchCapable = IsTouchDevice(context),
                IsMobile = IsMobileDevice(context),
                IsTablet = IsTabletDevice(context),
                ScreenWidth =
                    GetIntHeader(context, "X-Device-Width")
                    ?? GetIntHeader(context, "Viewport-Width")
                    ?? 0,
                ScreenHeight = GetIntHeader(context, "X-Device-Height") ?? 0,
                ChIsMobile = GetChUaMobile(context),
                ChUaPlatform = GetHeader(context, "Sec-CH-UA-Platform"),
                ChUa = GetHeader(context, "Sec-CH-UA"),
            };

            return info;
        }

        // ------------------------
        // Helpers
        // ------------------------
        private static string GetUserAgent(HttpContext context) =>
            GetHeader(context, "User-Agent") ?? string.Empty;

        private static string? GetHeader(HttpContext context, string name)
        {
            if (context?.Request?.Headers == null)
                return null;
            if (context.Request.Headers.TryGetValue(name, out var values))
                return values.ToString();
            return null;
        }

        private static int? GetIntHeader(HttpContext context, string name)
        {
            var s = GetHeader(context, name);
            if (int.TryParse(s, out var v))
                return v;
            return null;
        }

        private static bool? GetChUaMobile(HttpContext context)
        {
            var s = GetHeader(context, "Sec-CH-UA-Mobile");
            // formatos comuns: "?1" (mobile) / "?0" (desktop)
            if (string.IsNullOrEmpty(s))
                return null;
            if (s.Contains("?1"))
                return true;
            if (s.Contains("?0"))
                return false;
            return null;
        }

        private static string GetOperatingSystem(string ua)
        {
            if (string.IsNullOrEmpty(ua))
                return "Unknown";

            if (ua.Contains("Windows", StringComparison.OrdinalIgnoreCase))
                return "Windows";
            if (ua.Contains("Android", StringComparison.OrdinalIgnoreCase))
                return "Android";
            if (ua.Contains("iPhone", StringComparison.OrdinalIgnoreCase))
                return "iOS";
            if (ua.Contains("iPad", StringComparison.OrdinalIgnoreCase))
                return "iPadOS";
            if (
                ua.Contains("Mac OS X", StringComparison.OrdinalIgnoreCase)
                || ua.Contains("Macintosh", StringComparison.OrdinalIgnoreCase)
            )
                return "macOS";
            if (ua.Contains("Linux", StringComparison.OrdinalIgnoreCase))
                return "Linux";

            return "Unknown";
        }

        private static string GetBrowser(string ua)
        {
            if (string.IsNullOrEmpty(ua))
                return "Unknown";

            // A ordem importa (Edge/Opera baseados em Chromium antes de Chrome)
            if (ua.Contains("Edg/", StringComparison.OrdinalIgnoreCase))
                return "Edge";
            if (
                ua.Contains("OPR/", StringComparison.OrdinalIgnoreCase)
                || ua.Contains("Opera", StringComparison.OrdinalIgnoreCase)
            )
                return "Opera";
            if (
                ua.Contains("Chrome", StringComparison.OrdinalIgnoreCase)
                && !ua.Contains("Chromium", StringComparison.OrdinalIgnoreCase)
            )
                return "Chrome";
            if (ua.Contains("Firefox", StringComparison.OrdinalIgnoreCase))
                return "Firefox";
            if (
                ua.Contains("Safari", StringComparison.OrdinalIgnoreCase)
                && !ua.Contains("Chrome", StringComparison.OrdinalIgnoreCase)
            )
                return "Safari";
            if (ua.Contains("Chromium", StringComparison.OrdinalIgnoreCase))
                return "Chromium";

            return "Unknown";
        }

        private static string? GetForcedLayout(HttpContext context)
        {
            // querystring tem prioridade para forçar layout: ?layout=desktop|mobile|tablet
            var q = context?.Request?.Query;
            if (q != null && q.TryGetValue("layout", out var v1))
            {
                var forced = v1.ToString().Trim().ToLowerInvariant();
                if (forced is "desktop" or "mobile" or "tablet")
                    return forced;
            }

            // cookie secundário: fs_layout
            var cookies = context?.Request?.Cookies;
            if (cookies != null && cookies.TryGetValue("fs_layout", out var v2))
            {
                var forced = v2.Trim().ToLowerInvariant();
                if (forced is "desktop" or "mobile" or "tablet")
                    return forced;
            }

            return null;
        }
    }
}
