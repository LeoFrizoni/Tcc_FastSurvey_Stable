#nullable enable
using Microsoft.AspNetCore.Http;

namespace FASTSURVEY.Services.Mobile
{
    public interface IMobileService
    {
        bool IsMobileDevice(HttpContext context);
        bool IsTabletDevice(HttpContext context);
        bool IsTouchDevice(HttpContext context);
        string GetDeviceType(HttpContext context); // "mobile" | "tablet" | "desktop"
        DeviceInfo GetDeviceInfo(HttpContext context);
        MobileOptimizationOptions GetOptimizationOptions(HttpContext context);
        bool ShouldUseMobileLayout(HttpContext context);
    }

    public sealed class MobileOptimizationOptions
    {
        public bool UseTouchOptimizedButtons { get; set; } = true;
        public bool EnableSwipeGestures { get; set; } = true;
        public bool UseSimplifiedLayout { get; set; } = true;
        public int TouchTargetSize { get; set; } = 44; // px
        public bool EnableHapticFeedback { get; set; } = true;
        public bool OptimizeImages { get; set; } = true;
        public bool UseMobileNavigation { get; set; } = true;
    }

    public sealed class DeviceInfo
    {
        public string UserAgent { get; set; } = string.Empty;
        public string DeviceType { get; set; } = "desktop"; // mobile/tablet/desktop
        public string OperatingSystem { get; set; } = "Unknown";
        public string Browser { get; set; } = "Unknown";
        public int ScreenWidth { get; set; } // se enviados pelo front via headers
        public int ScreenHeight { get; set; }
        public bool IsTouchCapable { get; set; }
        public bool IsMobile { get; set; }
        public bool IsTablet { get; set; }

        // Client Hints (quando presentes)
        public bool? ChIsMobile { get; set; }
        public string? ChUaPlatform { get; set; }
        public string? ChUa { get; set; }
    }
}
