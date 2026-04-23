DELIMITER //

CREATE PROCEDURE sp_consultar_historico_cartera_grupo(IN p_mes int, IN p_anio int, IN p_id_usuario int,
                                                                   IN p_rol varchar(50), IN tipo_reporte int)
BEGIN
    SELECT
        id, fecha_corte, fecha_registro, ejecutivo_asignado,
        total_clientes, total_prestamos, monto_colocado,
        saldo_cartera, capital_vencido, saldo_final,
        porcentaje_calidad, tipo, S_GRUPO, nombre_grupo
    FROM autentificacion.reporte_cartera_historico_grupo
 WHERE
        tipo=tipo_reporte and
        MONTH(fecha_corte) = p_mes
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
    ORDER BY fecha_corte DESC, S_GRUPO ASC;
END //

DELIMITER ;