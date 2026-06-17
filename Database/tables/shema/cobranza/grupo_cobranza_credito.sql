create table grupo_cobranza_credito
(
    id                bigint auto_increment primary key,
    grupo_cobranza_id bigint not null,
    credito_id        bigint not null,
    constraint fk_grupo_cobranza_credito_grupo
        foreign key (grupo_cobranza_id) references grupo_cobranza (id)
);
