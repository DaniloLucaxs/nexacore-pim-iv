# Diagnóstico da base recebida

## Fontes e estrutura
Backend: `PIM-3sem-backend - Copia.zip`. Frontend: `nexacore-entrega.zip` (única versão visual recebida). Não foi fornecido backend incremental. O repositório de destino estava vazio; o original remoto não será alterado. Fontes importadas sem metadados Git internos, bin ou obj.

## Backend existente
.NET 8 / EF Core 8 / SQL Server; Controllers → Services → Repositories. Entidades: Funcionario, Departamento, Usuario, Perfil, Salario e PontoRegistro. DbContext configura departamento, gerente e usuário de funcionário, perfil de usuário e pontos. Salario não possui FK configurada. Quatro migrations existentes preservadas. DI explícita e middleware de exceções.

Endpoints originais: GET Departamento (lista e id), Perfil (lista e id); POST/GET/PATCH/DELETE Usuario; POST Login retornando booleano; POST/GET/PATCH/DELETE Funcionario e GET Funcionario/Gerentes e Gerentes/{id}.

Problemas: login sem sessão, senha em texto puro, CORS aberto, endpoints sem autorização; exclusão física; cadastro salva usuário antes de validar gerente sem transação; alteração salarial direta sem histórico; middleware ignora erro de domínio. Teste FuncionarioServiceTest contém `var` sem declaração completa e não compila. Há seis testes completos de departamento/perfil. Não há testes SQL ou frontend. Connection string original é específica de máquina Windows.

## Frontend existente
HTML/CSS/JS sem framework. Landing page, login, dashboard, funcionários, reajustes e auditoria. Marca e CSS responsivo reutilizáveis, além de tabelas, cards, formulários e dialogs. Login apenas redireciona. app.js guarda funcionários e auditoria em localStorage; reajustes são executados em arrays. Dashboard mistura oito colaboradores simulados com total fixo de 48 e folha fixa. Não há chamadas HTTP. A navegação móvel precisa de controle acessível.

## Evolução e dependências
1. Preservar namespaces, migrations, marca e testes; organizar backend/frontend/database/docs.
2. Login ASP.NET Core por cookie HttpOnly, hashing de senha, perfis Administrador/RH/Gestor e validação de escopo no servidor. Servir frontend pela API evita CORS e simplifica execução local. Proteção CSRF para mutações.
3. Integrar cadastro transacional, listagem paginada, detalhes/histórico, edição e inativação; gestor restrito aos subordinados.
4. Solicitações: criação/simulação → pendência → decisão RH/Admin. Procedure transacional usa bloqueios para impedir duplicação e conflito com salário alterado. Auditoria por trigger multirregistro. Views para indicadores.
5. Nova migration aditiva para situação/admissão, solicitações, auditoria, índices, FK de salários e objetos SQL. SQL é instalado pelas migrations, sem aplicação manual duplicada.
6. Seed fictício opt-in e idempotente. Testes de unidade, API e SQL Server real, frontend responsivo e documentação Windows.

## Riscos
SQL Server exige acesso ao registro Microsoft; a rede inicial bloqueou seu domínio de blobs. SDK .NET ausente inicialmente. Migrations novas em base legada precisam de backup e saneamento de eventuais salários órfãos/emails repetidos. Senhas legadas não serão aceitas como texto puro: redefinição necessária. Concorrência, autorização e contexto de auditoria precisam de teste com banco real. IA externa fica fora desta fase.
