﻿using DDD.Core.Contracts.Library.ApplicationServices.Queries;
using DDD.Core.RequestResponse.Library.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Extensions.Logger.Abstractions;

namespace DDD.Core.ApplicationServices.Library.Queries;

public class QueryDispatcher : IQueryDispatcher
{
    #region Fields
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueryDispatcher> _logger;
    private readonly Stopwatch _stopwatch;
    #endregion

    #region Constructors
    public QueryDispatcher(IServiceProvider serviceProvider, ILogger<QueryDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _stopwatch = new Stopwatch();
        _logger = logger;
    }
    #endregion

    #region Query Dispatcher
    public Task<QueryResult<TData>> Execute<TQuery, TData>(TQuery query) where TQuery : class, IQuery<TData>
    {
        _stopwatch.Start();
        try
        {
            _logger.LogDebug("Routing query of type {QueryType} With value {Query}  Start at {StartDateTime}", query.GetType(), query, DateTime.Now);
            var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TData>>();
            return handler.Handle(query);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "There is not suitable handler for {QueryType} Routing failed at {StartDateTime}.", query.GetType(), DateTime.Now);
            throw;
        }
        finally
        {
            _stopwatch.Stop();
            _logger.LogInformation(EventID.PerformanceMeasurement, "Processing the {QueryType} query tooks {Millisecconds} Millisecconds", query.GetType(), _stopwatch.ElapsedMilliseconds);
        }
    }
    #endregion
}