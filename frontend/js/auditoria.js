import {api,query} from './api.js';
import {$,boot,run,esc,date,pager} from './shared.js';
let page=1;
async function listar(){const values=Object.fromEntries(new FormData($('filtros')));if(values.inicio)values.inicio=new Date(values.inicio+'T00:00:00').toISOString();if(values.fim)values.fim=new Date(values.fim+'T23:59:59.999').toISOString();
const data=await api('/auditoria'+query({...values,pagina:page}));$('log').innerHTML=data.items.length?data.items.map(a=>`<li><time datetime="${esc(a.dataHora)}">${date(a.dataHora)}</time><div><b>${esc(a.operacao)}</b> · ${esc(a.funcionario)}<p class="chg">${a.valorAnterior!==null?`${esc(a.valorAnterior)} → `:''}${esc(a.valorNovo)}</p><p>por ${esc(a.responsavel)}</p></div></li>`).join(''):'<li>Nenhum registro encontrado neste período ou escopo.</li>';pager($('paginacao'),page,data.total,20,n=>run(async()=>{page=n;await listar();}));}
$('filtros').onsubmit=e=>{e.preventDefault();page=1;run(listar);};run(async()=>{await boot();await listar();});
