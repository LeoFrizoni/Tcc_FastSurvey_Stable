// Funções de segurança e criptografia

export function sanitizeHtml(html) {
    const div = document.createElement('div');
    div.textContent = html;
    return div.innerHTML;
}

export function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, m => map[m]);
}

export function generateRandomString(length = 32) {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let result = '';
    for (let i = 0; i < length; i++) {
        result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
}

export function generateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        const r = Math.random() * 16 | 0;
        const v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

export function hashString(str) {
    let hash = 0;
    if (str.length === 0) return hash;
    for (let i = 0; i < str.length; i++) {
        const char = str.charCodeAt(i);
        hash = ((hash << 5) - hash) + char;
        hash = hash & hash; // Convert to 32bit integer
    }
    return hash.toString();
}

export function validateToken(token) {
    if (!token) return false;
    
    try {
        // Verifica se é um JWT válido (formato básico)
        const parts = token.split('.');
        if (parts.length !== 3) return false;
        
        // Verifica se as partes são base64 válidas
        const header = JSON.parse(atob(parts[0]));
        const payload = JSON.parse(atob(parts[1]));
        
        // Verifica se não expirou
        if (payload.exp && payload.exp < Date.now() / 1000) {
            return false;
        }
        
        return true;
    } catch {
        return false;
    }
}

export function getTokenPayload(token) {
    if (!token) return null;
    
    try {
        const parts = token.split('.');
        if (parts.length !== 3) return null;
        
        return JSON.parse(atob(parts[1]));
    } catch {
        return null;
    }
}

export function isTokenExpired(token) {
    const payload = getTokenPayload(token);
    if (!payload || !payload.exp) return true;
    
    return payload.exp < Date.now() / 1000;
}

export function validatePasswordStrength(password) {
    const checks = {
        length: password.length >= 8,
        uppercase: /[A-Z]/.test(password),
        lowercase: /[a-z]/.test(password),
        numbers: /\d/.test(password),
        special: /[!@#$%^&*(),.?":{}|<>]/.test(password)
    };
    
    const score = Object.values(checks).filter(Boolean).length;
    
    return {
        isValid: score >= 4,
        score,
        checks,
        strength: score < 2 ? 'weak' : score < 4 ? 'medium' : 'strong'
    };
}

export function maskSensitiveData(data, fields = ['password', 'token', 'secret']) {
    if (typeof data === 'object' && data !== null) {
        const masked = Array.isArray(data) ? [] : {};
        
        for (const [key, value] of Object.entries(data)) {
            if (fields.includes(key.toLowerCase())) {
                masked[key] = '***';
            } else if (typeof value === 'object' && value !== null) {
                masked[key] = maskSensitiveData(value, fields);
            } else {
                masked[key] = value;
            }
        }
        
        return masked;
    }
    
    return data;
}

export function validateCSRF(token) {
    // Implementação básica de validação CSRF
    const storedToken = localStorage.getItem('csrf_token');
    return token === storedToken;
}

export function generateCSRFToken() {
    const token = generateRandomString(32);
    localStorage.setItem('csrf_token', token);
    return token;
}

export function clearSensitiveData() {
    const sensitiveKeys = [
        'token', 'csrf_token', 'password', 'secret',
        'access_token', 'refresh_token', 'api_key'
    ];
    
    sensitiveKeys.forEach(key => {
        localStorage.removeItem(key);
        sessionStorage.removeItem(key);
    });
}

export function validateInput(input, type = 'text') {
    const validators = {
        email: (value) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value),
        url: (value) => {
            try {
                new URL(value);
                return true;
            } catch {
                return false;
            }
        },
        phone: (value) => /^[\+]?[1-9][\d]{0,15}$/.test(value.replace(/\s/g, '')),
        number: (value) => !isNaN(value) && isFinite(value),
        date: (value) => !isNaN(Date.parse(value)),
        text: (value) => typeof value === 'string' && value.trim().length > 0
    };
    
    const validator = validators[type] || validators.text;
    return validator(input);
}

export function preventXSS(input) {
    if (typeof input !== 'string') return input;
    
    return input
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#x27;')
        .replace(/\//g, '&#x2F;');
}
