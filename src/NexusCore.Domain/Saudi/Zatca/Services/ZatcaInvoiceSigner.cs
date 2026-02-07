using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// Signs ZATCA invoice XML documents using X.509 certificates.
/// Implements XML digital signature according to ZATCA Phase 2 requirements.
/// </summary>
public class ZatcaInvoiceSigner : ITransientDependency
{
    /// <summary>
    /// Signs the invoice XML using the provided certificate.
    /// </summary>
    /// <param name="xmlContent">The UBL 2.1 XML invoice document</param>
    /// <param name="certificate">The ZATCA certificate with private key</param>
    /// <param name="certificatePassword">Password to decrypt the private key (if encrypted)</param>
    /// <returns>Signed XML document as string</returns>
    /// <exception cref="BusinessException">Thrown when certificate is invalid or signing fails</exception>
    public string SignInvoice(string xmlContent, ZatcaCertificate certificate, string certificatePassword = "")
    {
        try
        {
            // Load the XML document
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            xmlDoc.LoadXml(xmlContent);

            // Load the certificate with private key
            var x509Cert = LoadCertificateWithPrivateKey(certificate, certificatePassword);

            if (!x509Cert.HasPrivateKey)
            {
                throw new BusinessException(
                    code: "ZATCA:CERTIFICATE_NO_PRIVATE_KEY",
                    message: "The certificate does not contain a private key.");
            }

            // Create the signed XML
            var signedXml = CreateSignedXml(xmlDoc, x509Cert);

            // Find the UBLExtensions element where signature should be inserted
            var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            nsmgr.AddNamespace("inv", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");

            var signatureExtension = xmlDoc.SelectSingleNode(
                "//ext:UBLExtension[ext:ExtensionURI='urn:oasis:names:specification:ubl:dsig:enveloped:xades']/ext:ExtensionContent",
                nsmgr);

            if (signatureExtension == null)
            {
                throw new BusinessException(
                    code: "ZATCA:XML_SIGNATURE_EXTENSION_NOT_FOUND",
                    message: "Could not find the signature extension element in the XML.");
            }

            // Remove the placeholder comment
            signatureExtension.RemoveAll();

            // Import and append the signature
            var signatureNode = xmlDoc.ImportNode(signedXml.GetXml(), true);
            signatureExtension.AppendChild(signatureNode);

            // Return the signed XML
            return xmlDoc.OuterXml;
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BusinessException(
                code: "ZATCA:INVOICE_SIGNING_FAILED",
                message: "Failed to sign the invoice XML.",
                innerException: ex);
        }
    }

    /// <summary>
    /// Loads the X.509 certificate with its private key from the entity.
    /// </summary>
    private X509Certificate2 LoadCertificateWithPrivateKey(ZatcaCertificate certificate, string password)
    {
        try
        {
            // Decode the certificate content (Base64)
            var certBytes = Convert.FromBase64String(certificate.CertificateContent);

            // If we have an encrypted private key, we need to combine cert + key
            // For now, assume the certificate content includes the private key (PFX/P12 format)
            var x509Cert = new X509Certificate2(
                certBytes,
                password,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);

            return x509Cert;
        }
        catch (Exception ex)
        {
            throw new BusinessException(
                code: "ZATCA:CERTIFICATE_LOAD_FAILED",
                message: "Failed to load the certificate. Ensure the certificate format and password are correct.",
                innerException: ex);
        }
    }

    /// <summary>
    /// Creates a SignedXml instance with the invoice signature.
    /// </summary>
    private SignedXml CreateSignedXml(XmlDocument xmlDoc, X509Certificate2 certificate)
    {
        var signedXml = new SignedXml(xmlDoc)
        {
            SigningKey = certificate.GetRSAPrivateKey()
        };

        // Create a reference to the root document element
        var reference = new Reference
        {
            Uri = "", // Empty string signs the whole document
            DigestMethod = SignedXml.XmlDsigSHA256Url
        };

        // Add enveloped signature transform
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());

        // Add the reference to the SignedXml object
        signedXml.AddReference(reference);

        // Add key info (certificate details)
        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(certificate));
        signedXml.KeyInfo = keyInfo;

        // Set signature method to SHA256
        signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

        // Compute the signature
        signedXml.ComputeSignature();

        return signedXml;
    }

    /// <summary>
    /// Computes the SHA-256 hash of the invoice XML (used as invoice hash/ICV).
    /// </summary>
    /// <param name="xmlContent">The XML content to hash</param>
    /// <returns>Base64-encoded SHA-256 hash</returns>
    public string ComputeInvoiceHash(string xmlContent)
    {
        try
        {
            var xmlBytes = Encoding.UTF8.GetBytes(xmlContent);
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(xmlBytes);
            return Convert.ToBase64String(hashBytes);
        }
        catch (Exception ex)
        {
            throw new BusinessException(
                code: "ZATCA:HASH_COMPUTATION_FAILED",
                message: "Failed to compute the invoice hash.",
                innerException: ex);
        }
    }
}
