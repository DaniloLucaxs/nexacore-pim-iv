# Executar o NexaCore no Windows — guia para o grupo

Este é o caminho com **C#/.NET 8 e SQL Server local**, sem Python ou Docker. Frontend e API funcionam juntos. Cada integrante terá seu próprio banco; registros cadastrados em uma máquina não aparecem automaticamente nas demais.

O fluxo foi confirmado por Danilo: restore, build sem avisos/erros, cinco migrations aplicadas e servidor iniciado com SQL Server local e autenticação Windows. A suíte completa de testes foi executada na nuvem Linux; não foi repetida automaticamente no computador dele.

## 1. Instalar os requisitos

- Git: https://git-scm.com/downloads/win
- SDK .NET 8.0.426 Windows x64: https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.426/dotnet-sdk-8.0.426-win-x64.exe
- SQL Server 2022 Express ou Developer: https://www.microsoft.com/pt-br/sql-server/sql-server-downloads
- SSMS, para conectar e conferir os dados: https://learn.microsoft.com/pt-br/ssms/install/install

Escolha **SDK**, não somente Runtime. .NET 8 e .NET 10 podem coexistir. SQL Server Developer é destinado a desenvolvimento/testes; não é licença de produção. Instalar apenas o SSMS não instala o serviço de banco.

Depois de instalar, feche e reabra o terminal. Este guia usa Windows x64; outras arquiteturas exigem instaladores correspondentes.

## 2. Escolher o terminal

Use **um terminal por vez**. Execute cada comando separado e aguarde aparecer o caminho da pasta novamente antes do próximo.

| Terminal | Como reconhecer | Entrar em uma pasta | Definir variável |
|---|---|---|---|
| CMD / Prompt de Comando | `C:\Users\seunome>` | `cd /d "CAMINHO"` | `set "NOME=valor"` |
| PowerShell | `PS C:\Users\seunome>` | `Set-Location "CAMINHO"` | `$env:NOME = 'valor'` |

Copie somente o comando dentro do bloco, sem o prefixo `C:\...>` ou `PS ...>`. Não cole dois blocos grudados, como `--seedcd` ou `pasta$env:...`.

## 3. Baixar o projeto

Os próximos dois comandos funcionam tanto no CMD quanto no PowerShell. Execute um de cada vez, a partir da pasta em que deseja guardar o projeto:

```text
git clone https://github.com/DaniloLucaxs/nexacore-pim-iv.git
```

```text
cd nexacore-pim-iv
```

Se já existe uma pasta com esse nome, não clone dentro dela novamente. Entre na cópia existente e confira se há `README.md`, `global.json`, `backend` e `frontend`. Se deseja uma segunda cópia independente, escolha outro destino no clone, por exemplo `git clone https://github.com/DaniloLucaxs/nexacore-pim-iv.git nexacore-grupo`.

Confirme a pasta de trabalho:

```text
dir backend\PIM3SemBackend.sln
```

O arquivo deve existir nessa pasta. Todos os comandos seguintes partem da **raiz do projeto**, não da pasta `backend`.

## 4. Conferir o SDK

```text
dotnet --version
```

A configuração versionada seleciona **8.0.426**. Se faltar um SDK compatível, instale o SDK do passo 1 e reabra o terminal. Para ver o que está instalado:

```text
dotnet --list-sdks
```

### Alternativa para quem já possui o SDK 8.0.132

Danilo conseguiu restaurar, compilar e iniciar usando 8.0.132 após mudar a seleção local. Caso seja exatamente essa sua versão instalada, abra:

```text
notepad global.json
```

Substitua o conteúdo por:

```json
{ "sdk": { "version": "8.0.132", "rollForward": "latestFeature" } }
```

Salve e repita `dotnet --version`. Isso altera apenas a escolha do SDK nessa cópia. Não muda o projeto para .NET 10. Não envie essa preferência local para o Git por engano. O ambiente de nuvem continua usando 8.0.426; outras versões não foram verificadas por esse relato.

## 5. Restaurar e compilar

```text
dotnet restore .\backend\PIM3SemBackend.sln --locked-mode
```

Espere terminar. Depois:

```text
dotnet build .\backend\PIM3SemBackend.sln --no-restore
```

Resultado esperado: compilação com êxito. Mensagens de telemetria e certificado de desenvolvimento no primeiro uso do .NET são normais. A aplicação deste guia usa HTTP local e não exige o comando de confiança do certificado HTTPS para iniciar.

## 6. Identificar sua instância SQL Server

Abra o SSMS, use **Autenticação do Windows** e conecte. Anote o campo **Nome do Servidor** que realmente funcionou.

Exemplos: `localhost`, `localhost\SQLEXPRESS`, `localhost\MSSQLSERVER03`. O último funcionou na máquina de Danilo; cada colega deve usar sua própria instância.

O usuário Windows precisa de permissão para criar o banco e aplicar migrations, tabelas, procedure, trigger e view. Se essas permissões não existirem, peça ao responsável pela instalação do SQL Server para preparar uma base de desenvolvimento e conceder as permissões necessárias. Não use um banco com dados reais.

O nome sugerido para uma base dedicada é `NexaCorePimIVLocal`. As migrations criam sua estrutura. Não importe scripts SQL manualmente por cima delas e não exclua bases antigas para tentar corrigir erros.

## 7A. Iniciar pelo CMD

Use esta seção se o prompt **não** começa com `PS`. Substitua `localhost\SQLEXPRESS` pela instância que conectou no SSMS. Execute uma linha por vez, no mesmo CMD:

```cmd
set "ConnectionStrings__DefaultConnection=Server=localhost\SQLEXPRESS;Database=NexaCorePimIVLocal;Integrated Security=true;Encrypt=true;TrustServerCertificate=true"
```

```cmd
set "Demo__Seed=true"
```

A linha abaixo contém um **marcador**. Antes de executar, substitua `SUA_SENHA_FICTICIA` por uma senha nova de demonstração com pelo menos 10 caracteres. Não use senha pessoal, não compartilhe capturas com seu valor e não coloque a senha em arquivos versionados. Para facilitar a sintaxe do CMD, evite `%` e aspas nessa senha fictícia.

```cmd
set "Demo__Password=SUA_SENHA_FICTICIA"
```

```cmd
set "ASPNETCORE_ENVIRONMENT=Development"
```

```cmd
dotnet run --no-launch-profile --project .\backend\PIM3SemBackend --urls http://127.0.0.1:5080
```

## 7B. Alternativa: iniciar pelo PowerShell

Use esta seção **em vez da seção 7A**, se o prompt começa com `PS`. Ajuste a instância SQL e execute uma linha por vez:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=localhost\SQLEXPRESS;Database=NexaCorePimIVLocal;Integrated Security=true;Encrypt=true;TrustServerCertificate=true'
```

```powershell
$env:Demo__Seed = 'true'
```

```powershell
$env:Demo__Password = Read-Host 'Defina uma senha fictícia com pelo menos 10 caracteres'
```

Digite a senha ao aparecer a pergunta e pressione Enter. O Read-Host acima não oculta a digitação: use apenas senha fictícia e não compartilhe capturas com seu valor.

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
```

```powershell
dotnet run --no-launch-profile --project .\backend\PIM3SemBackend --urls http://127.0.0.1:5080
```

## 8. Abrir e testar

Aguarde aparecer `Now listening on: http://127.0.0.1:5080`. Na primeira execução a API aplica cinco migrations e cria dados fictícios. O seed prepara 50 colaboradores, cinco departamentos, duas pendências e um exemplo de reajuste aprovado com auditoria real.

Digite no navegador: `http://127.0.0.1:5080/login.html`.

| E-mail | Perfil | Senha |
|---|---|---|
| `admin@nexacore.example` | Administrador | A definida no primeiro seed |
| `rh@nexacore.example` | RH | A definida no primeiro seed |
| `gestor@nexacore.example` | Gestor | A definida no primeiro seed |

1. Entre como gestor e consulte os colaboradores vinculados a ele.
2. Escolha um colaborador sem pendência; simule um aumento de 5% e envie com motivo.
3. Saia e entre como RH. Aprove a solicitação.
4. Consulte salário, histórico, autoria da auditoria e indicadores atualizados.

O frontend é servido pela API. Não abra arquivos HTML diretamente e não precisa de Live Server. `http://127.0.0.1:5080/api/health` verifica a view no SQL; `/swagger` fica disponível quando o ambiente é Development. `Hosting environment: Production` no log não impede o login, mas mantém Swagger desativado.

## 9. Encerrar, abrir novamente e atualizar

- Mantenha o terminal da API aberto durante o uso. `Ctrl+C` encerra o servidor; os dados continuam no SQL Server.
- As variáveis valem somente no terminal em que foram definidas. Ao abrir outro, entre na raiz e repita a configuração de conexão/ambiente e a inicialização da seção escolhida.
- O seed é idempotente: reiniciar não apaga nem duplica o lote inicial. Alterar `Demo__Password` depois do primeiro seed **não redefine** as senhas dos usuários existentes. Guarde sua senha fictícia de forma privada.
- Para baixar atualizações, pare a API e execute `git status`. Se houver alterações locais (por exemplo global.json), preserve-as antes de `git pull --ff-only`. Não use reset hard ou limpeza automática. Depois restaure dependências e compile novamente.
- Não compartilhe `.local/`, `.env`, connection strings com credenciais ou backups contendo informações pessoais. A connection string deste guia usa a conta Windows; nenhuma senha SQL precisa ser colocada no código.

## 10. Testes e limites

Com a API compilada:

```text
dotnet test .\backend\PIM3SemBackend.sln --no-build
```

Sem as variáveis de integração, esse comando executa 20 casos unitários e ignora explicitamente dois testes API/SQL; não equivale à validação completa do banco. O runner `python scripts/test.py` do README é uma alternativa adicional que **exige Python e Docker**, cria um banco de teste separado e executa os 22 testes. Não o use como etapa obrigatória neste caminho com SQL Server instalado.

A IA ainda não está implementada. O fluxo atual funciona sem provedor externo ou chave de API. Veja [plano de IA](plano-ia.md).

## Solução dos problemas encontrados

| Mensagem/sintoma | Causa e ação |
|---|---|
| `destination path ... already exists` | A pasta já existe. Entre na cópia correta; não clone novamente dentro dela. |
| `Set-Location não é reconhecido` | Você está no CMD. Use `cd /d "CAMINHO"`, ou abra PowerShell. |
| `Não é possível localizar um parâmetro ... '='` | Comandos ficaram juntos. Limpe a linha e execute somente um comando por vez. |
| `MSB1009: Arquivo de projeto não existe` / scripts não encontrados | Pasta incorreta. Execute `dir backend\PIM3SemBackend.sln` e volte à raiz correta. |
| `A compatible .NET SDK was not found` | Instale SDK compatível com global.json; Runtime não basta. Consulte passo 4. |
| Apenas `Determining projects to restore...` | Aguarde o download terminar; confira o resultado final antes de continuar. |
| `docker não é reconhecido` | Docker não é necessário neste guia. Use SQL Server instalado e seção 7A/7B. |
| Falha de conexão/instância SQL | Confirme no SSMS o nome exato, serviço iniciado e autenticação Windows sob a mesma conta. Não copie automaticamente a instância de outra pessoa. |
| Permissão negada ao criar base ou objetos | A conta Windows não tem permissão suficiente no SQL; peça a configuração de acesso ao responsável. |
| Aviso `No XML encryptor configured` | A API pode iniciar; chaves locais de cookie estão em `.local/keys`. Essa proteção precisa de configuração própria para distribuição/produção. |
| Navegador não conecta | Aguarde `Now listening`, mantenha o CMD aberto e confira porta/erro no terminal. |
| Login inválido depois de mudar senha no terminal | Seed não redefine usuários já existentes. Use a senha do primeiro seed; não exclua o banco para corrigir. |
| Erro 429 no login | Limite de 10 tentativas por minuto/IP. Aguarde a janela antes de tentar novamente. |

`TrustServerCertificate=true` é uma conveniência para o certificado do SQL local de desenvolvimento. Hospedagem pública, banco remoto e publicação com dados reais exigem configuração própria; não estão cobertos por este guia.
