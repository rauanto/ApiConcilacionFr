-- Tabla para almacenar múltiples archivos (grabaciones y evidencias) por bitácora
-- Reemplaza los campos url_grabacion / url_evidencia de bitacora_gestion (1:N)
CREATE TABLE bitacora.bitacora_archivos (
    id              BIGINT          NOT NULL AUTO_INCREMENT,
    bitacora_id     BIGINT          NOT NULL,
    tipo            ENUM('GRABACION', 'EVIDENCIA') NOT NULL,
    url             VARCHAR(500)    NOT NULL,
    nombre_original VARCHAR(255)    NOT NULL,
    created_at      DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (id),
    INDEX idx_barch_bitacora_id (bitacora_id),
    INDEX idx_barch_tipo        (tipo),

    CONSTRAINT fk_barch_bitacora
        FOREIGN KEY (bitacora_id)
        REFERENCES bitacora_gestion(id)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- ─── Migración de datos existentes ───────────────────────────────────────────
-- Ejecutar SOLO si se quiere conservar los archivos ya registrados en las
-- columnas url_grabacion / url_evidencia de bitacora_gestion.

INSERT INTO bitacora.bitacora_archivos (bitacora_id, tipo, url, nombre_original, created_at)
SELECT id, 'GRABACION', url_grabacion, 'archivo_migrado', created_at
FROM   bitacora.bitacora_gestion
WHERE  url_grabacion IS NOT NULL AND url_grabacion <> '';

INSERT INTO bitacora.bitacora_archivos (bitacora_id, tipo, url, nombre_original, created_at)
SELECT id, 'EVIDENCIA', url_evidencia, 'archivo_migrado', created_at
FROM   bitacora.bitacora_gestion
WHERE  url_evidencia IS NOT NULL AND url_evidencia <> '';

-- Tras verificar la migración, eliminar las columnas antiguas:
-- ALTER TABLE bitacora.bitacora_gestion
--     DROP COLUMN url_grabacion,
--     DROP COLUMN url_evidencia;
