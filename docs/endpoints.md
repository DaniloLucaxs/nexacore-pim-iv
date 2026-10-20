# Contratos HTTP

Todos os caminhos abaixo partem de `/api`. JSON camelCase, GUIDs, datas ISO e valores monetários numéricos. Sessão via cookie HttpOnly. Obter `GET /session` antes de mutações; enviar `csrfToken` no header `X-CSRF-TOKEN`. Após login, obter novo token associado à identidade autenticada. Sem CORS entre origens: frontend é servido pela API.

| Método / caminho | Acesso | Comportamento |
|---|---|---|
| GET /health | Público | Consulta a view e responde status ok |
| GET /session | Público | Estado da sessão, e-mail, perfil, id e token CSRF |
| POST /Login | Público + CSRF | `{email,senha}`, cria cookie; 401 em credenciais inválidas; limite por IP |
| POST /logout | Autenticado | Encerra sessão |
| GET /Departamento, /Departamento/{id} | Admin/RH/Gestor | Cadastros auxiliares |
| GET /Perfil, /Perfil/{id} | Admin/RH/Gestor | Inclui perfis legados, que não dão acesso aos novos módulos |
| GET /Funcionario | Admin/RH/Gestor | Paginação e escopo obrigatório |
| GET /Funcionario/{id} | Admin/RH/Gestor | `{funcionario,historico,movimentacoes}`; 404 fora do escopo |
| GET /Funcionario/Gerentes | Admin/RH/Gestor | Gestores ativos dentro do escopo |
| GET /Funcionario/Gerentes/{id} | Admin/RH/Gestor | Subordinados do gerente, intersectados com o escopo |
| POST /Funcionario | Admin/RH | Cadastro transacional; 201 com id |
| PATCH /Funcionario/{id} | Admin/RH | Edição cadastral; 204 |
| DELETE /Funcionario/{id} | Admin/RH | Inativação; 204 |
| POST /solicitacoes/simular | Admin/RH/Gestor | Simula no servidor, sem gravar |
| POST /solicitacoes | Admin/RH/Gestor | Abre pendência; 201 com id |
| GET /solicitacoes | Admin/RH/Gestor | Filtros status, funcionario, pagina; 20 por página |
| POST /solicitacoes/{id}/aprovar | Admin/RH | Procedure transacional; sem body; 204 |
| POST /solicitacoes/{id}/rejeitar | Admin/RH | `{justificativa}`; 204 |
| GET /dashboard | Admin/RH/Gestor | Agregados reais do escopo |
| GET /auditoria | Admin/RH/Gestor | Leitura, filtros e paginação |
| POST/GET/PATCH/DELETE /Usuario[/{id}] | Admin | Rotas legadas protegidas; DELETE desativa acesso |

GET /Funcionario aceita `busca`, `departamento` (GUID), `ativo` (booleano), `ordem` (nome/salario), `pagina` (>=1), `tamanho` (1..100). Resposta `{items,total,pagina,tamanho}`; mudou em relação à lista sem paginação do PIM III. Cada item inclui ids dos vínculos, nomes, salário, e-mail, perfil, situação e admissão.

POST /Funcionario mantém nomes do contrato original: `{nome,salario,cargo,idDepartamento,idGerente,email,senha,idPerfil}`. PATCH usa `{nome,cargo,idDepartamento,ativo,dataAdmissao,idGerente?}`; omitir gestor mantém o vínculo. Alteração salarial deve usar solicitações.

Solicitar/simular: `{idFuncionario,percentual?,valor?,motivo}`. `valor` significa aumento, não salário final. Simulação responde `{salarioAnterior,novoSalario}`. GET /solicitacoes inclui motivo, responsáveis, datas, justificativa e valores para a tela detalhar a decisão.

GET /auditoria aceita `busca` (nome ou responsável), `operacao`, `funcionario` (GUID), `inicio`, `fim` (DateTimeOffset), `pagina`. Intervalo inclusivo. Resposta `{items,total,pagina}` com 20 registros.

Erros: 400 contrato/regra/CSRF; 401 sessão ou login; 403 perfil; 404 inexistente/fora do escopo; 409 duplicação ou conflito de aprovação/concorrência; 500 falha interna registrada no servidor. Erros esperados retornam `{error,status}` quando tratados pelo middleware; validação automática ASP.NET Core retorna ProblemDetails com `errors`. Swagger de desenvolvimento descreve os contratos inferidos.
