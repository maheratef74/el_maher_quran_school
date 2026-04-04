/* El-Maher Quran School PWA Service Worker */

const CACHE_NAME = 'el-maher-v1';
const urlsToCache = [
  '/',
  '/Dashboard',
  '/image/app_logo.svg',
  '/image/logo_el_maher.svg'
];

self.addEventListener('install', event => {
    // Perform install steps
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    event.waitUntil(self.clients.claim());
});

self.addEventListener('fetch', event => {
    // Simple pass-through for now to ensure reliability
    event.respondWith(fetch(event.request).catch(() => caches.match(event.request)));
});
