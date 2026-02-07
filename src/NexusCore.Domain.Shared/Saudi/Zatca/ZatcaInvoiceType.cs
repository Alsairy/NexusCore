namespace NexusCore.Saudi.Zatca;

public enum ZatcaInvoiceType
{
    /// <summary>
    /// Standard tax invoice (B2B)
    /// </summary>
    Standard = 388,

    /// <summary>
    /// Simplified tax invoice (B2C)
    /// </summary>
    Simplified = 383,

    /// <summary>
    /// Standard debit note
    /// </summary>
    StandardDebitNote = 389,

    /// <summary>
    /// Standard credit note
    /// </summary>
    StandardCreditNote = 381,

    /// <summary>
    /// Simplified debit note
    /// </summary>
    SimplifiedDebitNote = 390,

    /// <summary>
    /// Simplified credit note
    /// </summary>
    SimplifiedCreditNote = 382
}
