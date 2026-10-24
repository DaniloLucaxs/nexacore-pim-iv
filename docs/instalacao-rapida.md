# NexaCore PIM IV — instalação rápida

**Versão atual:** C#/.NET 8 + SQL Server + HTML/CSS/JavaScript. Este guia é para **Windows 10/11 x64**, com banco na própria máquina, sem Python ou Docker. Para outros sistemas, consulte a alternativa Docker no README; os comandos abaixo são específicos do Windows.

## 1. Instalar uma única vez

| Requisito | Instalação |
|---|---|
| Git | [Instalador oficial](https://git-scm.com/downloads/win) |
| .NET **SDK 8.0.426** | [Instalador Windows x64](https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.426/dotnet-sdk-8.0.426-win-x64.exe) — escolha SDK, não apenas Runtime |
| SQL Server 2022 Express ou Developer | [Download oficial](https://www.microsoft.com/pt-br/sql-server/sql-server-downloads) — instale o mecanismo de banco local |
| SSMS (recomendado) | [Instalação oficial](https://learn.microsoft.com/pt-br/ssms/install/install) — para conferir a conexão com o banco |

VS Code é opcional. Node.js não é necessário para usar o sistema; é usado apenas nos testes de navegador. Internet é necessária para baixar ferramentas e dependências; a execução normal do MVP não usa API externa de IA.

Depois de instalar, reabra o terminal. Abra o SSMS e confirme que consegue conectar usando **Autenticação do Windows**. Guarde o nome da instância que funcionou, por exemplo `localhost\SQLEXPRESS`. Cada computador pode ter um nome diferente. A conta Windows deve ter permissão para criar o banco e aplicar migrations na instância de desenvolvimento.

## 2. Baixar e preparar o projeto

Abra **PowerShell** pelo menu Iniciar ou pelo VS Code. Use apenas PowerShell neste guia. Execute **uma linha por vez**, aguardando terminar:

```powershell
git clone https://github.com/DaniloLucaxs/nexacore-pim-iv.git
```

```powershell
cd nexacore-pim-iv
```

```powershell
dotnet restore .\backend\PIM3SemBackend.sln --locked-mode
```

```powershell
dotnet build .\backend\PIM3SemBackend.sln --no-restore
```

A compilação deve terminar com êxito. O restore baixa as bibliotecas .NET automaticamente; não é necessário instalar cada pacote manualmente.

**Já clonou?** Entre na pasta que contém `backend`, `frontend` e `global.json`; não execute clone dentro dela novamente. **Erro de SDK?** Confira `dotnet --list-sdks`: o projeto fixa 8.0.426. Para a alternativa já confirmada com 8.0.132, consulte [o guia detalhado](execucao-local-windows.md#alternativa-para-quem-já-possui-o-sdk-80132).

## 3. Configurar e iniciar

Na mesma janela do PowerShell e na raiz do projeto, execute cada linha separadamente. O primeiro comando pergunta o nome da instância SQL: digite o nome que conectou no SSMS, não o nome de outro integrante.

```powershell
$servidorNexaCore = Read-Host 'Nome da sua instancia SQL Server, conforme o SSMS'
```

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=$servidorNexaCore;Database=NexaCorePimIVLocal;Integrated Security=true;Encrypt=true;TrustServerCertificate=true"
```

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
```

```powershell
$env:Demo__Seed = 'true'
```

```powershell
$env:Demo__Password = Read-Host 'Senha ficticia de demonstracao, com pelo menos 10 caracteres'
```

Defina uma senha nova apenas para a demonstração. A digitação fica visível: não compartilhe capturas com a senha e não use senha pessoal. Guarde-a para os próximos acessos.

```powershell
dotnet run --no-launch-profile --project .\backend\PIM3SemBackend --urls http://127.0.0.1:5080
```

A API cria o banco dedicado `NexaCorePimIVLocal`, aplica as migrations e gera dados fictícios. Não é necessário importar scripts SQL manualmente. Aguarde aparecer **`Now listening on: http://127.0.0.1:5080`**.

## 4. Acessar e apresentar

No navegador, digite **`http://127.0.0.1:5080/login.html`**. Não abra os arquivos HTML diretamente.

| Conta | Perfil |
|---|---|
| `admin@nexacore.example` | Administrador |
| `rh@nexacore.example` | RH |
| `gestor@nexacore.example` | Gestor |

As contas usam a senha definida no **primeiro seed dessa máquina**. O banco inicial possui 50 colaboradores, cinco departamentos, duas solicitações pendentes e uma aprovada.

**Demonstração curta:** entrar como gestor → escolher colaborador sem pendência → solicitar reajuste → sair e entrar como RH → aprovar → conferir salário, histórico, auditoria e painel. O roteiro completo está em [roteiro-apresentacao.md](roteiro-apresentacao.md).

## 5. Abrir novamente

Mantenha o terminal aberto durante o uso. Para encerrar, pressione **Ctrl+C**. O banco e as alterações continuam salvos no SQL Server.

Ao abrir outro terminal, entre na raiz do projeto e repita o **passo 3**, usando a mesma instância e a senha original de demonstração. As variáveis anteriores não são herdadas de uma janela já fechada. O seed não duplica o lote inicial e **alterar a senha no terminal não redefine as contas existentes**. Cada integrante tem seu próprio banco: os dados não são compartilhados entre máquinas.

## Se não abrir

- **Projeto não encontrado:** pasta errada. Na raiz, `Test-Path .\backend\PIM3SemBackend.sln` deve mostrar `True`.
- **SDK incompatível:** instale o SDK indicado no passo 1, ou consulte a alternativa 8.0.132 no guia detalhado.
- **Erro de conexão SQL:** confira a instância e o serviço no SSMS; configure a conexão no mesmo terminal em que executará o .NET.
- **Navegador com conexão recusada:** a API não iniciou ou encerrou. Confira o erro no terminal e aguarde `Now listening`; não precisa alterar firewall por padrão.
- **Comando não reconhecido:** use PowerShell, copie apenas o comando e não junte linhas. Não copie o texto `PS C:\...>` do terminal.

Mais detalhes: [guia de suporte Windows](execucao-local-windows.md). **IA não está implementada nesta versão** e não exige chave para rodar o sistema atual.
