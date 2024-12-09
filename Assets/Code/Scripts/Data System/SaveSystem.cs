using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string EncryptionKey = "AndreaFrigerio01/02/2002";

    private static string SavePath(string fileName) => Path.Combine(Application.persistentDataPath, fileName + ".json");

    public static void Save<T>(T data, string fileName)
    {
        // Converte l'oggetto in JSON
        string json = JsonUtility.ToJson(data);
        // Cifra il JSON
        string encryptedJson = Encrypt(json, EncryptionKey);
        // Scrive il JSON cifrato nel file
        File.WriteAllText(SavePath(fileName), encryptedJson);
    }

    public static T Load<T>(string fileName) where T : new()
    {
        string path = SavePath(fileName);
        if (File.Exists(path))
        {
            // Legge il contenuto cifrato del file
            string encryptedJson = File.ReadAllText(path);

            // Verifica se il file è vuoto
            if (string.IsNullOrEmpty(encryptedJson))
            {
                //Debug.LogError("The saved file is empty.");
                return new T();
            }

            // Decripta il JSON
            string json = Decrypt(encryptedJson, EncryptionKey);
            // Converte il JSON decriptato nell'oggetto
            return JsonUtility.FromJson<T>(json);
        }
        return new T();
    }

    public static bool Exists(string fileName)
    {
        return File.Exists(SavePath(fileName));
    }

    public static void Delete(string fileName)
    {
        string path = SavePath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static string Encrypt(string plainText, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = new byte[16]; // IV vuoto per CBC
            aes.Padding = PaddingMode.PKCS7; // Specifica esplicitamente il padding

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (StreamWriter writer = new StreamWriter(cs))
                    {
                        writer.Write(plainText);
                    }
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private static string Decrypt(string cipherText, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = new byte[16];
            aes.Padding = PaddingMode.PKCS7; // Specifica esplicitamente il padding

            using (MemoryStream ms = new MemoryStream(cipherBytes))
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(cs))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
