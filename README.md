# NexaCore PIM IV

Evolução do PIM III para gestão de pessoas com .NET 8, EF Core, SQL Server e HTML/CSS/JavaScript. Mantém entidades, namespaces, migrations originais e identidade visual recebida. Não há integração externa de IA nesta fase.

## Começar com Docker (Windows, Linux ou macOS x64)

Requisitos: SDK **.NET 8.0.426** (global.json), Python 3.10+, Docker com suporte a containers Linux e pelo menos 4 GB disponíveis para o SQL Server. No Windows, Docker Desktop/WSL2. Node.js 22+ é necessário somente para testes de navegador. Não é necessário SQL Server pago, hospedagem ou domínio.

Na raiz do projeto, defina uma senha **fictícia de demonstração local** com 10+ caracteres antes do primeiro seed. No PowerShell:

```powershell
$env:Demo__Password = Read-Host "Senha para os usuários fictícios"
dotnet restore backend/PIM3SemBackend.sln --locked-mode
python scripts/dev.py --docker --seed
```

No Bash, use `read -rs -p 'Senha demonstrativa: ' Demo__Password; export Demo__Password` e os mesmos comandos de restore/execução.

O script inicia SQL Server 2022 Developer com imagem fixada por digest, volume `nexacore-sql-data`, senha SQL aleatória local e porta 1433 restrita ao loopback. Espera uma consulta SQL funcionar antes de iniciar a API. A API aplica migrations e seed idempotente. **API e frontend compartilham o mesmo servidor HTTP**, sem necessidade de outro servidor ou CORS aberto.

Acesso local: `http://127.0.0.1:5080` (landing), `/login.html` (login), `/swagger` (documentação em Development). Não abra HTML com `file://`. Saúde funcional: `GET /api/health` consulta a view SQL.

Contas fictícias: `admin@nexacore.example` (Administrador), `rh@nexacore.example` (RH), `gestor@nexacore.example` (Gestor). Todas usam a senha definida antes do primeiro seed. Colaboradores possuem vínculos de usuário preservados do modelo original, mas não têm área de login nesta fase. Nunca use senha pessoal ou dados reais no seed.

Sem senha fornecida, `--docker` gera uma senha demonstrativa em `.local/runtime.json`; consulte esse arquivo **localmente** se necessário, sem compartilhá-lo. O mesmo arquivo guarda a senha aleatória do SQL; `.local` é ignorado pelo Git. Alterar a variável depois do primeiro seed não redefine senhas já persistidas. Não exclua o volume para redefini-las: solicite uma redefinição controlada.

Ctrl+C encerra a API. O banco permanece no Docker para preservar dados. `docker stop nexacore-sql` para pará-lo; o comando de desenvolvimento o reinicia depois. Não apague volumes para atualizar o sistema.

## SQL Server/Express instalado no Windows

Instale .NET 8.0.426 e SQL Server/Express; SSMS é opcional. Use uma base de desenvolvimento e uma conta com permissões para criar a base e aplicar migrations (incluindo procedure, trigger e view). Exemplo PowerShell, adaptando a instância:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=localhost\SQLEXPRESS;Database=NexaCore;Integrated Security=true;Encrypt=true;TrustServerCertificate=true'
$env:Demo__Password = Read-Host "Senha fictícia para o seed"
python scripts/dev.py --seed
```

Sem `--docker`, o script usa a connection string do terminal. `TrustServerCertificate=true` é apenas para o certificado local de desenvolvimento; não desativa TLS no download de dependências. Um SQL Server remoto exige configuração própria de certificado, credenciais e rede; não foi validado nesta entrega.

## Migrations, objetos SQL e dados

Quatro migrations originais foram preservadas; `EvolucaoPimIV` acrescenta situação/admissão, solicitações, auditoria, FK de salários, índices e objetos SQL embutidos no assembly. A inicialização aplica `Database.MigrateAsync()` antes do seed. **Não execute os scripts SQL manualmente depois das migrations.** Consulte [database/README.md](database/README.md) antes de atualizar uma base legada.

Para criar a próxima migration: `dotnet tool restore` e `dotnet ef migrations add Nome --project backend/PIM3SemBackend`. Para apenas gerar o SQL para revisão: `dotnet ef migrations script --idempotent --project backend/PIM3SemBackend --output .local/migrations.sql`. Aplique em base legada somente após backup e revisão; nunca use `database drop` como etapa de instalação.

O seed é opt-in (`--seed` / `Demo__Seed=true`), transacional e idempotente: 50 colaboradores fictícios, cinco departamentos, três perfis administrativos, duas pendências e um reajuste aprovado pela mesma procedure do produto. O exemplo aprovado produz histórico e auditoria reais. O seed não reinicia nem apaga suas alterações nas execuções seguintes.

## Testes

```powershell
dotnet restore backend/PIM3SemBackend.sln --locked-mode
dotnet build backend/PIM3SemBackend.sln --no-restore
dotnet test backend/PIM3SemBackend.sln --no-build
python scripts/test.py
```

`dotnet test` isolado executa os testes unitários e declara os testes SQL ignorados quando faltam as variáveis da API de teste. **`python scripts/test.py` executa todos**, iniciando uma API na porta 5081 com banco `NexaCoreTests_<id>` e seed próprios, sem usar os registros da demonstração. O banco de testes é retido para diagnóstico; nenhuma base é apagada automaticamente. Resultado TRX e logs ficam em `.local/`.

Para testar também o navegador:

```powershell
npm ci --prefix tests/browser
cd tests/browser
npx playwright install chromium
cd ../..
python scripts/test.py --browser
```

No ambiente de nuvem já preparado, o runner usa `/usr/bin/chromium`. Em outras máquinas usa o Chromium instalado pelo Playwright; `CHROMIUM_PATH` permite informar um executável local. Suítes API e UI usam ciclos separados da API, preservando o limite de login e verificando a repetição do seed. Não há traces de autenticação gravados.

Validação executada na nuvem Linux x64: restore com lockfiles, build, **22 testes .NET aprovados, sem skips**, e **2 testes de navegador aprovados** com SQL Server 2022 real. Inclui concorrência, rollback provocado depois da escrita do histórico, trigger multirregistro, autoria em conexões reutilizadas, permissões e fluxo visual completo. Layout verificado em 390/768/1440 px. Windows/SQL Express e dispositivos físicos não foram executados aqui.

## Organização e colaboração

- `backend/`: API original evoluída e testes preservados/complementados.
- `frontend/`: marca, landing, CSS e páginas integradas sem React ou localStorage de negócio.
- `database/`: procedure, trigger, view e instruções de seed.
- `docs/`: diagnóstico, arquitetura, contratos, regras e roteiro de apresentação.
- `scripts/`: execução e testes; `install-cloud.sh` prepara dependências Linux do ambiente cloud.

Trabalho sugerido: Danilo/Gustavo/Fred no backend e SQL; Rafa/Yaponan no frontend; integração, testes e apresentação compartilhados. Use `main` estável, `develop` para integração e `feature/*` por tarefa. O grupo deve revisar as alterações e coordenar a integração das próximas funcionalidades.

## Limites desta versão

Aplicação para demonstração e desenvolvimento local. Aumento por colaborador, percentual ou valor, com limite de 30%; aprovação em lote não faz parte do escopo. Sem redefinição de senha por e-mail, folha com encargos, exportação ou integração de IA. Admissões de dados legados sem data ficam como desconhecidas até atualização. Consulte [docs/arquitetura.md](docs/arquitetura.md) para a futura IA autorizada e somente leitura.
