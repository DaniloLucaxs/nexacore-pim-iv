# Seed reproduzível

Implementado em `backend/PIM3SemBackend/Data/DemoSeeder.cs`, após migrations. Ativação explícita: `Demo__Seed=true`, com `Demo__Password` de 10+ caracteres, ou `python scripts/dev.py --docker --seed`.

Cria em transação perfis Administrador/RH/Gestor/Colaborador, cinco departamentos, 50 colaboradores com salário inicial e gerente, usuários fictícios, duas solicitações pendentes e um exemplo aprovado pela procedure. A auditoria de cadastro é Sistema; aprovação demonstrativa tem contexto de RH. E-mails usam domínio reservado `.example`. Usuários e colaboradores adicionais de testes usam `.test`.

A presença do administrador demonstrativo marca o lote inicial concluído de forma atômica. O exemplo aprovado possui motivo marcador e só é criado uma vez. O seed não reverte dados já editados, não redefine senhas e não repovoa exclusões manuais. Não habilitar em base de dados reais.
