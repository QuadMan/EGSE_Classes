﻿//-----------------------------------------------------------------------
// <copyright file="EmbeddedAssembly.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Utilites
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography;

    /// <summary>
    /// A class for loading Embedded Assembly
    /// </summary>
    public class EmbeddedAssembly
    {
        /// <summary>
        /// The dic
        /// </summary>
        private static Dictionary<string, Assembly> dic = null;

        /// <summary>
        /// Load Assembly, DLL from Embedded Resources into memory.
        /// </summary>
        /// <param name="embeddedResource">Embedded Resource string. Example: WindowsFormsApplication1.SomeTools.dll</param>
        /// <param name="fileName">File Name. Example: SomeTools.dll</param>
        public static void Load(string embeddedResource, string fileName)
        {
            if (null == dic)
            {
                dic = new Dictionary<string, Assembly>();
            }

            byte[] ba = null;
            Assembly asm = null;
            Assembly curAsm = Assembly.GetExecutingAssembly();

            using (Stream stm = curAsm.GetManifestResourceStream(embeddedResource))
            {
                if (null == stm)
                {
                    throw new Exception(embeddedResource + " is not found in Embedded Resources.");
                }

                ba = new byte[(int)stm.Length];
                stm.Read(ba, 0, (int)stm.Length);
                try
                {
                    asm = Assembly.Load(ba);
                    dic.Add(asm.FullName, asm);
                    return;
                }
                catch
                {
                }
            }

            bool fileOk = false;
            string tempFile = string.Empty;

            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                // Get the hash value from embedded DLL/assembly
                string fileHash = BitConverter.ToString(sha1.ComputeHash(ba)).Replace("-", string.Empty);

                // Define the temporary storage location of the DLL/assembly
                tempFile = Path.GetTempPath() + fileName;

                // Determines whether the DLL/assembly is existed or not
                if (File.Exists(tempFile))
                {
                    // Get the hash value of the existed file
                    byte[] bb = File.ReadAllBytes(tempFile);
                    string fileHash2 = BitConverter.ToString(sha1.ComputeHash(bb)).Replace("-", string.Empty);

                    // Compare the existed DLL/assembly with the Embedded DLL/assembly
                    if (fileHash == fileHash2)
                    {
                        // Same file
                        fileOk = true;
                    }
                    else
                    {
                        // Not same
                        fileOk = false;
                    }
                }
                else
                {
                    // The DLL/assembly is not existed yet
                    fileOk = false;
                }
            }

            // Create the file on disk
            if (!fileOk)
            {
                System.IO.File.WriteAllBytes(tempFile, ba);
            }

            // Load it into memory
            asm = Assembly.LoadFile(tempFile);

            // Add the loaded DLL/assembly into dictionary
            dic.Add(asm.FullName, asm);
        }

        /// <summary>
        /// Retrieve specific loaded DLL/assembly from memory
        /// </summary>
        /// <param name="assemblyFullName">Full name of the assembly.</param>
        /// <returns>Экземпляр сборки, содержащей ресурсы.</returns>
        public static Assembly Get(string assemblyFullName)
        {
            if ((null == dic) || (0 == dic.Count))
            {
                return null;
            }

            if (dic.ContainsKey(assemblyFullName))
            {
                return dic[assemblyFullName];
            }

            return null;

            // Don't throw Exception if the dictionary does not contain the requested assembly.
            // This is because the event of AssemblyResolve will be raised for every
            // Embedded Resources (such as pictures) of the projects.
            // Those resources wil not be loaded by this class and will not exist in dictionary.
        }
    }
}