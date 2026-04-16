// Infrastructure/Repositories/ReporteRepository.cs
using System.Data;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;

namespace ApiConcilacionFr.Infrastructure.Repositories;

public class ReporteCarteraHistoricoRepository : IReporteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReporteCarteraHistoricoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    


}
