create
    definer = prosigo@`%` procedure sp_reporte_otorgados_grupo(IN p_fecha_inicio date, IN p_rol_usuario varchar(50),
                                                               IN p_id_usuario int)
BEGIN
    -- Si el usuario es Administrador, ve todo el universo de grupos definidos
    IF p_rol_usuario = 'Admin' THEN
        SELECT Socios.S_GRUPO,
               Pobla.P_NOMBRE                    AS nombre_grupo,

               Prestamo.PQ_FECHA_OPERACION,

               COUNT(DISTINCT Socios.S_CLAVE)    AS total_clientes,

               COUNT(DISTINCT Prestamo.PQ_CLAVE) AS total_prestamos,

               SUM(Prestamo.PQ_IMPORTE)          AS monto_otorgado,

               IFNULL(
                       (SELECT COALESCE(u.NombreUsuario, 'Sin asignacion')
                        FROM autentificacion.grupo_asignado ga
                                 LEFT JOIN autentificacion.Usuarios u ON u.Id = ga.usuario_id
                        WHERE ga.S_GRUPO = Socios.S_GRUPO
                        LIMIT 1),
                       'No identificado'
               )                                 AS ejecutivo_asignado

        FROM Pobla

                 INNER JOIN Socios
                            ON Socios.S_GRUPO = Pobla.P_CLAVE

                 INNER JOIN Prestamo
                            ON Prestamo.S_CLAVE = Socios.S_CLAVE

        WHERE Pobla.P_CLAVE BETWEEN 990000 AND 994999
          AND Prestamo.PQ_DOCUMENTO_RENOVO = 0
          AND Prestamo.PQ_FECHA_OPERACION >= '2026-02-28'
          AND Prestamo.PQ_FECHA_OPERACION <> Prestamo.PQ_FECHA_LIQUIDACION
        GROUP BY Socios.S_GRUPO,
                 Pobla.P_NOMBRE,
                 ejecutivo_asignado

        ORDER BY
#     Prestamo.PQ_FECHA_OPERACION,
        ejecutivo_asignado,
        Socios.S_GRUPO;


        -- Si no es Admin, filtramos por la tabla de asignación
    ELSE
        SELECT
                Socios.S_GRUPO,
                Pobla.P_NOMBRE AS nombre_grupo,

                Prestamo.PQ_FECHA_OPERACION,

                COUNT(DISTINCT Socios.S_CLAVE) AS total_clientes,

                COUNT(DISTINCT Prestamo.PQ_CLAVE) AS total_prestamos,

                SUM(Prestamo.PQ_IMPORTE) AS monto_otorgado,
               U_Asig.NombreUsuario AS ejecutivo_asignado
        FROM Pobla

                 INNER JOIN Socios
                            ON Socios.S_GRUPO = Pobla.P_CLAVE

                 INNER JOIN Prestamo
                            ON Prestamo.S_CLAVE = Socios.S_CLAVE
                 INNER JOIN autentificacion.grupo_asignado ga_filtro ON ga_filtro.S_GRUPO = Socios.S_GRUPO
                 INNER JOIN autentificacion.Usuarios U_Asig ON U_Asig.Id = ga_filtro.usuario_id
        WHERE ga_filtro.usuario_id = p_id_usuario and
    Pobla.P_CLAVE BETWEEN 990000 AND 994999
    AND Prestamo.PQ_DOCUMENTO_RENOVO = 0
    AND Prestamo.PQ_FECHA_OPERACION >= '2026-02-28'
AND Prestamo.PQ_FECHA_OPERACION<>Prestamo.PQ_FECHA_LIQUIDACION
GROUP BY
    Socios.S_GRUPO,
    Pobla.P_NOMBRE,
    ejecutivo_asignado

ORDER BY
#     Prestamo.PQ_FECHA_OPERACION,
    ejecutivo_asignado,
    Socios.S_GRUPO;

    END IF;
END;

