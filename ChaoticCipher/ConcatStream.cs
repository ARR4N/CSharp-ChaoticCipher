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
using System.Collections;

namespace ChaoticCipher
{
	public class ConcatStream : Stream
	{
		private ArrayList streams = new ArrayList();
		private long pos, len=0;
		
		public ConcatStream (){}
		
		public ConcatStream(ArrayList s){
			this.streams = s;	
		}
		
		public void Push(Stream str)
		{
			this.streams.Add(str);
			len += str.Length;
		}
		
		public override bool CanRead { get { return true; } }
		public override bool CanSeek { get { return true; } }
		public override bool CanWrite { get { return false; } }
		public override long Length { get { return len; } }

		public override long Position
		{
			get { return this.pos; }
			set { this.Seek(value, SeekOrigin.Begin); }
		}
			
		public override void Write(byte[] b, int o, int c){}
		public override void Flush(){}
		public override void SetLength(long l){}
		
		public override long Seek(long offset, SeekOrigin origin)
		{
			switch(origin){
			case SeekOrigin.Begin:
					pos = offset;
					break;
			case SeekOrigin.Current:
					pos += offset;
					break;
			case SeekOrigin.End:
					pos = Length - offset;
					break;
			}
			
			pos = Math.Min(Math.Max(0, pos), Length);
			
			return pos;
		}
		
		public override int Read(byte[] buffer, int offset, int count)
		{
			long start=0, sLen;
			int totalRead=0, bufferPos=offset, read;
			
			foreach(Stream s in streams) 
			{
				sLen = s.Length;
				if(pos < (start+sLen)) //start is initialised at 0 and monotonic increasing so no need to check lower bound
				{
					s.Position = pos - start;
					read = s.Read(buffer, bufferPos, count);
					totalRead += read;
					bufferPos += read;
					pos += read;
					
					if(read==count)
					{
						break;
					}
					
					count -= read;
				}
				start += sLen;
			}
			return totalRead;
		}
	}
}

