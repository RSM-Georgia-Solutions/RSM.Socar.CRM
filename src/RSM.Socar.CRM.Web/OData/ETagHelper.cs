using System.Text;

namespace RSM.Socar.CRM.Web.OData;

public static class ETagHelper
{
    public static bool TryParse(string? eTag, out byte[] rowVersion)
    {
        rowVersion = Array.Empty<byte>();

        if (string.IsNullOrWhiteSpace(eTag))
            return false;

        // Clean quotes and optional W/ prefix
        eTag = eTag.Trim().Trim('"');
        if (eTag.StartsWith("W/"))
        {
            eTag = eTag.Trim().Trim('"').Replace("W/", "");
            eTag = eTag.Trim('"');
            eTag = eTag.Trim('\\');
            eTag = eTag.Trim('"');
        }

        try
        {
            // 1) RAW RowVersion Base64 (8 bytes)
            var rawBytes = Convert.FromBase64String(eTag);
            if (rawBytes.Length == 8)
            {
                rowVersion = rawBytes;
                return true;
            }

            // 2) OData format → binary'base64'
            var decoded = Encoding.UTF8.GetString(rawBytes);

            if (!decoded.StartsWith("binary'") || !decoded.EndsWith("'"))
                return false;

            var innerBase64 = decoded.Substring(7, decoded.Length - 8);

            rowVersion = Convert.FromBase64String(innerBase64);

            return rowVersion.Length == 8;
        }
        catch
        {
            return false;
        }
    }
}
