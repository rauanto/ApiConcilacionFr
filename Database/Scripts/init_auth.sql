CREATE SCHEMA IF NOT EXISTS autentificacion;

USE autentificacion;

CREATE TABLE IF NOT EXISTS Usuarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    NombreUsuario VARCHAR(50) NOT NULL UNIQUE,
    Correo VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Rol VARCHAR(20) NOT NULL DEFAULT 'Usuario',
    Activo BOOLEAN NOT NULL DEFAULT TRUE,
    FechaCreacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Insertar un usuario administrador por defecto
-- PasswordHash para "admin123" generado con BCrypt de C# (es un ejemplo, debería cambiarse al loguear/registrar).
-- Un hash BCrypt valido generico para admin123 (Work Factor 11): 
-- $2a$11$F6n1q7j.1mQO2d27.D2.VumU18V//z4/Lw9pT0T1Z1.xX/Wl82cHO  
INSERT INTO Usuarios (NombreUsuario, Correo, PasswordHash, Rol)
VALUES ('admin', 'admin@fincrece.com', '$2a$11$F6n1q7j.1mQO2d27.D2.VumU18V//z4/Lw9pT0T1Z1.xX/Wl82cHO', 'Admin')
ON DUPLICATE KEY UPDATE Id=Id;

-- ── Nueva tabla grupo_asignado ─────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS grupo_asignado (
    S_GRUPO INT PRIMARY KEY,       -- ID manual, no consecutivo
    nombre_grupo VARCHAR(150) NOT NULL,
    usuario_id INT NOT NULL,               -- Relación con la tabla Usuarios
    FOREIGN KEY (usuario_id) REFERENCES Usuarios(Id) ON DELETE CASCADE
);




USE autentificacion;
CREATE TABLE IF NOT EXISTS AuditoriaRegistros (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Entidad VARCHAR(100),
    EntidadId VARCHAR(50),
    Operacion VARCHAR(20),
    Data JSON, -- O el nombre que configuraste en JsonColumnName
    UsuarioResponsable VARCHAR(100),
    FechaEvento TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE reporte_cartera_historico (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fecha_corte DATE NOT NULL,              -- El último día del mes que se reporta
    fecha_registro DATETIME DEFAULT NOW(),  -- Cuándo se ejecutó el proceso
    ejecutivo_asignado VARCHAR(100),
    total_clientes INT,
    total_prestamos INT,
    monto_colocado DECIMAL(18,2),
    saldo_cartera DECIMAL(18,2),
    capital_vencido DECIMAL(18,2),
    saldo_final DECIMAL(18,2),
    porcentaje_calidad DECIMAL(5,2)
);
