import { api, session } from './api.js';
export const $ = id => document.getElementById(id);
export const esc = value => String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
export const money = value => Number(value).toLocaleString('pt-BR', {style:'currency', currency:'BRL'});
export const date = value => value ? new Date(value).toLocaleString('pt-BR', {dateStyle:'short',timeStyle:'short'}) : '—';
export function toast(message) {const el=document.createElement('div');el.className='toast';el.role='status';el.textContent=message;document.body.append(el);setTimeout(()=>el.remove(),4500);}
export async function run(fn) {
  $('erro').textContent=''; $('loading').hidden=false;
  try { return await fn(); } catch(error) { $('erro').textContent=error.message; } finally { $('loading').hidden=true; }
}
export async function boot() {
  const user=await session(); if(!user.autenticado){location.replace('login.html');throw new Error('Autenticação necessária.');}
  const nav={dashboard:'Painel',funcionarios:'Colaboradores',reajustes:'Solicitações e reajustes',auditoria:'Auditoria'};
  $('side').innerHTML=`<a class="brand" href="dashboard.html"><img class="mark" src="assets/nexacore-mark.png" alt="">NexaCore</a><nav aria-label="Principal">${Object.entries(nav).map(([key,label])=>`<a href="${key}.html" ${document.body.dataset.page===key?'aria-current="page"':''}>${label}</a>`).join('')}</nav><div class="me"><b>${esc(user.email)}</b><span>${esc(user.perfil)}</span><button id="sair" class="btn ghost">Sair da conta</button></div>`;
  const top=document.createElement('div');top.className='mobile-top';top.innerHTML='<a href="dashboard.html">NexaCore</a><button class="btn ghost" aria-expanded="false" aria-controls="side">Menu</button>';document.body.prepend(top);
  top.querySelector('button').onclick=e=>{const open=$('side').classList.toggle('open');e.currentTarget.setAttribute('aria-expanded',String(open));};
  document.addEventListener('keydown',e=>{if(e.key==='Escape'){$('side').classList.remove('open');top.querySelector('button').setAttribute('aria-expanded','false');}});
  $('sair').onclick=()=>run(async()=>{await api('/logout',{method:'POST'});location.replace('login.html');});
  return {...user,podeGerir:['Administrador','RH'].includes(user.perfil)};
}
export function options(list, label='nome') { return list.map(x=>`<option value="${esc(x.id)}">${esc(x[label])}</option>`).join(''); }
export function pager(el, page, total, size, change) {
  const max=Math.max(1,Math.ceil(total/size));
  el.innerHTML=`<span>${total} registro(s) · Página ${page} de ${max}</span><div class="acts"><button class="btn ghost" ${page<=1?'disabled':''} data-step="-1">Anterior</button><button class="btn ghost" ${page>=max?'disabled':''} data-step="1">Próxima</button></div>`;
  el.querySelectorAll('button').forEach(b=>b.onclick=()=>change(page+Number(b.dataset.step)));
}
export function confirm(message,label='Confirmar') {
  return new Promise(resolve=>{const d=document.createElement('dialog');d.innerHTML=`<form method="dialog"><h2>Confirmar operação</h2><p>${esc(message)}</p><div class="acts"><button class="btn ghost" value="cancel">Cancelar</button><button class="btn" value="ok">${esc(label)}</button></div></form>`;d.onclose=()=>{resolve(d.returnValue==='ok');d.remove();};document.body.append(d);d.showModal();});
}
