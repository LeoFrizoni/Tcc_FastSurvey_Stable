// src/hooks/usePerformanceMonitor.js
import { useEffect, useRef } from 'react';

export const usePerformanceMonitor = (componentName) => {
  const mountTime = useRef(performance.now());
  const renderCount = useRef(0);

  useEffect(() => {
    renderCount.current += 1;
    
    if (renderCount.current === 1) {
      // Primeira renderização
      const loadTime = performance.now() - mountTime.current;
      
      if (process.env.NODE_ENV === 'development') {
        console.log(`🚀 ${componentName} carregou em ${loadTime.toFixed(2)}ms`);
      }
      
      // Reportar métricas de performance
      if (loadTime > 1000) {
        console.warn(`⚠️ ${componentName} demorou ${loadTime.toFixed(2)}ms para carregar`);
      }
    }
  });

  return {
    renderCount: renderCount.current,
    measureOperation: (operationName, operation) => {
      const start = performance.now();
      const result = operation();
      const duration = performance.now() - start;
      
      if (process.env.NODE_ENV === 'development') {
        console.log(`⏱️ ${componentName} - ${operationName}: ${duration.toFixed(2)}ms`);
      }
      
      return result;
    }
  };
};

// Hook para monitorar Web Vitals
export const useWebVitals = () => {
  useEffect(() => {
    if ('web-vitals' in window) {
      return;
    }

    // Monitorar LCP (Largest Contentful Paint)
    const observer = new PerformanceObserver((list) => {
      const entries = list.getEntries();
      const lastEntry = entries[entries.length - 1];
      
      if (process.env.NODE_ENV === 'development') {
        console.log('📊 LCP:', lastEntry.startTime);
      }
    });

    observer.observe({ entryTypes: ['largest-contentful-paint'] });

    return () => observer.disconnect();
  }, []);
};
