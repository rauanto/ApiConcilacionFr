create table bajas_tipos
(
    id     int auto_increment
        primary key,
    nombre varchar(15) null,
    constraint bajas_tipos_pk
        unique (nombre)
)
    comment 'coleccion de bajas';

