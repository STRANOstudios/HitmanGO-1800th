using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SaveSystem
{
    // Encryption key
    private static readonly string EncryptionKey = "AndreaFrigerio01"; //16 bytes/characters

    // Save path
    private static string SavePath(string fileName) => Path.Combine(Application.persistentDataPath, fileName + ".json");

    /// <summary>
    /// Save the file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="fileName"></param>
    public static void Save<T>(T data, string fileName)
    {
        string json = JsonUtility.ToJson(data);

        string encryptedJson = Encrypt(json, EncryptionKey);

        File.WriteAllText(SavePath(fileName), encryptedJson);
    }

    /// <summary>
    /// Load the file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static T Load<T>(string fileName) where T : new()
    {
        string path = SavePath(fileName);
        if (File.Exists(path))
        {
            string encryptedJson = File.ReadAllText(path);

            if (string.IsNullOrEmpty(encryptedJson)) return new T();

            string json = Decrypt(encryptedJson, EncryptionKey);

            return JsonUtility.FromJson<T>(json);
        }
        return new T();
    }

    /// <summary>
    /// Check if the file exists
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static bool Exists(string fileName)
    {
        return File.Exists(SavePath(fileName));
    }

    /// <summary>
    /// Delete the file
    /// </summary>
    /// <param name="fileName"></param>
    public static void Delete(string fileName)
    {
        string path = SavePath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Encrypts a plain text using AES-128 algorithm.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <param name="key">The encryption key. It must be exactly 16 bytes/characters long.</param>
    /// <returns>The encrypted text in base64 format.</returns>
    /// <exception cref="ArgumentException">Thrown if the key is not exactly 16 bytes/characters long.</exception>
    private static string Encrypt(string plainText, string key)
    {
        // Convert the key to bytes using UTF8 encoding
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);

        // Check if the key is exactly 16 bytes/characters long
        if (keyBytes.Length != 16)
        {
            throw new ArgumentException("The key must be exactly 16 bytes/characters long.");
        }

        // Create a new instance of the AES algorithm
        using Aes aes = Aes.Create();

        // Set the encryption key
        aes.Key = keyBytes;

        // Generate a new IV (Initialization Vector)
        aes.GenerateIV();

        // Set the padding mode to PKCS7
        aes.Padding = PaddingMode.PKCS7;

        // Create a new MemoryStream
        using MemoryStream ms = new();

        // Write the IV to the MemoryStream
        ms.Write(aes.IV, 0, aes.IV.Length);

        // Create a new CryptoStream using the MemoryStream and the AES encryptor
        using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            // Create a new StreamWriter using the CryptoStream
            using StreamWriter writer = new(cs);

            // Write the plain text to the StreamWriter
            writer.Write(plainText);
        }

        // Return the encrypted text in base64 format
        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// Decrypts a cipher text using AES-128 algorithm.
    /// </summary>
    /// <param name="cipherText">The cipher text to decrypt.</param>
    /// <param name="key">The decryption key. It must be exactly 16 bytes/characters long.</param>
    /// <returns>The decrypted text.</returns>
    /// <exception cref="ArgumentException">Thrown if the key is not exactly 16 bytes/characters long.</exception>
    private static string Decrypt(string cipherText, string key)
    {
        // Convert the cipher text to bytes using Base64 decoding
        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        // Extract the initialization vector (IV) from the cipher bytes
        byte[] iv = new byte[16];
        Array.Copy(cipherBytes, iv, iv.Length);

        // Extract the encrypted data from the cipher bytes
        byte[] encryptedData = new byte[cipherBytes.Length - iv.Length];
        Array.Copy(cipherBytes, iv.Length, encryptedData, 0, encryptedData.Length);

        // Convert the key to bytes using UTF8 encoding
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        if (keyBytes.Length != 16)
        {
            // Throw an exception if the key is not exactly 16 bytes/characters long
            throw new ArgumentException("The key must be exactly 16 bytes/characters long.");
        }

        // Create a new instance of the AES algorithm
        using Aes aes = Aes.Create();
        aes.Key = keyBytes; // Set the decryption key
        aes.IV = iv; // Set the initialization vector
        aes.Padding = PaddingMode.PKCS7; // Set the padding mode to PKCS7

        // Create a new MemoryStream to hold the encrypted data
        using MemoryStream ms = new(encryptedData);
        // Create a new CryptoStream to decrypt the data
        using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        // Create a new StreamReader to read the decrypted data
        using StreamReader reader = new(cs);
        // Return the decrypted text
        return reader.ReadToEnd();
    }
}
