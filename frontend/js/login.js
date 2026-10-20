import {api,session} from './api.js';
const form=document.querySelector('form'),password=form.elements.senha,message=document.getElementById('msg'),button=form.querySelector('[type=submit]');
document.getElementById('ver').onclick=e=>{const hidden=password.type==='password';password.type=hidden?'text':'password';e.currentTarget.textContent=hidden?'Ocultar':'Mostrar';};
form.onsubmit=async e=>{e.preventDefault();button.disabled=true;message.textContent='';try{await api('/Login',{method:'POST',body:{email:form.elements.email.value,senha:password.value}});location.replace('dashboard.html');}catch(error){message.textContent=error.message;}finally{button.disabled=false;}};
session().then(s=>{if(s.autenticado)location.replace('dashboard.html');}).catch(e=>message.textContent=e.message);
