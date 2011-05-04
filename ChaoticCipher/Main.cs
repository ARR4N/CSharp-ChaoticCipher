/*
Copyright 2011 Arran Schlosberg (http://arranschlosberg.com);

This file is part of CSharp-ChaoticCipher (https://github.com/aschlosberg/CSharp-ChaoticCipher).

    CSharp-ChaoticCipher is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    CSharp-ChaoticCipher is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with CSharp-ChaoticCipher.  If not, see <http://www.gnu.org/licenses/>.

---------------------------------------------------------------------------------------
*/

using System;
using System.IO;
using System.Security.Cryptography;

namespace ChaoticCipher
{
	class MainClass
	{
		private static string action, inFile, outFile, key;
		private static NonLinearSystem[] systems;
		private static byte[] nudges, inBuffer, outBuffer, iv = new byte[64];
		
		public static void Main (string[] args)
		{
			systems = new NonLinearSystem[16];
			nudges = new byte[systems.Length];
			
			for(ushort i=0; i<systems.Length; i++){
				systems[i] = new Ball();
				nudges[i] = (byte) 0;
			}
			
			action = args[0].ToLower();
			inFile = args[1];
			outFile = args[2];
			
			FileStream inStream = new FileStream(inFile, FileMode.Open);
			FileStream outStream = new FileStream(outFile, FileMode.Create);
			
			inBuffer = new byte[256];
			outBuffer = new byte[inBuffer.Length];
			/*
			 * N.B. each time the ConvertBuffer function is called
			 * it causes a slight break in the plainText nudging as
			 * it treats the buffers as if they are the beginning
			 * of the stream. A smaller buffer uses less memory
			 * but also reduces the effect of the plainText on the
			 * PRNG stream.
			 */
			
			int MACLength;
			try {
				MACLength = Math.Max(0, int.Parse(args[3]));
			}
			catch(Exception){
				Console.WriteLine("No MAC length provided. Using default of 64 bytes.");
				MACLength = 64;
			}
			
			switch(action){
			case "enc":
				Console.Write("Key: ");
				key = ReadBlind();
				Console.Write("Confirm key: ");
				if(key!=ReadBlind()){
					Console.WriteLine("Keys do not match");
					return;
				}
				
				RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
				rng.GetBytes(iv);
				
				outStream.Write(iv, 0, iv.Length);
				
				/*
				 * ### See note after initialisation of in/outBuffer ###
				 * Because of the plainText breaks the null byte authentication
				 * stream had to be concatenated with the inStream so that the
				 * PRNG output was consistent with the one generated when
				 * deciphering.
				 */
				ConcatStream MACStream = new ConcatStream();
				MACStream.Push(inStream);
				MACStream.Push(new MemoryStream(new byte[MACLength]));
				
				Cipher(MACStream, outStream, inBuffer);
				
				break;
			case "dec":
				Console.Write("Key: ");
				key = ReadBlind();
				
				inStream.Read(iv, 0, iv.Length);
				
				Cipher(inStream, outStream, outBuffer);
				
				/*
				 * N.B. if DEciphering is performed with a MACLength
				 * shorter than that used for ENciphering the integrity
				 * of the MAC is compromised and may return true even
				 * if the ciphertext was altered.
				 */
				outStream.Seek(-MACLength, SeekOrigin.End);
				
				byte[] MACbuffer = new byte[MACLength];
				outStream.Read(MACbuffer, 0, MACLength);
				
				outStream.SetLength(outStream.Length - MACLength);
				
				foreach(byte b in MACbuffer){
					if((int)b > 0){
						Console.WriteLine("*** ERROR ***");
						Console.WriteLine("Message authentication failed!");
						Console.WriteLine("Either you provided the wrong decryption settings or the encrypted message was altered.");
						return;
					}
				}
				
				Console.WriteLine("Message authenticated. It is *unlikely* that the encrypted message was altered.");
				
				break;
			};
		}
		
		private static void Cipher(Stream inStream, Stream outStream, byte[] plainBuffer){
			byte[] bKey = System.Text.Encoding.UTF8.GetBytes(key);
			Pass(new SHA512Managed().ComputeHash(bKey, 0, bKey.Length));
			
			Pass(iv);
			
			int read;
			
			while((read=inStream.Read(inBuffer, 0, inBuffer.Length))!=0){
				ConvertBuffer(read, plainBuffer);
				outStream.Write(outBuffer, 0, read);
			}
		}
		
		private static void ConvertBuffer(int len, byte[] plainBuffer){
			int bits, remain, diff;
			ushort nudge, feedForward = 0;
			
			for(int i=0; i<len; i++){
				bits = 0;
				for(int j=0; j<systems.Length; j++){
					diff = i - j - 1;
					if(diff<0){
						diff += plainBuffer.Length;
					}
					nudge = plainBuffer[diff];
					systems[j].Iterate((ushort) Math.Min((ushort) (nudge + feedForward), ushort.MaxValue));
					feedForward = systems[j].GetBits();
					remain = systems.Length - j - 1;
					if(remain<8){
						bits += (int) systems[j].GetBits(1) * (int) Math.Pow(2, remain);
					}
				}
				outBuffer[i] = (byte) ((int) inBuffer[i] ^ bits);
			}
		}
		
		private static void Pass(byte[] bytes){
			ushort nudge, feedForward = 0;
			short diff;
			
			for(ushort i=0; i<bytes.Length + 2*systems.Length; i++){
				for(ushort j=0; j<systems.Length; j++){
					diff = (short) (i-j);
					if(diff < 0 || diff >= bytes.Length){
						nudge = 0;
					}
					else {
						nudge = (ushort) bytes[diff];
					}
					systems[j].Iterate((ushort) Math.Min(nudge + feedForward, ushort.MaxValue));
					feedForward = systems[j].GetBits();
				}
			}
		}
		
		private static String ReadBlind(){
			String line = "";
			ConsoleKeyInfo c;
			while(true){
				c = Console.ReadKey(true);
				if(c.Key.ToString()=="Enter"){
					Console.WriteLine();
					break;	
				}
				line += c.KeyChar;
			}
			return line;
		}
	}
}

