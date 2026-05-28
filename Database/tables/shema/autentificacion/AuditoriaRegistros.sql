-- auto-generated definition
create table AuditoriaRegistros
(
    Id                 int auto_increment
        primary key,
    Entidad            varchar(100)                        null,
    EntidadId          varchar(50)                         null,
    Operacion          varchar(20)                         null,
    Data               text                                null,
    UsuarioResponsable varchar(100)                        null,
    FechaEvento        timestamp default CURRENT_TIMESTAMP not null
);

