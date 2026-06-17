create table grupo_cobranza
(
    id               bigint auto_increment primary key,
    nombre           varchar(255)                        not null,
    usuario_creo_id  bigint                              not null,
    descripcion      varchar(255)                        not null,
    fecha_cobranza   datetime                            not null,
    fecha_creacion   timestamp default CURRENT_TIMESTAMP not null
);
