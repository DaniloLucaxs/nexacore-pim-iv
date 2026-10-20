using PIM_3sem_backend.Exceptions.BadRequest;
namespace PIM_3sem_backend.Services.Gestao;
public static class RegrasReajuste
{
    public static decimal Calcular(decimal atual, decimal? percentual, decimal? valor)
    {
        if ((percentual.HasValue == valor.HasValue) || atual <= 0)
            throw new BadRequestException("Informe apenas percentual ou valor de aumento.");
        if (percentual is <= 0 or > 30 || valor is <= 0)
            throw new BadRequestException("Percentual deve ser maior que zero e até 30%; valor deve ser positivo.");
        var novo = decimal.Round(atual + (valor ?? atual * percentual!.Value / 100), 2, MidpointRounding.AwayFromZero);
        if (novo <= atual || novo > decimal.Round(atual * 1.30m, 2, MidpointRounding.AwayFromZero) || novo > 99999999.99m)
            throw new BadRequestException("O aumento deve alterar o salário e respeitar o limite de 30% e a capacidade do banco.");
        return novo;
    }
}
