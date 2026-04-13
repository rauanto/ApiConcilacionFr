create
    definer = prosigo@`%` procedure sp_reporte_liquidados_grupo(IN p_fecha_inicio date, IN p_rol_usuario varchar(50),
                                                                IN p_id_usuario int)
BEGIN
    -- Si el usuario es Administrador, ve todo el universo de grupos definidos
    IF p_rol_usuario = 'Admin' THEN
        SELECT
            Socios.S_GRUPO,
            Pobla.P_NOMBRE AS nombre_grupo,
            Prestamo.PQ_FECHA_LIQUIDACION,
            COUNT(DISTINCT Socios.S_CLAVE) AS total_clientes,
            COUNT(DISTINCT Prestamo.PQ_CLAVE) AS total_prestamos,
            SUM(Prestamo.PQ_IMPORTE) AS monto_liquidado,
            IFNULL(
                (
                    SELECT COALESCE(u.NombreUsuario, 'Sin asignacion')
                    FROM autentificacion.grupo_asignado ga
                    LEFT JOIN autentificacion.Usuarios u ON u.Id = ga.usuario_id
                    WHERE ga.S_GRUPO = Socios.S_GRUPO
                    LIMIT 1
                ),
                'No identificado'
            ) AS ejecutivo_asignado
        FROM Pobla
        INNER JOIN Socios ON Socios.S_GRUPO = Pobla.P_CLAVE
        INNER JOIN Prestamo ON Prestamo.S_CLAVE = Socios.S_CLAVE
        WHERE Pobla.P_CLAVE BETWEEN 990000 AND 994999
          AND Prestamo.PQ_DOCUMENTO_RENOVO = 0
          AND Prestamo.PQ_FECHA_LIQUIDACION <> '9999-12-31'
          AND Prestamo.PQ_FECHA_LIQUIDACION >= p_fecha_inicio
        GROUP BY 1, 2, 3, 7
        ORDER BY 3, 7, 1;

    -- Si no es Admin, filtramos por la tabla de asignación
    ELSE
        SELECT
            Socios.S_GRUPO,
            Pobla.P_NOMBRE AS nombre_grupo,
            Prestamo.PQ_FECHA_LIQUIDACION,
            COUNT(DISTINCT Socios.S_CLAVE) AS total_clientes,
            COUNT(DISTINCT Prestamo.PQ_CLAVE) AS total_prestamos,
            SUM(Prestamo.PQ_IMPORTE) AS monto_liquidado,
            U_Asig.NombreUsuario AS ejecutivo_asignado
        FROM Pobla
        INNER JOIN Socios ON Socios.S_GRUPO = Pobla.P_CLAVE
        INNER JOIN Prestamo ON Prestamo.S_CLAVE = Socios.S_CLAVE
        -- Filtro de seguridad: Solo grupos asignados a este ID de usuario
        INNER JOIN autentificacion.grupo_asignado ga_filtro ON ga_filtro.S_GRUPO = Socios.S_GRUPO
        INNER JOIN autentificacion.Usuarios U_Asig ON U_Asig.Id = ga_filtro.usuario_id
        WHERE ga_filtro.usuario_id = p_id_usuario
          AND Pobla.P_CLAVE BETWEEN 990000 AND 994999
          AND Prestamo.PQ_DOCUMENTO_RENOVO = 0
          AND Prestamo.PQ_FECHA_LIQUIDACION <> '9999-12-31'
          AND Prestamo.PQ_FECHA_LIQUIDACION >= p_fecha_inicio
        GROUP BY 1, 2, 3, 7
        ORDER BY 3, 7, 1;
    END IF;
END;

