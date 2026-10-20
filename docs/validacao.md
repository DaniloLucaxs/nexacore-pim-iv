# Evidências de validação

Ambiente executado: Linux x64, SDK .NET 8.0.426, EF Core 8.0.28, SQL Server 2022 Developer CU18 em Docker e Chromium instalado na máquina. Não foi executado Windows/SQL Express ou dispositivo móvel físico.

- Script `scripts/install-cloud.sh` executado com instalação limpa do SDK em diretório separado, arquivo oficial verificado por SHA-512, restore `--locked-mode`, restauração do EF Tool, `npm ci`, imagem SQL por digest e build. Resultado: sucesso, zero avisos e zero erros.
- `python scripts/test.py --browser`: **22 testes .NET aprovados, zero falhas e zero ignorados; dois testes Playwright aprovados**. TRX e logs desta execução ficam em `.local/`, fora do versionamento.
- Unitários: seis testes completos originais de departamento/perfil preservados, teste incompleto de funcionário concluído e testes novos para cálculo, arredondamento, limites e hash de senha.
- Integração: cadastro real, validação de escopo e papéis, proteção CSRF, simulação, criação de pendência, bloqueio de duplicação, duas aprovações concorrentes com uma única aplicação, histórico único, autoria da auditoria, atualização da folha, rejeição, impedimento de autoaprovação e inativação com preservação do histórico.
- Banco real: gatilho temporário no banco isolado força falha após escrita do histórico; a procedure reverte salário/histórico/status, e a solicitação pode ser aprovada depois. Alteração SQL com duas linhas produz duas entradas de auditoria. O teste impede uso de banco sem prefixo NexaCoreTests_.
- Navegador: login funcional, navegação, quatro páginas sem erro JavaScript ou overflow do documento em 390/768/1440 px, menu móvel com Escape e fluxo completo cadastro RH → pedido Gestor → aprovação RH → auditoria SQL.
- Correções encontradas durante testes: dados de InlineData nullable no teste novo; largura mínima dos painéis no layout responsivo; seletores ambíguos do teste de formulário; janela de limite de login compartilhada entre suítes. O runner agora reinicia a API entre API/UI e mantém o limite de login, validando também a repetição do seed.
- Após ajuste final de contraste do menu, o teste de layout foi executado novamente e passou. Consulta autenticada da instância demonstrativa confirmou 50 ativos, cinco departamentos, duas pendências e uma aprovação.
- `/api/health` retornou 200 com SQL Server, após reinicialização da API final e reaplicação idempotente do seed.
- `dotnet ef migrations has-pending-model-changes --no-build --project backend/PIM3SemBackend`: sem mudanças pendentes de modelo.
- Oito arquivos das quatro migrations originais foram comparados byte a byte e permanecem intactos. Snapshot foi evoluído pela nova migration. Módulos JS passaram também em `node --check`.

O remoto de destino estava inicialmente vazio. Durante o trabalho apareceu o commit inicial `3517077ff44cdb10a5fea62efc1e694dbac6eead`, contendo apenas o título do README. A branch local de trabalho passou a usar esse commit como base, sem sobrescrever arquivos. A implementação foi inicialmente entregue como alterações locais para revisão. O versionamento no repositório de destino foi solicitado posteriormente pelo usuário. O repositório original do PIM III permanece intacto.

Foram salvos no rascunho do ambiente `install_script`, `start_skill` e repositório/caminho/ref exatos. A tentativa inicial com SQL latest exigiu domínio adicional; a imagem fixada funcionou com os presets existentes, então a regra temporária foi removida. Não foram solicitados novos segredos. Salvar o rascunho não publica o ambiente; revisão/salvamento/publicação e restauração em tarefa nova ainda não foram executados.
