DELIMITER //

DROP PROCEDURE IF EXISTS `sp_insertar_bitacora_baja`//

CREATE PROCEDURE `sp_insertar_bitacora_baja`(
    IN p_credito_id BIGINT,
    IN p_cliente_id BIGINT,
    IN p_monto_otorgado DOUBLE,
    IN p_saldo_capital DOUBLE,
    IN p_saldo_insoluto DOUBLE,
    IN p_capital_vencido DOUBLE,
    IN p_amorticaciones_vencidas INT,
    IN p_interes_cobrado DOUBLE,
    IN p_dias_vencidos VARCHAR(10),
    IN p_cartera_vencida_contable VARCHAR(35),
    IN p_demanda VARCHAR(35),
    IN p_estatus VARCHAR(35),
    IN p_fecha_alta DATETIME,
    IN p_fecha_vencimiento DATETIME,
    IN p_inicio_cobranza DATETIME,
    IN p_ult_cobranza DATETIME,
    IN p_gestor_id BIGINT,
    IN p_grupo_id INT,
    IN p_obervaciones TEXT,
    IN p_nombre_cliente VARCHAR(100),
    IN p_sindicato VARCHAR(100),
    IN p_baja INT
)
BEGIN
    INSERT INTO bitacora_bajas (
        credito_id, cliente_id, monto_otorgado, saldo_capital, saldo_insoluto, 
        capital_vencido, amorticaciones_vencidas, interes_cobrado, dias_vencidos, 
        cartera_vencida_contable, demanda, estatus, fecha_alta, fecha_vencimiento, 
        inicio_cobranza, ult_cobranza, gestor_id, grupo_id, obervaciones, baja, NombreCliente, Sindicato
    ) VALUES (
        p_credito_id, p_cliente_id, p_monto_otorgado, p_saldo_capital, p_saldo_insoluto, 
        p_capital_vencido, p_amorticaciones_vencidas, p_interes_cobrado, p_dias_vencidos, 
        p_cartera_vencida_contable, p_demanda, p_estatus, p_fecha_alta, p_fecha_vencimiento, 
        p_inicio_cobranza, p_ult_cobranza, p_gestor_id, p_grupo_id, p_obervaciones, p_baja, p_nombre_cliente, p_sindicato
    );

    SELECT LAST_INSERT_ID() AS Id;
END//


DROP PROCEDURE IF EXISTS `sp_listar_bitacora_bajas`//

CREATE DEFINER = prosigo@`%` PROCEDURE `sp_listar_bitacora_bajas`(
    IN p_limit INT,
    IN p_offset INT,
    IN p_lista_grupos TEXT
)
BEGIN
    -- Manejo básico de paginación
    SET @limit_val = IFNULL(p_limit, 100);
    SET @offset_val = IFNULL(p_offset, 0);

    SELECT 
        id, credito_id, cliente_id, monto_otorgado, saldo_capital, saldo_insoluto, 
        capital_vencido, amorticaciones_vencidas, interes_cobrado, dias_vencidos, 
        cartera_vencida_contable, demanda, estatus, fecha_alta, fecha_vencimiento, 
        inicio_cobranza, ult_cobranza, gestor_id, created_at, grupo_id, obervaciones, baja, NombreCliente, Sindicato
    FROM bitacora_bajas
    WHERE 1=1
      AND (p_lista_grupos IS NULL OR p_lista_grupos = '' OR FIND_IN_SET(grupo_id, p_lista_grupos) > 0)
    ORDER BY id DESC
    LIMIT @limit_val OFFSET @offset_val;

    -- Total count para paginacion
    SELECT COUNT(*) AS TotalRecords
    FROM bitacora_bajas
    WHERE 1=1
      AND (p_lista_grupos IS NULL OR p_lista_grupos = '' OR FIND_IN_SET(grupo_id, p_lista_grupos) > 0);
END//

DROP PROCEDURE IF EXISTS `sp_actualizar_bitacora_baja`//

CREATE PROCEDURE `sp_actualizar_bitacora_baja`(
    IN p_credito_id BIGINT,
    IN p_baja INT,
    IN p_obervaciones TEXT
)
BEGIN
    UPDATE bitacora_bajas
    SET baja        = p_baja,
        obervaciones = p_obervaciones
    WHERE credito_id = p_credito_id;

    SELECT ROW_COUNT() AS RowsAffected;

    SELECT
        id, credito_id, cliente_id, monto_otorgado, saldo_capital, saldo_insoluto,
        capital_vencido, amorticaciones_vencidas, interes_cobrado, dias_vencidos,
        cartera_vencida_contable, demanda, estatus, fecha_alta, fecha_vencimiento,
        inicio_cobranza, ult_cobranza, gestor_id, created_at, grupo_id, obervaciones, baja, NombreCliente, Sindicato
    FROM bitacora_bajas
    WHERE credito_id = p_credito_id;
END//

DELIMITER ;
