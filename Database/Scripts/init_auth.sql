CREATE SCHEMA IF NOT EXISTS autentificacion;

USE autentificacion;

-- auto-generated definition
create table Usuarios
(
    Id            int auto_increment
        primary key,
    NombreUsuario varchar(50)                          not null,
    Correo        varchar(100)                         not null,
    PasswordHash  varchar(255)                         not null,
    Activo        tinyint(1) default 1                 not null,
    FechaCreacion timestamp  default CURRENT_TIMESTAMP not null,
    constraint Correo
        unique (Correo),
    constraint NombreUsuario
        unique (NombreUsuario)
);



ON DUPLICATE KEY UPDATE Id=Id;

-- ── Nueva tabla grupo_asignado ─────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS grupo_asignado (
    S_GRUPO INT PRIMARY KEY,       -- ID manual, no consecutivo
    nombre_grupo VARCHAR(150) NOT NULL,
    usuario_id INT ,               -- Relación con la tabla Usuarios
    FOREIGN KEY (usuario_id) REFERENCES Usuarios(Id) ON DELETE CASCADE
);




USE autentificacion;
CREATE TABLE IF NOT EXISTS AuditoriaRegistros (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Entidad VARCHAR(100),
    EntidadId VARCHAR(50),
    Operacion VARCHAR(20),
    Data TEXT, 
    UsuarioResponsable VARCHAR(100),
    FechaEvento TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE reporte_cartera_historico (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fecha_corte DATE NOT NULL,              
    fecha_registro DATETIME DEFAULT NOW(),  
    ejecutivo_asignado VARCHAR(100),
    total_clientes INT,
    total_prestamos INT,
    monto_colocado DECIMAL(18,2),
    saldo_cartera DECIMAL(18,2),
    capital_vencido DECIMAL(18,2),
    saldo_final DECIMAL(18,2),
    porcentaje_calidad DECIMAL(5,2),
    tipo INT
);


CREATE EVENT e_reporte_mensual_limpio
    ON SCHEDULE EVERY 1 MONTH
        STARTS LAST_DAY(CURDATE()) + INTERVAL 23 HOUR + INTERVAL 59 MINUTE
    DO
    CALL sp_job_generar_historico_limpio();