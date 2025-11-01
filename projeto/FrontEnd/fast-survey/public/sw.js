// public/sw.js
const CACHE_NAME = 'fast-survey-v1.0.4';
const urlsToCache = [
  '/',
  '/static/js/bundle.js',
  '/static/css/main.css',
  '/manifest.json'
];

const API_CACHE_NAME = 'fast-survey-api-v1.0.2';
const apiUrlsToCache = [
  '/api/tipos/pergunta',
  '/api/tipos/pesquisa',
  '/api/tipos/usuario'
];

// Install event
self.addEventListener('install', event => {
  event.waitUntil(
    Promise.all([
      caches.open(CACHE_NAME)
        .then(cache => cache.addAll(urlsToCache)),
      caches.open(API_CACHE_NAME)
        .then(cache => {
          // Pre-cache API endpoints
          return Promise.allSettled(
            apiUrlsToCache.map(url => 
              fetch(url).then(response => 
                response.ok ? cache.put(url, response) : null
              ).catch(() => {})
            )
          );
        })
    ])
  );
  self.skipWaiting();
});

// Activate event
self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames.map(cacheName => {
          if (cacheName !== CACHE_NAME && cacheName !== API_CACHE_NAME) {
            return caches.delete(cacheName);
          }
        })
      );
    })
  );
  self.clients.claim();
});

// Fetch event
self.addEventListener('fetch', event => {
  const { request } = event;
  const url = new URL(request.url);

  // Strategy: Cache First for static assets
  if (request.destination === 'script' || 
      request.destination === 'style' || 
      request.destination === 'image') {
    event.respondWith(
      caches.match(request)
        .then(response => {
          if (response) {
            return response;
          }
          return fetch(request).then(response => {
            if (response.status === 200) {
              const responseClone = response.clone();
              caches.open(CACHE_NAME)
                .then(cache => cache.put(request, responseClone));
            }
            return response;
          });
        })
    );
    return;
  }

  // Strategy: Network First for API calls
  if (url.pathname.startsWith('/api/')) {
    event.respondWith(
      fetch(request)
        .then(response => {
          if (response.status === 200 && request.method === 'GET') {
            const responseClone = response.clone();
            caches.open(API_CACHE_NAME)
              .then(cache => cache.put(request, responseClone));
          }
          return response;
        })
        .catch(() => {
          return caches.match(request);
        })
    );
    return;
  }

  // Strategy: Network First for HTML documents
  if (request.destination === 'document') {
    event.respondWith(
      fetch(request)
        .then(response => {
          if (response.status === 200) {
            const responseClone = response.clone();
            caches.open(CACHE_NAME)
              .then(cache => cache.put(request, responseClone));
          }
          return response;
        })
        .catch(() => {
          return caches.match(request);
        })
    );
  }
});
