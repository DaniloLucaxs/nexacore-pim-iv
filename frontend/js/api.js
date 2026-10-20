import { API_BASE } from './config.js';
let csrfToken;
export async function session() {
  const response = await fetch(`${API_BASE}/session`, { credentials: 'same-origin', cache: 'no-store' });
  if (!response.ok) throw new Error('Não foi possível conectar à API.');
  const data = await response.json(); csrfToken = data.csrfToken; return data;
}
export async function api(path, { method = 'GET', body, signal } = {}) {
  if (method !== 'GET' && !csrfToken) await session();
  const response = await fetch(API_BASE + path, {
    method, signal, credentials: 'same-origin', cache: 'no-store',
    headers: { ...(body !== undefined ? { 'Content-Type': 'application/json' } : {}), ...(method !== 'GET' ? { 'X-CSRF-TOKEN': csrfToken } : {}) },
    ...(body !== undefined ? { body: JSON.stringify(body) } : {})
  });
  if (response.status === 401 && path !== '/Login') { location.replace('login.html'); throw new Error('Sessão encerrada. Entre novamente.'); }
  if (!response.ok) {
    const data = await response.json().catch(() => ({}));
    throw new Error(data.error || Object.values(data.errors || {}).flat().join(' ') || (response.status === 403 ? 'Seu perfil não permite esta operação.' : 'Não foi possível concluir a operação.'));
  }
  return response.status === 204 ? null : response.json();
}
export function query(values) { return '?' + new URLSearchParams(Object.entries(values).filter(([, v]) => v !== '' && v != null)); }
