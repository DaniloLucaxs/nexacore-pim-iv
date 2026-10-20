# Roteiro de apresentação

1. Iniciar conforme README. Explicar que API e frontend são locais, SQL Server guarda os dados e não há IA implementada. Mostrar a marca/landing; distinguir simulação institucional dos indicadores do banco.
2. Entrar como `gestor@nexacore.example`. Consultar a equipe, filtros e detalhe/histórico de um colaborador. Mostrar que não há cadastro/aprovação para esse perfil e que o backend também rejeita tais operações.
3. Ir a Solicitações. Escolher colaborador sem pendência (os dois primeiros já possuem exemplos), preencher aumento e motivo, simular e enviar. Estado Pendente; salário ainda não muda.
4. Sair e entrar como `rh@nexacore.example`. Localizar a solicitação, revisar valores e aprovar. Explicar procedure, bloqueios, transação, histórico e trigger na mesma operação.
5. Consultar detalhe salarial e Auditoria: salário anterior/novo, responsável, horário. Filtrar pelo nome e operação Salário. Voltar ao painel: a folha e os contadores refletem a aprovação.
6. Mostrar rejeição com justificativa e inativação preservando histórico. Não inativar um gestor com equipe antes de transferir os vínculos.
7. Exibir testes e documentação. A suíte demonstra também a concorrência e o rollback por falha provocada; não é preciso editar salários manualmente no banco da demonstração.

## Respostas técnicas

- **Por que Services?** Coordenam regras, permissões de processo, cálculos e transações; Controllers lidam com HTTP.
- **Repositories?** Organizam persistência, includes, filtros de escopo e execução SQL parametrizada, usando EF Core.
- **DTOs?** Delimitam entradas/saídas e validação; não expõem hash de senha ou permitem editar campos internos livremente.
- **Procedure?** Rotina SQL chamada explicitamente para validar e aplicar uma aprovação atomicamente.
- **Trigger vs procedure?** Trigger executa automaticamente após alteração de funcionário; procedure é invocada pelo fluxo de aprovação.
- **Transação e rollback?** Salário, histórico, decisão e auditoria confirmam juntos; uma falha desfaz o conjunto.
- **Concorrência?** Bloqueios de atualização e verificação de status impedem aplicar uma pendência duas vezes; índices filtrados reforçam unicidade.
- **Frontend/API?** fetch com JSON e cookie, token CSRF nas mutações, feedback de erro e atualização dos dados após sucesso.
- **Permissões?** Policies/roles no backend e filtro de subordinados nas consultas; esconder botão melhora a experiência, não substitui autorização.
- **Indicadores?** Backend agrega a view SQL e solicitações do escopo do usuário; navegador só apresenta os resultados.
- **Autoria com pool?** Interceptor sobrescreve contexto em toda abertura, usando id da sessão autenticada; trigger resolve o autor no SQL.
- **IA?** Apenas arquitetura futura documentada; consultas fechadas e autorizadas, sem SQL livre ou credenciais no navegador.

## Divisão de estudo

Danilo/Gustavo/Fred: Models, Services, Repositories, migrations, SQL e testes transacionais. Rafa/Yaponan: HTML/CSS, módulos JS, API, estados de erro/carregamento e responsividade. Todos devem executar o cenário e explicar o fluxo de ponta a ponta. Não apresentar funcionalidades futuras como concluídas.
