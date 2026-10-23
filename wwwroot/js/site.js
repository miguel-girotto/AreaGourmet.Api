'use strict';
document.addEventListener('submit', e => {
  const message = e.target.dataset.confirm;
  if (message && !window.confirm(message)) e.preventDefault();
});
document.querySelectorAll('img').forEach(img => img.addEventListener('error', () => {
  if (!img.src.endsWith('/img/placeholder.svg')) img.src = '/img/placeholder.svg';
}));
