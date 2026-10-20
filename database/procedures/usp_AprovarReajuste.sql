CREATE OR ALTER PROCEDURE dbo.usp_AprovarReajuste @Id uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @Ator uniqueidentifier = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'UsuarioId'));
    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios u JOIN dbo.Perfis p ON p.Id=u.IdPerfil WHERE u.Id=@Ator AND u.Ativo=1 AND p.Nome IN (N'Administrador',N'RH'))
        THROW 50001, N'Apenas RH e Administrador podem aprovar.', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Funcionario uniqueidentifier, @Anterior decimal(10,2), @Novo decimal(10,2), @Status nvarchar(20), @Solicitante uniqueidentifier, @Motivo nvarchar(500);
        SELECT @Funcionario=IdFuncionario, @Anterior=SalarioAnterior, @Novo=NovoSalario, @Status=Status, @Solicitante=IdSolicitante, @Motivo=Motivo
        FROM dbo.Solicitacoes WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Id;
        IF @Funcionario IS NULL THROW 50002, N'Solicitação não encontrada.', 1;
        IF @Status<>N'Pendente' THROW 50003, N'Solicitação já processada.', 1;
        IF @Solicitante=@Ator THROW 50004, N'O solicitante não pode decidir a própria solicitação.', 1;
        DECLARE @Atual decimal(10,2), @Ativo bit;
        SELECT @Atual=Salario, @Ativo=Ativo FROM dbo.Funcionarios WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Funcionario;
        IF @Ativo<>1 THROW 50005, N'Colaborador inativo.', 1;
        IF @Atual<>@Anterior THROW 50006, N'Salário mudou desde a solicitação. Rejeite e solicite novamente.', 1;
        IF @Novo<=@Atual OR @Novo>ROUND(@Atual*1.30,2) THROW 50007, N'Reajuste deve ser positivo e limitado a 30%.', 1;
        UPDATE dbo.Salarios SET DataFim=CONVERT(date,SYSUTCDATETIME()) WHERE IdFuncionario=@Funcionario AND DataFim IS NULL;
        INSERT dbo.Salarios (Id,IdFuncionario,Valor,DataInicio,DataFim,MotivoAlteracao)
        VALUES (NEWID(),@Funcionario,@Novo,CONVERT(date,SYSUTCDATETIME()),NULL,@Motivo);
        UPDATE dbo.Funcionarios SET Salario=@Novo WHERE Id=@Funcionario;
        UPDATE dbo.Solicitacoes SET Status=N'Aprovada',IdResponsavel=@Ator,DecididaEm=SYSUTCDATETIME() WHERE Id=@Id;
        COMMIT;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK;
        THROW;
    END CATCH
END
