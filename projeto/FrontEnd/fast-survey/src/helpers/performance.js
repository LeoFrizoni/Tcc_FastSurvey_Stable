// Funções de performance para otimizar chamadas de API e eventos

export function debounce(func, wait, immediate = false) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            timeout = null;
            if (!immediate) func(...args);
        };
        const callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func(...args);
    };
}

export function throttle(func, limit) {
    let inThrottle;
    return function(...args) {
        if (!inThrottle) {
            func.apply(this, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}

export function memoize(func) {
    const cache = new Map();
    return function(...args) {
        const key = JSON.stringify(args);
        if (cache.has(key)) {
            return cache.get(key);
        }
        const result = func.apply(this, args);
        cache.set(key, result);
        return result;
    };
}

export function lazyLoad(importFunc) {
    return React.lazy(importFunc);
}

export function withErrorBoundary(WrappedComponent, fallback = null) {
    return class ErrorBoundary extends React.Component {
        constructor(props) {
            super(props);
            this.state = { hasError: false, error: null };
        }

        static getDerivedStateFromError(error) {
            return { hasError: true, error };
        }

        componentDidCatch(error, errorInfo) {
            console.error('Error caught by boundary:', error, errorInfo);
        }

        render() {
            if (this.state.hasError) {
                return fallback || <div>Algo deu errado. Tente recarregar a página.</div>;
            }
            return <WrappedComponent {...this.props} />;
        }
    };
}

export function retry(fn, retries = 3, delay = 1000) {
    return new Promise((resolve, reject) => {
        fn()
            .then(resolve)
            .catch((error) => {
                if (retries > 0) {
                    setTimeout(() => {
                        retry(fn, retries - 1, delay)
                            .then(resolve)
                            .catch(reject);
                    }, delay);
                } else {
                    reject(error);
                }
            });
    });
}

export function batchUpdate(updates, delay = 16) {
    return new Promise((resolve) => {
        requestAnimationFrame(() => {
            updates.forEach(update => update());
            setTimeout(resolve, delay);
        });
    });
}
