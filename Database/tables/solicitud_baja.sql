CREATE TABLE IF NOT EXISTS solicitud_baja
(
    id            bigint auto_increment
        primary key,
    CreditoId     bigint                              not null,
    ClienteId     bigint                              not null,
    Obervaciones  text                                null,
    Baja          bigint    default 1                 null comment '1 estado baja',
    CreatedAt     timestamp default CURRENT_TIMESTAMP not null,
    solicitante   int                                 not null,
    solventado    int                                 null,
    GrupoId       int                                 null,
    NombreCliente varchar(100)                        null,
    constraint solicitud_baja_Usuarios_Id_fk
        foreign key (solicitante) references autentificacion.Usuarios (Id),
    constraint solicitud_baja_Usuarios_Id_fk_2
        foreign key (solventado) references autentificacion.Usuarios (Id)
);
