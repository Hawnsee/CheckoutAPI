# Registro de Deuda Técnica (Technical Debt Ledger)

## 1. Bloqueo Huérfano en Patrón de Idempotencia (Stale/Zombie Lock)

**Fecha de registro:** 11 de Agosto de 2026
**Componente afectado:** `CheckoutAPI.Application.Commands.IdentifiedCommandHandler`
**Impacto:** Alto (Bloqueo permanente de usuario en caso de desastre).

### Descripción del Problema
El patrón de idempotencia actual utiliza un bloqueo optimista de dos fases. Al recibir un `CheckoutCommand`, el decorador inserta un registro en la tabla `IdempotentRequest` con estado `CREATED`. 
Si el sistema sufre una caída catastrófica (ej. *OOMKilled*, corte eléctrico, pánico del kernel) mientras el `CheckoutCommandHandler` interno espera la respuesta de un servicio externo, el hilo de ejecución muere y el estado del registro jamás se actualiza a `COMPLETED` o `FAILED`. 
El registro queda petrificado en `CREATED`.

### Vector de Fallo del Usuario
Si el cliente asume que hubo un error de red y reintenta su compra con el mismo `Id`, el decorador intentará insertar el registro nuevamente. Esto provocará una `UniqueConstraintException`. El bloque `catch` recuperará el registro zombi, leerá su estado (`CREATED`) y devolverá erróneamente un resultado `DUPLICATED`. El usuario quedará atrapado en un punto muerto sin poder procesar su compra.

### Solución Arquitectónica Propuesta (Fase de Resolución)
Transformar el bloqueo rígido en un **Lease Lock (Bloqueo por Arrendamiento) con TTL (Time-To-Live)**.

1. **Modificación del Esquema (Base de datos):** Añadir una propiedad `CreatedAt` (Timestamp) a la entidad `IdempotentRequest`.
2. **Modificación del Dominio (Lógica):** 
   Dentro del bloque `catch (UniqueConstraintException)`, recuperar el registro de la base de datos.
   * Regla: Si `Status == CREATED` **Y** `(UtcNow - CreatedAt) > X minutos` (TTL excedido).
   * Acción: Reclamar el bloqueo (considerar el proceso anterior muerto), actualizar `CreatedAt` a la hora actual y continuar con la ejecución del `_mediator.Send()` en lugar de devolver `DUPLICATED`.