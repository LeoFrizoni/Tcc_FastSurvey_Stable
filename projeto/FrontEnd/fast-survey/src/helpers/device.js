// Funções para detectar dispositivo e gerenciar responsividade

export function isMobile() {
    return /Mobi|Android|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(
        window.navigator.userAgent
    );
}

export function isTablet() {
    return /iPad|Android(?=.*\bMobile\b)(?=.*\bSafari\b)/i.test(
        window.navigator.userAgent
    );
}

export function isDesktop() {
    return !isMobile() && !isTablet();
}

export function isTouchDevice() {
    return 'ontouchstart' in window || navigator.maxTouchPoints > 0;
}

export function getScreenSize() {
    const width = window.innerWidth;
    if (width < 768) return 'mobile';
    if (width < 1024) return 'tablet';
    return 'desktop';
}

export function getOrientation() {
    return window.innerHeight > window.innerWidth ? 'portrait' : 'landscape';
}

export function isOnline() {
    return navigator.onLine;
}

export function getConnectionInfo() {
    if ('connection' in navigator) {
        const connection = navigator.connection;
        return {
            effectiveType: connection.effectiveType,
            downlink: connection.downlink,
            rtt: connection.rtt,
            saveData: connection.saveData
        };
    }
    return null;
}

export function isLowBandwidth() {
    const connectionInfo = getConnectionInfo();
    if (connectionInfo) {
        return connectionInfo.effectiveType === 'slow-2g' || 
               connectionInfo.effectiveType === '2g' ||
               connectionInfo.saveData;
    }
    return false;
}

export function getDevicePixelRatio() {
    return window.devicePixelRatio || 1;
}

export function isRetina() {
    return getDevicePixelRatio() > 1;
}

export function getViewportSize() {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
}

export function addResizeListener(callback) {
    let timeout;
    const debouncedCallback = () => {
        clearTimeout(timeout);
        timeout = setTimeout(callback, 100);
    };
    
    window.addEventListener('resize', debouncedCallback);
    return () => window.removeEventListener('resize', debouncedCallback);
}

export function addOrientationListener(callback) {
    window.addEventListener('orientationchange', callback);
    return () => window.removeEventListener('orientationchange', callback);
}

export function addOnlineListener(callback) {
    window.addEventListener('online', callback);
    window.addEventListener('offline', callback);
    return () => {
        window.removeEventListener('online', callback);
        window.removeEventListener('offline', callback);
    };
}

export function getPreferredColorScheme() {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

export function addColorSchemeListener(callback) {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    mediaQuery.addListener(callback);
    return () => mediaQuery.removeListener(callback);
}

export function isReducedMotion() {
    return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
}

export function getDeviceInfo() {
    return {
        userAgent: navigator.userAgent,
        platform: navigator.platform,
        language: navigator.language,
        cookieEnabled: navigator.cookieEnabled,
        onLine: navigator.onLine,
        screenSize: getScreenSize(),
        orientation: getOrientation(),
        pixelRatio: getDevicePixelRatio(),
        isRetina: isRetina(),
        isTouch: isTouchDevice(),
        isMobile: isMobile(),
        isTablet: isTablet(),
        isDesktop: isDesktop()
    };
}
