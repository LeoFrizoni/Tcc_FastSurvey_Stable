// Funções para gerenciar cache e storage local

const CACHE_PREFIX = 'fs_cache_';
const CACHE_EXPIRY = 5 * 60 * 1000; // 5 minutos

export function setCacheItem(key, data, expiry = CACHE_EXPIRY) {
    const item = {
        data,
        timestamp: Date.now(),
        expiry
    };
    localStorage.setItem(CACHE_PREFIX + key, JSON.stringify(item));
}

export function getCacheItem(key) {
    const item = localStorage.getItem(CACHE_PREFIX + key);
    if (!item) return null;
    
    try {
        const parsed = JSON.parse(item);
        const isExpired = Date.now() - parsed.timestamp > parsed.expiry;
        
        if (isExpired) {
            localStorage.removeItem(CACHE_PREFIX + key);
            return null;
        }
        
        return parsed.data;
    } catch {
        localStorage.removeItem(CACHE_PREFIX + key);
        return null;
    }
}

export function clearCache() {
    const keys = Object.keys(localStorage);
    keys.forEach(key => {
        if (key.startsWith(CACHE_PREFIX)) {
            localStorage.removeItem(key);
        }
    });
}

export function clearExpiredCache() {
    const keys = Object.keys(localStorage);
    keys.forEach(key => {
        if (key.startsWith(CACHE_PREFIX)) {
            getCacheItem(key.replace(CACHE_PREFIX, '')); // Isso remove automaticamente se expirado
        }
    });
}

export function getUserPreferences() {
    const prefs = localStorage.getItem('fs_user_preferences');
    return prefs ? JSON.parse(prefs) : {};
}

export function setUserPreferences(preferences) {
    localStorage.setItem('fs_user_preferences', JSON.stringify(preferences));
}

export function getTheme() {
    return localStorage.getItem('fs_theme') || 'light';
}

export function setTheme(theme) {
    localStorage.setItem('fs_theme', theme);
    document.documentElement.setAttribute('data-theme', theme);
}

export function getLanguage() {
    return localStorage.getItem('fs_language') || 'pt-BR';
}

export function setLanguage(language) {
    localStorage.setItem('fs_language', language);
}

export function clearUserData() {
    const keys = Object.keys(localStorage);
    keys.forEach(key => {
        if (key.startsWith('fs_') || key === 'token' || key === 'userId' || key === 'loginId') {
            localStorage.removeItem(key);
        }
    });
}
