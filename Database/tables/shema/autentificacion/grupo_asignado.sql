-- auto-generated definition
create table grupo_asignado
(
    S_GRUPO      int          not null
        primary key,
    nombre_grupo varchar(150) not null,
    usuario_id   int          null,
    constraint grupo_asignado_ibfk_1
        foreign key (usuario_id) references Usuarios (Id)
            on delete cascade
);

create index usuario_id
    on grupo_asignado (usuario_id);

