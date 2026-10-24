# NexaCore PIM IV — resumo da versão atual

## Objetivo

Evoluir a base do PIM III para um MVP de gestão de pessoas com processos de RH, permissões, indicadores e rastreabilidade. A aplicação funciona localmente com C#/.NET 8, SQL Server e frontend HTML/CSS/JavaScript, preservando entidades, namespaces, migrations e identidade visual da base fornecida.

## Funcionalidades e evidências

| Área | Entrega disponível | Como demonstrar |
|---|---|---|
| Autenticação | Login real, senha com hash, cookie de sessão, logout e proteção CSRF | Entrar/sair com contas fictícias; acesso não autenticado é bloqueado |
| Perfis | Administrador, RH e Gestor, com autorização no servidor | Gestor consulta subordinados; RH/Admin gerenciam colaboradores e decidem solicitações |
| Colaboradores | Cadastro, consulta, busca, filtros, ordenação, paginação, edição, detalhes e inativação | Cadastrar/consultar funcionário; inativação preserva histórico |
| Solicitações | Simulação por percentual ou valor; criação e consulta de pendências; aprovação e rejeição | Gestor solicita; outro usuário RH/Admin decide |
| Regras de reajuste | Aumento positivo limitado a 30%, validação de salário original, impedimento de duplicação e autoaprovação | Tentar valores inválidos ou aprovar uma solicitação já processada |
| Histórico salarial | Salário inicial e registros das alterações aprovadas | Consultar detalhe após a aprovação |
| Procedure | `usp_AprovarReajuste`, com bloqueios, transação e tratamento de erro/rollback | Explicar a aprovação e mostrar a definição no SQL Server |
| Trigger e auditoria | Registro automático de cadastro, salário, departamento, situação e cargo; suporte a múltiplas linhas | Filtrar auditoria após editar ou aprovar reajuste; conferir autor e valores |
| Views e indicadores | `vw_IndicadoresFuncionarios` e agregações reais de equipe, folha, departamentos, solicitações e movimentos | Comparar painel antes/depois da aprovação |
| Integridade SQL | Relacionamentos/FKs e índices, inclusive unicidade de pendência e salário vigente | Mostrar migrations e explicar o que cada índice impede |
| Interface | Landing, login, painel, colaboradores, solicitações e auditoria; estados de erro/carregamento e menu móvel | Navegar com dados persistidos, sem localStorage de negócio |
| Demonstração | Seed transacional e idempotente com 50 colaboradores fictícios e cinco departamentos | Iniciar a base nova e realizar o fluxo de ponta a ponta |
| Testes | 22 testes .NET e dois testes de navegador aprovados na nuvem | Consultar resultados descritos em `validacao.md` e explicar os cenários |
| Execução e documentação | Clone, restore, build, configuração SQL, migrations e início local documentados | Seguir `instalacao-rapida.md` numa máquina do grupo |

## Arquitetura e tecnologias

Fluxo principal: **Frontend → Controllers → Services → Repositories/EF Core → SQL Server**. Controllers tratam HTTP, Services coordenam regras e transações, Repositories organizam acesso e escopo dos dados. Na aprovação, o banco executa a procedure e a trigger na mesma transação.

A API serve também os arquivos do frontend. Não há dependência de React, hospedagem pública, pagamento ou API externa para executar o MVP. Python/Docker são alternativas auxiliares de ambiente/testes; não são a linguagem do backend nem requisitos do caminho Windows com SQL Server instalado.

## Regras essenciais para explicar

- Gestor vê apenas colaboradores e indicadores do próprio escopo; esconder botões não substitui autorização no backend.
- Uma aprovação atualiza salário, histórico, decisão e auditoria atomicamente. Falha provoca rollback.
- Solicitação não pode ser aplicada duas vezes, inclusive sob concorrência.
- Só RH/Admin diferente do solicitante decide; rejeição precisa de justificativa.
- Inativação preserva os registros históricos.
- Indicadores vêm do banco, não de números fixos na interface autenticada.
- O contexto do usuário é redefinido a cada abertura lógica da conexão para a auditoria não atribuir ações ao usuário anterior do pool.

## Validação e limites

Na nuvem Linux: restore com lockfiles, build sem avisos/erros, 22 testes .NET sem skips e dois testes de navegador aprovados, com SQL Server real; inclui concorrência, rollback provocado, trigger multirregistro e fluxo visual. Layout conferido em 390/768/1440 px.

Na máquina Windows de Danilo: SDK 8.0.132 selecionado localmente, restore, build sem avisos/erros, migrations, inicialização e funcionamento de login/painel/colaboradores relatados pelo usuário. Isso não significa que toda a suíte automatizada foi repetida no Windows ou nas máquinas dos colegas.

Não incluídos nesta versão: IA externa, redefinição de senha por e-mail, área autenticada de Colaborador, encargos de folha, exportação e aprovação salarial em lote. A landing tem uma simulação explicitamente ilustrativa; os indicadores autenticados são consultados no banco.

## IA: próxima evolução

Planejada como consultor de RH somente leitura, usando os serviços autorizados existentes e respeitando o escopo do usuário. Provedor ainda será escolhido. Não haverá SQL livre, acesso direto do modelo ao banco ou chave no frontend. A integração e seus testes são uma entrega futura, não um recurso disponível hoje. Ver [plano-ia.md](plano-ia.md).

## Uso no documento acadêmico

Este resumo mapeia a implementação atual ao pedido de evolução fornecido pelo grupo. Não substitui o enunciado oficial, a rubrica ou as exigências de formatação da instituição; a conferência de atendimento integral deve ser feita contra esses documentos. Não foi fornecido um novo enunciado oficial nesta solicitação.

Referências: [instalação rápida](instalacao-rapida.md), [arquitetura](arquitetura.md), [regras de negócio](regras-negocio.md), [endpoints](endpoints.md), [evidências de validação](validacao.md) e [roteiro de apresentação](roteiro-apresentacao.md).
