export function validateEmail(email) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(email);
}

export function validatePassword(password) {
    const minLength = password.length >= 8;
    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumbers = /\d/.test(password);
    const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(password);
    
    return {
        isValid: minLength && hasUpperCase && hasLowerCase && hasNumbers,
        minLength,
        hasUpperCase,
        hasLowerCase,
        hasNumbers,
        hasSpecialChar
    };
}

export function validateUsername(username) {
    const minLength = username.length >= 3;
    const maxLength = username.length <= 50;
    const validChars = /^[a-zA-Z0-9_]+$/.test(username);
    
    return {
        isValid: minLength && maxLength && validChars,
        minLength,
        maxLength,
        validChars
    };
}

export function sanitizeInput(input) {
    return input.trim().replace(/[<>]/g, '');
}

export function validateRequired(value) {
    return value && value.trim().length > 0;
}
  