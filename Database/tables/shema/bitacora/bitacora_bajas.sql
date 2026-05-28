-- auto-generated definition
create table bitacora_bajas
(
    id                     bigint auto_increment
        primary key,
    CreditoId              bigint                              not null,
    ClienteId              bigint                              not null,
    MontoOtorgado          double                              not null,
    SaldoCapital           double                              not null,
    SaldoInsoluto          double                              not null,
    CapitalVencido         double                              not null,
    AmorticacionesVencidas int                                 not null,
    InteresCobrado         double                              not null,
    DiasVencidos           varchar(10)                         not null,
    CarteraVencidaContable varchar(35)                         not null,
    Demanda                varchar(35)                         not null,
    Estatus                varchar(35)                         not null,
    FechaAlta              datetime                            not null,
    FechaVencimiento       datetime                            not null,
    InicioCobranza         datetime                            not null,
    UltCobranza            datetime                            not null,
    GestorId               bigint                              not null,
    CreatedAt              timestamp default CURRENT_TIMESTAMP not null,
    GrupoId                int                                 null,
    Obervaciones           text                                null,
    Baja                   bigint    default 1                 null comment '1 estado baja',
    NombreCliente          varchar(100)                        null,
    constraint bitacora_bajas_pk
        unique (CreditoId)
);

