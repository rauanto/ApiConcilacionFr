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
