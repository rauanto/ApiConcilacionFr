create table bitacora_gestion
(
    id                  bigint auto_increment
        primary key,
    amortizacion_id     bigint                                                                                                                                                                                                                                                                 not null,
    credito_id          bigint                                                                                                                                                                                                                          not null,
    cliente_id          bigint                                                                                                                                                                                                                                                   not null,
    gestor_id           bigint                                                                                                                                                                                                                                                   not null,
    medio_contacto_id   bigint                                                                                                                                                                                                                                                   null,
    fecha_hora_gestion  datetime                                                                                                                                                                                                                                                 not null,
    tipo_gestion        enum ('LLAMADA', 'SMS', 'WHATSAPP', 'EMAIL', 'VISITA', 'CARTA', 'BUROFAX')                                                                                                                                                                               not null,
    sentido             enum ('SALIENTE', 'ENTRANTE')                                                                                                                                                                                                                            not null,
    resultado           enum ('CONTACTO_EFECTIVO', 'CONTACTO_TERCERO', 'NO_CONTESTA', 'BUZON', 'NUMERO_EQUIVOCADO', 'NUMERO_FUERA_SERVICIO', 'RECHAZA_LLAMADA', 'PROMESA_PAGO', 'NEGOCIACION', 'NEGATIVA_PAGO', 'REPROGRAMACION', 'ENVIADO', 'ENTREGADO', 'LEIDO', 'RESPONDIDO') not null,
    duracion_segundos   int                                                                                                                                                                                                                                                      null,
    mensaje_enviado     text                                                                                                                                                                                                                                                     null,
    asunto              varchar(255)                                                                                                                                                                                                                                             null,
    respuesta_cliente   text                                                                                                                                                                                                                                                     null,
    promesa_fecha_pago  date                                                                                                                                                                                                                                                     null,
    promesa_monto       decimal(15, 2)                                                                                                                                                                                                                                           null,
    promesa_cumplida    tinyint(1)                                                                                                                                                                                                                                               null,
    url_grabacion       varchar(500)                                                                                                                                                                                                                                             null,
    url_evidencia       varchar(500)                                                                                                                                                                                                                                             null,
    observaciones       text                                                                                                                                                                                                                                                     null,
    geolocalizacion_lat decimal(10, 8)                                                                                                                                                                                                                                           null,
    geolocalizacion_lng decimal(11, 8)                                                                                                                                                                                                                                           null,
    created_at          timestamp default CURRENT_TIMESTAMP                                                                                                                                                                                                                      not null,
    fecha_cobro         date                                                                                                                                                                                                                                                     null
);

create index idx_amortizacion
    on bitacora_gestion (amortizacion_id);

create index idx_cliente_fecha
    on bitacora_gestion (cliente_id, fecha_hora_gestion);

create index idx_gestor_fecha
    on bitacora_gestion (gestor_id, fecha_hora_gestion);

