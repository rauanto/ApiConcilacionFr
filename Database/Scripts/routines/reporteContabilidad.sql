create
    definer = raul@`%` procedure sp_obtener_saldos_contables(IN p_mes varchar(5))
BEGIN

    DECLARE v_cta_base INT DEFAULT 1018;

    -- Parámetros para el Bloque 1
    DECLARE v_b1_scta1 INT DEFAULT 5;
    DECLARE v_b1_scta2 INT DEFAULT 16;
    DECLARE v_b1_scta3 INT DEFAULT 63;
    DECLARE v_b1_scta4 INT DEFAULT 109;

    -- Parámetros compartidos para los Bloques 2 y 3
    DECLARE v_b23_scta1 INT DEFAULT 5;
    DECLARE v_b23_scta2 INT DEFAULT 16;
    DECLARE v_b23_scta3 INT DEFAULT 62;
    DECLARE v_b23_scta4 INT DEFAULT 104;

    -- Parámetros para el Bloque 4
    DECLARE v_b4_scta1 INT DEFAULT 2;

    IF p_mes NOT IN ('MES0', 'MES1', 'MES2', 'MES3', 'MES4',
                     'MES5', 'MES6', 'MES7', 'MES8', 'MES9', 'MES10', 'MES11', 'MES12') THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El mes debe estar entre MES0 y MES12';
    END IF;

    -- BLOQUE 1: Cuenta con SCTA5 y Etapa 3
    SELECT reporte.*,
           IFNULL(reporte.SALDO - reporte.SALDO_INSOLUTO, 0) AS DIFERENCIA
    FROM (
    SELECT CONCAT(C_CTA, '-', C_SCTA1, '-', C_SCTA2, '-', C_SCTA3, '-', C_SCTA4, '-', C_SCTA5)    AS CUENTA,
           C.C_DESCRIPCION,
           (
               CASE p_mes
                   WHEN 'MES0' THEN C_DEBE_MES0 - C_HABER_MES0
                   WHEN 'MES1' THEN (C_DEBE_MES0 + C_DEBE_MES1) -
                                    (C_HABER_MES0 + C_HABER_MES1)
                   WHEN 'MES2' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2)
                   WHEN 'MES3' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3)
                   WHEN 'MES4' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4)
                   WHEN 'MES5' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5)
                   WHEN 'MES6' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5 + C_DEBE_MES6) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5 + C_HABER_MES6)
                   WHEN 'MES7' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7)
                   WHEN 'MES8' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8)
                   WHEN 'MES9' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9)
                   WHEN 'MES10' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                      C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                      C_DEBE_MES10) -
                                     (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                      C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9 +
                                      C_HABER_MES10)
                   WHEN 'MES11' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                      C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                      C_DEBE_MES10 + C_DEBE_MES11) -
                                     (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                      C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9 +
                                      C_HABER_MES10 + C_HABER_MES11)
                   WHEN 'MES12' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                      C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                      C_DEBE_MES10 + C_DEBE_MES11 + C_HABER_MES12) -
                                     (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                      C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9 +
                                      C_HABER_MES10 + C_HABER_MES11 + C_HABER_MES12)
                   END
               )                                                                               AS SALDO,
           S.S_CLAVE,
           S.S_GRUPO,
           3                                                                                   AS ETAPA,
           P2.P_NOMBRE                                                                         AS GRUPO,
           COALESCE((SELECT SUM(loan_balance.A_SALDO_INSOLUTO)
                     FROM (SELECT creditos_fincrece.Socios.S_CLAVE,
                                  creditos_fincrece.Prestamo.PQ_CLAVE,
                                  MAX(creditos_fincrece.Amortiza.A_SALDO_INSOLUTO) AS A_SALDO_INSOLUTO
                           FROM creditos_fincrece.Socios
                                    INNER JOIN creditos_fincrece.Prestamo
                                               ON creditos_fincrece.Socios.S_CLAVE = creditos_fincrece.Prestamo.S_CLAVE
                                    INNER JOIN creditos_fincrece.Amortiza
                                               ON creditos_fincrece.Amortiza.PQ_CLAVE =
                                                  creditos_fincrece.Prestamo.PQ_CLAVE
                                    INNER JOIN creditos_fincrece.Pobla AS p
                                               ON creditos_fincrece.Socios.S_REGION = p.P_CLAVE
                                    INNER JOIN creditos_fincrece.Pobla AS g
                                               ON creditos_fincrece.Socios.S_GRUPO = g.P_CLAVE
                           WHERE PQ_FECHA_OPERACION <= CURDATE()
                             AND A_FECHA_LIQUIDACION > CURDATE()
                           GROUP BY creditos_fincrece.Socios.S_CLAVE,
                                    creditos_fincrece.Prestamo.PQ_CLAVE) AS loan_balance
                     WHERE loan_balance.S_CLAVE = S.S_CLAVE), 0)                               AS SALDO_INSOLUTO
    FROM Pcontabil_2026.Catalogo C
             LEFT JOIN creditos_fincrece.Socios S ON S.S_CLAVE = C.C_SCTA5
             LEFT JOIN creditos_fincrece.Pobla P2 ON P2.P_CLAVE = S.S_GRUPO
    WHERE C.C_CTA = v_cta_base
      AND C.C_SCTA1 = v_b1_scta1
      AND C.C_SCTA2 = v_b1_scta2
      AND C.C_SCTA3 = v_b1_scta3
      AND C.C_SCTA4 = v_b1_scta4
      AND C.C_SCTA5 > 0
    HAVING SALDO > 0

    UNION ALL

    -- BLOQUE 2: Cuenta con SCTA6 > 1 y Etapa 3
    SELECT CONCAT(C_CTA, '-', C_SCTA1, '-', C_SCTA2, '-', C_SCTA3, '-', C_SCTA4, '-', C_SCTA5, '-', C_SCTA6, '-',
                  C_SCTA7)                                       AS CUENTA,
           C.C_DESCRIPCION,
           (
               CASE p_mes
                   WHEN 'MES0' THEN C_DEBE_MES0 - C_HABER_MES0
                   WHEN 'MES1' THEN (C_DEBE_MES0 + C_DEBE_MES1) -
                                    (C_HABER_MES0 + C_HABER_MES1)
                   WHEN 'MES2' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2)
                   WHEN 'MES3' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3)
                   WHEN 'MES4' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4)
                   WHEN 'MES5' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5)
                   WHEN 'MES6' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5 + C_DEBE_MES6) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5 + C_HABER_MES6)
                   WHEN 'MES7' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7)
                   WHEN 'MES8' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8)
                   WHEN 'MES9' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9)
                   WHEN 'MES10' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                      C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                      C_DEBE_MES10) -
                                     (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                      C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9 +
                                      C_HABER_MES10)
                   WHEN 'MES11' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                      C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                      C_DEBE_MES10 + C_DEBE_MES11) -
                                     (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                      C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9 +
                                      C_HABER_MES10 + C_HABER_MES11)
                   WHEN 'MES12' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                      C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                      C_DEBE_MES10 + C_DEBE_MES11 + C_HABER_MES12) -
                                     (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                      C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9 +
                                      C_HABER_MES10 + C_HABER_MES11 + C_HABER_MES12)
                   END
               )                                                 AS SALDO,
           S.S_CLAVE,
           S.S_GRUPO,
           3                                                     AS ETAPA,
           P2.P_NOMBRE                                           AS GRUPO,
           COALESCE((SELECT SUM(loan_balance.A_SALDO_INSOLUTO)
                     FROM (SELECT creditos_fincrece.Socios.S_CLAVE,
                                  creditos_fincrece.Prestamo.PQ_CLAVE,
                                  MAX(creditos_fincrece.Amortiza.A_SALDO_INSOLUTO) AS A_SALDO_INSOLUTO
                           FROM creditos_fincrece.Socios
                                    INNER JOIN creditos_fincrece.Prestamo
                                               ON creditos_fincrece.Socios.S_CLAVE = creditos_fincrece.Prestamo.S_CLAVE
                                    INNER JOIN creditos_fincrece.Amortiza
                                               ON creditos_fincrece.Amortiza.PQ_CLAVE =
                                                  creditos_fincrece.Prestamo.PQ_CLAVE
                                    INNER JOIN creditos_fincrece.Pobla AS p
                                               ON creditos_fincrece.Socios.S_REGION = p.P_CLAVE
                                    INNER JOIN creditos_fincrece.Pobla AS g
                                               ON creditos_fincrece.Socios.S_GRUPO = g.P_CLAVE
                           WHERE PQ_FECHA_OPERACION <= CURDATE()
                             AND A_FECHA_LIQUIDACION > CURDATE()
                           GROUP BY creditos_fincrece.Socios.S_CLAVE,
                                    creditos_fincrece.Prestamo.PQ_CLAVE) AS loan_balance
                     WHERE loan_balance.S_CLAVE = S.S_CLAVE), 0) AS SALDO_INSOLUTO
    FROM Pcontabil_2026.Catalogo C
             LEFT JOIN creditos_fincrece.Socios S ON S.S_CLAVE = C.C_SCTA6
             LEFT JOIN creditos_fincrece.Pobla P2 ON P2.P_CLAVE = S.S_GRUPO
    WHERE C.C_CTA = v_cta_base
      AND C.C_SCTA1 = v_b23_scta1
      AND C.C_SCTA2 = v_b23_scta2
      AND C.C_SCTA3 = v_b23_scta3
      AND C.C_SCTA4 = v_b23_scta4
      AND C.C_SCTA6 > 1
    HAVING SALDO > 0

    UNION ALL

    -- BLOQUE 3: Cuenta con SCTA7 > 0 y Etapa 3
    SELECT CONCAT(C_CTA, '-', C_SCTA1, '-', C_SCTA2, '-', C_SCTA3, '-', C_SCTA4, '-', C_SCTA5, '-', C_SCTA6, '-',
                  C_SCTA7)                                       AS CUENTA,
           C.C_DESCRIPCION,
           (
               CASE p_mes
                   WHEN 'MES0' THEN C_DEBE_MES0 - C_HABER_MES0
                   WHEN 'MES1' THEN (C_DEBE_MES0 + C_DEBE_MES1) -
                                    (C_HABER_MES0 + C_HABER_MES1)
                   WHEN 'MES2' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2)
                   WHEN 'MES3' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3)
                   WHEN 'MES4' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4)
                   WHEN 'MES5' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5)
                   WHEN 'MES6' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5 + C_DEBE_MES6) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5 + C_HABER_MES6)
                   WHEN 'MES7' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7)
                   WHEN 'MES8' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8)
                   WHEN 'MES9' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                     C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9) -
                                    (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                     C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9)
                   WHEN 'MES10' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                      C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                      C_DEBE_MES10) -
                                     (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                      C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9 +
                                      C_HABER_MES10)
                   WHEN 'MES11' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                      C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                      C_DEBE_MES10 + C_DEBE_MES11) -
                                     (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                      C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9 +
                                      C_HABER_MES10 + C_HABER_MES11)
                   WHEN 'MES12' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                      C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                      C_DEBE_MES10 + C_DEBE_MES11 + C_HABER_MES12) -
                                     (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                      C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9 +
                                      C_HABER_MES10 + C_HABER_MES11 + C_HABER_MES12)
                   END
               )                                                 AS SALDO,
           S.S_CLAVE,
           S.S_GRUPO,
           3                                                     AS ETAPA,
           P2.P_NOMBRE                                           AS GRUPO,
           COALESCE((SELECT SUM(loan_balance.A_SALDO_INSOLUTO)
                     FROM (SELECT creditos_fincrece.Socios.S_CLAVE,
                                  creditos_fincrece.Prestamo.PQ_CLAVE,
                                  MAX(creditos_fincrece.Amortiza.A_SALDO_INSOLUTO) AS A_SALDO_INSOLUTO
                           FROM creditos_fincrece.Socios
                                    INNER JOIN creditos_fincrece.Prestamo
                                               ON creditos_fincrece.Socios.S_CLAVE = creditos_fincrece.Prestamo.S_CLAVE
                                    INNER JOIN creditos_fincrece.Amortiza
                                               ON creditos_fincrece.Amortiza.PQ_CLAVE =
                                                  creditos_fincrece.Prestamo.PQ_CLAVE
                                    INNER JOIN creditos_fincrece.Pobla AS p
                                               ON creditos_fincrece.Socios.S_REGION = p.P_CLAVE
                                    INNER JOIN creditos_fincrece.Pobla AS g
                                               ON creditos_fincrece.Socios.S_GRUPO = g.P_CLAVE
                           WHERE PQ_FECHA_OPERACION <= CURDATE()
                             AND A_FECHA_LIQUIDACION > CURDATE()
                           GROUP BY creditos_fincrece.Socios.S_CLAVE,
                                    creditos_fincrece.Prestamo.PQ_CLAVE) AS loan_balance
                     WHERE loan_balance.S_CLAVE = S.S_CLAVE), 0) AS SALDO_INSOLUTO
    FROM Pcontabil_2026.Catalogo C
             LEFT JOIN creditos_fincrece.Socios S ON S.S_CLAVE = C.C_SCTA7
             LEFT JOIN creditos_fincrece.Pobla P2 ON P2.P_CLAVE = S.S_GRUPO
    WHERE C.C_CTA = v_cta_base
      AND C.C_SCTA1 = v_b23_scta1
      AND C.C_SCTA2 = v_b23_scta2
      AND C.C_SCTA3 = v_b23_scta3
      AND C.C_SCTA4 = v_b23_scta4
      AND C.C_SCTA7 > 0
    HAVING SALDO > 0

    UNION ALL

    -- BLOQUE 4: Cuentas de Etapa 1
    SELECT CASE
               WHEN C.C_SCTA1 = 1 AND C.C_SCTA2 = 1 AND C.C_SCTA3 = 1
                   AND C.C_SCTA4 = 1 AND C.C_SCTA5 = 6 AND C.C_SCTA7 > 1
                   THEN CONCAT(C_CTA, '-', C_SCTA1, '-', C_SCTA2, '-', C_SCTA3, '-',
                               C_SCTA4, '-', C_SCTA5, '-', C_SCTA6, '-', C_SCTA7)
               ELSE CONCAT(C_CTA, '-', C_SCTA1, '-', C_SCTA2, '-', C_SCTA3, '-',
                           C_SCTA4, '-', C_SCTA5, '-', C_SCTA6)
               END                                               AS CUENTA,
           C.C_DESCRIPCION,
           ROUND(
                   CASE p_mes
                       WHEN 'MES0' THEN C_DEBE_MES0 - C_HABER_MES0
                       WHEN 'MES1' THEN (C_DEBE_MES0 + C_DEBE_MES1) -
                                        (C_HABER_MES0 + C_HABER_MES1)
                       WHEN 'MES2' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2) -
                                        (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2)
                       WHEN 'MES3' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3) -
                                        (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3)
                       WHEN 'MES4' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4) -
                                        (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4)
                       WHEN 'MES5' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                         C_DEBE_MES5) -
                                        (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                         C_HABER_MES5)
                       WHEN 'MES6' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                         C_DEBE_MES5 + C_DEBE_MES6) -
                                        (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                         C_HABER_MES5 + C_HABER_MES6)
                       WHEN 'MES7' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                         C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7) -
                                        (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                         C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7)
                       WHEN 'MES8' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                         C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8) -
                                        (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                         C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8)
                       WHEN 'MES9' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                         C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9) -
                                        (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                         C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9)
                       WHEN 'MES10' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                          C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                          C_DEBE_MES10) -
                                         (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                          C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9 +
                                          C_HABER_MES10)
                       WHEN 'MES11' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                          C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                          C_DEBE_MES10 + C_DEBE_MES11) -
                                         (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                          C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9 +
                                          C_HABER_MES10 + C_HABER_MES11)
                       WHEN 'MES12' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                          C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                          C_DEBE_MES10 + C_DEBE_MES11 + C_HABER_MES12) -
                                         (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 +
                                          C_HABER_MES4 + C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 +
                                          C_HABER_MES9 + C_HABER_MES10 + C_HABER_MES11 + C_HABER_MES12)
                       END, 2
           )                                                     AS SALDO,
           S.S_CLAVE,
           S.S_GRUPO,
           1                                                     AS ETAPA,
           P2.P_NOMBRE                                           AS GRUPO,
           COALESCE((SELECT SUM(loan_balance.A_SALDO_INSOLUTO)
                     FROM (SELECT creditos_fincrece.Socios.S_CLAVE,
                                  creditos_fincrece.Prestamo.PQ_CLAVE,
                                  MAX(creditos_fincrece.Amortiza.A_SALDO_INSOLUTO) AS A_SALDO_INSOLUTO
                           FROM creditos_fincrece.Socios
                                    INNER JOIN creditos_fincrece.Prestamo
                                               ON creditos_fincrece.Socios.S_CLAVE = creditos_fincrece.Prestamo.S_CLAVE
                                    INNER JOIN creditos_fincrece.Amortiza
                                               ON creditos_fincrece.Amortiza.PQ_CLAVE =
                                                  creditos_fincrece.Prestamo.PQ_CLAVE
                                    INNER JOIN creditos_fincrece.Pobla AS p
                                               ON creditos_fincrece.Socios.S_REGION = p.P_CLAVE
                                    INNER JOIN creditos_fincrece.Pobla AS g
                                               ON creditos_fincrece.Socios.S_GRUPO = g.P_CLAVE
                           WHERE PQ_FECHA_OPERACION <= CURDATE()
                             AND A_FECHA_LIQUIDACION > CURDATE()
                           GROUP BY creditos_fincrece.Socios.S_CLAVE,
                                    creditos_fincrece.Prestamo.PQ_CLAVE) AS loan_balance
                     WHERE loan_balance.S_CLAVE = S.S_CLAVE), 0) AS SALDO_INSOLUTO
    FROM Pcontabil_2026.Catalogo C
             LEFT JOIN creditos_fincrece.Socios S
                       ON S.S_CLAVE = CASE
                                          WHEN C.C_SCTA1 = 1 AND C.C_SCTA2 = 1 AND C.C_SCTA3 = 1
                                              AND C.C_SCTA4 = 1 AND C.C_SCTA5 = 6 AND C.C_SCTA7 > 1
                                              THEN C.C_SCTA7
                                          ELSE C.C_SCTA6
                           END
             LEFT JOIN creditos_fincrece.Pobla P2 ON P2.P_CLAVE = S.S_GRUPO
    WHERE C.C_CTA = 1018
      AND (
        (C.C_SCTA1 = 1 AND C.C_SCTA2 = 1 AND C.C_SCTA3 = 1
            AND C.C_SCTA4 = 1 AND C.C_SCTA5 = 6 AND C.C_SCTA7 > 1)
            OR
        (C.C_SCTA1 = 1 AND C.C_SCTA2 = 1 AND C.C_SCTA3 = 1
            AND C.C_SCTA4 = 1 AND C.C_SCTA5 IN (9, 10, 12) AND C.C_SCTA6 > 1)
            OR
        (C.C_SCTA1 = 1 AND C.C_SCTA2 = 1 AND C.C_SCTA3 = 1
            AND C.C_SCTA4 IN (4, 5) AND C.C_SCTA5 = 8 AND C.C_SCTA6 > 1)
            OR
        (C.C_SCTA1 = 1 AND C.C_SCTA2 = 1 AND C.C_SCTA3 = 3
            AND C.C_SCTA4 = 2 AND C.C_SCTA5 = 8 AND C.C_SCTA6 > 1)
            OR
        (C.C_SCTA1 = 1 AND C.C_SCTA2 = 1 AND C.C_SCTA3 = 4
            AND C.C_SCTA4 = 1 AND C.C_SCTA5 IN (1, 8) AND C.C_SCTA6 > 1)
            OR
        (C.C_SCTA1 = 1 AND C.C_SCTA2 = 1 AND C.C_SCTA3 = 7
            AND C.C_SCTA4 = 1 AND C.C_SCTA5 = 1 AND C.C_SCTA6 > 1)
        )
    HAVING SALDO != 0

    UNION ALL

    -- BLOQUE 5: Cuenta con SCTA1 = @B4_SCTA1, Etapa 2 y Redondeo a 2 decimales
    SELECT CONCAT(C_CTA, '-', C_SCTA1, '-', C_SCTA2, '-', C_SCTA3, '-', C_SCTA4, '-', C_SCTA5, '-', C_SCTA6, '-',
                  C_SCTA7)                                       AS CUENTA,
           C.C_DESCRIPCION,
           ROUND(
                   (
                       CASE p_mes
                           WHEN 'MES0' THEN C_DEBE_MES0 - C_HABER_MES0
                           WHEN 'MES1' THEN (C_DEBE_MES0 + C_DEBE_MES1) -
                                            (C_HABER_MES0 + C_HABER_MES1)
                           WHEN 'MES2' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2) -
                                            (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2)
                           WHEN 'MES3' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3) -
                                            (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3)
                           WHEN 'MES4' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4) -
                                            (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4)
                           WHEN 'MES5' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                             C_DEBE_MES5) -
                                            (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                             C_HABER_MES5)
                           WHEN 'MES6' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                             C_DEBE_MES5 + C_DEBE_MES6) -
                                            (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                             C_HABER_MES5 + C_HABER_MES6)
                           WHEN 'MES7' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                             C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7) -
                                            (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                             C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7)
                           WHEN 'MES8' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                             C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8) -
                                            (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                             C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8)
                           WHEN 'MES9' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                             C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9) -
                                            (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                             C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9)
                           WHEN 'MES10' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                              C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                              C_DEBE_MES10) -
                                             (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                              C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9 +
                                              C_HABER_MES10)
                           WHEN 'MES11' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                              C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                              C_DEBE_MES10 + C_DEBE_MES11) -
                                             (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                              C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9 +
                                              C_HABER_MES10 + C_HABER_MES11)
                           WHEN 'MES12' THEN (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                                              C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9 +
                                              C_DEBE_MES10 + C_DEBE_MES11 + C_HABER_MES12) -
                                             (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                                              C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9 +
                                              C_HABER_MES10 + C_HABER_MES11 + C_HABER_MES12)
                           END
                       ), 2
           )                                                     AS SALDO,
           S.S_CLAVE,
           S.S_GRUPO,
           2                                                     AS ETAPA,
           P2.P_NOMBRE                                           AS GRUPO,
           COALESCE((SELECT SUM(loan_balance.A_SALDO_INSOLUTO)
                     FROM (SELECT creditos_fincrece.Socios.S_CLAVE,
                                  creditos_fincrece.Prestamo.PQ_CLAVE,
                                  MAX(creditos_fincrece.Amortiza.A_SALDO_INSOLUTO) AS A_SALDO_INSOLUTO
                           FROM creditos_fincrece.Socios
                                    INNER JOIN creditos_fincrece.Prestamo
                                               ON creditos_fincrece.Socios.S_CLAVE = creditos_fincrece.Prestamo.S_CLAVE
                                    INNER JOIN creditos_fincrece.Amortiza
                                               ON creditos_fincrece.Amortiza.PQ_CLAVE =
                                                  creditos_fincrece.Prestamo.PQ_CLAVE
                                    INNER JOIN creditos_fincrece.Pobla AS p
                                               ON creditos_fincrece.Socios.S_REGION = p.P_CLAVE
                                    INNER JOIN creditos_fincrece.Pobla AS g
                                               ON creditos_fincrece.Socios.S_GRUPO = g.P_CLAVE
                           WHERE PQ_FECHA_OPERACION <= CURDATE()
                             AND A_FECHA_LIQUIDACION > CURDATE()
                           GROUP BY creditos_fincrece.Socios.S_CLAVE,
                                    creditos_fincrece.Prestamo.PQ_CLAVE) AS loan_balance
                     WHERE loan_balance.S_CLAVE = S.S_CLAVE), 0) AS SALDO_INSOLUTO
    FROM Pcontabil_2026.Catalogo C
             LEFT JOIN creditos_fincrece.Socios S ON S.S_CLAVE = C.C_SCTA7
             LEFT JOIN creditos_fincrece.Pobla P2 ON P2.P_CLAVE = S.S_GRUPO
    WHERE C.C_CTA = v_cta_base
      AND C.C_SCTA1 = v_b4_scta1
      AND C.C_SCTA7 > 0
    HAVING SALDO != 0
    ) AS reporte;

END;
