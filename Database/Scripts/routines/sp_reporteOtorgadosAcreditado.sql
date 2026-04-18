create
    definer = prosigo@`%` procedure sp_reporte_liquidados_grupo_acreditados(IN p_fecha_inicio date,
                                                                            IN p_rol_usuario varchar(50),
                                                                            IN p_id_usuario int, IN grupo int)
BEGIN
    IF p_rol_usuario = 'Admin' THEN

    SELECT

    Prestamo.PQ_CLAVE AS credito,
    Prestamo.PQ_FECHA_OPERACION,

    Socios.S_CLAVE AS cliente,
    Socios.S_GRUPO,
    Pobla.P_NOMBRE AS nombre_grupo,

    Prestamo.PQ_IMPORTE AS monto_otorgado,
Socios.S_NOMBRE,

               IFNULL(
                       (SELECT COALESCE(u.NombreUsuario, 'Sin asignacion')
                        FROM autentificacion.grupo_asignado ga
                                 LEFT JOIN autentificacion.Usuarios u ON u.Id = ga.usuario_id
                        WHERE ga.S_GRUPO = Socios.S_GRUPO
                        LIMIT 1),
                       'No identificado'
               )                   AS ejecutivo_asignado

FROM Pobla

INNER JOIN Socios
    ON Socios.S_GRUPO = Pobla.P_CLAVE

INNER JOIN Prestamo
    ON Prestamo.S_CLAVE = Socios.S_CLAVE

WHERE
    Socios.S_GRUPO = grupo and
    Pobla.P_CLAVE BETWEEN 990000 AND 994999
    AND Prestamo.PQ_DOCUMENTO_RENOVO = 0
    AND Prestamo.PQ_FECHA_OPERACION >= p_fecha_inicio
    AND Prestamo.PQ_FECHA_OPERACION <> Prestamo.PQ_FECHA_LIQUIDACION

ORDER BY
    Prestamo.PQ_FECHA_OPERACION,
    ejecutivo_asignado,
    Socios.S_GRUPO,
    Prestamo.PQ_CLAVE;


    ELSE
           SELECT

    Prestamo.PQ_CLAVE AS credito,
    Prestamo.PQ_FECHA_OPERACION,

    Socios.S_CLAVE AS cliente,
    Socios.S_GRUPO,
    Pobla.P_NOMBRE AS nombre_grupo,

    Prestamo.PQ_IMPORTE AS monto_otorgado,
Socios.S_NOMBRE,

               U_Asig.NombreUsuario AS ejecutivo_asignado

FROM Pobla

INNER JOIN Socios
    ON Socios.S_GRUPO = Pobla.P_CLAVE

INNER JOIN Prestamo
    ON Prestamo.S_CLAVE = Socios.S_CLAVE
                 INNER JOIN autentificacion.grupo_asignado ga_filtro ON ga_filtro.S_GRUPO = Socios.S_GRUPO
                 INNER JOIN autentificacion.Usuarios U_Asig ON U_Asig.Id = ga_filtro.usuario_id
WHERE
ga_filtro.usuario_id = p_id_usuario AND
    Socios.S_GRUPO = grupo and
    Pobla.P_CLAVE BETWEEN 990000 AND 994999
    AND Prestamo.PQ_DOCUMENTO_RENOVO = 0
    AND Prestamo.PQ_FECHA_OPERACION >= p_fecha_inicio
    AND Prestamo.PQ_FECHA_OPERACION <> Prestamo.PQ_FECHA_LIQUIDACION

ORDER BY
    Prestamo.PQ_FECHA_OPERACION,
    ejecutivo_asignado,
    Socios.S_GRUPO,
    Prestamo.PQ_CLAVE;

    END IF;
    END;

