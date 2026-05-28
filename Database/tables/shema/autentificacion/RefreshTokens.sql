create table RefreshTokens
(
    Id            int auto_increment
        primary key,
    Token         varchar(512)                         not null,
    UsuarioId     int                                  not null,
    Expiracion    datetime                             not null,
    Revocado      tinyint(1) default 0                 not null,
    FechaCreacion timestamp  default CURRENT_TIMESTAMP not null,
    constraint Token
        unique (Token),
    constraint RefreshTokens_ibfk_1
        foreign key (UsuarioId) references Usuarios (Id)
            on delete cascade
);

create index UsuarioId
    on RefreshTokens (UsuarioId);

