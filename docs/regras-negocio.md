# Regras de negócio

## Colaboradores

Nome/cargo obrigatórios (até 50 caracteres), e-mail válido e único, salário positivo com duas casas e limite do decimal(10,2). Senha inicial com 10+ caracteres. Cadastro aceita perfil Gestor ou Colaborador; Colaborador exige gestor ativo. Departamento e gestor devem existir. Usuário, colaborador e salário inicial são gravados na mesma transação. Formulários validam usabilidade; o servidor valida contratos e vínculos.

Consulta paginada e ordenada, filtro por situação/departamento e busca nome/cargo. Edição permite nome, cargo, departamento, situação, admissão e transferência para gestor ativo; não aceita alteração salarial direta. Admissão futura é inválida. DELETE é inativação: preserva salário, solicitações e auditoria e desativa o acesso do usuário vinculado. Gestor com subordinados ativos precisa transferi-los antes de ser inativado.

## Solicitações e aprovação

1. Usuário autorizado escolhe um colaborador ativo do seu escopo.
2. Informa percentual **ou** valor de aumento, nunca ambos. Percentual positivo e no máximo 30%; valor também limitado a 30% do salário. Arredondamento comercial a duas casas; aumento sem efeito é rejeitado.
3. Simulação no servidor não persiste. Solicitação captura salário anterior e novo salário, autor, motivo e data UTC. Índice filtrado permite apenas uma pendência por colaborador.
4. Somente RH/Admin diferente do solicitante decide. Rejeição exige justificativa; atualização condicional de Pendente impede duas decisões.
5. Aprovação executa `usp_AprovarReajuste`. Verifica papel no banco a partir do contexto de sessão, bloqueia solicitação e funcionário (UPDLOCK/HOLDLOCK), confirma pendência, situação e salário original. Salário alterado desde a solicitação causa conflito: rejeitar e criar nova.
6. Dentro de transação, encerra salário vigente, cria histórico, atualiza funcionário e marca solicitação aprovada com responsável/data. Trigger registra alteração de salário na mesma transação. XACT_ABORT + TRY/CATCH + rollback em qualquer falha.

Não há exclusão de solicitações nem nova aplicação de uma decisão. O histórico é consultado no detalhe do funcionário. Pendência ou aprovação repetida retorna erro; a interface exibe o resultado sem simular sucesso.

## Auditoria e indicadores

Trigger AFTER INSERT/UPDATE set-based: Cadastro, Salário, Departamento, Situação e Cargo. Funciona com várias linhas e registra valor anterior/novo, data UTC e responsável. API de auditoria somente leitura, paginada, com período, operação, colaborador e busca por nome/responsável. Alteração de perfil de usuário não é um evento auditado nesta fase.

View de indicadores fornece os funcionários com departamento/gerente/situação/salário. Backend agrega total, ativos, departamentos com equipe ativa, folha de ativos, estados de solicitações, evolução mensal e movimentos. O gestor recebe agregados apenas do próprio escopo. A folha é bruta, sem encargos; dados não são indicadores contábeis completos.
