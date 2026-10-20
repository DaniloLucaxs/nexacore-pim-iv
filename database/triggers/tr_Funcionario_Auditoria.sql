CREATE OR ALTER TRIGGER dbo.tr_Funcionario_Auditoria ON dbo.Funcionarios
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Ator uniqueidentifier=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'UsuarioId'));
    DECLARE @Email nvarchar(100)=COALESCE((SELECT Email FROM dbo.Usuarios WHERE Id=@Ator),N'Sistema');
    INSERT dbo.Auditorias (Entidade,RegistroId,Operacao,ValorAnterior,ValorNovo,DataHora,IdUsuario,Responsavel)
    SELECT N'Funcionario',i.Id,v.Operacao,v.Anterior,v.Novo,SYSUTCDATETIME(),@Ator,@Email
    FROM inserted i LEFT JOIN deleted d ON d.Id=i.Id
    CROSS APPLY (VALUES
        (N'Cadastro',CONVERT(nvarchar(500),NULL),CONVERT(nvarchar(500),i.Nome),CASE WHEN d.Id IS NULL THEN 1 ELSE 0 END),
        (N'Salário',CONVERT(nvarchar(500),d.Salario),CONVERT(nvarchar(500),i.Salario),CASE WHEN d.Id IS NOT NULL AND d.Salario<>i.Salario THEN 1 ELSE 0 END),
        (N'Departamento',CONVERT(nvarchar(500),(SELECT Nome FROM dbo.Departamentos WHERE Id=d.IdDepartamento)),CONVERT(nvarchar(500),(SELECT Nome FROM dbo.Departamentos WHERE Id=i.IdDepartamento)),CASE WHEN d.IdDepartamento<>i.IdDepartamento THEN 1 ELSE 0 END),
        (N'Situação',CONVERT(nvarchar(500),CASE WHEN d.Ativo=1 THEN N'Ativo' ELSE N'Inativo' END),CONVERT(nvarchar(500),CASE WHEN i.Ativo=1 THEN N'Ativo' ELSE N'Inativo' END),CASE WHEN d.Ativo<>i.Ativo THEN 1 ELSE 0 END),
        (N'Cargo',CONVERT(nvarchar(500),d.Cargo),CONVERT(nvarchar(500),i.Cargo),CASE WHEN d.Cargo<>i.Cargo THEN 1 ELSE 0 END)
    ) v(Operacao,Anterior,Novo,Alterado)
    WHERE v.Alterado=1;
END
