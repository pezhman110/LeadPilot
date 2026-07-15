namespace LeadPilot.Application.Contacts;

public sealed record ProtectedContact(string HmacHash, string MaskedValue, byte[] Ciphertext, byte[] Nonce, byte[] AuthenticationTag);