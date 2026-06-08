create table peticion_bajas
(
    Id                       bigint auto_increment primary key,
    CreditoId                bigint                              not null,
    ClienteId                bigint                              not null,
    Observaciones            text                                null,
    Baja                     bigint                              not null,
    UsuarioSolicitaId        bigint                              not null,
    UsuarioAutorizaId        bigint                              null,
    AtendidoPrimeraInstancia tinyint(1) default 0                not null,
    CreatedAt                timestamp default CURRENT_TIMESTAMP not null
);
