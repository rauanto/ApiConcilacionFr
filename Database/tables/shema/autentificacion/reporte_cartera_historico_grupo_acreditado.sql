-- auto-generated definition
create table reporte_cartera_historico_grupo_acreditado
(
    id                 int auto_increment
        primary key,
    fecha_corte        date                                not null,
    fecha_registro     timestamp default CURRENT_TIMESTAMP not null,
    S_NOMBRE           varchar(100)                        null,
    credito            int                                 null,
    cliente            int                                 null,
    ejecutivo_asignado varchar(100)                        null,
    monto_colocado     decimal(18, 2)                      null,
    saldo_cartera      decimal(18, 2)                      null,
    capital_vencido    decimal(18, 2)                      null,
    saldo_final        decimal(18, 2)                      null,
    tipo               int                                 null,
    S_GRUPO            int                                 not null,
    nombre_grupo       varchar(100)                        null,
    id_usuario         int                                 null
);

