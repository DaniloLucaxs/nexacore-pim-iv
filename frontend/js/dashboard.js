import {api} from './api.js';
import {$,boot,run,esc,money} from './shared.js';
run(async()=>{await boot();const data=await api('/dashboard');
const pending=data.solicitacoes.find(s=>s.status==='Pendente')?.quantidade||0;
$('kpis').innerHTML=[['Colaboradores',data.total,`${data.ativos} ativos`],['Folha mensal',money(data.folha),'Equipe ativa · sem encargos'],['Departamentos',data.departamentos.length,'Com colaboradores ativos'],['Solicitações pendentes',pending,'Aguardando decisão']].map(([title,value,hint])=>`<div><dt>${title}</dt><dd>${value}</dd><small>${hint}</small></div>`).join('');
function bars(id,list,label,value,extra){const max=Math.max(1,...list.map(x=>x[value]));$(id).innerHTML=list.length?list.map(x=>`<li><span>${esc(x[label])}</span><span class="num">${x[value]} ${extra?`· ${money(x[extra])}`:''}</span><i style="--w:${Math.max(0,x[value]/max*100)}%"></i></li>`).join(''):'<li>Nenhum registro neste escopo.</li>';}
bars('departamentos',data.departamentos,'departamento','quantidade','folha');bars('estados',data.solicitacoes,'status','quantidade');bars('movimentos',data.movimentos,'operacao','quantidade');
$('evolucao').innerHTML=data.evolucao.length?data.evolucao.map(x=>`<tr><td>${String(x.mes).padStart(2,'0')}/${x.ano}</td><td>${x.quantidade}</td><td class="num">${money(x.impacto)}</td></tr>`).join(''):'<tr><td colspan="3">Os reajustes aparecerão após a primeira aprovação.</td></tr>';
});
