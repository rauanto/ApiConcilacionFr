create
    definer = root@localhost procedure sp_ReporteCarteraPorGrupo(IN p_lista_grupos text)
BEGIN
    SELECT
        *,
        /* ============================================================
           CÁLCULO DE INTERÉS BASE SEGÚN TIPO DE CÁLCULO
           ============================================================ */
        CASE
            /* TIPO 5 */
            WHEN T.PQ_TIPO_CALCULO_INTERES = 5 THEN
                FORMAT(ROUND(T.PQ_IMPORTE * T.PQ_PTOS_TASA_NORMAL * DATEDIFF(T.PQ_FECHA_VENCIMIENTO, T.PQ_FECHA_OPERACION) / (360 * 100) / T.PQ_NUM_AMORTIZACIONES, 2), 2)

            /* TIPO 3 */
            WHEN T.PQ_TIPO_CALCULO_INTERES = 3 THEN
            (
                SELECT ROUND(SUM(Amortiza.A_SALDO_INSOLUTO * PQ_PTOS_TASA_NORMAL * (
                            IF(Amortiza.A_FECHA_VENCIMIENTO < CURDATE(),
                                IF(Amortiza.A_NUMERO = 1,
                                    DATEDIFF(Amortiza.A_FECHA_VENCIMIENTO, Amortiza.A_FECHA_INICIO) + 1,
                                    IF(Amortiza.A_NUMERO = (SELECT MAX(A_NUMERO) FROM Amortiza WHERE PQ_CLAVE = T.PQ_CLAVE),
                                        DATEDIFF(Amortiza.A_FECHA_VENCIMIENTO, Amortiza.A_FECHA_INICIO) - 1,
                                        DATEDIFF(Amortiza.A_FECHA_VENCIMIENTO, Amortiza.A_FECHA_INICIO)
                                    )
                                ),
                                IF(Amortiza.A_FECHA_VENCIMIENTO > CURDATE(),
                                    IF(DATEDIFF(CURDATE(), Amortiza.A_FECHA_INICIO) > 0,
                                        DATEDIFF(CURDATE(), Amortiza.A_FECHA_INICIO),
                                        0
                                    ),
                                    "menor"
                                )
                            )
                        )
                    ) / (360 * 100), 2)
                FROM Prestamo
                INNER JOIN Amortiza ON Amortiza.PQ_CLAVE = Prestamo.PQ_CLAVE
                WHERE Prestamo.PQ_CLAVE = T.PQ_CLAVE
                  AND PQ_FECHA_OPERACION <= CURDATE()
                  AND PQ_FECHA_LIQUIDACION > CURDATE()
            )
            WHEN T.PQ_TIPO_CALCULO_INTERES = 0 THEN "otro calculo"
        END AS INTERES_BASE,

        FORMAT(ROUND(T.Interes_cobrado, 2), 2) AS Mont_int_cobrado,

        CASE
            WHEN DATEDIFF(CURDATE(), T.A_FECHA_VENCIMIENTO) <= 30 THEN '0 a 30'
            WHEN DATEDIFF(CURDATE(), T.A_FECHA_VENCIMIENTO) BETWEEN 31 AND 90 THEN '31 a 90'
            WHEN DATEDIFF(CURDATE(), T.A_FECHA_VENCIMIENTO) >= 91 THEN '91'
            ELSE 'NUEVOS'
        END AS VENCIDA1

    FROM (
        SELECT
            g.P_NOMBRE, Socios.S_CLAVE, Prestamo.PQ_CLAVE, Prestamo.PQ_IMPORTE AS p,
            p.P_NOMBRE sindicato, g.P_NOMBRE AS convenio,
            (SELECT ROUND(SUM(CO_IMPORTE),2) FROM Cobros WHERE pq_clave = Prestamo.PQ_CLAVE AND CO_FECHA_COBRO < "2023-12-31") AS importe,
            (SELECT ROUND(SUM(CO_IMPORTE),2) FROM Cobros WHERE pq_clave = Prestamo.PQ_CLAVE AND CO_FECHA_COBRO > "2023-12-31") AS importe2,
            p.P_CLAVE AS cla_pobla, Socios.S_GRUPO AS G,
            ABS(IFNULL((SELECT SUM(CO_IMPORTE) FROM Cobros WHERE pq_clave = Prestamo.PQ_CLAVE AND CO_FECHA_COBRO <= CURDATE()) - Prestamo.PQ_IMPORTE, PQ_IMPORTE)) AS resultado,
            ABS(IFNULL((SELECT SUM(CO_INTERES_NORMAL) FROM Cobros WHERE pq_clave = Prestamo.PQ_CLAVE AND CO_FECHA_COBRO <= CURDATE() AND CO_FECHA_COBRO > (SELECT MIN(A_FECHA_INICIO) FROM Amortiza WHERE Amortiza.PQ_CLAVE = Prestamo.PQ_CLAVE)), 0)) AS Interes_cobrado,
            Prestamo.PQ_IMPORTE, Prestamo.PQ_PTOS_TASA_NORMAL,
            DATEDIFF(PQ_FECHA_VENCIMIENTO, PQ_FECHA_OPERACION) AS DIAS_OPERADOS,
            MIN(Amortiza.A_NUMERO) AS MIN_AMORTIZA, MAX(Amortiza.A_NUMERO) AS MAX_AMORTIZA,
            PQ_NUM_AMORTIZACIONES, PQ_AMORTIZACION_GRACIA, PQ_SOBRE_TASA_MORATORIA,
            PQ_FECHA_VENCIMIENTO, PQ_FECHA_OPERACION, PQ_TIPO_CALCULO_INTERES,
            Amortiza.A_FECHA_INICIO, Amortiza.A_FECHA_VENCIMIENTO, Amortiza.A_SALDO_INSOLUTO,
            (SELECT COUNT(*) FROM Amortiza WHERE A_FECHA_LIQUIDACION = '9999-12-31' AND A_FECHA_VENCIMIENTO <= CURDATE() AND PQ_CLAVE = Prestamo.PQ_CLAVE) AS AMORTIZACIONES_VENCIDAS,
            (SELECT SUM(A_IMPORTE) FROM Amortiza WHERE A_FECHA_LIQUIDACION = '9999-12-31' AND A_FECHA_VENCIMIENTO <= CURDATE() AND PQ_CLAVE = Prestamo.PQ_CLAVE) AS CAPITAL_VENCIDO,
            (SELECT COUNT(*) FROM Amortiza WHERE A_FECHA_LIQUIDACION = '9999-12-31' AND A_FECHA_VENCIMIENTO > CURDATE() AND PQ_CLAVE = Prestamo.PQ_CLAVE) AS AMORT_X_VENCER,
            (SELECT SUM(ROUND((Amortiza.A_IMPORTE - CO_IMPORTE) * (PQ_PTOS_TASA_NORMAL * PQ_SOBRE_TASA_MORATORIA) / (360 * 100) * DATEDIFF(CURDATE(), A_FECHA_VENCIMIENTO), 2)) FROM Amortiza, Cobros WHERE Amortiza.A_FECHA_LIQUIDACION = '9999-12-31' AND A_FECHA_VENCIMIENTO < CURDATE() AND Amortiza.PQ_CLAVE = Prestamo.PQ_CLAVE AND Cobros.A_NUMERO = Amortiza.A_NUMERO AND Cobros.PQ_CLAVE = Prestamo.PQ_CLAVE) AS calculo_interes_moratorio,
            Socios.S_NOMBRE, Prestamo.PQ_LITIGIO,
            CASE WHEN Prestamo.PQ_DOCUMENTO_RENOVO = 0 THEN 'no castigado' ELSE 'castigado' END AS estatus,
            CASE WHEN Prestamo.PQ_CART_VENCIDA = 1 THEN 'cartera_vencida_contable' ELSE 'sin agregar' END AS cartera_vencida_contable,
            CASE WHEN Prestamo.PQ_LITIGIO = 1 THEN 'demandado' ELSE 'sin demanda' END AS demanda,
            CASE
                WHEN Socios.S_GRUPO IN (990031,990033,990070,990071,990072,990075,990077,990079,990086,990088,990095,990110,990111,990123,990127,990128,990143,990144,990145,990146,990147,990148,990149,990150,990151,990152,990153,990154,990156,990159,990160,990162,990164,990165,990168) THEN 'Neivi'
                WHEN Socios.S_GRUPO IN (990038,990042,990043,990044,990045,990047,990048,990049,990050,990051,990053,990054,990055,990056,990057,990058,990060,990061,990063,990064,990065,990066,990067,990068,990090,990091,990093,990100,990117,990131,990132,990133,990134,990135,990157,990158,990161,990163) THEN 'Julian'
                WHEN Socios.S_GRUPO IN (990037,990040,990069,990099,990103,990109,990112,990121,990122) THEN 'Juan Miguel'
                WHEN Socios.S_GRUPO IN (990035,990036,990073,990074,990078,990082,990083,990087,990089,990092,990096,990113,990114,990115,990116,990118,990119,990140,990155,990166,990167) THEN 'Juan de Dios'
                WHEN Socios.S_GRUPO IN (990034,990039,990094,990102,990120,990125,990126,990136,990137,990138,990139) THEN 'Amairani'
                WHEN Socios.S_GRUPO IN (990101,990000,990028,990029,990030,990032,990041,990046,990052,990059,990062,990080,990081,990097,990124,990129) THEN 'Sin asignacion'
                ELSE 'No identificado'
            END AS ejecutivo_asignado
        FROM Socios
        INNER JOIN Prestamo ON Socios.S_CLAVE = Prestamo.S_CLAVE
        INNER JOIN Amortiza ON Amortiza.PQ_CLAVE = Prestamo.PQ_CLAVE
        INNER JOIN Pobla AS p ON Socios.S_REGION = p.P_CLAVE
        INNER JOIN Pobla AS g ON Socios.S_GRUPO = g.P_CLAVE
        WHERE
            PQ_FECHA_OPERACION <= CURDATE()
            AND A_FECHA_LIQUIDACION > CURDATE()

            /* ============================================================
               AQUÍ ES DONDE APLICAMOS EL FILTRO DINÁMICO
               ============================================================ */
            AND FIND_IN_SET(Socios.S_GRUPO, p_lista_grupos) > 0

        GROUP BY Prestamo.PQ_CLAVE
    ) AS T
    ORDER BY VENCIDA1 ASC;
END;

