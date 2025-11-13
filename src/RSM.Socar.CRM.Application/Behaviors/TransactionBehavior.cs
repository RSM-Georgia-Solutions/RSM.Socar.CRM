using MediatR;
using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Application.Abstractions;

public sealed class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IDbExecutionStrategy _strategyFactory;
    private readonly IDbTransactionFactory _txFactory;
    private readonly IUnitOfWork _uow;

    public TransactionBehavior(
        IDbExecutionStrategy strategyFactory,
        IDbTransactionFactory txFactory,
        IUnitOfWork uow)
    {
        _strategyFactory = strategyFactory;
        _txFactory = txFactory;
        _uow = uow;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (IsQuery(request))
            return await next();

        var strategy = _strategyFactory.CreateStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _txFactory.BeginTransactionAsync(ct);

            try
            {
                var response = await next();
                await _uow.CommitAsync(ct);
                return response;
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        });
    }

    private static bool IsQuery(object request) =>
        request.GetType().Name.EndsWith("Query", StringComparison.OrdinalIgnoreCase);
}
