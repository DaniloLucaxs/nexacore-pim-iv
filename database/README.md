# SQL Server e migrations

Fonte de instalação: `backend/PIM3SemBackend/Migrations`. As quatro migrations originais são preservadas. `EvolucaoPimIV` acrescenta campos de cadastro e as tabelas Solicitacoes/Auditorias; liga Salarios a Funcionarios; cria índices e instala os scripts abaixo, embutidos no assembly.

- `procedures/usp_AprovarReajuste.sql`: aplicação transacional e concorrente de uma aprovação.
- `triggers/tr_Funcionario_Auditoria.sql`: auditoria set-based de inserção/alterações.
- `views/vw_IndicadoresFuncionarios.sql`: base de consultas gerenciais com escopo por gerente.
- `seeds/README.md`: seed EF idempotente.

**Não execute scripts separadamente após migrations.** Sem comandos GO nos recursos: cada objeto é um comando SQL próprio da migration. Os scripts CREATE OR ALTER permitem futuras revisões controladas por novas migrations. Ao alterar um objeto já publicado, crie uma nova migration que aplique sua nova definição; não reescreva migrations aplicadas de outros ambientes.

## Atualização de dados legados

Antes: backup, revisão do SQL idempotente gerado por EF, teste em cópia. E-mails repetidos, salários órfãos ou dois históricos abertos para um colaborador impedem os novos índices/FK. Não há saneamento destrutivo automático: corrija a base com decisão do responsável. A nova situação é inicializada a partir do usuário vinculado. Admissão desconhecida permanece 0001-01-01 até ser cadastrada; não é uma data histórica real.

Funcionário sem salário vigente recebe um registro do valor atual na data da migração, identificado como saldo inicial com histórico anterior indisponível. Não são inventados reajustes passados. Senhas legadas em texto puro não são migradas para credenciais válidas automaticamente: redefina sob controle do administrador.

## Integridade e índices

FKs Restrict preservam histórico e impedem remoção de dados referenciados. Email de usuário único. Apenas um salário aberto por funcionário (`DataFim IS NULL`) e uma solicitação pendente (`Status=Pendente`). Índice situação/departamento atende filtros; status/data atende central; registro/data atende histórico de auditoria. Contexto de usuário é fornecido pelo interceptor a cada abertura lógica da conexão.

No desenvolvimento, a conta SQL aplica migrations e por isso possui permissões elevadas no banco local. Produção precisa separar conta de migração de conta operacional e restringir escrita direta em tabelas/auditoria; esse provisionamento não foi implementado nesta fase local. A aplicação não fornece endpoint de escrita em auditoria.

Os testes de integração utilizam SQL Server real e um banco com prefixo NexaCoreTests_, sem SQLite/InMemory como substituto. Teste de rollback cria temporariamente um gatilho de falha apenas nesse banco isolado e o remove em finally. Bases de teste permanecem para diagnóstico; limpeza exige escolha explícita do responsável.
