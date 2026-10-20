CREATE OR ALTER VIEW dbo.vw_IndicadoresFuncionarios AS
SELECT f.Id,f.IdDepartamento,d.Nome AS Departamento,f.IdGerente,f.Ativo,f.Salario
FROM dbo.Funcionarios f JOIN dbo.Departamentos d ON d.Id=f.IdDepartamento;
