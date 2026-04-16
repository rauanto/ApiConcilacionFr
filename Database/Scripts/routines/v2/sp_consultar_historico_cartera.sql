DELIMITER //

CREATE PROCEDURE sp_consultar_historico_cartera(
    IN p_mes INT,
    IN p_anio INT,
    IN p_id_usuario INT,
    IN p_rol VARCHAR(50)
)
BEGIN
    -- 1. Normalizamos el rol para evitar errores por espacios en blanco
    SET p_rol = IFNULL(TRIM(p_rol), '');

    -- 2. Consultamos la tabla histórica
    SELECT
        id,
        fecha_corte,
        fecha_registro,
        ejecutivo_asignado,
        total_clientes,
        total_prestamos,
        monto_colocado,
        saldo_cartera,
        capital_vencido,
        saldo_final,
        porcentaje_calidad
    FROM autentificacion.reporte_cartera_historico

    -- Filtramos por el mes y año solicitados
    WHERE MONTH(fecha_corte) = p_mes
      AND YEAR(fecha_corte) = p_anio

      -- Aplicamos la lógica de roles
      AND (
        p_rol = 'Admin'
            OR
            -- Si no es Admin, cruzamos el ID ingresado con la tabla Usuarios para obtener su nombre
        (p_rol != 'Admin' AND ejecutivo_asignado = (
            SELECT NombreUsuario
            FROM autentificacion.Usuarios
            WHERE Id = p_id_usuario
            LIMIT 1
        ))
        )
    -- Ordenamos de mayor a menor saldo para que los ejecutivos con más cartera salgan primero
    ORDER BY saldo_cartera DESC;

END //
DELIMITER ;

