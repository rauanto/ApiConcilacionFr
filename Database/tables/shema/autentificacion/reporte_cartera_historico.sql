

create table reporte_cartera_historico
(
    id                 int auto_increment
        primary key,
    fecha_corte        date                                not null,
    fecha_registro     timestamp default CURRENT_TIMESTAMP not null,
    ejecutivo_asignado varchar(100)                        null,
    total_clientes     int                                 null,
    total_prestamos    int                                 null,
    monto_colocado     decimal(18, 2)                      null,
    saldo_cartera      decimal(18, 2)                      null,
    capital_vencido    decimal(18, 2)                      null,
    saldo_final        decimal(18, 2)                      null,
    porcentaje_calidad decimal(5, 2)                       null,
    tipo               int                                 null,
    id_usuario         int                                 null,
    id_usuario_g       int                                 null
);

